namespace HireKarlo.Domain.Enums;

public enum ApplicationStage
{
    Saved = 0,
    Applied = 1,
    OnlineAssessment = 2,
    PhoneScreen = 3,
    TechnicalInterview = 4,
    OnsiteInterview = 5,
    Offer = 6,
    Rejected = 7,
    Withdrawn = 8
}

public enum ResumeFileType
{
    Pdf = 0,
    Docx = 1
}

public enum MatchStatus
{
    Pending = 0,
    Reviewed = 1,
    Applied = 2,
    Dismissed = 3
}

public enum RoadmapItemType
{
    Skill = 0,
    Project = 1,
    Course = 2,
    Certification = 3,
    Practice = 4
}

public enum RoadmapItemStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3
}

public enum OutreachStatus
{
    Draft = 0,
    Sent = 1,
    Responded = 2,
    NoResponse = 3
}

public enum JobSource
{
    Adzuna = 0,
    RemoteOK = 1,
    Arbeitnow = 2,
    Greenhouse = 3,
    Lever = 4,
    Manual = 5
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Skill proficiency levels for the Career Operating System
/// </summary>
public enum SkillLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    Expert = 3
}

/// <summary>
/// Categories for interview digest entries (RAG-based content)
/// </summary>
public enum DigestCategory
{
    Technical = 0,
    Behavioral = 1,
    SystemDesign = 2,
    DataStructures = 3,
    Algorithms = 4,
    CodingProblem = 5,
    CompanySpecific = 6,
    Other = 7
}

/// <summary>
/// Referral target status tracking
/// </summary>
public enum ReferralStatus
{
    NoAction = 0,
    Contacted = 1,
    Responded = 2,
    Referred = 3,
    Rejected = 4
}

/// <summary>
/// Career milestone types for progress tracking
/// </summary>
public enum MilestoneType
{
    SkillAcquired = 0,
    ProjectCompleted = 1,
    CertificationEarned = 2,
    InterviewScheduled = 3,
    OfferReceived = 4,
    ApplicationSubmitted = 5,
    ReferralMade = 6,
    Other = 7
}
