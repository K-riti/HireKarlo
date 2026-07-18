using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class DreamCompany : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? CareersPageUrl { get; set; }
    public string? GreenhouseBoardToken { get; set; } // For Greenhouse API
    public string? LeverCompanyId { get; set; } // For Lever API
    public bool SponsorsVisa { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string? Notes { get; set; }
    public string? TargetRoles { get; set; } // JSON array of target roles at this company
    public bool IsTrackingJobs { get; set; } = true; // Auto-fetch jobs from this company
    public DateTime? LastJobFetch { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
