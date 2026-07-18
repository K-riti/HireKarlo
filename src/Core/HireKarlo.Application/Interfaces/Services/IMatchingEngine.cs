using HireKarlo.Domain.ValueObjects;

namespace HireKarlo.Application.Interfaces.Services;

public interface IMatchingEngine
{
    Task<MatchReport> CalculateMatchAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default);
    Task<double> CalculateSemanticSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default);
    Task<GapAnalysis> AnalyzeGapsAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default);
    Task<List<MatchResult>> FindMatchingJobsAsync(Guid userId, double minScore = 80, int limit = 50, CancellationToken cancellationToken = default);
}

public record MatchResult
{
    public Guid JobListingId { get; init; }
    public double Score { get; init; }
    public MatchReport Report { get; init; } = null!;
}
