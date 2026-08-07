using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// USP #2: Skill ROI Engine - AI-generated recommendations for skills to learn
/// Instead of "Learn Kubernetes", says "Learn Terraform → +12% to Adobe, +7% to Atlassian"
/// </summary>
public class SkillGapRecommendation : BaseEntity
{
    /// <summary>
    /// User receiving this recommendation
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Reference to the skill from SkillGraph (if already learning something similar)
    /// </summary>
    public Guid? SkillGraphId { get; set; }

    /// <summary>
    /// Dream company this skill would help reach
    /// Generate separate recommendations per dream company
    /// </summary>
    public Guid? DreamCompanyId { get; set; }

    /// <summary>
    /// Recommended skill to learn
    /// Examples: "Terraform", "Kubernetes", "Distributed Systems Design"
    /// </summary>
    public string RecommendedSkill { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (1-5, higher = more important)
    /// 5 = Critical for immediate access
    /// 1 = Nice to have
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Why this skill is recommended (human-readable)
    /// Example: "Critical for 4/5 dream companies. Appears in 80% of recent job postings"
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Learning resources (JSON array)
    /// Example: [
    ///   {"type": "course", "name": "Docker Mastery", "link": "...", "hours": 30, "cost": "free"},
    ///   {"type": "project", "name": "Deploy microservices", "link": "...", "hours": 20}
    /// ]
    /// </summary>
    public string? LearningResources { get; set; }

    /// <summary>
    /// Project idea to build and demonstrate mastery
    /// Example: "Build a multi-container microservices app with CI/CD pipeline"
    /// </summary>
    public string? ProjectIdea { get; set; }

    /// <summary>
    /// Impact summary showing ROI across companies
    /// Example: "Learn this → +12% Adobe, +7% Atlassian, +4% Microsoft, +15% Databricks"
    /// </summary>
    public string ImpactSummary { get; set; } = string.Empty;

    /// <summary>
    /// Estimated hours to learn this skill
    /// </summary>
    public int EstimatedHours { get; set; }

    /// <summary>
    /// Target completion date suggested by system
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }

    /// <summary>
    /// ROI score (0-100) - how much this improves overall prospects
    /// Calculated based on frequency across dream companies and user's current gap
    /// </summary>
    public double ROIScore { get; set; }

    /// <summary>
    /// When item was created/recommended
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Additional notes or context
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual SkillGraph? SkillGraph { get; set; }
    public virtual DreamCompany? DreamCompany { get; set; }
}
