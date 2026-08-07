using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } // For email/password login
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

    // Job Application Automation
    public bool AutomationEnabled { get; set; } = false;
    public int DailyApplicationTarget { get; set; } = 5; // Target number of applications per day
    public double MinimumMatchScoreForAutomation { get; set; } = 70.0; // Only apply if >= this score
    public bool AutoTailorResume { get; set; } = true; // Auto-tailor resume for each job
    public Guid? PreferredResumeIdForAutomation { get; set; } // Which resume to use for automation
    public DateTime? LastAutomationRunAt { get; set; } // Last time automation ran
    public int AutomationApplicationsThisMonth { get; set; } = 0; // Track monthly count
    public string? AutomationHistory { get; set; } // JSON log of automation runs

    // Career Operating System - Onboarding
    public bool HasCompletedOnboarding { get; set; } = false;
    public DateTime? OnboardingCompletedAt { get; set; }
    public string? CareerGoalSummary { get; set; } // User's stated career goal/vision

    // Navigation properties
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<DreamCompany> DreamCompanies { get; set; } = new List<DreamCompany>();
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<RoadmapItem> RoadmapItems { get; set; } = new List<RoadmapItem>();

    // Career Operating System - New relationships
    public virtual ICollection<SkillGraph> Skills { get; set; } = new List<SkillGraph>();
    public virtual ICollection<DreamCompanyMatch> DreamCompanyMatches { get; set; } = new List<DreamCompanyMatch>();
    public virtual ICollection<OpportunityMatch> OpportunityMatches { get; set; } = new List<OpportunityMatch>();
    public virtual ICollection<ReferralTarget> ReferralTargets { get; set; } = new List<ReferralTarget>();
    public virtual ICollection<InterviewDigestEntry> InterviewDigests { get; set; } = new List<InterviewDigestEntry>();
    public virtual ICollection<SkillGapRecommendation> SkillGapRecommendations { get; set; } = new List<SkillGapRecommendation>();
    public virtual ICollection<CareerProgress> CareerProgress { get; set; } = new List<CareerProgress>();
}

