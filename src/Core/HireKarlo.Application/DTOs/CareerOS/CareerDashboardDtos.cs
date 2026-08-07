namespace HireKarlo.Application.DTOs.CareerOS;

/// <summary>
/// The main career dashboard response - "The Wow Moment"
/// Shows match %, gaps, recommendations, and next steps
/// </summary>
public class CareerDashboardResponse
{
    public UserSkillProfile SkillProfile { get; set; } = new();
    public List<DreamCompanyStatusDto> DreamCompanies { get; set; } = new();
    public List<OpportunityDto> TopOpportunities { get; set; } = new();
    public RecommendedActionsDto RecommendedActions { get; set; } = new();
    public CareerProgressSummaryDto ProgressSummary { get; set; } = new();
}

/// <summary>
/// User's overall skill profile
/// </summary>
public class UserSkillProfile
{
    public int TotalSkills { get; set; }
    public double OverallProficiency { get; set; }
    public List<string> TopSkills { get; set; } = new();
    public List<string> SkillsByCategory { get; set; } = new();
    public string CurrentRole { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
}

/// <summary>
/// Status of match for each dream company
/// Shows current %, target %, and what's needed
/// </summary>
public class DreamCompanyStatusDto
{
    public Guid DreamCompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public double CurrentMatch { get; set; }
    public double TargetMatch { get; set; } = 90.0;
    public double ProgressToTarget => CurrentMatch >= TargetMatch ? 100 : (CurrentMatch / TargetMatch) * 100;
    public List<GapDto> GapsToFill { get; set; } = new();
    public int DaysToReachTarget { get; set; }
    public string Status { get; set; } = "On Track"; // "On Track", "Behind", "Ready"
}

/// <summary>
/// Individual gap that needs to be filled
/// </summary>
public class GapDto
{
    public Guid SkillGapId { get; set; }
    public string Skill { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty; // "Needed by Adobe, Atlassian, Microsoft"
    public string ROI { get; set; } = string.Empty; // "+12% to Adobe, +7% to Atlassian"
    public int EstimatedHours { get; set; }
    public int Priority { get; set; } // 1-5
    public string? ProjectIdea { get; set; }
}

/// <summary>
/// Opportunity matching user's profile
/// From Opportunity Radar
/// </summary>
public class OpportunityDto
{
    public Guid JobListingId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public double MatchPercentage { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> MatchingFactors { get; set; } = new();
    public List<string> MissingFactors { get; set; } = new();
    public bool IsDreamCompanyJob { get; set; }
}

/// <summary>
/// Recommended actions for user to take (Skill ROI Engine)
/// "Learn this → +12% to Adobe"
/// </summary>
public class RecommendedActionsDto
{
    public List<SkillToLearnDto> SkillsToLearn { get; set; } = new();
    public List<ReferralOpportunityDto> ReferralTargets { get; set; } = new();
    public List<InterviewPrepDto> UpcomingInterviewPrep { get; set; } = new();
}

/// <summary>
/// Skill recommendation with ROI
/// USP #2: Skill ROI Engine
/// </summary>
public class SkillToLearnDto
{
    public Guid SkillGapId { get; set; }
    public string Skill { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string ImpactSummary { get; set; } = string.Empty; // "Learn → +12% Adobe, +7% Atlassian"
    public List<string> ProjectIdeas { get; set; } = new();
    public List<ResourceDto> Resources { get; set; } = new();
    public int EstimatedHours { get; set; }
    public double ROIScore { get; set; }
}

/// <summary>
/// Learning resource for a skill
/// </summary>
public class ResourceDto
{
    public string Type { get; set; } = string.Empty; // "course", "project", "book", "video"
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public int Hours { get; set; }
    public string Cost { get; set; } = string.Empty;
}

/// <summary>
/// Referral opportunity
/// USP #3: Referral Intelligence
/// </summary>
public class ReferralOpportunityDto
{
    public Guid ReferralId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactTitle { get; set; } = string.Empty;
    public double SimilarityScore { get; set; } // "85% similar background"
    public string SimilarityReason { get; set; } = string.Empty;
    public string OutreachStrategy { get; set; } = string.Empty;
    public string? DraftMessage { get; set; }
    public string Status { get; set; } = string.Empty; // "NoAction", "Contacted", etc.
}

/// <summary>
/// Interview preparation info
/// USP #4: Interview Digest Engine
/// </summary>
public class InterviewPrepDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<InterviewTopicDto> MostAskedTopics { get; set; } = new();
    public List<InterviewTopicDto> BehavioralTopics { get; set; } = new();
    public int TotalQuestionsCollected { get; set; }
    public DateTime? DigestLastGeneratedAt { get; set; }
}

/// <summary>
/// Interview topic frequency
/// </summary>
public class InterviewTopicDto
{
    public string Topic { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string? ExampleQuestion { get; set; }
    public string? SuggestedLearningPath { get; set; }
}

/// <summary>
/// Career progress summary
/// </summary>
public class CareerProgressSummaryDto
{
    public int TotalMilestones { get; set; }
    public List<MilestoneDto> RecentMilestones { get; set; } = new();
    public double AverageImpactPerMilestone { get; set; }
}

/// <summary>
/// Individual milestone
/// </summary>
public class MilestoneDto
{
    public Guid CareerProgressId { get; set; }
    public string MilestoneType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Impact { get; set; }
    public string? Evidence { get; set; }
}
