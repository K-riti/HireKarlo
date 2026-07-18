using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class Match : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid JobListingId { get; set; }
    public Guid? ResumeId { get; set; } // Resume used for matching
    public double OverallScore { get; set; } // 0-100
    public double SemanticScore { get; set; } // Vector similarity score
    public double KeywordScore { get; set; } // Keyword overlap score
    public double TitleScore { get; set; } // Title match score
    public MatchStatus Status { get; set; } = MatchStatus.Pending;
    public string? GapReport { get; set; } // JSON structured gap analysis
    public string? MissingKeywords { get; set; } // JSON array
    public string? MatchingKeywords { get; set; } // JSON array
    public string? Strengths { get; set; } // JSON array
    public string? Weaknesses { get; set; } // JSON array
    public string? Recommendations { get; set; } // JSON array of improvement suggestions
    public DateTime MatchedAt { get; set; }
    public bool NotificationSent { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual JobListing JobListing { get; set; } = null!;
    public virtual Resume? Resume { get; set; }
}
