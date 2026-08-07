using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// Represents a skill in the user's skill graph for Career Operating System
/// Tracks proficiency level, acquisition date, and impact on dream companies
/// </summary>
public class SkillGraph : BaseEntity
{
    /// <summary>
    /// User who owns this skill
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Name of the skill (e.g., "Docker", "Kubernetes", "Terraform")
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Proficiency level of the user in this skill
    /// </summary>
    public SkillLevel Level { get; set; }

    /// <summary>
    /// Numeric score of proficiency (0-100)
    /// </summary>
    public int Proficiency { get; set; }

    /// <summary>
    /// Category of skill (e.g., "DevOps", "Backend", "Frontend", "Cloud", "Data")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Date when skill was acquired or learnt
    /// </summary>
    public DateTime AcquiredDate { get; set; }

    /// <summary>
    /// Evidence of skill (project URL, certificate link, GitHub repo)
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Vector embedding for semantic search and similarity matching
    /// Stored as JSON serialized array or string for database compatibility
    /// </summary>
    public string? EmbeddingVector { get; set; }

    /// <summary>
    /// JSON object mapping dream companies to ROI (impact %)
    /// Example: {"Adobe": 12, "Atlassian": 7, "Microsoft": 4}
    /// </summary>
    public string? ImpactMetrics { get; set; }

    /// <summary>
    /// When this skill was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<SkillGapRecommendation> Recommendations { get; set; } = new List<SkillGapRecommendation>();
}
