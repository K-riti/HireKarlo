using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using HireKarlo.Application.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace HireKarlo.Infrastructure.AI;

public class AzureOpenAISettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4o";
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-3-large";
}

public class AzureOpenAIService : IOpenAIService
{
    private readonly AzureOpenAIClient _client;
    private readonly AzureOpenAISettings _settings;
    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(
        IOptions<AzureOpenAISettings> settings,
        ILogger<AzureOpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new ApiKeyCredential(_settings.ApiKey));
    }

    public async Task<string> CompleteAsync(
        string prompt,
        CompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await CompleteWithSystemPromptAsync(
            "You are a helpful AI assistant for career development and job searching.",
            prompt,
            options,
            cancellationToken);
    }

    public async Task<string> CompleteWithSystemPromptAsync(
        string systemPrompt,
        string userPrompt,
        CompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatClient = _client.GetChatClient(_settings.DeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = (float)(options?.Temperature ?? 0.7),
                MaxOutputTokenCount = options?.MaxTokens ?? 2000,
                TopP = (float)(options?.TopP ?? 1.0),
                FrequencyPenalty = (float)(options?.FrequencyPenalty ?? 0),
                PresencePenalty = (float)(options?.PresencePenalty ?? 0)
            };

            var response = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI completion API");
            throw;
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        string prompt,
        CompletionOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatClient = _client.GetChatClient(_settings.DeploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant for career development and job searching."),
            new UserChatMessage(prompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = (float)(options?.Temperature ?? 0.7),
            MaxOutputTokenCount = options?.MaxTokens ?? 2000
        };

        await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, chatOptions, cancellationToken))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    yield return part.Text;
                }
            }
        }
    }

    public async Task<T?> CompleteAsJsonAsync<T>(
        string prompt,
        CompletionOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var jsonPrompt = $"{prompt}\n\nRespond with valid JSON only, no markdown formatting.";
        var response = await CompleteAsync(jsonPrompt, options, cancellationToken);

        // Clean up potential markdown formatting
        response = response.Trim();
        if (response.StartsWith("```json"))
            response = response[7..];
        if (response.StartsWith("```"))
            response = response[3..];
        if (response.EndsWith("```"))
            response = response[..^3];
        response = response.Trim();

        try
        {
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON response: {Response}", response);
            return null;
        }
    }
}
