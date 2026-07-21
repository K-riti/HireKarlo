using System.Net.Http.Json;

namespace HireKarlo.Web.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // Auth - Email/Password
    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResult>();
            }
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return new AuthResult { Success = false, Error = error?.Error ?? "Login failed" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = $"Connection error: {ex.Message}" };
        }
    }

    public async Task<AuthResult?> RegisterAsync(string email, string firstName, string lastName, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new 
            { 
                Email = email, 
                FirstName = firstName, 
                LastName = lastName, 
                Password = password 
            });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResult>();
            }
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return new AuthResult { Success = false, Error = error?.Error ?? "Registration failed" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = $"Connection error: {ex.Message}" };
        }
    }

    // Auth - Social Login
    public async Task<AuthResult?> LoginWithGoogleAsync(string idToken)
        => await _http.PostAsJsonAsync<AuthResult>("api/auth/google", new { IdToken = idToken });

    public async Task<AuthResult?> LoginWithLinkedInAsync(string code, string redirectUri)
        => await _http.PostAsJsonAsync<AuthResult>("api/auth/linkedin", new { Code = code, RedirectUri = redirectUri });

    // Dashboard
    public async Task<DashboardData?> GetDashboardAsync()
        => await _http.GetFromJsonAsync<DashboardData>("api/dashboard");

    // Resume
    public async Task<ResumeDto?> UploadResumeAsync(MultipartFormDataContent content)
    {
        var response = await _http.PostAsync("api/resumes/upload", content);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ResumeDto>() : null;
    }

    public async Task<List<ResumeDto>> GetResumesAsync()
        => await _http.GetFromJsonAsync<List<ResumeDto>>("api/resumes") ?? new();

    public async Task<AtsScoreResult?> GetAtsScoreAsync(Guid resumeId, string jobDescription)
        => await _http.PostAsJsonAsync<AtsScoreResult>($"api/resumes/{resumeId}/ats-score", new { JobDescription = jobDescription });

    // Jobs
    public async Task<List<JobDto>> GetJobsAsync(string? search = null, bool? remote = null)
    {
        var query = $"api/jobs?search={search ?? ""}&remote={remote}";
        return await _http.GetFromJsonAsync<List<JobDto>>(query) ?? new();
    }

    public async Task<MatchResultDto?> GetJobMatchAsync(Guid jobId)
        => await _http.GetFromJsonAsync<MatchResultDto>($"api/jobs/{jobId}/match");

    // Applications
    public async Task<List<ApplicationDto>> GetApplicationsAsync()
        => await _http.GetFromJsonAsync<List<ApplicationDto>>("api/applications") ?? new();

    public async Task<ApplicationDto?> CreateApplicationAsync(CreateApplicationRequest request)
        => await _http.PostAsJsonAsync<ApplicationDto>("api/applications", request);

    public async Task UpdateApplicationStageAsync(Guid id, string stage)
        => await _http.PutAsJsonAsync($"api/applications/{id}/stage", new { Stage = stage });

    // Mock Interview
    public async Task<MockInterviewSession?> StartMockInterviewAsync(MockInterviewRequest request)
        => await _http.PostAsJsonAsync<MockInterviewSession>("api/mockinterview/start", request);

    public async Task<InterviewQuestion?> GetNextQuestionAsync(Guid sessionId)
        => await _http.GetFromJsonAsync<InterviewQuestion>($"api/mockinterview/{sessionId}/question");

    public async Task<AnswerFeedback?> SubmitAnswerAsync(Guid sessionId, string answer)
        => await _http.PostAsJsonAsync<AnswerFeedback>($"api/mockinterview/{sessionId}/answer", new { Answer = answer });

    public async Task<SessionSummary?> EndSessionAsync(Guid sessionId)
        => await _http.PostAsJsonAsync<SessionSummary>($"api/mockinterview/{sessionId}/end", new { });

    // Learning Path
    public async Task<LearningPathResult?> GenerateCompanyPathAsync(string company, string role)
        => await _http.PostAsJsonAsync<LearningPathResult>("api/learningpath/generate/company", new { Company = company, TargetRole = role });

    public async Task<LearningPathResult?> GenerateSkillPathAsync(List<string> skills, int difficulty)
        => await _http.PostAsJsonAsync<LearningPathResult>("api/learningpath/generate/skills", new { Skills = skills, DifficultyLevel = difficulty });

    public async Task<LearningPathDto?> GetActivePathAsync()
        => await _http.GetFromJsonAsync<LearningPathDto>("api/learningpath/active");

    public async Task<ModuleContent?> GetModuleContentAsync(Guid moduleId)
        => await _http.GetFromJsonAsync<ModuleContent>($"api/learningpath/modules/{moduleId}/content");

    public async Task<QuizDto?> GetQuizAsync(Guid moduleId)
        => await _http.GetFromJsonAsync<QuizDto>($"api/learningpath/modules/{moduleId}/quiz");

    public async Task<QuizResult?> SubmitQuizAsync(Guid moduleId, List<QuizAnswer> answers)
        => await _http.PostAsJsonAsync<QuizResult>($"api/learningpath/modules/{moduleId}/quiz/submit", new { Answers = answers });

    // LinkedIn Optimizer
    public async Task<LinkedInOptimizationResult?> OptimizeProfileAsync(LinkedInProfileRequest request)
        => await _http.PostAsJsonAsync<LinkedInOptimizationResult>("api/linkedinoptimizer/optimize", request);

    // AI Chat
    public async Task<ChatResponse?> SendChatMessageAsync(string message, string? context = null)
        => await _http.PostAsJsonAsync<ChatResponse>("api/chat", new { Message = message, Context = context });

    // Newsletter
    public async Task SubscribeToNewsletterAsync(string email, string name)
        => await _http.PostAsJsonAsync("api/newsletter/subscribe", new { Email = email, Name = name });

    // Additional methods for pages
    public async Task<List<JobDto>> SearchJobsAsync(string? query, string? location, bool remoteOnly)
    {
        var url = $"api/jobs/search?query={query ?? ""}&location={location ?? ""}&remote={remoteOnly}";
        return await _http.GetFromJsonAsync<List<JobDto>>(url) ?? new();
    }

    public async Task<LearningPathDto?> GetActiveLearningPathAsync()
        => await _http.GetFromJsonAsync<LearningPathDto>("api/learningpath/active");

    public async Task<LearningPathDto?> GenerateSkillPathAsync(string skill, string level)
        => await _http.PostAsJsonAsync<LearningPathDto>("api/learningpath/generate/skill", new { Skill = skill, Level = level });
}

