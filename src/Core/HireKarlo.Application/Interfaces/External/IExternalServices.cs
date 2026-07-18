using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Application.Interfaces.External;

public interface IJobFetchService
{
    Task<List<JobListing>> FetchJobsAsync(JobFetchOptions options, CancellationToken cancellationToken = default);
    Task<List<JobListing>> FetchFromAdzunaAsync(string query, string? location, int limit = 50, CancellationToken cancellationToken = default);
    Task<List<JobListing>> FetchFromRemoteOKAsync(string? tags, int limit = 50, CancellationToken cancellationToken = default);
    Task<List<JobListing>> FetchFromArbeitnowAsync(string? query, int limit = 50, CancellationToken cancellationToken = default);
    Task<List<JobListing>> FetchFromGreenhouseAsync(string boardToken, CancellationToken cancellationToken = default);
    Task<List<JobListing>> FetchFromLeverAsync(string companyId, CancellationToken cancellationToken = default);
}

public record JobFetchOptions
{
    public List<JobSource> Sources { get; init; } = new() { JobSource.Adzuna, JobSource.RemoteOK, JobSource.Arbeitnow };
    public string? Query { get; init; }
    public string? Location { get; init; }
    public bool RemoteOnly { get; init; }
    public int LimitPerSource { get; init; } = 50;
    public List<string>? GreenhouseBoardTokens { get; init; }
    public List<string>? LeverCompanyIds { get; init; }
}

public interface IInterviewExperienceFetcher
{
    Task<List<InterviewExperienceResult>> FetchAsync(string company, string? role = null, int limit = 20, CancellationToken cancellationToken = default);
}

public record InterviewExperienceResult
{
    public string SourceUrl { get; init; } = string.Empty;
    public string SourcePlatform { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
    public DateTime? PublishedDate { get; init; }
}

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string blobUrl, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string blobUrl, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task SendMatchAlertAsync(string toEmail, string userName, List<MatchAlertItem> matches, CancellationToken cancellationToken = default);
    Task SendWeeklyDigestAsync(string toEmail, string userName, WeeklyDigestContent content, CancellationToken cancellationToken = default);
    Task SendReferralDraftReadyAsync(string toEmail, string userName, string contactName, string company, CancellationToken cancellationToken = default);
}

public record MatchAlertItem
{
    public string JobTitle { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public double MatchScore { get; init; }
    public string ApplyUrl { get; init; } = string.Empty;
}

public record WeeklyDigestContent
{
    public int NewMatches { get; init; }
    public int ApplicationsThisWeek { get; init; }
    public List<InterviewDigestSummary> InterviewExperiences { get; init; } = new();
    public List<string> RoadmapReminders { get; init; } = new();
}

public record InterviewDigestSummary
{
    public string Company { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
}
