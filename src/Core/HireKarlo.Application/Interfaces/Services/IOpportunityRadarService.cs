namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Application.DTOs.CareerOS;
using HireKarlo.Domain.Entities;

/// <summary>
/// USP #5: Opportunity Radar
/// Replaces job automation with intelligent opportunity discovery
/// Finds relevant opportunities and explains why they match user's dream company goals
/// Instead of auto-applying: finds + explains + surfaces
/// </summary>
public interface IOpportunityRadarService
{
    /// <summary>
    /// Find new opportunities that match user's profile and dream companies
    /// Called daily to surface new relevant jobs
    /// </summary>
    Task<List<OpportunityMatch>> FindNewOpportunitiesAsync(Guid userId);

    /// <summary>
    /// Find opportunities for a specific dream company
    /// </summary>
    Task<List<OpportunityMatch>> FindOpportunitiesForCompanyAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Generate human-friendly explanation of why this job matches
    /// "92% match: You have strong Docker background. Missing: Terraform (your learning path)"
    /// </summary>
    Task<string> GenerateOpportunityExplanationAsync(Guid userId, Guid jobListingId, Guid dreamCompanyId);

    /// <summary>
    /// Get opportunity details with full breakdown
    /// </summary>
    Task<OpportunityDto> GetOpportunityDetailsAsync(Guid userId, Guid opportunityMatchId);

    /// <summary>
    /// Send daily Opportunity Radar digest to user
    /// "12 new matches found today"
    /// </summary>
    Task SendOpportunityRadarNotificationAsync(Guid userId, List<OpportunityMatch> opportunities);

    /// <summary>
    /// Mark opportunity as viewed/dismissed
    /// </summary>
    Task<OpportunityMatch> UpdateOpportunityStatusAsync(Guid opportunityMatchId, string status);

    /// <summary>
    /// Get opportunities grouped by dream company
    /// Used in dashboard
    /// </summary>
    Task<Dictionary<string, List<OpportunityDto>>> GetOpportunitiesByCompanyAsync(Guid userId);

    /// <summary>
    /// Schedule opportunity radar to run at specific times (6 AM, 12 PM UTC)
    /// Replaces job automation scheduling
    /// </summary>
    Task ScheduleOpportunityRadarAsync(Guid userId);
}