public static class HttpClientExtensions
{
    public static async Task<T?> PostAsJsonAsync<T>(this HttpClient http, string url, object data)
    {
        var response = await http.PostAsJsonAsync(url, data);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : default;
    }
}

#region DTOs

public record AuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Error { get; init; }
    public bool IsNewUser { get; init; }
}
public class UserInfo { public Guid Id { get; set; } public string Email { get; set; } = ""; public string Name { get; set; } = ""; public string? ProfilePictureUrl { get; set; } }

public record DashboardData
{
    public int TotalApplications { get; init; }
    public int ActiveJobs { get; init; }
    public int Interviews { get; init; }
    public int LearningProgress { get; init; }
    public List<RecentActivity> RecentActivities { get; init; } = new();
    public List<UpcomingItem> UpcomingItems { get; init; } = new();
    public List<JobMatchDto> TopMatches { get; init; } = new();
}

public record RecentActivity(string Type, string Title, string Description, DateTime Timestamp);
public record UpcomingItem(string Type, string Title, DateTime DueDate);
public record JobMatchDto(Guid JobId, string Title, string Company, int MatchScore);

public record ResumeDto(Guid Id, string FileName, DateTime UploadedAt, bool IsMaster, string? Summary);
public record AtsScoreResult(int Score, List<string> MissingKeywords, List<string> MatchingKeywords, List<string> Suggestions);

public class JobDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string? SalaryRange { get; set; }
    public bool IsRemote { get; set; }
    public DateTime PostedDate { get; set; }
    public int MatchScore { get; set; }
    public List<string> Skills { get; set; } = new();
    public string? Description { get; set; }
    public bool IsSaved { get; set; }
}

public record MatchResultDto(int OverallScore, int SemanticScore, int KeywordScore, List<string> Strengths, List<string> Gaps, List<string> Recommendations);

public record ApplicationDto(Guid Id, string JobTitle, string Company, string Stage, DateTime AppliedDate, int? MatchScore);
public record CreateApplicationRequest(Guid JobId, Guid? ResumeId, string? CoverLetter);

public record MockInterviewRequest(string Type, string? TargetCompany, string? TargetRole, int NumberOfQuestions = 5);
public record MockInterviewSession(Guid SessionId, DateTime StartedAt, int TotalQuestions);
public record InterviewQuestion(int QuestionNumber, string Question, string Category, string? Hint, int TimeLimitSeconds);
public record AnswerFeedback(int OverallScore, List<string> Strengths, List<string> AreasForImprovement, string? SuggestedAnswer);
public record SessionSummary(int TotalQuestions, int AverageScore, List<string> StrongestAreas, List<string> WeakestAreas, string OverallFeedback, List<string> ImprovementPlan);

public record LearningPathResult(bool Success, Guid? PathId, string? Title, int TotalModules, int EstimatedWeeks);
public class LearningPathDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int CompletionPercentage { get; set; }
    public List<LearningModuleDto> Modules { get; set; } = new();
}
public class LearningModuleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public int EstimatedHours { get; set; }
    public bool HasQuiz { get; set; }
    public List<ResourceDto>? Resources { get; set; }
}
public record ModuleContent(Guid ModuleId, string Title, string Description, List<LessonDto> Lessons, List<ResourceDto> Resources);
public record LessonDto(int Order, string Title, string Content, string? CodeExample);
public record ResourceDto(string Title, string Url, string Type);
public class QuizDto
{
    public string ModuleTitle { get; set; } = "";
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
public class QuizQuestionDto
{
    public string Question { get; set; } = "";
    public List<string> Options { get; set; } = new();
}
public record QuizAnswer(int QuestionNumber, string Answer);
public record QuizResult(int TotalQuestions, int CorrectAnswers, int ScorePercentage, bool Passed, List<string> WeakAreas);

public record LinkedInProfileRequest(string? Headline, string? About, List<ExperienceInput> Experiences, List<string> Skills, List<string> TargetRoles, List<string> TargetKeywords);
public record ExperienceInput(string Title, string Company, string? Description);
public record LinkedInOptimizationResult(int CurrentScore, int ProjectedScore, HeadlineResult Headline, AboutResult About, List<string> Recommendations);
public record HeadlineResult(string Original, List<string> Suggestions, int KeywordScore);
public record AboutResult(string Original, string Optimized, int KeywordDensityBefore, int KeywordDensityAfter);

public record ChatResponse(string Message, List<string>? Suggestions, string? ActionType);

public record ErrorResponse(string? Error);

#endregion
