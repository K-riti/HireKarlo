namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;

/// <summary>
/// Tracks career milestones and progress towards dream company goals
/// Provides visibility into user's journey and impact of each achievement
/// </summary>
public interface ICareerProgressService
{
    /// <summary>
    /// Record a career milestone (skill learned, project completed, cert earned, etc.)
    /// Automatically recalculates match % for affected dream companies
    /// </summary>
    Task<CareerProgress> RecordMilestoneAsync(Guid userId, MilestoneType type, string description, string? evidence = null);

    /// <summary>
    /// Get user's complete career journey
    /// Chronological list of all milestones with impact
    /// </summary>
    Task<List<CareerProgress>> GetUserJourneyAsync(Guid userId);

    /// <summary>
    /// Get milestones for a specific dream company
    /// </summary>
    Task<List<CareerProgress>> GetMilestonesForCompanyAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Calculate impact of a milestone
    /// Returns how much it improved match % for each company
    /// </summary>
    Task<Dictionary<string, double>> CalculateMilestoneImpactAsync(Guid userId, Guid progressId);

    /// <summary>
    /// Get progress summary (# of milestones, average impact, etc.)
    /// </summary>
    Task<ProgressSummaryDto> GetProgressSummaryAsync(Guid userId);

    /// <summary>
    /// Link milestone to skill(s) unlocked
    /// </summary>
    Task<CareerProgress> LinkSkillsToMilestoneAsync(Guid progressId, List<Guid> skillIds);

    /// <summary>
    /// Delete milestone (if user made a mistake)
    /// </summary>
    Task DeleteMilestoneAsync(Guid progressId);

    /// <summary>
    /// Get share-ready milestone summary
    /// For user to post on LinkedIn
    /// </summary>
    Task<string> GetMilestoneShareTextAsync(Guid progressId);
}

/// <summary>
/// Progress summary
/// </summary>
public class ProgressSummaryDto
{
    public int TotalMilestones { get; set; }
    public double AverageImpactPerMilestone { get; set; }
    public int MaxMonthlyMilestones { get; set; }
    public DateTime? MostRecentMilestone { get; set; }
    public List<CareerProgress> RecentMilestones { get; set; } = new();
    public Dictionary<MilestoneType, int> MilestonesByType { get; set; } = new();
}
