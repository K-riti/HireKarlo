using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// Tracks career milestones and progress towards dream companies
/// Provides visibility into user's journey and impact of achievements
/// </summary>
public class CareerProgress : BaseEntity
{
    /// <summary>
    /// User whose progress this tracks
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// When this milestone occurred
    /// </summary>
    public DateTime CheckinDate { get; set; }

    /// <summary>
    /// Type of milestone achieved
    /// </summary>
    public MilestoneType MilestoneType { get; set; }

    /// <summary>
    /// Human-readable description of the milestone
    /// Example: "Completed Docker & Kubernetes course on Udemy"
    /// </summary>
    public string? MilestoneDescription { get; set; }

    /// <summary>
    /// Which dream company does this milestone help reach
    /// Can be null if it helps multiple companies
    /// </summary>
    public Guid? RelatedDreamCompanyId { get; set; }

    /// <summary>
    /// Average impact on dream company match percentages
    /// Example: A Docker certification might increase overall average by 5%
    /// </summary>
    public double ImpactOnDreamCompanies { get; set; }

    /// <summary>
    /// Link to evidence (project URL, certificate, GitHub repo, etc.)
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Optional skills unlocked by this milestone
    /// Example: Completing Docker course unlocks ["Docker", "Container Orchestration"]
    /// </summary>
    public string? SkillsUnlocked { get; set; }

    /// <summary>
    /// Additional notes about this milestone
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether user shared this milestone publicly/on LinkedIn
    /// </summary>
    public bool IsPublic { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DreamCompany? DreamCompany { get; set; }
}
