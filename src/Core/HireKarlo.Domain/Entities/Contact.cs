using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class Contact : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? DreamCompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Relationship { get; set; } // How you know them
    public string? Notes { get; set; }
    public OutreachStatus OutreachStatus { get; set; } = OutreachStatus.Draft;
    public string? DraftedMessage { get; set; } // AI-generated outreach message
    public DateTime? LastContactDate { get; set; }
    public DateTime? FollowUpDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual DreamCompany? DreamCompany { get; set; }
    public virtual ICollection<Application> ReferredApplications { get; set; } = new List<Application>();
}
