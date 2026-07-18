using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.AI;

public class RAGOrchestrator
{
    private readonly IOpenAIService _openAIService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly ILogger<RAGOrchestrator> _logger;

    public RAGOrchestrator(
        IOpenAIService openAIService,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        ILogger<RAGOrchestrator> logger)
    {
        _openAIService = openAIService;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _logger = logger;
    }

    public async Task<MatchReport> GenerateMatchReportAsync(
        string resumeText,
        string jobDescription,
        CancellationToken cancellationToken = default)
    {
        // Calculate semantic similarity
        var semanticScore = await _embeddingService.CalculateCosineSimilarityAsync(
            resumeText, jobDescription, cancellationToken);

        // Use LLM for detailed gap analysis
        var analysisPrompt = $@"Analyze the match between this resume and job description.

RESUME:
{resumeText}

JOB DESCRIPTION:
{jobDescription}

Provide a detailed analysis in the following JSON format:
{{
    ""keywordScore"": <0-100>,
    ""titleScore"": <0-100>,
    ""matchingKeywords"": [""keyword1"", ""keyword2""],
    ""missingKeywords"": [""keyword1"", ""keyword2""],
    ""strengths"": [""strength1"", ""strength2""],
    ""weaknesses"": [""weakness1"", ""weakness2""],
    ""recommendations"": [""recommendation1"", ""recommendation2""],
    ""skillGaps"": [
        {{""skill"": ""skillName"", ""severity"": ""Minor|Moderate|Major|Blocker"", ""suggestion"": ""how to address""}}
    ]
}}";

        var analysis = await _openAIService.CompleteAsJsonAsync<MatchAnalysisResponse>(
            analysisPrompt, 
            new CompletionOptions { Temperature = 0.3 },
            cancellationToken);

        var overallScore = CalculateOverallScore(
            semanticScore * 100, 
            analysis?.KeywordScore ?? 50, 
            analysis?.TitleScore ?? 50);

        return new MatchReport
        {
            OverallScore = overallScore,
            SemanticScore = semanticScore * 100,
            KeywordScore = analysis?.KeywordScore ?? 50,
            TitleScore = analysis?.TitleScore ?? 50,
            GapAnalysis = new GapAnalysis
            {
                MatchingKeywords = analysis?.MatchingKeywords ?? new(),
                MissingKeywords = analysis?.MissingKeywords ?? new(),
                SkillGaps = analysis?.SkillGaps?.Select(s => new SkillGap
                {
                    RequiredSkill = s.Skill,
                    Severity = Enum.TryParse<GapSeverity>(s.Severity, true, out var sev) ? sev : GapSeverity.Moderate,
                    SuggestedResources = new List<string> { s.Suggestion }
                }).ToList() ?? new()
            },
            Strengths = analysis?.Strengths ?? new(),
            Weaknesses = analysis?.Weaknesses ?? new(),
            Recommendations = analysis?.Recommendations ?? new()
        };
    }

    public async Task<string> GenerateInterviewDigestAsync(
        string company,
        List<InterviewExperienceInput> experiences,
        CancellationToken cancellationToken = default)
    {
        var experienceText = string.Join("\n\n---\n\n", 
            experiences.Select(e => $"Source: {e.SourcePlatform}\nTitle: {e.Title}\nContent: {e.Snippet}"));

        var prompt = $@"Summarize these interview experiences for {company} into a helpful digest for job seekers.

INTERVIEW EXPERIENCES:
{experienceText}

Create a well-structured digest that includes:
1. Overview of the interview process at {company}
2. Common interview stages and formats
3. Frequently asked questions or topics
4. Tips and recommendations from candidates
5. Key takeaways

Keep it concise but informative. Use bullet points where appropriate.";

        return await _openAIService.CompleteAsync(prompt, 
            new CompletionOptions { Temperature = 0.5, MaxTokens = 1500 },
            cancellationToken);
    }

