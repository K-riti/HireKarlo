using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Social Login Identifiers
    public string? AzureAdB2CId { get; set; }
    public string? GoogleId { get; set; }
    public string? LinkedInId { get; set; }
    public string? GitHubId { get; set; }

    // OAuth tokens for API access (encrypted)
    public string? LinkedInAccessToken { get; set; }
    public DateTime? LinkedInTokenExpiry { get; set; }

    // Contact & Profile
    public string? LinkedInProfileUrl { get; set; }
    public string? GitHubProfileUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? Headline { get; set; } // Professional headline
    public string? About { get; set; } // About/Summary

    // Job Preferences
    public string? TargetRole { get; set; }
    public string? TargetLocations { get; set; } // JSON array of locations
    public int? TargetSalaryMin { get; set; }
    public int? TargetSalaryMax { get; set; }
    public bool RequiresVisa { get; set; }
    public bool IsOpenToRemote { get; set; } = true;
    public bool IsOpenToRelocation { get; set; }
    public string? Preferences { get; set; } // JSON for additional preferences

    // Newsletter & Notifications
    public bool SubscribedToNewsletter { get; set; } = true;
    public bool SubscribedToMatchAlerts { get; set; } = true;
    public bool SubscribedToWeeklyDigest { get; set; } = true;
    public string? NotificationPreferences { get; set; } // JSON
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginProvider { get; set; } // Google, LinkedIn, Email

    // Navigation properties
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<DreamCompany> DreamCompanies { get; set; } = new List<DreamCompany>();
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<RoadmapItem> RoadmapItems { get; set; } = new List<RoadmapItem>();
}
