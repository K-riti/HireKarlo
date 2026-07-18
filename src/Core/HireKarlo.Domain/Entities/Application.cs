using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class Application : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid JobListingId { get; set; }
    public Guid? ResumeId { get; set; } // Resume used for application
    public Guid? MatchId { get; set; } // Associated match if any
    public ApplicationStage Stage { get; set; } = ApplicationStage.Saved;
    public DateTime? AppliedDate { get; set; }
    public DateTime? OaDate { get; set; } // Online Assessment date
    public DateTime? InterviewDate { get; set; }
    public DateTime? OfferDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? Notes { get; set; }
    public string? StageHistory { get; set; } // JSON array of stage changes with timestamps
    public bool UsedReferral { get; set; }
    public Guid? ReferralContactId { get; set; }
    public string? CoverLetter { get; set; }
    public string? DraftedMessage { get; set; } // Auto-drafted application message
    public int? AtsScore { get; set; } // ATS score at time of application
    public string? AtsReport { get; set; } // JSON ATS analysis report

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual JobListing JobListing { get; set; } = null!;
    public virtual Resume? Resume { get; set; }
    public virtual Match? Match { get; set; }
    public virtual Contact? ReferralContact { get; set; }
}
