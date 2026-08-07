using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// Tracks the match percentage and gaps for a specific dream company
/// USP #1: Dream Company Intelligence - detailed breakdown of why you're X% matched
/// </summary>
public class DreamCompanyMatch : BaseEntity
{
    /// <summary>
    /// User who is tracking this match
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Reference to the dream company
    /// </summary>
    public Guid DreamCompanyId { get; set; }

    /// <summary>
    /// Current match percentage (0-100)
    /// </summary>
    public double CurrentMatchPercentage { get; set; }

    /// <summary>
    /// Target match percentage user wants to reach (default 90)
    /// </summary>
    public double TargetMatchPercentage { get; set; } = 90.0;

    /// <summary>
    /// Match breakdown by dimension (JSON)
    /// Example: {"Skills": 60, "Experience": 75, "CultureFit": 85, "IndustryKnowledge": 50}
    /// </summary>
    public string MatchBreakdown { get; set; } = "{}";

    /// <summary>
    /// Gap analysis identifying what's missing to reach target (JSON)
    /// Example: [{"skill": "Docker", "priority": "High", "reason": "In 4/5 recent job openings"}]
    /// </summary>
    public string GapAnalysis { get; set; } = "[]";

    /// <summary>
    /// AI-generated recommendations to improve match (JSON)
    /// Example: [{"skill": "Terraform", "roi": "+12%", "effort": "4 weeks", "resources": [...]}]
    /// </summary>
    public string Recommendations { get; set; } = "[]";

    /// <summary>
    /// When the match was last calculated
    /// </summary>
    public DateTime LastCalculatedAt { get; set; }

    /// <summary>
    /// When to recalculate next
    /// </summary>
    public DateTime NextRecalculateAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DreamCompany DreamCompany { get; set; } = null!;
}
