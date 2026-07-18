using System.Collections.Concurrent;
using HireKarlo.Application.Interfaces.AI;

namespace HireKarlo.Infrastructure.AI;

/// <summary>
/// In-memory vector store - completely free, no external dependencies.
/// Suitable for small to medium datasets. For production with large datasets,
/// consider using PostgreSQL with pgvector extension (still free on Render).
/// </summary>
public class InMemoryVectorStore : IVectorStoreService
{
    private readonly ConcurrentDictionary<string, VectorDocument> _documents = new();
    private readonly IEmbeddingService _embeddingService;

    public InMemoryVectorStore(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    public async Task<string> IndexDocumentAsync(string content, string documentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var embedding = await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);

        var document = new VectorDocument
        {
            Id = id,
            Content = content,
            DocumentType = documentType,
            Embedding = embedding,
            Metadata = metadata ?? new Dictionary<string, string>(),
            IndexedAt = DateTime.UtcNow
        };

        _documents.AddOrUpdate(id, document, (_, _) => document);
        return id;
    }

    public async Task<List<VectorSearchResult>> SearchAsync(
        string query, 
        string? documentType = null, 
        int topK = 10, 
        double minScore = 0.7,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

        var results = _documents.Values
            .Where(d => documentType == null || d.DocumentType == documentType)
            .Select(d => new
            {
                Document = d,
                Score = CosineSimilarity(queryEmbedding, d.Embedding)
            })
            .Where(x => x.Score >= minScore)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => new VectorSearchResult
            {
                DocumentId = x.Document.Id,
                Content = x.Document.Content,
                Score = x.Score,
                Metadata = x.Document.Metadata
            });

        return results.ToList();
    }

    public Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        _documents.TryRemove(documentId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> DocumentExistsAsync(string documentId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_documents.ContainsKey(documentId));
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
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

    private class VectorDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTime IndexedAt { get; set; }
    }
}
