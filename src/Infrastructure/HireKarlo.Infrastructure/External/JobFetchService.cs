using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HireKarlo.Application.Interfaces.External;
using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HireKarlo.Infrastructure.External;

public class JobFetchSettings
{
    public string? AdzunaAppId { get; set; }
    public string? AdzunaApiKey { get; set; }
    public string AdzunaCountry { get; set; } = "us";
}

public class JobFetchService : IJobFetchService
{
    private readonly HttpClient _httpClient;
    private readonly JobFetchSettings _settings;
    private readonly ILogger<JobFetchService> _logger;

    public JobFetchService(
        IHttpClientFactory httpClientFactory,
        IOptions<JobFetchSettings> settings,
        ILogger<JobFetchService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("JobFetch");
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<JobListing>> FetchJobsAsync(JobFetchOptions options, CancellationToken cancellationToken = default)
    {
        var allJobs = new List<JobListing>();

        var tasks = new List<Task<List<JobListing>>>();

        if (options.Sources.Contains(JobSource.Adzuna))
        {
            tasks.Add(FetchFromAdzunaAsync(options.Query, options.Location, options.LimitPerSource, cancellationToken));
        }

        if (options.Sources.Contains(JobSource.RemoteOK))
        {
            tasks.Add(FetchFromRemoteOKAsync(options.Query, options.LimitPerSource, cancellationToken));
        }

        if (options.Sources.Contains(JobSource.Arbeitnow))
        {
            tasks.Add(FetchFromArbeitnowAsync(options.Query, options.LimitPerSource, cancellationToken));
        }

        if (options.GreenhouseBoardTokens != null)
        {
            foreach (var token in options.GreenhouseBoardTokens)
            {
                tasks.Add(FetchFromGreenhouseAsync(token, cancellationToken));
            }
        }

        if (options.LeverCompanyIds != null)
        {
            foreach (var companyId in options.LeverCompanyIds)
            {
                tasks.Add(FetchFromLeverAsync(companyId, cancellationToken));
            }
        }

        var results = await Task.WhenAll(tasks);
        foreach (var jobs in results)
        {
            allJobs.AddRange(jobs);
        }

        // Filter remote only if requested
        if (options.RemoteOnly)
        {
            allJobs = allJobs.Where(j => j.IsRemote).ToList();
        }

        _logger.LogInformation("Fetched {Count} total jobs from {SourceCount} sources", 
            allJobs.Count, options.Sources.Count);

        return allJobs;
    }

    public async Task<List<JobListing>> FetchFromAdzunaAsync(
        string? query, 
        string? location, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        if (string.IsNullOrWhiteSpace(_settings.AdzunaAppId) || string.IsNullOrWhiteSpace(_settings.AdzunaApiKey))
        {
            _logger.LogWarning("Adzuna API credentials not configured");
            return jobs;
        }

        try
        {
            var searchQuery = query ?? "software developer";
            var locationQuery = location ?? "";

            var url = $"https://api.adzuna.com/v1/api/jobs/{_settings.AdzunaCountry}/search/1?" +
                      $"app_id={_settings.AdzunaAppId}&app_key={_settings.AdzunaApiKey}" +
                      $"&results_per_page={limit}" +
                      $"&what={Uri.EscapeDataString(searchQuery)}";

            if (!string.IsNullOrWhiteSpace(locationQuery))
            {
                url += $"&where={Uri.EscapeDataString(locationQuery)}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AdzunaResponse>(cancellationToken: cancellationToken);

            if (result?.Results != null)
            {
                foreach (var job in result.Results)
                {
                    jobs.Add(new JobListing
                    {
                        ExternalId = job.Id ?? Guid.NewGuid().ToString(),
                        Source = JobSource.Adzuna,
                        Title = job.Title ?? "Unknown Title",
                        Company = job.Company?.DisplayName ?? "Unknown Company",
                        Location = job.Location?.DisplayName,
                        IsRemote = job.Title?.Contains("remote", StringComparison.OrdinalIgnoreCase) == true ||
                                   job.Description?.Contains("remote", StringComparison.OrdinalIgnoreCase) == true,
                        Description = job.Description,
                        SalaryMin = (int?)job.SalaryMin,
                        SalaryMax = (int?)job.SalaryMax,
                        ApplyUrl = job.RedirectUrl,
                        PostedDate = job.Created ?? DateTime.UtcNow,
                        FetchedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            _logger.LogInformation("Fetched {Count} jobs from Adzuna", jobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Adzuna");
        }

        return jobs;
    }

    public async Task<List<JobListing>> FetchFromRemoteOKAsync(
        string? tags, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            var url = "https://remoteok.com/api";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "HireKarlo/1.0");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<List<RemoteOKJob>>(content);

            if (results != null)
            {
                // First item is usually metadata, skip it
                foreach (var job in results.Skip(1).Take(limit))
                {
                    if (string.IsNullOrWhiteSpace(job.Position)) continue;

                    // Filter by tags if specified
                    if (!string.IsNullOrWhiteSpace(tags))
                    {
                        var tagList = tags.Split(',').Select(t => t.Trim().ToLower());
                        var jobTags = job.Tags?.Select(t => t.ToLower()) ?? Enumerable.Empty<string>();
                        if (!tagList.Any(t => jobTags.Contains(t) || 
                                              job.Position?.ToLower().Contains(t) == true))
                        {
                            continue;
                        }
                    }

                    jobs.Add(new JobListing
                    {
                        ExternalId = job.Id ?? job.Slug ?? Guid.NewGuid().ToString(),
                        Source = JobSource.RemoteOK,
                        Title = job.Position ?? "Unknown Title",
                        Company = job.Company ?? "Unknown Company",
                        CompanyLogoUrl = job.CompanyLogo,
                        Location = job.Location ?? "Remote",
                        IsRemote = true, // RemoteOK is all remote jobs
                        Description = job.Description,
                        SalaryRange = !string.IsNullOrEmpty(job.SalaryMin) && !string.IsNullOrEmpty(job.SalaryMax) 
                            ? $"${job.SalaryMin} - ${job.SalaryMax}" : null,
                        ApplyUrl = job.Url ?? $"https://remoteok.com/jobs/{job.Slug}",
                        PostedDate = ParseUnixTimestamp(job.Date) ?? DateTime.UtcNow,
                        FetchedDate = DateTime.UtcNow,
                        IsActive = true,
                        ExtractedSkills = job.Tags != null ? JsonSerializer.Serialize(job.Tags) : null
                    });
                }
            }

            _logger.LogInformation("Fetched {Count} jobs from RemoteOK", jobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from RemoteOK");
        }

        return jobs;
    }

    public async Task<List<JobListing>> FetchFromArbeitnowAsync(
        string? query, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            var url = "https://www.arbeitnow.com/api/job-board-api";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ArbeitnowResponse>(cancellationToken: cancellationToken);

            if (result?.Data != null)
            {
                var filtered = result.Data.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    filtered = filtered.Where(j => 
                        j.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                        j.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                        j.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) == true);
                }

                foreach (var job in filtered.Take(limit))
                {
                    jobs.Add(new JobListing
                    {
                        ExternalId = job.Slug ?? Guid.NewGuid().ToString(),
                        Source = JobSource.Arbeitnow,
                        Title = job.Title ?? "Unknown Title",
                        Company = job.CompanyName ?? "Unknown Company",
                        CompanyLogoUrl = job.CompanyLogo,
                        Location = job.Location,
                        IsRemote = job.Remote == true,
                        Description = job.Description,
                        ApplyUrl = job.Url,
                        PostedDate = DateTime.TryParse(job.CreatedAt, out var date) ? date : DateTime.UtcNow,
                        FetchedDate = DateTime.UtcNow,
                        IsActive = true,
                        ExtractedSkills = job.Tags != null ? JsonSerializer.Serialize(job.Tags) : null
                    });
                }
            }

            _logger.LogInformation("Fetched {Count} jobs from Arbeitnow", jobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Arbeitnow");
        }

        return jobs;
    }

    public async Task<List<JobListing>> FetchFromGreenhouseAsync(
        string boardToken, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            var url = $"https://boards-api.greenhouse.io/v1/boards/{boardToken}/jobs?content=true";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GreenhouseResponse>(cancellationToken: cancellationToken);

            if (result?.Jobs != null)
            {
                foreach (var job in result.Jobs)
                {
                    var location = job.Offices?.FirstOrDefault()?.Name ?? 
                                   job.Location?.Name ?? "Unknown";

                    jobs.Add(new JobListing
                    {
                        ExternalId = job.Id?.ToString() ?? Guid.NewGuid().ToString(),
                        Source = JobSource.Greenhouse,
                        Title = job.Title ?? "Unknown Title",
                        Company = result.Name ?? boardToken, // Board name is company name
                        Location = location,
                        IsRemote = location.Contains("remote", StringComparison.OrdinalIgnoreCase),
                        Description = job.Content, // HTML content
                        ApplyUrl = job.AbsoluteUrl,
                        PostedDate = job.UpdatedAt ?? DateTime.UtcNow,
                        FetchedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            _logger.LogInformation("Fetched {Count} jobs from Greenhouse ({Board})", jobs.Count, boardToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Greenhouse board {Board}", boardToken);
        }

        return jobs;
    }

    public async Task<List<JobListing>> FetchFromLeverAsync(
        string companyId, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<JobListing>();

        try
        {
            var url = $"https://api.lever.co/v0/postings/{companyId}?mode=json";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<LeverJob>>(cancellationToken: cancellationToken);

            if (results != null)
            {
                foreach (var job in results)
                {
                    var location = job.Categories?.Location ?? "Unknown";

                    jobs.Add(new JobListing
                    {
                        ExternalId = job.Id ?? Guid.NewGuid().ToString(),
                        Source = JobSource.Lever,
                        Title = job.Text ?? "Unknown Title",
                        Company = companyId, // Company ID is the company name in Lever
                        Location = location,
                        IsRemote = location.Contains("remote", StringComparison.OrdinalIgnoreCase),
                        Description = job.DescriptionPlain ?? job.Description,
                        ApplyUrl = job.ApplyUrl ?? job.HostedUrl,
                        PostedDate = ParseUnixTimestampMillis(job.CreatedAt) ?? DateTime.UtcNow,
                        FetchedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            _logger.LogInformation("Fetched {Count} jobs from Lever ({Company})", jobs.Count, companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Lever company {Company}", companyId);
        }

        return jobs;
    }

    private static DateTime? ParseUnixTimestamp(long? timestamp)
    {
        if (timestamp == null) return null;
        return DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime;
    }

    private static DateTime? ParseUnixTimestampMillis(long? timestamp)
    {
        if (timestamp == null) return null;
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value).UtcDateTime;
    }
}

// Adzuna API Response Models
internal class AdzunaResponse
{
    [JsonPropertyName("results")]
    public List<AdzunaJob>? Results { get; set; }
}

internal class AdzunaJob
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("salary_min")]
    public double? SalaryMin { get; set; }

    [JsonPropertyName("salary_max")]
    public double? SalaryMax { get; set; }

    [JsonPropertyName("company")]
    public AdzunaCompany? Company { get; set; }

    [JsonPropertyName("location")]
    public AdzunaLocation? Location { get; set; }
}

internal class AdzunaCompany
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

internal class AdzunaLocation
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

// RemoteOK API Response Models
internal class RemoteOKJob
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("company_logo")]
    public string? CompanyLogo { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("salary_min")]
    public string? SalaryMin { get; set; }

    [JsonPropertyName("salary_max")]
    public string? SalaryMax { get; set; }

    [JsonPropertyName("date")]
    public long? Date { get; set; }
}

// Arbeitnow API Response Models
internal class ArbeitnowResponse
{
    [JsonPropertyName("data")]
    public List<ArbeitnowJob>? Data { get; set; }
}

internal class ArbeitnowJob
{
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("company_logo")]
    public string? CompanyLogo { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("remote")]
    public bool? Remote { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
}

// Greenhouse API Response Models
internal class GreenhouseResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("jobs")]
    public List<GreenhouseJob>? Jobs { get; set; }
}

internal class GreenhouseJob
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("absolute_url")]
    public string? AbsoluteUrl { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("location")]
    public GreenhouseLocation? Location { get; set; }

    [JsonPropertyName("offices")]
    public List<GreenhouseOffice>? Offices { get; set; }
}

internal class GreenhouseLocation
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal class GreenhouseOffice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

// Lever API Response Models
internal class LeverJob
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("descriptionPlain")]
    public string? DescriptionPlain { get; set; }

    [JsonPropertyName("hostedUrl")]
    public string? HostedUrl { get; set; }

    [JsonPropertyName("applyUrl")]
    public string? ApplyUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("categories")]
    public LeverCategories? Categories { get; set; }
}

internal class LeverCategories
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("commitment")]
    public string? Commitment { get; set; }
}
