using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// USP #5: Opportunity Radar - Tracks job opportunities matched to dream companies
/// Replaces the job automation feature with intelligent opportunity discovery
/// Instead of auto-applying, finds relevant opportunities with match explanation
/// </summary>
public class OpportunityMatch : BaseEntity
{
    /// <summary>
    /// User discovering this opportunity
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Job listing this opportunity is for
    /// </summary>
    public Guid JobListingId { get; set; }

    /// <summary>
    /// Which dream company is this opportunity for?
    /// </summary>
    public Guid DreamCompanyId { get; set; }

    /// <summary>
    /// Overall match percentage (0-100)
    /// Shows how well this job aligns with user's profile and dream company goal
    /// </summary>
    public double MatchPercentage { get; set; }

    /// <summary>
    /// When this opportunity was discovered
    /// </summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>
    /// Whether user has been notified about this opportunity
    /// </summary>
    public bool NotificationSent { get; set; }

    /// <summary>
    /// Human-friendly explanation of why this is a match
    /// Example: "92% match: You have strong Docker background. Missing: Terraform (learning path → +12%)"
    /// </summary>
    public string ExplanationForMatch { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of factors that make this a good match
    /// Example: ["5+ years backend experience", "Docker expert", "Microservices architecture"]
    /// </summary>
    public string MatchingFactors { get; set; } = "[]";

    /// <summary>
    /// JSON array of missing factors
    /// Example: ["Terraform experience", "Kubernetes at scale"]
    /// </summary>
    public string MissingFactors { get; set; } = "[]";

    /// <summary>
    /// Count of skills user already has that this job needs
    /// </summary>
    public int SkillsAlreadyHave { get; set; }

    /// <summary>
    /// Count of skills user needs to learn for this job
    /// </summary>
    public int SkillsToLearn { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual JobListing JobListing { get; set; } = null!;
    public virtual DreamCompany DreamCompany { get; set; } = null!;
}