    public async Task<List<ProjectRecommendation>> GenerateProjectRecommendationsAsync(
        string resumeText,
        List<string> targetJobDescriptions,
        CancellationToken cancellationToken = default)
    {
        // Find skill gaps across target jobs
        var combinedJds = string.Join("\n\n", targetJobDescriptions.Take(3));

        var prompt = $@"Based on this resume and target job requirements, recommend portfolio projects that would help close skill gaps and improve job match rates.

RESUME:
{resumeText}

TARGET JOB REQUIREMENTS:
{combinedJds}

Recommend 3-5 projects in this JSON format:
{{
    ""projects"": [
        {{
            ""title"": ""Project Title"",
            ""description"": ""What to build and why it helps"",
            ""skillsAddressed"": [""skill1"", ""skill2""],
            ""difficulty"": ""Beginner|Intermediate|Advanced"",
            ""estimatedHours"": 20,
            ""impactScore"": 85
        }}
    ]
}}";

        var response = await _openAIService.CompleteAsJsonAsync<ProjectRecommendationsResponse>(
            prompt,
            new CompletionOptions { Temperature = 0.6 },
            cancellationToken);

        return response?.Projects?.Select(p => new ProjectRecommendation
        {
            Title = p.Title,
            Description = p.Description,
            SkillsAddressed = p.SkillsAddressed,
            Difficulty = p.Difficulty,
            EstimatedHours = p.EstimatedHours,
            ImpactScore = p.ImpactScore
        }).ToList() ?? new();
    }

    public async Task<MockInterviewQuestion> GenerateInterviewQuestionAsync(
        string company,
        string role,
        string questionType,
        List<string> previousQuestions,
        CancellationToken cancellationToken = default)
    {
        // Search for relevant interview questions from vector store
        var searchQuery = $"{company} {role} {questionType} interview questions";
        var relevantExperiences = await _vectorStoreService.SearchAsync(
            searchQuery, "interview_experience", 5, 0.6, cancellationToken);

        var context = string.Join("\n", relevantExperiences.Select(r => r.Content));
        var previousQuestionsText = previousQuestions.Any() 
            ? $"Previous questions asked (avoid repeating): {string.Join("; ", previousQuestions)}"
            : "";

        var prompt = $@"Generate a realistic {questionType} interview question for a {role} position at {company}.

CONTEXT FROM REAL INTERVIEWS:
{context}

{previousQuestionsText}

Provide the question in this JSON format:
{{
    ""question"": ""The interview question"",
    ""category"": ""{questionType}"",
    ""expectedTopics"": [""topic1"", ""topic2""],
    ""hint"": ""A subtle hint if they get stuck"",
    ""timeLimitSeconds"": 180
}}";

        var response = await _openAIService.CompleteAsJsonAsync<MockInterviewQuestion>(
            prompt,
            new CompletionOptions { Temperature = 0.7 },
            cancellationToken);

        return response ?? new MockInterviewQuestion
        {
            Question = $"Tell me about a challenging project you worked on as a {role}.",
            Category = questionType,
            ExpectedTopics = new List<string> { "problem-solving", "technical skills", "teamwork" },
            TimeLimitSeconds = 180
        };
    }

    private static double CalculateOverallScore(double semantic, double keyword, double title)
    {
        return (semantic * 0.4) + (keyword * 0.35) + (title * 0.25);
    }

    // Response DTOs for JSON parsing
    private class MatchAnalysisResponse
    {
        public int KeywordScore { get; set; }
        public int TitleScore { get; set; }
        public List<string> MatchingKeywords { get; set; } = new();
        public List<string> MissingKeywords { get; set; } = new();
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public List<SkillGapDto> SkillGaps { get; set; } = new();
    }

    private class SkillGapDto
    {
        public string Skill { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
    }

    private class ProjectRecommendationsResponse
    {
        public List<ProjectDto> Projects { get; set; } = new();
    }

    private class ProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> SkillsAddressed { get; set; } = new();
        public string Difficulty { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }
        public double ImpactScore { get; set; }
    }
}

public record InterviewExperienceInput
{
    public string SourcePlatform { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
}

public record MockInterviewQuestion
{
    public string Question { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> ExpectedTopics { get; set; } = new();
    public string? Hint { get; set; }
    public int TimeLimitSeconds { get; set; }
}

public record ProjectRecommendation
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> SkillsAddressed { get; init; } = new();
    public string Difficulty { get; init; } = string.Empty;
    public int EstimatedHours { get; init; }
    public double ImpactScore { get; init; }
}
