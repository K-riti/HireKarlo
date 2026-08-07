namespace HireKarlo.Application.DTOs.CareerOS;

/// <summary>
/// Aggregated opportunities grouped by company
/// </summary>
public class OpportunitiesByCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public int TotalOpportunities { get; set; }
    public double AverageMatchPercentage { get; set; }
    public List<OpportunityDto> Opportunities { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Daily Opportunity Radar digest for notifications
/// </summary>
public class OpportunityRadarDigestDto
{
    public DateTime DigestDate { get; set; }
    public int NewMatchesFound { get; set; }
    public List<OpportunityDto> TopMatches { get; set; } = new(); // Top 5
    public string Summary { get; set; } = string.Empty;
    public string NotificationMessage { get; set; } = string.Empty;
}

/// <summary>
/// DTO from ReferralIntelligenceService
/// Represents a potential referral contact with detailed matching scores
/// </summary>
public class ReferralOpportunityDetailsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;

    // Overall matching score (0-100)
    public int OverallReferralScore { get; set; }

    // Detailed breakdown of similarity scores
    public int TechStackSimilarity { get; set; } // 0-100
    public int ExperienceSimilarity { get; set; } // 0-100
    public int LocationSimilarity { get; set; } // 0-100
    public int CareerPathSimilarity { get; set; } // 0-100
    public int ReachabilitySimilarity { get; set; } // 0-100

    // Contact information
    public string? LinkedInProfile { get; set; }
    public string? GitHubProfile { get; set; }
    public List<string> SharedConnections { get; set; } = new(); // Shared schools, companies, interests

    // Outreach information
    public string Background { get; set; } = string.Empty;
    public string OutreachStrategy { get; set; } = string.Empty;
    public string DraftMessage { get; set; } = string.Empty;
    public string? Status { get; set; } // NoAction, Contacted, Responded, Referred, Rejected
    public DateTime? FirstContactedAt { get; set; }
    public DateTime? ReferredAt { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

/// <summary>
/// DTO from ReferralIntelligenceService
/// Represents a potential referral contact with detailed matching scores
/// </summary>
public class ReferralOpportunitySummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;

    // Overall matching score (0-100)
    public int OverallReferralScore { get; set; }

    // Detailed breakdown of similarity scores
    public int TechStackSimilarity { get; set; } // 0-100
    public int ExperienceSimilarity { get; set; } // 0-100
    public int LocationSimilarity { get; set; } // 0-100
    public int CareerPathSimilarity { get; set; } // 0-100
    public int ReachabilitySimilarity { get; set; } // 0-100

    // Contact information
    public string? LinkedInProfile { get; set; }
    public string? GitHubProfile { get; set; }
    public List<string> SharedConnections { get; set; } = new(); // Shared schools, companies, interests
}

/// <summary>
/// Grouped referral opportunities by company
/// </summary>
public class ReferralTargetsByCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public int TotalTargets { get; set; }
    public double AverageReferralScore { get; set; }
    public List<ReferralOpportunitySummaryDto> Targets { get; set; } = new();
    public int AlreadyContacted { get; set; }
    public int AlreadyReferred { get; set; }
}

/// <summary>
/// Notification for new referral opportunities
/// </summary>
public class ReferralOpportunityNotificationDto
{
    public string CompanyName { get; set; } = string.Empty;
    public int NewTargetsFound { get; set; }
    public ReferralOpportunityDto? BestMatch { get; set; }
    public string Message { get; set; } = string.Empty;
}
