using System.Net.Http.Json;
using HireKarlo.Application.Interfaces.AI;
using Microsoft.Extensions.Options;

namespace HireKarlo.Infrastructure.AI;

public class HuggingFaceSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "sentence-transformers/all-MiniLM-L6-v2"; // Free, fast, good quality
}

public class HuggingFaceEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceSettings _settings;
    private const string BaseUrl = "https://api-inference.huggingface.co/pipeline/feature-extraction";

    public HuggingFaceEmbeddingService(HttpClient httpClient, IOptions<HuggingFaceSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new { inputs = text };

        var response = await _httpClient.PostAsJsonAsync(
            $"{BaseUrl}/{_settings.Model}", 
            request, 
            cancellationToken);

        response.EnsureSuccessStatusCode();

        // HuggingFace returns the embedding directly as an array
        var embedding = await response.Content.ReadFromJsonAsync<float[]>(cancellationToken: cancellationToken);
        return embedding ?? Array.Empty<float>();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var results = new List<float[]>();

        // Process in batches to avoid rate limits
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
            results.Add(embedding);

            // Small delay to avoid rate limiting
            await Task.Delay(100, cancellationToken);
        }

        return results;
    }

    public async Task<double> CalculateCosineSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default)
    {
        var embedding1 = await GenerateEmbeddingAsync(text1, cancellationToken);
        var embedding2 = await GenerateEmbeddingAsync(text2, cancellationToken);

        return CosineSimilarity(embedding1, embedding2);
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
}
