using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// Persisted vector document for semantic search.
/// Stores embeddings in PostgreSQL for persistence across cold starts.
/// </summary>
public class VectorDocument : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Embedding stored as comma-separated floats.
    /// For production with high volume, consider pgvector extension.
    /// This approach works without pgvector and survives Render cold starts.
    /// </summary>
    public string EmbeddingData { get; set; } = string.Empty;

    public string MetadataJson { get; set; } = "{}";
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;

    // Helper to convert to/from float array
    public float[] GetEmbedding()
    {
        if (string.IsNullOrEmpty(EmbeddingData))
            return Array.Empty<float>();

        return EmbeddingData.Split(',')
            .Select(s => float.TryParse(s, out var f) ? f : 0f)
            .ToArray();
    }

    public void SetEmbedding(float[] embedding)
    {
        EmbeddingData = string.Join(",", embedding.Select(f => f.ToString("G9")));
    }
}
