using HireKarlo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly JobApplicationService _jobApplicationService;
    private readonly ILogger<JobApplicationsController> _logger;

    public JobApplicationsController(
        JobApplicationService jobApplicationService,
        ILogger<JobApplicationsController> logger)
    {
        _jobApplicationService = jobApplicationService;
        _logger = logger;
    }

    /// <summary>
    /// Evaluate how well a resume matches a job before applying
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<ApplicationEvaluationResponse>> EvaluateApplication(
        [FromBody] EvaluateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _jobApplicationService.EvaluateApplicationAsync(
            userId, request.JobListingId, request.ResumeId, cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new ApplicationEvaluationResponse
        {
            MatchScore = result.MatchScore,
            AtsScore = result.AtsScore,
            IsGoodFit = result.IsGoodFit,
            MinimumRecommendedScore = 70,
            FitIssues = result.FitIssues.Select(f => new FitIssueDto
            {
                Category = f.Category,
                Severity = f.Severity,
                Description = f.Description,
                Suggestion = f.Suggestion
            }).ToList(),
            Recommendations = result.Recommendations,
            Message = result.IsGoodFit 
                ? "Great match! You're a strong candidate for this role." 
                : $"Your match score is {result.MatchScore:F1}%, which is below our recommended threshold of 70%. Consider improving your resume before applying."
        });
    }

    /// <summary>
    /// Apply to a job (will check match score and warn if below 70%)
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<ApplyToJobResponse>> ApplyToJob(
        [FromBody] ApplyToJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _jobApplicationService.ApplyToJobAsync(
            userId, request.JobListingId, request.ResumeId, request.ForceApply, cancellationToken);

        if (!result.Success)
        {
            if (result.RequiresConfirmation)
            {
                // Return 200 but with requiresConfirmation flag
                return Ok(new ApplyToJobResponse
                {
                    Success = false,
                    RequiresConfirmation = true,
                    MatchScore = result.MatchScore,
                    AtsScore = result.AtsScore,
                    Warning = result.Warning,
                    FitIssues = result.FitIssues?.Select(f => new FitIssueDto
                    {
                        Category = f.Category,
                        Severity = f.Severity,
                        Description = f.Description,
                        Suggestion = f.Suggestion
                    }).ToList(),
                    Recommendations = result.Recommendations,
                    Message = "Your match score is below 70%. Review the issues and recommendations, then resubmit with forceApply=true if you still want to proceed."
                });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new ApplyToJobResponse
        {
            Success = true,
            ApplicationId = result.ApplicationId,
            MatchScore = result.MatchScore,
            AtsScore = result.AtsScore,
            IsGoodFit = result.IsGoodFit,
            Message = result.Message
        });
    }

    /// <summary>
    /// Generate a tailored resume for a specific job description
    /// </summary>
    [HttpPost("generate-resume")]
    public async Task<ActionResult<GenerateResumeResponse>> GenerateResumeForJob(
        [FromBody] GenerateResumeRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _jobApplicationService.GenerateResumeForJobAsync(
            userId, request.JobListingId, request.BaseResumeId, cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new GenerateResumeResponse
        {
            TailoredContent = result.TailoredContent?.ResumeText,
            TailoredSummary = result.TailoredContent?.Summary,
            OriginalMatchScore = result.OriginalMatchScore,
            NewMatchScore = result.NewMatchScore,
            ImprovementPercentage = result.ImprovementPercentage,
            ChangesMade = result.ChangesMade,
            Message = $"Resume optimized! Match score improved from {result.OriginalMatchScore:F1}% to {result.NewMatchScore:F1}% (+{result.ImprovementPercentage:F1}%)"
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
        return Guid.Parse(userIdClaim);
    }
}

// Request/Response DTOs
public record EvaluateApplicationRequest
{
    public Guid JobListingId { get; init; }
    public Guid ResumeId { get; init; }
}

public record ApplicationEvaluationResponse
{
    public double MatchScore { get; init; }
    public int AtsScore { get; init; }
    public bool IsGoodFit { get; init; }
    public int MinimumRecommendedScore { get; init; }
    public string? Message { get; init; }
    public List<FitIssueDto> FitIssues { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
}

public record FitIssueDto
{
    public string Category { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Suggestion { get; init; } = string.Empty;
}

public record ApplyToJobRequest
{
    public Guid JobListingId { get; init; }
    public Guid ResumeId { get; init; }
    public bool ForceApply { get; init; } = false;
}

public record ApplyToJobResponse
{
    public bool Success { get; init; }
    public bool RequiresConfirmation { get; init; }
    public Guid? ApplicationId { get; init; }
    public double MatchScore { get; init; }
    public int AtsScore { get; init; }
    public bool IsGoodFit { get; init; }
    public string? Warning { get; init; }
    public string? Message { get; init; }
    public List<FitIssueDto>? FitIssues { get; init; }
    public List<string>? Recommendations { get; init; }
}

public record GenerateResumeRequest
{
    public Guid JobListingId { get; init; }
    public Guid? BaseResumeId { get; init; }
}

public record GenerateResumeResponse
{
    public string? TailoredContent { get; init; }
    public string? TailoredSummary { get; init; }
    public double OriginalMatchScore { get; init; }
    public double NewMatchScore { get; init; }
    public double ImprovementPercentage { get; init; }
    public List<string> ChangesMade { get; init; } = new();
    public string? Message { get; init; }
}
