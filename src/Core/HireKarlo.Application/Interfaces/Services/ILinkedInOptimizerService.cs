namespace HireKarlo.Application.Interfaces.Services;

public interface ILinkedInOptimizerService
{
    Task<ProfileOptimizationResult> OptimizeProfileAsync(LinkedInProfileInput input, OptimizationOptions options, CancellationToken cancellationToken = default);
    Task<HeadlineOptimizationResult> OptimizeHeadlineAsync(string currentHeadline, List<string> targetRoles, CancellationToken cancellationToken = default);
    Task<AboutOptimizationResult> OptimizeAboutAsync(string currentAbout, List<string> targetRoles, List<string> targetKeywords, CancellationToken cancellationToken = default);
}

public record LinkedInProfileInput
{
    public string? Headline { get; init; }
    public string? About { get; init; }
    public List<LinkedInExperience> Experiences { get; init; } = new();
    public List<string> Skills { get; init; } = new();
}

public record LinkedInExperience
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record OptimizationOptions
{
    public List<string> TargetRoles { get; init; } = new();
    public List<string> TargetKeywords { get; init; } = new();
    public string Tone { get; init; } = "Professional"; // Professional, Casual, Technical
    public bool IncludeMetrics { get; init; } = true;
}

public record ProfileOptimizationResult
{
    public int CurrentScore { get; init; }
    public int ProjectedScore { get; init; }
    public HeadlineOptimizationResult Headline { get; init; } = null!;
    public AboutOptimizationResult About { get; init; } = null!;
    public List<ExperienceOptimizationResult> Experiences { get; init; } = new();
    public List<string> MissingKeywords { get; init; } = new();
    public List<string> GeneralRecommendations { get; init; } = new();
}

public record HeadlineOptimizationResult
{
    public string Original { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public List<string> KeywordsToInclude { get; init; } = new();
    public int KeywordScore { get; init; }
}

public record AboutOptimizationResult
{
    public string Original { get; init; } = string.Empty;
    public string Optimized { get; init; } = string.Empty;
    public List<string> AddedKeywords { get; init; } = new();
    public int KeywordDensityBefore { get; init; }
    public int KeywordDensityAfter { get; init; }
    public List<string> ImprovementNotes { get; init; } = new();
}

public record ExperienceOptimizationResult
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? OriginalDescription { get; init; }
    public string? OptimizedDescription { get; init; }
    public List<string> SuggestedBullets { get; init; } = new();
}
