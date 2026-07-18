using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class RoadmapItem : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoadmapItemType Type { get; set; }
    public RoadmapItemStatus Status { get; set; } = RoadmapItemStatus.NotStarted;
    public int Order { get; set; } // Display order
    public int WeekNumber { get; set; } // Which week in the 6-month plan
    public string? Category { get; set; } // DSA, System Design, Behavioral, etc.
    public string? ResourceLinks { get; set; } // JSON array of {title, url, type}
    public string? SkillTags { get; set; } // JSON array of related skills
    public int? EstimatedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public Guid? ParentItemId { get; set; } // For sub-tasks
    public bool IsAiGenerated { get; set; } // Was this auto-generated

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual RoadmapItem? ParentItem { get; set; }
    public virtual ICollection<RoadmapItem> SubItems { get; set; } = new List<RoadmapItem>();
}
