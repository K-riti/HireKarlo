using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;
using HireKarlo.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.Services;

public class JobApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IAtsScorer _atsScorer;
    private readonly IMatchingEngine _matchingEngine;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<JobApplicationService> _logger;

    private const double MinimumRecommendedMatchScore = 70.0;

    public JobApplicationService(
        IApplicationRepository applicationRepository,
        IResumeRepository resumeRepository,
        IJobListingRepository jobListingRepository,
        IMatchRepository matchRepository,
        IAtsScorer atsScorer,
        IMatchingEngine matchingEngine,
        IOpenAIService openAIService,
        ILogger<JobApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _resumeRepository = resumeRepository;
        _jobListingRepository = jobListingRepository;
        _matchRepository = matchRepository;
        _atsScorer = atsScorer;
        _matchingEngine = matchingEngine;
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<ApplicationEvaluationResult> EvaluateApplicationAsync(
        Guid userId,
        Guid jobListingId,
        Guid resumeId,
        CancellationToken cancellationToken = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId, cancellationToken);
        var job = await _jobListingRepository.GetByIdAsync(jobListingId, cancellationToken);

        if (resume == null || job == null)
        {
            return new ApplicationEvaluationResult
            {
                Success = false,
                Error = "Resume or job listing not found"
            };
        }

        // Calculate match score
        var matchReport = await _matchingEngine.CalculateMatchAsync(
            resume.RawText ?? "", 
            job.Description ?? "", 
            cancellationToken);

        // Get ATS score
        var atsReport = await _atsScorer.AnalyzeAsync(
            resume.RawText ?? "",
            job.Description ?? "",
            job.Title,
            cancellationToken);

        var overallScore = matchReport.OverallScore;
        var isGoodFit = overallScore >= MinimumRecommendedMatchScore;

        // Generate fit issues and recommendations if not a good fit
        var fitIssues = new List<FitIssue>();
        var recommendations = new List<string>();

        if (!isGoodFit)
        {
            fitIssues = await GenerateFitIssuesAsync(
                resume, job, matchReport, atsReport, cancellationToken);
            recommendations = await GenerateImprovementRecommendationsAsync(
                resume, job, matchReport, atsReport, cancellationToken);
        }

        return new ApplicationEvaluationResult
        {
            Success = true,
            MatchScore = overallScore,
            AtsScore = atsReport.Score.OverallScore,
            IsGoodFit = isGoodFit,
            MatchReport = matchReport,
            AtsReport = atsReport,
            FitIssues = fitIssues,
            Recommendations = recommendations
        };
    }

    public async Task<ApplyResult> ApplyToJobAsync(
        Guid userId,
        Guid jobListingId,
        Guid resumeId,
        bool forceApply = false,
        CancellationToken cancellationToken = default)
    {
        // First evaluate the application
        var evaluation = await EvaluateApplicationAsync(
            userId, jobListingId, resumeId, cancellationToken);

        if (!evaluation.Success)
        {
            return new ApplyResult
            {
                Success = false,
                Error = evaluation.Error
            };
        }

        // If not a good fit and user hasn't forced apply, return warning
        if (!evaluation.IsGoodFit && !forceApply)
        {
            return new ApplyResult
            {
                Success = false,
                RequiresConfirmation = true,
                MatchScore = evaluation.MatchScore,
                AtsScore = evaluation.AtsScore,
                FitIssues = evaluation.FitIssues,
                Recommendations = evaluation.Recommendations,
                Warning = $"Your match score is {evaluation.MatchScore:F1}%, which is below our recommended threshold of {MinimumRecommendedMatchScore}%. We recommend improving your resume before applying."
            };
        }

        // Create the application
        var application = new Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobListingId = jobListingId,
            ResumeId = resumeId,
            Stage = ApplicationStage.Applied,
            AppliedDate = DateTime.UtcNow,
            AtsScore = evaluation.AtsScore,
            AtsReport = System.Text.Json.JsonSerializer.Serialize(evaluation.AtsReport),
            StageHistory = System.Text.Json.JsonSerializer.Serialize(new[]
            {
                new { Stage = "Applied", Timestamp = DateTime.UtcNow }
            })
        };

        await _applicationRepository.AddAsync(application, cancellationToken);

        // Also store/update the match record
        var existingMatch = (await _matchRepository.GetByUserIdAsync(userId, cancellationToken))
            .FirstOrDefault(m => m.JobListingId == jobListingId);

        if (existingMatch == null)
        {
            var match = new Match
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                JobListingId = jobListingId,
                ResumeId = resumeId,
                OverallScore = evaluation.MatchScore,
                SemanticScore = evaluation.MatchReport?.SemanticScore ?? 0,
                KeywordScore = evaluation.MatchReport?.KeywordScore ?? 0,
                TitleScore = evaluation.MatchReport?.TitleScore ?? 0,
                Status = MatchStatus.Applied,
                MatchedAt = DateTime.UtcNow,
                GapReport = System.Text.Json.JsonSerializer.Serialize(evaluation.MatchReport?.GapAnalysis),
                MissingKeywords = System.Text.Json.JsonSerializer.Serialize(evaluation.MatchReport?.GapAnalysis.MissingKeywords),
                MatchingKeywords = System.Text.Json.JsonSerializer.Serialize(evaluation.MatchReport?.GapAnalysis.MatchingKeywords)
            };
            await _matchRepository.AddAsync(match, cancellationToken);
            application.MatchId = match.Id;
        }
        else
        {
            existingMatch.Status = MatchStatus.Applied;
            await _matchRepository.UpdateAsync(existingMatch, cancellationToken);
            application.MatchId = existingMatch.Id;
        }

        return new ApplyResult
        {
            Success = true,
            ApplicationId = application.Id,
            MatchScore = evaluation.MatchScore,
            AtsScore = evaluation.AtsScore,
            IsGoodFit = evaluation.IsGoodFit,
            Message = evaluation.IsGoodFit 
                ? "Application submitted successfully! You're a great match for this role."
                : "Application submitted. Consider improving your resume for better chances."
        };
    }

    public async Task<ResumeGenerationResult> GenerateResumeForJobAsync(
        Guid userId,
        Guid jobListingId,
        Guid? baseResumeId = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobListingRepository.GetByIdAsync(jobListingId, cancellationToken);
        if (job == null)
        {
            return new ResumeGenerationResult
            {
                Success = false,
                Error = "Job listing not found"
            };
        }

        Resume? baseResume = null;
        if (baseResumeId.HasValue)
        {
            baseResume = await _resumeRepository.GetByIdAsync(baseResumeId.Value, cancellationToken);
        }
        else
        {
            // Get master resume
            baseResume = await _resumeRepository.GetMasterResumeAsync(userId, cancellationToken);
        }

        if (baseResume == null)
        {
            return new ResumeGenerationResult
            {
                Success = false,
                Error = "No base resume found. Please upload a resume first."
            };
        }

        // Generate tailored resume content using AI
        var tailoredContent = await GenerateTailoredResumeContentAsync(
            baseResume, job, cancellationToken);

        // Analyze the improvements
        var originalMatch = await _matchingEngine.CalculateMatchAsync(
            baseResume.RawText ?? "", job.Description ?? "", cancellationToken);

        var newMatch = await _matchingEngine.CalculateMatchAsync(
            tailoredContent.ResumeText, job.Description ?? "", cancellationToken);

        return new ResumeGenerationResult
        {
            Success = true,
            TailoredContent = tailoredContent,
            OriginalMatchScore = originalMatch.OverallScore,
            NewMatchScore = newMatch.OverallScore,
            ImprovementPercentage = newMatch.OverallScore - originalMatch.OverallScore,
            ChangesMade = tailoredContent.ChangesMade
        };
    }

    private async Task<List<FitIssue>> GenerateFitIssuesAsync(
        Resume resume,
        JobListing job,
        MatchReport matchReport,
        AtsReport atsReport,
        CancellationToken cancellationToken)
    {
        var issues = new List<FitIssue>();

        // Check title mismatch
        if (matchReport.TitleScore < 50)
        {
            issues.Add(new FitIssue
            {
                Category = "Title Mismatch",
                Severity = "High",
                Description = $"Your title doesn't closely match '{job.Title}'",
                Suggestion = $"Consider updating your resume title to align with '{job.Title}' or similar"
            });
        }

        // Check missing skills
        if (matchReport.GapAnalysis.MissingKeywords.Count > 5)
        {
            var topMissing = matchReport.GapAnalysis.MissingKeywords.Take(5);
            issues.Add(new FitIssue
            {
                Category = "Missing Skills",
                Severity = "High",
                Description = $"You're missing key skills: {string.Join(", ", topMissing)}",
                Suggestion = "Add these skills to your resume if you have experience with them, or consider learning them"
            });
        }

        // Check ATS issues
        if (atsReport.Score.OverallScore < 60)
        {
            issues.Add(new FitIssue
            {
                Category = "ATS Compatibility",
                Severity = "Medium",
                Description = "Your resume may not parse well in Applicant Tracking Systems",
                Suggestion = string.Join("; ", atsReport.Recommendations.Take(3))
            });
        }

        // Check skill gaps
        var majorGaps = matchReport.GapAnalysis.SkillGaps
            .Where(g => g.Severity >= GapSeverity.Major)
            .ToList();

        foreach (var gap in majorGaps.Take(3))
        {
            issues.Add(new FitIssue
            {
                Category = "Skill Gap",
                Severity = gap.Severity.ToString(),
                Description = $"Missing required skill: {gap.RequiredSkill}",
                Suggestion = gap.SuggestedResources.FirstOrDefault() ?? "Consider learning this skill"
            });
        }

        return issues;
    }

    private async Task<List<string>> GenerateImprovementRecommendationsAsync(
        Resume resume,
        JobListing job,
        MatchReport matchReport,
        AtsReport atsReport,
        CancellationToken cancellationToken)
    {
        var prompt = $@"Based on this job and resume analysis, provide 3-5 specific, actionable recommendations to improve the candidate's chances.

JOB TITLE: {job.Title}
COMPANY: {job.Company}

MATCH SCORE: {matchReport.OverallScore:F1}%
MISSING KEYWORDS: {string.Join(", ", matchReport.GapAnalysis.MissingKeywords.Take(10))}
WEAKNESSES: {string.Join("; ", matchReport.Weaknesses.Take(3))}

Be specific and helpful. Format as a JSON array of strings.";

        var recommendations = await _openAIService.CompleteAsJsonAsync<List<string>>(
            prompt,
            new CompletionOptions { Temperature = 0.5, MaxTokens = 500 },
            cancellationToken);

        return recommendations ?? new List<string>
        {
            "Add more relevant keywords from the job description to your resume",
            "Quantify your achievements with specific metrics and numbers",
            "Tailor your summary to highlight experience relevant to this role"
        };
    }

    private async Task<TailoredResumeContent> GenerateTailoredResumeContentAsync(
        Resume baseResume,
        JobListing job,
        CancellationToken cancellationToken)
    {
        var prompt = $@"You are an expert resume writer. Optimize this resume for the given job description.

ORIGINAL RESUME:
{baseResume.RawText}

TARGET JOB:
Title: {job.Title}
Company: {job.Company}
Description: {job.Description}

Instructions:
1. Rewrite the summary to target this specific role
2. Reorder skills to prioritize those mentioned in the JD
3. Enhance bullet points to include relevant keywords
4. Maintain truthfulness - only add/emphasize skills the candidate actually has
5. Keep the same overall structure

Return the optimized resume text and a list of changes made.

Format as JSON:
{{
    ""resumeText"": ""full optimized resume text"",
    ""summary"": ""rewritten summary"",
    ""changesMade"": [""change1"", ""change2"", ...]
}}";

        var result = await _openAIService.CompleteAsJsonAsync<TailoredResumeContent>(
            prompt,
            new CompletionOptions { Temperature = 0.4, MaxTokens = 3000 },
            cancellationToken);

        return result ?? new TailoredResumeContent
        {
            ResumeText = baseResume.RawText ?? "",
            Summary = baseResume.Summary ?? "",
            ChangesMade = new List<string>()
        };
    }
}

public record ApplicationEvaluationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public double MatchScore { get; init; }
    public int AtsScore { get; init; }
    public bool IsGoodFit { get; init; }
    public MatchReport? MatchReport { get; init; }
    public AtsReport? AtsReport { get; init; }
    public List<FitIssue> FitIssues { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
}

public record ApplyResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public bool RequiresConfirmation { get; init; }
    public Guid? ApplicationId { get; init; }
    public double MatchScore { get; init; }
    public int AtsScore { get; init; }
    public bool IsGoodFit { get; init; }
    public string? Warning { get; init; }
    public string? Message { get; init; }
    public List<FitIssue>? FitIssues { get; init; }
    public List<string>? Recommendations { get; init; }
}

public record FitIssue
{
    public string Category { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Suggestion { get; init; } = string.Empty;
}

public record ResumeGenerationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public TailoredResumeContent? TailoredContent { get; init; }
    public double OriginalMatchScore { get; init; }
    public double NewMatchScore { get; init; }
    public double ImprovementPercentage { get; init; }
    public List<string> ChangesMade { get; init; } = new();
}

public record TailoredResumeContent
{
    public string ResumeText { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public List<string> ChangesMade { get; init; } = new();
}
