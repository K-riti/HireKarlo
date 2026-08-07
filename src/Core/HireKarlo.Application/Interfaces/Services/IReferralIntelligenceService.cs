namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Application.DTOs.CareerOS;
using HireKarlo.Domain.Entities;

/// <summary>
/// USP #3: Referral Intelligence
/// Finds potential referrers at dream companies based on profile similarity
/// Generates outreach strategies and draft messages
/// </summary>
public interface IReferralIntelligenceService
{
    /// <summary>
    /// Find potential referral targets at a dream company
    /// Returns employees with similar background, education, experience
    /// </summary>
    Task<List<ReferralOpportunitySummaryDto>> FindReferralTargetsAsync(Guid userId, Guid dreamCompanyId, CancellationToken ct = default);

    /// <summary>
    /// Get details for a specific referral target
    /// </summary>
    Task<ReferralTarget> GetReferralTargetAsync(Guid userId, Guid referralTargetId, CancellationToken ct = default);

    /// <summary>
    /// Generate AI-powered outreach strategy for a referral target
    /// "Start with LinkedIn message mentioning shared IIT background"
    /// </summary>
    Task<string> GenerateOutreachStrategyAsync(Guid userId, Guid referralTargetId, CancellationToken ct = default);

    /// <summary>
    /// Generate draft message for initial contact
    /// Ready-to-use template that preserves personalization
    /// </summary>
    Task<string> GenerateDraftMessageAsync(Guid userId, Guid referralTargetId, CancellationToken ct = default);

    /// <summary>
    /// Update referral status (NoAction, Contacted, Responded, Referred, Rejected)
    /// </summary>
    Task<ReferralTarget> UpdateReferralStatusAsync(Guid referralTargetId, string newStatus, CancellationToken ct = default);

    /// <summary>
    /// Set follow-up reminder for referral
    /// </summary>
    Task<ReferralTarget> SetFollowUpReminderAsync(Guid referralTargetId, DateTime followUpDate, CancellationToken ct = default);

    /// <summary>
    /// Get all referral targets grouped by company
    /// </summary>
    Task<Dictionary<string, List<ReferralOpportunitySummaryDto>>> GetAllReferralTargetsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Send referral follow-up reminder notifications
    /// </summary>
    Task SendReferralReminderNotificationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Calculate similarity score between user and referral target
    /// Based on education, experience, skills, background
    /// </summary>
    Task<double> CalculateSimilarityScoreAsync(Guid userId, Guid referralTargetId, CancellationToken ct = default);

    /// <summary>
    /// Ingest LinkedIn referral targets from API
    /// </summary>
    Task IngestLinkedInReferralsAsync(Guid userId, Guid dreamCompanyId, CancellationToken ct = default);
}
