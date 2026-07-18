namespace HireKarlo.Domain.ValueObjects;

public record MatchReport
{
    public double OverallScore { get; init; }
    public double SemanticScore { get; init; }
    public double KeywordScore { get; init; }
    public double TitleScore { get; init; }
    public GapAnalysis GapAnalysis { get; init; } = null!;
    public List<string> Strengths { get; init; } = new();
    public List<string> Weaknesses { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public bool MeetsThreshold => OverallScore >= 90;
}

public record GapAnalysis
{
    public List<SkillGap> SkillGaps { get; init; } = new();
    public List<ExperienceGap> ExperienceGaps { get; init; } = new();
    public List<string> MissingKeywords { get; init; } = new();
    public List<string> MatchingKeywords { get; init; } = new();
    public List<string> PartialMatches { get; init; } = new();
}

public record SkillGap
{
    public string RequiredSkill { get; init; } = string.Empty;
    public string? CurrentLevel { get; init; }
    public string RequiredLevel { get; init; } = string.Empty;
    public GapSeverity Severity { get; init; }
    public List<string> SuggestedResources { get; init; } = new();
    public string? AlternativeSkill { get; init; } // Related skill they might have
}

public record ExperienceGap
{
    public string Requirement { get; init; } = string.Empty;
    public string? CurrentExperience { get; init; }
    public GapSeverity Severity { get; init; }
    public string? Recommendation { get; init; }
}

public enum GapSeverity
{
    Minor = 0,    // Nice to have, can be learned quickly
    Moderate = 1, // Important but can be addressed
    Major = 2,    // Critical skill gap
    Blocker = 3   // Hard requirement not met
}
