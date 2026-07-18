using System.ClientModel;
using Azure.AI.OpenAI;
using HireKarlo.Application.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HireKarlo.Infrastructure.AI;

public class EmbeddingService : IEmbeddingService
{
    private readonly AzureOpenAIClient _client;
    private readonly AzureOpenAISettings _settings;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IOptions<AzureOpenAISettings> settings,
        ILogger<EmbeddingService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new ApiKeyCredential(_settings.ApiKey));
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingClient = _client.GetEmbeddingClient(_settings.EmbeddingDeploymentName);
            var response = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);

            return response.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingClient = _client.GetEmbeddingClient(_settings.EmbeddingDeploymentName);
            var textList = texts.ToList();
            var response = await embeddingClient.GenerateEmbeddingsAsync(textList, cancellationToken: cancellationToken);

            return response.Value.Select(e => e.ToFloats().ToArray()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for texts");
            throw;
        }
    }

    public async Task<double> CalculateCosineSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default)
    {
        var embeddings = await GenerateEmbeddingsAsync(new[] { text1, text2 }, cancellationToken);
        return CosineSimilarity(embeddings[0], embeddings[1]);
    }

    private static double CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0;

        return dotProduct / (magnitude1 * magnitude2);
    }
}
