using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// USP #3: Referral Intelligence - Tracks potential referral contacts at dream companies
/// Finds employees with similar backgrounds and suggests outreach strategies
/// </summary>
public class ReferralTarget : BaseEntity
{
    /// <summary>
    /// User looking to get referred
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Dream company this referral target works at
    /// </summary>
    public Guid DreamCompanyId { get; set; }

    /// <summary>
    /// Full name of the potential referrer
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Job title at the company (e.g., "Backend Engineer", "Engineering Manager")
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Department (e.g., "Engineering", "Platform", "Infrastructure")
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// LinkedIn profile URL
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Email address (if available)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number (if available)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Similarity score to user's profile (0-100)
    /// Calculated based on education, experience, skills, background
    /// </summary>
    public double SimilarityScore { get; set; }

    /// <summary>
    /// JSON breakdown of similarity
    /// Example: {"education": "Both IIT Bombay", "experience": "5 years backend", "skills": "Docker, K8s overlap"}
    /// </summary>
    public string? BackgroundSimilarity { get; set; }

    /// <summary>
    /// AI-generated strategy for reaching out
    /// Example: "Start with LinkedIn message mentioning shared IIT background, mention Docker interest"
    /// </summary>
    public string? SuggestedOutreach { get; set; }

    /// <summary>
    /// AI-generated message template for initial contact
    /// Ready-to-use draft that user can customize
    /// </summary>
    public string? DraftMessage { get; set; }

    /// <summary>
    /// When outreach was sent (if sent)
    /// </summary>
    public DateTime? OutreachSentAt { get; set; }

    /// <summary>
    /// When follow-up is due
    /// </summary>
    public DateTime? FollowUpDueAt { get; set; }

    /// <summary>
    /// Current status of referral attempt
    /// </summary>
    public ReferralStatus Status { get; set; } = ReferralStatus.NoAction;

    /// <summary>
    /// Additional notes about the referral (optional)
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DreamCompany DreamCompany { get; set; } = null!;
}
