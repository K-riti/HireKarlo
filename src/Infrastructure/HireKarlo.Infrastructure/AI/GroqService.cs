using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HireKarlo.Application.Interfaces.AI;
using Microsoft.Extensions.Options;

namespace HireKarlo.Infrastructure.AI;

public class GroqSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile"; // Free, very capable
}

public class GroqService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly GroqSettings _settings;
    private const string BaseUrl = "https://api.groq.com/openai/v1";

    public GroqService(HttpClient httpClient, IOptions<GroqSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public async Task<string> CompleteAsync(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = options?.MaxTokens ?? 2048,
            temperature = options?.Temperature ?? 0.7
        };

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: cancellationToken);
        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    public async Task<string> CompleteWithSystemPromptAsync(string systemPrompt, string userPrompt, CompletionOptions? options = null, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settings.Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            max_tokens = options?.MaxTokens ?? 2048,
            temperature = options?.Temperature ?? 0.7
        };

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: cancellationToken);
        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    public async Task<T?> CompleteAsJsonAsync<T>(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var jsonPrompt = $@"{prompt}

IMPORTANT: Respond ONLY with valid JSON that matches the expected structure. No markdown, no code blocks, no explanations.";

        var response = await CompleteAsync(jsonPrompt, options, cancellationToken);

        // Clean up the response - remove markdown code blocks if present
        response = CleanJsonResponse(response);

        try
        {
            var result = JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(string prompt, CompletionOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = options?.MaxTokens ?? 2048,
            temperature = options?.Temperature ?? 0.7,
            stream = true
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..];
            if (data == "[DONE]")
                break;

            var chunk = JsonSerializer.Deserialize<GroqResponse>(data);
            var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(content))
                yield return content;
        }
    }

    private static string CleanJsonResponse(string response)
    {
        response = response.Trim();

        // Remove markdown code blocks
        if (response.StartsWith("```json"))
            response = response[7..];
        else if (response.StartsWith("```"))
            response = response[3..];

        if (response.EndsWith("```"))
            response = response[..^3];

        return response.Trim();
    }
}

// Groq API Response Models
public class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

public class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public GroqMessage? Delta { get; set; }
}

public class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
