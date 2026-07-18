using HireKarlo.Domain.Entities;

namespace HireKarlo.Application.Interfaces.Services;

public interface IRoadmapService
{
    Task<List<RoadmapItem>> GenerateRoadmapAsync(Guid userId, RoadmapGenerationOptions options, CancellationToken cancellationToken = default);
    Task<List<ProjectRecommendation>> GetProjectRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SkillTrajectoryResult> SimulateSkillTrajectoryAsync(Guid userId, List<string> skillsToLearn, CancellationToken cancellationToken = default);
}

public record RoadmapGenerationOptions
{
    public int WeeksToGenerate { get; init; } = 24; // 6 months
    public List<string> TargetRoles { get; init; } = new();
    public List<string> TargetCompanies { get; init; } = new();
    public int HoursPerWeek { get; init; } = 20;
    public bool IncludeDSA { get; init; } = true;
    public bool IncludeSystemDesign { get; init; } = true;
    public bool IncludeBehavioral { get; init; } = true;
    public bool IncludeProjects { get; init; } = true;
}

public record ProjectRecommendation
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> SkillsAddressed { get; init; } = new();
    public string Difficulty { get; init; } = string.Empty;
    public int EstimatedHours { get; init; }
    public List<ResourceLink> Resources { get; init; } = new();
    public double ImpactScore { get; init; } // How much this improves match rates
}

public record ResourceLink
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // YouTube, Article, Course, GitHub
}

public record SkillTrajectoryResult
{
    public double CurrentAverageMatchRate { get; init; }
    public double ProjectedMatchRate { get; init; }
    public Dictionary<string, double> MatchRateByCompany { get; init; } = new();
    public List<SkillImpact> SkillImpacts { get; init; } = new();
}

public record SkillImpact
{
    public string Skill { get; init; } = string.Empty;
    public double MatchRateIncrease { get; init; }
    public int JobsUnlocked { get; init; }
}
