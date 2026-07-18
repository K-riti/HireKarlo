using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using HireKarlo.Application.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HireKarlo.Infrastructure.AI;

public class AzureAISearchSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = "hirekarlo-documents";
    public int VectorDimensions { get; set; } = 3072; // text-embedding-3-large dimensions
}

public class AzureAISearchService : IVectorStoreService
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly AzureAISearchSettings _settings;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<AzureAISearchService> _logger;
    private bool _indexInitialized;

    public AzureAISearchService(
        IOptions<AzureAISearchSettings> settings,
        IEmbeddingService embeddingService,
        ILogger<AzureAISearchService> logger)
    {
        _settings = settings.Value;
        _embeddingService = embeddingService;
        _logger = logger;

        var credential = new AzureKeyCredential(_settings.ApiKey);
        _indexClient = new SearchIndexClient(new Uri(_settings.Endpoint), credential);
        _searchClient = new SearchClient(new Uri(_settings.Endpoint), _settings.IndexName, credential);
    }

    public async Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_indexInitialized) return;

        try
        {
            var index = new SearchIndex(_settings.IndexName)
            {
                Fields = new List<SearchField>
                {
                    new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                    new SearchableField("content") { AnalyzerName = LexicalAnalyzerName.EnLucene },
                    new SimpleField("documentType", SearchFieldDataType.String) { IsFilterable = true },
                    new SimpleField("createdAt", SearchFieldDataType.DateTimeOffset) { IsSortable = true },
                    new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                    {
                        IsSearchable = true,
                        VectorSearchDimensions = _settings.VectorDimensions,
                        VectorSearchProfileName = "vector-profile"
                    }
                },
                VectorSearch = new VectorSearch
                {
                    Profiles =
                    {
                        new VectorSearchProfile("vector-profile", "hnsw-config")
                    },
                    Algorithms =
                    {
                        new HnswAlgorithmConfiguration("hnsw-config")
                        {
                            Parameters = new HnswParameters
                            {
                                M = 4,
                                EfConstruction = 400,
                                EfSearch = 500,
                                Metric = VectorSearchAlgorithmMetric.Cosine
                            }
                        }
                    }
                }
            };

            await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
            _indexInitialized = true;
            _logger.LogInformation("Azure AI Search index {IndexName} initialized", _settings.IndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Azure AI Search index");
            throw;
        }
    }

    public async Task<string> IndexDocumentAsync(
        string content, 
        string documentType, 
        Dictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexExistsAsync(cancellationToken);

        var documentId = Guid.NewGuid().ToString();
        var embedding = await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);

        var document = new SearchDocument
        {
            ["id"] = documentId,
            ["content"] = content,
            ["documentType"] = documentType,
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["contentVector"] = embedding
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                document[kvp.Key] = kvp.Value;
            }
        }

        await _searchClient.IndexDocumentsAsync(
            IndexDocumentsBatch.Upload(new[] { document }), 
            cancellationToken: cancellationToken);

        _logger.LogInformation("Indexed document {DocumentId} of type {DocumentType}", documentId, documentType);
        return documentId;
    }

    public async Task<List<VectorSearchResult>> SearchAsync(
        string query, 
        string? documentType = null, 
        int topK = 10, 
        double minScore = 0.7, 
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexExistsAsync(cancellationToken);

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

        var vectorQuery = new VectorizedQuery(queryEmbedding)
        {
            KNearestNeighborsCount = topK,
            Fields = { "contentVector" }
        };

        var searchOptions = new SearchOptions
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries = { vectorQuery }
            },
            Size = topK,
            Select = { "id", "content", "documentType", "createdAt" }
        };

        if (!string.IsNullOrEmpty(documentType))
        {
            searchOptions.Filter = $"documentType eq '{documentType}'";
        }

        var response = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions, cancellationToken);

        var results = new List<VectorSearchResult>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            if (result.Score >= minScore)
            {
                results.Add(new VectorSearchResult
                {
                    DocumentId = result.Document["id"]?.ToString() ?? string.Empty,
                    Content = result.Document["content"]?.ToString() ?? string.Empty,
                    Score = result.Score ?? 0,
                    Metadata = new Dictionary<string, string>
                    {
                        ["documentType"] = result.Document["documentType"]?.ToString() ?? string.Empty
                    }
                });
            }
        }

        return results;
    }

    public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        await _searchClient.DeleteDocumentsAsync(
            "id", 
            new[] { documentId }, 
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted document {DocumentId}", documentId);
    }

    public async Task<bool> DocumentExistsAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _searchClient.GetDocumentAsync<SearchDocument>(documentId, cancellationToken: cancellationToken);
            return response.Value != null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
