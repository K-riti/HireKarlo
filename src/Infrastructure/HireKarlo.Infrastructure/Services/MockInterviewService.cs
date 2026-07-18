using System.Collections.Concurrent;
using System.Text.Json;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.Services;

public class MockInterviewService : IMockInterviewService
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<MockInterviewService> _logger;

    private static readonly ConcurrentDictionary<Guid, InterviewSessionState> _sessions = new();

    public MockInterviewService(
        IOpenAIService openAIService,
        ILogger<MockInterviewService> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<MockInterviewSession> StartSessionAsync(
        Guid userId,
        MockInterviewOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting mock interview session for user {UserId}, Type: {Type}, Company: {Company}", 
            userId, options.Type, options.TargetCompany);

        var sessionId = Guid.NewGuid();

        var questions = await GenerateQuestionsAsync(options, cancellationToken);

        var session = new MockInterviewSession
        {
            SessionId = sessionId,
            StartedAt = DateTime.UtcNow,
            Options = options,
            Questions = questions
        };

        _sessions[sessionId] = new InterviewSessionState
        {
            UserId = userId,
            Session = session,
            CurrentQuestionIndex = 0,
            Answers = new List<AnswerRecord>(),
            StartedAt = DateTime.UtcNow
        };

        return session;
    }

    public Task<InterviewQuestion> GetNextQuestionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var state))
        {
            throw new InvalidOperationException("Session " + sessionId + " not found");
        }

        if (state.CurrentQuestionIndex >= state.Session.Questions.Count)
        {
            throw new InvalidOperationException("No more questions in this session");
        }

        var question = state.Session.Questions[state.CurrentQuestionIndex];
        state.CurrentQuestionStartTime = DateTime.UtcNow;

        return Task.FromResult(question);
    }

    public async Task<AnswerFeedback> SubmitAnswerAsync(
        Guid sessionId,
        string answer,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var state))
        {
            throw new InvalidOperationException("Session " + sessionId + " not found");
        }

        if (state.CurrentQuestionIndex >= state.Session.Questions.Count)
        {
            throw new InvalidOperationException("No active question to answer");
        }

        var currentQuestion = state.Session.Questions[state.CurrentQuestionIndex];
        var timeTaken = state.CurrentQuestionStartTime.HasValue 
            ? (DateTime.UtcNow - state.CurrentQuestionStartTime.Value).TotalSeconds 
            : 0;

        _logger.LogInformation("Evaluating answer for question {QuestionNum} in session {SessionId}", 
            state.CurrentQuestionIndex + 1, sessionId);

        var feedback = await EvaluateAnswerAsync(
            currentQuestion, 
            answer, 
            state.Session.Options,
            cancellationToken);

        state.Answers.Add(new AnswerRecord
        {
            QuestionIndex = state.CurrentQuestionIndex,
            Answer = answer,
            Feedback = feedback,
            TimeTakenSeconds = timeTaken
        });

        state.CurrentQuestionIndex++;
        state.CurrentQuestionStartTime = null;

        return feedback;
    }

    public async Task<SessionSummary> EndSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var state))
        {
            throw new InvalidOperationException("Session " + sessionId + " not found");
        }

        _logger.LogInformation("Ending mock interview session {SessionId}", sessionId);

        var totalQuestions = state.Answers.Count;
        var averageScore = totalQuestions > 0 
            ? (int)state.Answers.Average(a => a.Feedback.OverallScore) 
            : 0;

        var analysis = await GenerateSessionAnalysisAsync(state, cancellationToken);
        var improvementPlan = await GenerateImprovementPlanAsync(state, cancellationToken);

        _sessions.TryRemove(sessionId, out _);

        return new SessionSummary
        {
            SessionId = sessionId,
            TotalQuestions = totalQuestions,
            AverageScore = averageScore,
            StrongestAreas = analysis.StrongestAreas,
            WeakestAreas = analysis.WeakestAreas,
            OverallFeedback = analysis.OverallFeedback,
            ImprovementPlan = improvementPlan
        };
    }

    private async Task<List<InterviewQuestion>> GenerateQuestionsAsync(
        MockInterviewOptions options,
        CancellationToken cancellationToken)
    {
        var prompt = "Generate " + options.NumberOfQuestions + " interview questions for a " + options.Type + " interview.\n\n" +
            "Context:\n" +
            "- Target Company: " + (options.TargetCompany ?? "General") + "\n" +
            "- Target Role: " + (options.TargetRole ?? "Software Developer") + "\n" +
            "- Difficulty: " + options.Difficulty + "\n\n" +
            "Return a JSON array of questions:\n" +
            "[\n" +
            "    {\n" +
            "        \"questionNumber\": 1,\n" +
            "        \"question\": \"The interview question\",\n" +
            "        \"category\": \"Category name\",\n" +
            "        \"hint\": \"Optional hint\",\n" +
            "        \"expectedTopics\": [\"topic1\", \"topic2\"],\n" +
            "        \"timeLimitSeconds\": 180\n" +
            "    }\n" +
            "]\n\n" +
            "Guidelines: Behavioral uses STAR method, Technical covers coding concepts, SystemDesign covers architecture.";

        try
        {
            var questions = await _openAIService.CompleteAsJsonAsync<List<InterviewQuestionDto>>(prompt, null, cancellationToken);

            return questions?.Select(q => new InterviewQuestion
            {
                QuestionNumber = q.QuestionNumber,
                Question = q.Question ?? "Tell me about yourself.",
                Category = q.Category ?? options.Type.ToString(),
                Hint = q.Hint,
                ExpectedTopics = q.ExpectedTopics ?? new List<string>(),
                TimeLimitSeconds = q.TimeLimitSeconds > 0 ? q.TimeLimitSeconds : 180
            }).ToList() ?? GenerateDefaultQuestions(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating interview questions");
            return GenerateDefaultQuestions(options);
        }
    }

    private async Task<AnswerFeedback> EvaluateAnswerAsync(
        InterviewQuestion question,
        string answer,
        MockInterviewOptions options,
        CancellationToken cancellationToken)
    {
        var prompt = "You are an experienced interviewer at " + (options.TargetCompany ?? "a top tech company") + ".\n" +
            "Evaluate this interview answer:\n\n" +
            "QUESTION (" + question.Category + "): " + question.Question + "\n" +
            "EXPECTED TOPICS: " + string.Join(", ", question.ExpectedTopics) + "\n\n" +
            "CANDIDATE'S ANSWER:\n\"" + answer + "\"\n\n" +
            "Return a JSON evaluation:\n" +
            "{\n" +
            "    \"overallScore\": 75,\n" +
            "    \"starScore\": {\n" +
            "        \"situationScore\": 20,\n" +
            "        \"taskScore\": 20,\n" +
            "        \"actionScore\": 20,\n" +
            "        \"resultScore\": 15,\n" +
            "        \"feedback\": \"STAR method feedback\"\n" +
            "    },\n" +
            "    \"technicalScore\": {\n" +
            "        \"conceptualUnderstanding\": 20,\n" +
            "        \"practicalApplication\": 18,\n" +
            "        \"problemSolving\": 19,\n" +
            "        \"communication\": 18\n" +
            "    },\n" +
            "    \"strengths\": [\"strength1\"],\n" +
            "    \"areasForImprovement\": [\"area1\"],\n" +
            "    \"suggestedAnswer\": \"A model answer\",\n" +
            "    \"missedTopics\": [\"topic\"]\n" +
            "}\n\n" +
            "Scoring: 90-100 exceptional, 75-89 strong, 60-74 adequate, 40-59 below expectations, 0-39 poor.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<AnswerFeedbackDto>(prompt, null, cancellationToken);

            return new AnswerFeedback
            {
                OverallScore = result?.OverallScore ?? 50,
                StarScore = result?.StarScore != null ? new StarMethodScore
                {
                    SituationScore = result.StarScore.SituationScore,
                    TaskScore = result.StarScore.TaskScore,
                    ActionScore = result.StarScore.ActionScore,
                    ResultScore = result.StarScore.ResultScore,
                    Feedback = result.StarScore.Feedback ?? ""
                } : null,
                TechnicalScore = result?.TechnicalScore != null ? new TechnicalDepthScore
                {
                    ConceptualUnderstanding = result.TechnicalScore.ConceptualUnderstanding,
                    PracticalApplication = result.TechnicalScore.PracticalApplication,
                    ProblemSolving = result.TechnicalScore.ProblemSolving,
                    Communication = result.TechnicalScore.Communication
                } : null,
                Strengths = result?.Strengths ?? new List<string>(),
                AreasForImprovement = result?.AreasForImprovement ?? new List<string>(),
                SuggestedAnswer = result?.SuggestedAnswer,
                MissedTopics = result?.MissedTopics ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating answer");
            return new AnswerFeedback
            {
                OverallScore = 50,
                Strengths = new List<string> { "Answer provided" },
                AreasForImprovement = new List<string> { "Unable to evaluate - please try again" }
            };
        }
    }

    private async Task<SessionAnalysis> GenerateSessionAnalysisAsync(
        InterviewSessionState state,
        CancellationToken cancellationToken)
    {
        var answersData = state.Answers.Select(a => new
        {
            Question = state.Session.Questions[a.QuestionIndex].Question,
            Category = state.Session.Questions[a.QuestionIndex].Category,
            Score = a.Feedback.OverallScore,
            Strengths = a.Feedback.Strengths,
            Weaknesses = a.Feedback.AreasForImprovement
        });
        var answersJson = JsonSerializer.Serialize(answersData);

        var prompt = "Analyze this mock interview session and provide summary feedback:\n\n" +
            "Interview Type: " + state.Session.Options.Type + "\n" +
            "Target Role: " + state.Session.Options.TargetRole + "\n" +
            "Target Company: " + state.Session.Options.TargetCompany + "\n\n" +
            "ANSWERS AND SCORES:\n" + answersJson + "\n\n" +
            "Return a JSON object:\n" +
            "{\n" +
            "    \"strongestAreas\": [\"area1\", \"area2\"],\n" +
            "    \"weakestAreas\": [\"area1\", \"area2\"],\n" +
            "    \"overallFeedback\": \"2-3 sentence summary of performance and readiness\"\n" +
            "}";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<SessionAnalysis>(prompt, null, cancellationToken);
            return result ?? new SessionAnalysis
            {
                StrongestAreas = new List<string> { "Communication" },
                WeakestAreas = new List<string> { "Technical depth" },
                OverallFeedback = "Session completed. Review individual feedback for detailed insights."
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating session analysis");
            return new SessionAnalysis
            {
                StrongestAreas = new List<string>(),
                WeakestAreas = new List<string>(),
                OverallFeedback = "Session completed successfully."
            };
        }
    }

    private async Task<List<string>> GenerateImprovementPlanAsync(
        InterviewSessionState state,
        CancellationToken cancellationToken)
    {
        var weakAreas = state.Answers
            .Where(a => a.Feedback.OverallScore < 70)
            .SelectMany(a => a.Feedback.AreasForImprovement)
            .Distinct()
            .Take(5)
            .ToList();

        if (!weakAreas.Any())
        {
            return GetDefaultImprovementPlan();
        }

        var prompt = "Create a specific improvement plan for these weak areas identified in a mock interview:\n\n" +
            "Weak Areas: " + string.Join(", ", weakAreas) + "\n" +
            "Interview Type: " + state.Session.Options.Type + "\n" +
            "Target Role: " + state.Session.Options.TargetRole + "\n\n" +
            "Return a JSON array of 5-7 specific, actionable improvement steps:\n" +
            "[\"step 1\", \"step 2\"]\n\n" +
            "Each step should be concrete and reference specific resources or practice methods.";

        try
        {
            var plan = await _openAIService.CompleteAsJsonAsync<List<string>>(prompt, null, cancellationToken);
            return plan ?? GetDefaultImprovementPlan();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating improvement plan");
            return GetDefaultImprovementPlan();
        }
    }

    private List<InterviewQuestion> GenerateDefaultQuestions(MockInterviewOptions options)
    {
        var questions = new List<InterviewQuestion>();
        var questionNum = 1;

        if (options.Type == InterviewType.Behavioral || options.Type == InterviewType.Mixed)
        {
            questions.Add(new InterviewQuestion
            {
                QuestionNumber = questionNum++,
                Question = "Tell me about a time when you had to deal with a difficult teammate. How did you handle it?",
                Category = "Behavioral - Conflict Resolution",
                ExpectedTopics = new List<string> { "Communication", "Empathy", "Problem-solving", "Resolution" },
                TimeLimitSeconds = 180
            });
        }

        if (options.Type == InterviewType.Technical || options.Type == InterviewType.Mixed)
        {
            questions.Add(new InterviewQuestion
            {
                QuestionNumber = questionNum++,
                Question = "Explain the difference between REST and GraphQL. When would you choose one over the other?",
                Category = "Technical - API Design",
                ExpectedTopics = new List<string> { "REST principles", "GraphQL schema", "Use cases", "Trade-offs" },
                TimeLimitSeconds = 120
            });
        }

        if (options.Type == InterviewType.SystemDesign || options.Type == InterviewType.Mixed)
        {
            questions.Add(new InterviewQuestion
            {
                QuestionNumber = questionNum++,
                Question = "Design a URL shortening service like bit.ly. Walk me through your approach.",
                Category = "System Design",
                ExpectedTopics = new List<string> { "Hash function", "Database", "Caching", "Scalability", "Analytics" },
                TimeLimitSeconds = 300
            });
        }

        while (questions.Count < options.NumberOfQuestions)
        {
            questions.Add(new InterviewQuestion
            {
                QuestionNumber = questionNum++,
                Question = "What's a project you're most proud of? Walk me through the technical decisions you made.",
                Category = "Behavioral - Technical Leadership",
                ExpectedTopics = new List<string> { "Architecture", "Decision-making", "Impact", "Learnings" },
                TimeLimitSeconds = 180
            });
        }

        return questions.Take(options.NumberOfQuestions).ToList();
    }

    private List<string> GetDefaultImprovementPlan()
    {
        return new List<string>
        {
            "Practice STAR method responses daily - record yourself and review",
            "Complete 2-3 LeetCode medium problems per day focusing on patterns",
            "Study system design through 'Designing Data-Intensive Applications' book",
            "Watch mock interview videos on YouTube to learn from others",
            "Schedule practice sessions with peers or use Pramp.com",
            "Review your past projects and prepare detailed technical walkthroughs",
            "Practice explaining complex concepts simply - teach someone else"
        };
    }
}

