namespace HireKarlo.Application.Interfaces.AI;

public interface IOpenAIService
{
    Task<string> CompleteAsync(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default);
    Task<string> CompleteWithSystemPromptAsync(string systemPrompt, string userPrompt, CompletionOptions? options = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamCompleteAsync(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default);
    Task<T?> CompleteAsJsonAsync<T>(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default) where T : class;
}

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    Task<double> CalculateCosineSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
}

public interface IVectorStoreService
{
    Task<string> IndexDocumentAsync(string content, string documentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> SearchAsync(string query, string? documentType = null, int topK = 10, double minScore = 0.7, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    Task<bool> DocumentExistsAsync(string documentId, CancellationToken cancellationToken = default);
}

public record CompletionOptions
{
    public string? Model { get; init; }
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2000;
    public double TopP { get; init; } = 1.0;
    public double FrequencyPenalty { get; init; } = 0;
    public double PresencePenalty { get; init; } = 0;
}

public record VectorSearchResult
{
    public string DocumentId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public double Score { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}
