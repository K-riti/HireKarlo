using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

public class LearningPath : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LearningPathType Type { get; set; }
    public string? TargetCompany { get; set; }
    public string? TargetRole { get; set; }
    public string? SkillsJson { get; set; } // JSON array of target skills
    public int TotalModules { get; set; }
    public int CompletedModules { get; set; }
    public int EstimatedWeeks { get; set; }
    public int DifficultyLevel { get; set; } // 1-5
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EmbeddingId { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<LearningModule> Modules { get; set; } = new List<LearningModule>();
}

public class LearningModule : BaseEntity
{
    public Guid LearningPathId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ModuleType Type { get; set; }
    public string? Category { get; set; } // DSA, System Design, Behavioral, Language-specific
    public string? ContentJson { get; set; } // JSON with lessons, resources
    public string? QuizQuestionsJson { get; set; } // JSON array of quiz questions
    public int EstimatedMinutes { get; set; }
    public LearningModuleStatus Status { get; set; } = LearningModuleStatus.Locked;
    public int? Score { get; set; } // Quiz score if applicable
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual LearningPath LearningPath { get; set; } = null!;
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public class QuizAttempt : BaseEntity
{
    public Guid LearningModuleId { get; set; }
    public Guid UserId { get; set; }
    public int AttemptNumber { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int ScorePercentage { get; set; }
    public string? AnswersJson { get; set; } // JSON array of {questionId, answer, isCorrect, explanation}
    public TimeSpan TimeTaken { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual LearningModule LearningModule { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum LearningPathType
{
    CompanySpecific,    // e.g., "Google Interview Prep"
    SkillBased,         // e.g., "Master System Design"
    RoleBased,          // e.g., "Senior Backend Engineer"
    InterviewPattern,   // Based on interview question patterns
    Custom
}

public enum ModuleType
{
    Lesson,
    Quiz,
    Practice,
    Project,
    MockInterview,
    SystemDesignExercise
}

public enum LearningModuleStatus
{
    Locked,
    Available,
    InProgress,
    Completed,
    Failed
}