internal class InterviewSessionState
{
    public Guid UserId { get; set; }
    public MockInterviewSession Session { get; set; } = null!;
    public int CurrentQuestionIndex { get; set; }
    public DateTime? CurrentQuestionStartTime { get; set; }
    public List<AnswerRecord> Answers { get; set; } = new();
    public DateTime StartedAt { get; set; }
}

internal class AnswerRecord
{
    public int QuestionIndex { get; set; }
    public string Answer { get; set; } = string.Empty;
    public AnswerFeedback Feedback { get; set; } = null!;
    public double TimeTakenSeconds { get; set; }
}

internal class SessionAnalysis
{
    public List<string> StrongestAreas { get; set; } = new();
    public List<string> WeakestAreas { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
}

internal class InterviewQuestionDto
{
    public int QuestionNumber { get; set; }
    public string? Question { get; set; }
    public string? Category { get; set; }
    public string? Hint { get; set; }
    public List<string>? ExpectedTopics { get; set; }
    public int TimeLimitSeconds { get; set; }
}

internal class AnswerFeedbackDto
{
    public int OverallScore { get; set; }
    public StarScoreDto? StarScore { get; set; }
    public TechnicalScoreDto? TechnicalScore { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? AreasForImprovement { get; set; }
    public string? SuggestedAnswer { get; set; }
    public List<string>? MissedTopics { get; set; }
}

internal class StarScoreDto
{
    public int SituationScore { get; set; }
    public int TaskScore { get; set; }
    public int ActionScore { get; set; }
    public int ResultScore { get; set; }
    public string? Feedback { get; set; }
}

internal class TechnicalScoreDto
{
    public int ConceptualUnderstanding { get; set; }
    public int PracticalApplication { get; set; }
    public int ProblemSolving { get; set; }
    public int Communication { get; set; }
}
