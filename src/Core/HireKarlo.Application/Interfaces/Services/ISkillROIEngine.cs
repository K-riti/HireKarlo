namespace HireKarlo.Application.Interfaces.Services;

/// <summary>
/// SkillROIEngine - Calculate return on investment for learning skills
/// 
/// Goes beyond "which skills am I missing?"
/// Predicts: "If I learn this skill, my match will improve by X% in Y weeks"
/// 
/// Example Output:
/// ┌─ Terraform: +12% in 6 weeks = 2.0% per week ⭐⭐⭐⭐⭐
/// ├─ Docker Security: +10% in 3 weeks = 3.3% per week ⭐⭐⭐⭐⭐
/// ├─ Python: +5% in 8 weeks = 0.6% per week ⭐⭐
/// └─ System Design: +20% in 12 weeks = 1.7% per week ⭐⭐⭐⭐
/// 
/// Used for personalized learning roadmaps.
/// </summary>
public interface ISkillROIEngine
{
    /// <summary>
    /// Analyze which skills would have the highest ROI for a user
    /// Returns ranked list of skills with impact estimates
    /// Ordered by ROI score (improvement per week)
    /// </summary>
    Task<List<SkillROIDto>> AnalyzeSkillsROIAsync(
        Guid userId,
        Guid dreamCompanyId,
        CancellationToken ct = default);

    /// <summary>
    /// Get ROI details for a specific skill across all dream companies
    /// "If I learn Kubernetes, how will it help with Adobe, Atlassian, Stripe?"
    /// </summary>
    Task<SkillROIComparisonDto> GetSkillROIAcrossCompaniesAsync(
        Guid userId,
        string skill,
        CancellationToken ct = default);

    /// <summary>
    /// Get recommended learning path to reach target match percentage
    /// Returns ordered list of skills to learn with estimated time
    /// Optimizes for fastest path to goal
    /// </summary>
    Task<LearningPathRecommendationDto> GetRecommendedLearningPathAsync(
        Guid userId,
        Guid dreamCompanyId,
        double targetMatchPercentage = 0.90,
        CancellationToken ct = default);
}

/// <summary>
/// DTO: Individual skill ROI for a dream company
/// </summary>
public class SkillROIDto
{
    public string Skill { get; set; } = string.Empty;
    public int CurrentMatch { get; set; }
    public int ProjectedMatch { get; set; }
    public int ImprovementPercentage { get; set; }
    public int EstimatedWeeks { get; set; }
    public double ROIScore { get; set; } // Improvement per week
    public string Difficulty { get; set; } = "Intermediate";
}

/// <summary>
/// DTO: Skill ROI comparison across multiple companies
/// </summary>
public class SkillROIComparisonDto
{
    public string Skill { get; set; } = string.Empty;
    public List<CompanySkillImpactDto> CompanyImpacts { get; set; } = new();
    public double AverageImprovement { get; set; }
    public int EstimatedLearningWeeks { get; set; }
    public double AverageROIPerWeek { get; set; }
    public List<string> RecommendedForCompanies { get; set; } = new();
}

public class CompanySkillImpactDto
{
    public string CompanyName { get; set; } = string.Empty;
    public double PercentageImprovement { get; set; }
    public int TimeToLearnWeeks { get; set; }
    public double ROIPerWeek { get; set; }
}

/// <summary>
/// DTO: Learning path recommendation to reach target match
/// </summary>
public class LearningPathRecommendationDto
{
    public double CurrentMatch { get; set; }
    public double TargetMatch { get; set; }
    public List<SkillLearningStepDto> Skills { get; set; } = new();
    public int TotalWeeksRequired { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SkillLearningStepDto
{
    public int Priority { get; set; }
    public string Skill { get; set; } = string.Empty;
    public int CurrentMatchPercentage { get; set; }
    public int MatchImprovementPercentage { get; set; }
    public int EstimatedWeeks { get; set; }
    public double ROIPerWeek { get; set; }
    public List<string> Resources { get; set; } = new();
    public string Difficulty { get; set; } = string.Empty;
}
