namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Application.DTOs.CareerOS;
using HireKarlo.Domain.Entities;

/// <summary>
/// USP #1: Dream Company Intelligence
/// Calculates and tracks match percentage for each dream company
/// Provides breakdown by dimension (skills, experience, culture fit, etc.)
/// </summary>
public interface IMatchPercentageService
{
    /// <summary>
    /// Calculate match percentage for a user against a specific dream company
    /// Returns detailed breakdown and gaps
    /// </summary>
    Task<MatchCalculationResult> CalculateMatchPercentageAsync(
        Guid userId, 
        Guid dreamCompanyId);

    /// <summary>
    /// Recalculate when user adds skill or milestone
    /// </summary>
    Task<MatchCalculationResult> RecalculateMatchAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Get match breakdown for multiple companies
    /// Used in dashboard to show all company matches at once
    /// </summary>
    Task<Dictionary<string, MatchCalculationResult>> GetAllCompanyMatchesAsync(Guid userId);

    /// <summary>
    /// Calculate how much a skill improves match % (Skill ROI)
    /// Example: "Learn Terraform → +12% to Adobe, +7% to Atlassian"
    /// </summary>
    Task<SkillROIAnalysis> CalculateSkillROIAsync(string skill, List<Guid> dreamCompanyIds);

    /// <summary>
    /// Identify critical gaps (highest priority skills to learn)
    /// </summary>
    Task<List<GapDto>> IdentifyGapsAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Estimate days to reach target match percentage
    /// </summary>
    Task<int> EstimateDaysToTargetAsync(Guid userId, Guid dreamCompanyId, double targetPercentage = 90);
}

/// <summary>
/// Result of match calculation
/// </summary>
public class MatchCalculationResult
{
    public Guid DreamCompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public Dictionary<string, double> DimensionScores { get; set; } = new();
    public List<GapDto> Gaps { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public string ExplanationSummary { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// ROI (Return on Investment) for learning a skill
/// </summary>
public class SkillROIAnalysis
{
    public string Skill { get; set; } = string.Empty;
    public Dictionary<string, double> CompanyImpacts { get; set; } = new(); // Company -> % increase
    public double AverageROI { get; set; }
    public int TotalCompaniesAffected { get; set; }
    public string Summary { get; set; } = string.Empty;
}
