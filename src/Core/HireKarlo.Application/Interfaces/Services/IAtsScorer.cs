using HireKarlo.Domain.ValueObjects;

namespace HireKarlo.Application.Interfaces.Services;

public interface IAtsScorer
{
    Task<AtsReport> AnalyzeAsync(string resumeText, string jobDescription, string jobTitle, CancellationToken cancellationToken = default);
    Task<int> CalculateKeywordDensityScoreAsync(string resumeText, IEnumerable<string> requiredKeywords, CancellationToken cancellationToken = default);
    Task<TitleMatchResult> AnalyzeTitleMatchAsync(string resumeTitle, string jobTitle, CancellationToken cancellationToken = default);
    Task<List<string>> DetectFormattingIssuesAsync(string resumeText, CancellationToken cancellationToken = default);
    Task<List<string>> DetectMissingSectionsAsync(string resumeText, CancellationToken cancellationToken = default);
    Task<List<DateFormatIssue>> ValidateDateFormatsAsync(string resumeText, CancellationToken cancellationToken = default);
}
