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
