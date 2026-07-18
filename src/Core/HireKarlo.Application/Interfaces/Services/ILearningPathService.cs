using HireKarlo.Domain.Entities;

namespace HireKarlo.Application.Interfaces.Services;

public interface ILearningPathService
{
    // Path Generation
    Task<LearningPathResult> GenerateCompanyPathAsync(Guid userId, string company, string targetRole, CancellationToken cancellationToken = default);
    Task<LearningPathResult> GenerateSkillPathAsync(Guid userId, List<string> skills, int difficultyLevel, CancellationToken cancellationToken = default);
    Task<LearningPathResult> GenerateInterviewPatternPathAsync(Guid userId, string company, InterviewPatternOptions options, CancellationToken cancellationToken = default);

    // Path Management
    Task<LearningPath?> GetUserActivePathAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<LearningPath>> GetUserPathsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LearningPath?> GetPathWithModulesAsync(Guid pathId, CancellationToken cancellationToken = default);
    Task StartPathAsync(Guid pathId, CancellationToken cancellationToken = default);
    Task PausePathAsync(Guid pathId, CancellationToken cancellationToken = default);

    // Module Progress
    Task<ModuleContent> GetModuleContentAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task StartModuleAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task CompleteModuleAsync(Guid moduleId, int? score = null, CancellationToken cancellationToken = default);

    // Quizzes
    Task<QuizContent> GenerateQuizAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<QuizResult> SubmitQuizAsync(Guid moduleId, List<QuizAnswer> answers, CancellationToken cancellationToken = default);
    Task<List<QuizAttempt>> GetQuizAttemptsAsync(Guid moduleId, Guid userId, CancellationToken cancellationToken = default);

    // Recommendations
    Task<List<LearningRecommendation>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetWeakAreasAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record LearningPathResult
{
    public bool Success { get; init; }
    public Guid? PathId { get; init; }
    public string? Title { get; init; }
    public int TotalModules { get; init; }
    public int EstimatedWeeks { get; init; }
    public List<ModuleSummary> Modules { get; init; } = new();
    public string? Error { get; init; }
}

public record ModuleSummary
{
    public Guid ModuleId { get; init; }
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public ModuleType Type { get; init; }
    public int EstimatedMinutes { get; init; }
    public LearningModuleStatus Status { get; init; }
}

public record InterviewPatternOptions
{
    public List<string> FocusAreas { get; init; } = new(); // DSA, System Design, Behavioral
    public int WeeksToInterview { get; init; } = 4;
    public bool IncludeMockInterviews { get; init; } = true;
    public bool IncludeCompanySpecificQuestions { get; init; } = true;
}

public record ModuleContent
{
    public Guid ModuleId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ModuleType Type { get; init; }
    public string Category { get; init; } = string.Empty;
    public List<Lesson> Lessons { get; init; } = new();
    public List<Resource> Resources { get; init; } = new();
    public List<PracticeQuestion> PracticeQuestions { get; init; } = new();
    public int EstimatedMinutes { get; init; }
}

public record Lesson
{
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? CodeExample { get; init; }
    public List<string> KeyTakeaways { get; init; } = new();
}

public record Resource
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // video, article, documentation
    public bool IsPremium { get; init; }
}

public record PracticeQuestion
{
    public string Question { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public string Difficulty { get; init; } = "Medium";
    public string? SolutionApproach { get; init; }
    public List<string> Tags { get; init; } = new();
}

public record QuizContent
{
    public Guid ModuleId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int TimeLimitMinutes { get; init; }
    public List<QuizQuestion> Questions { get; init; } = new();
    public int PassingScore { get; init; } = 70;
}

public record QuizQuestion
{
    public int QuestionNumber { get; init; }
    public string Question { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public List<string> Options { get; init; } = new();
    public string? CodeSnippet { get; init; }
    public string? Hint { get; init; }
    public int Points { get; init; } = 10;
}

public enum QuestionType
{
    MultipleChoice,
    MultiSelect,
    TrueFalse,
    CodeOutput,
    FillInBlank,
    ShortAnswer
}

public record QuizAnswer
{
    public int QuestionNumber { get; init; }
    public string Answer { get; init; } = string.Empty;
    public List<string>? MultiAnswers { get; init; }
}

public record QuizResult
{
    public int TotalQuestions { get; init; }
    public int CorrectAnswers { get; init; }
    public int ScorePercentage { get; init; }
    public bool Passed { get; init; }
    public TimeSpan TimeTaken { get; init; }
    public List<QuestionResult> QuestionResults { get; init; } = new();
    public List<string> WeakAreas { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
}

public record QuestionResult
{
    public int QuestionNumber { get; init; }
    public bool IsCorrect { get; init; }
    public string CorrectAnswer { get; init; } = string.Empty;
    public string? Explanation { get; init; }
    public string? Category { get; init; }
}

public record LearningRecommendation
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public LearningPathType Type { get; init; }
    public string? TargetCompany { get; init; }
    public List<string> Skills { get; init; } = new();
    public int EstimatedWeeks { get; init; }
    public int DifficultyLevel { get; init; }
    public string Reason { get; init; } = string.Empty;
}
