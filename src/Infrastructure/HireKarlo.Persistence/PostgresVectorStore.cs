using System.Text.Json;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Persistence;

/// <summary>
/// PostgreSQL-backed vector store that persists across Render cold starts.
/// Uses standard PostgreSQL with embeddings stored as comma-separated text.
/// For higher performance at scale, consider enabling pgvector extension.
/// </summary>
public class PostgresVectorStore : IVectorStoreService
{
    private readonly HireKarloDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<PostgresVectorStore> _logger;

    public PostgresVectorStore(
        HireKarloDbContext dbContext,
        IEmbeddingService embeddingService,
        ILogger<PostgresVectorStore> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<string> IndexDocumentAsync(
        string content, 
        string documentType, 
        Dictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);

            var document = new VectorDocument
            {
                Id = Guid.NewGuid(),
                Content = content,
                DocumentType = documentType,
                MetadataJson = JsonSerializer.Serialize(metadata ?? new Dictionary<string, string>()),
                IndexedAt = DateTime.UtcNow
            };
            document.SetEmbedding(embedding);

            _dbContext.VectorDocuments.Add(document);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Indexed document {Id} of type {Type}", document.Id, documentType);
            return document.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document of type {Type}", documentType);
            throw;
        }
    }

    public async Task<List<VectorSearchResult>> SearchAsync(
        string query, 
        string? documentType = null, 
        int topK = 10, 
        double minScore = 0.7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            // Get candidates from database
            var documentsQuery = _dbContext.VectorDocuments.AsQueryable();

            if (!string.IsNullOrEmpty(documentType))
            {
                documentsQuery = documentsQuery.Where(d => d.DocumentType == documentType);
            }

            var documents = await documentsQuery.ToListAsync(cancellationToken);

            // Calculate cosine similarity in memory (for now)
            // For production scale, use pgvector's <=> operator
            var results = documents
                .Select(d => new
                {
                    Document = d,
                    Score = CosineSimilarity(queryEmbedding, d.GetEmbedding())
                })
                .Where(x => x.Score >= minScore)
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => new VectorSearchResult
                {
                    DocumentId = x.Document.Id.ToString(),
                    Content = x.Document.Content,
                    Score = x.Score,
                    Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(x.Document.MetadataJson) 
                              ?? new Dictionary<string, string>()
                })
                .ToList();

            _logger.LogDebug("Search for '{Query}' returned {Count} results", 
                query.Length > 50 ? query[..50] + "..." : query, results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search for query");
            return new List<VectorSearchResult>();
        }
    }

    public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(documentId, out var id))
        {
            _logger.LogWarning("Invalid document ID format: {DocumentId}", documentId);
            return;
        }

        var document = await _dbContext.VectorDocuments.FindAsync(new object[] { id }, cancellationToken);
        if (document != null)
        {
            _dbContext.VectorDocuments.Remove(document);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Deleted document {Id}", documentId);
        }
    }

    public async Task<bool> DocumentExistsAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(documentId, out var id))
            return false;

        return await _dbContext.VectorDocuments.AnyAsync(d => d.Id == id, cancellationToken);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0 || a.Length != b.Length)
            return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
