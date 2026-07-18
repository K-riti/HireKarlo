namespace HireKarlo.Application.Interfaces.Services;

public interface IMockInterviewService
{
    Task<MockInterviewSession> StartSessionAsync(Guid userId, MockInterviewOptions options, CancellationToken cancellationToken = default);
    Task<InterviewQuestion> GetNextQuestionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<AnswerFeedback> SubmitAnswerAsync(Guid sessionId, string answer, CancellationToken cancellationToken = default);
    Task<SessionSummary> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

public record MockInterviewOptions
{
    public string? TargetCompany { get; init; }
    public string? TargetRole { get; init; }
    public InterviewType Type { get; init; } = InterviewType.Behavioral;
    public string? Difficulty { get; init; } = "Medium";
    public int NumberOfQuestions { get; init; } = 5;
    public bool UseVoice { get; init; } = false;
}

public enum InterviewType
{
    Behavioral,
    Technical,
    SystemDesign,
    CodingLive,
    Mixed
}

public record MockInterviewSession
{
    public Guid SessionId { get; init; }
    public DateTime StartedAt { get; init; }
    public MockInterviewOptions Options { get; init; } = null!;
    public List<InterviewQuestion> Questions { get; init; } = new();
}

public record InterviewQuestion
{
    public int QuestionNumber { get; init; }
    public string Question { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public List<string> ExpectedTopics { get; init; } = new();
    public int TimeLimitSeconds { get; init; }
}

public record AnswerFeedback
{
    public int OverallScore { get; init; } // 0-100
    public StarMethodScore? StarScore { get; init; }
    public TechnicalDepthScore? TechnicalScore { get; init; }
    public List<string> Strengths { get; init; } = new();
    public List<string> AreasForImprovement { get; init; } = new();
    public string? SuggestedAnswer { get; init; }
    public List<string> MissedTopics { get; init; } = new();
}

public record StarMethodScore
{
    public int SituationScore { get; init; }
    public int TaskScore { get; init; }
    public int ActionScore { get; init; }
    public int ResultScore { get; init; }
    public string Feedback { get; init; } = string.Empty;
}

public record TechnicalDepthScore
{
    public int ConceptualUnderstanding { get; init; }
    public int PracticalApplication { get; init; }
    public int ProblemSolving { get; init; }
    public int Communication { get; init; }
}

public record SessionSummary
{
    public Guid SessionId { get; init; }
    public int TotalQuestions { get; init; }
    public int AverageScore { get; init; }
    public TimeSpan Duration { get; init; }
    public List<string> StrongestAreas { get; init; } = new();
    public List<string> WeakestAreas { get; init; } = new();
    public string OverallFeedback { get; init; } = string.Empty;
    public List<string> ImprovementPlan { get; init; } = new();
    // Legacy properties for backward compatibility
    public List<string> OverallStrengths => StrongestAreas;
    public List<string> OverallWeaknesses => WeakestAreas;
    public List<string> StudyRecommendations => ImprovementPlan;
    public string DetailedFeedback => OverallFeedback;
}
