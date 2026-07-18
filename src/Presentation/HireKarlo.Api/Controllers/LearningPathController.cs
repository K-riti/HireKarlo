using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LearningPathController : ControllerBase
{
    private readonly ILearningPathService _learningPathService;
    private readonly ILogger<LearningPathController> _logger;

    public LearningPathController(
        ILearningPathService learningPathService,
        ILogger<LearningPathController> logger)
    {
        _learningPathService = learningPathService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a company-specific learning path based on interview patterns
    /// </summary>
    [HttpPost("generate/company")]
    public async Task<ActionResult<LearningPathResult>> GenerateCompanyPath(
        [FromBody] GenerateCompanyPathRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _learningPathService.GenerateCompanyPathAsync(
            userId, request.Company, request.TargetRole, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate a skill-based learning path
    /// </summary>
    [HttpPost("generate/skills")]
    public async Task<ActionResult<LearningPathResult>> GenerateSkillPath(
        [FromBody] GenerateSkillPathRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _learningPathService.GenerateSkillPathAsync(
            userId, request.Skills, request.DifficultyLevel, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Generate an interview pattern-based path with time constraints
    /// </summary>
    [HttpPost("generate/interview-prep")]
    public async Task<ActionResult<LearningPathResult>> GenerateInterviewPatternPath(
        [FromBody] GenerateInterviewPrepRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var options = new InterviewPatternOptions
        {
            FocusAreas = request.FocusAreas,
            WeeksToInterview = request.WeeksToInterview,
            IncludeMockInterviews = request.IncludeMockInterviews,
            IncludeCompanySpecificQuestions = request.IncludeCompanySpecificQuestions
        };

        var result = await _learningPathService.GenerateInterviewPatternPathAsync(
            userId, request.Company, options, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user's active learning path
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<LearningPath>> GetActivePath(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = await _learningPathService.GetUserActivePathAsync(userId, cancellationToken);

        if (path == null)
        {
            return NotFound(new { message = "No active learning path found" });
        }

        return Ok(path);
    }

    /// <summary>
    /// Get all user's learning paths
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LearningPath>>> GetUserPaths(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var paths = await _learningPathService.GetUserPathsAsync(userId, cancellationToken);
        return Ok(paths);
    }

    /// <summary>
    /// Get a specific path with all modules
    /// </summary>
    [HttpGet("{pathId}")]
    public async Task<ActionResult<LearningPath>> GetPath(Guid pathId, CancellationToken cancellationToken)
    {
        var path = await _learningPathService.GetPathWithModulesAsync(pathId, cancellationToken);

        if (path == null)
        {
            return NotFound();
        }

        return Ok(path);
    }

    /// <summary>
    /// Start a learning path
    /// </summary>
    [HttpPost("{pathId}/start")]
    public async Task<ActionResult> StartPath(Guid pathId, CancellationToken cancellationToken)
    {
        await _learningPathService.StartPathAsync(pathId, cancellationToken);
        return Ok(new { message = "Learning path started" });
    }

    /// <summary>
    /// Pause a learning path
    /// </summary>
    [HttpPost("{pathId}/pause")]
    public async Task<ActionResult> PausePath(Guid pathId, CancellationToken cancellationToken)
    {
        await _learningPathService.PausePathAsync(pathId, cancellationToken);
        return Ok(new { message = "Learning path paused" });
    }

    /// <summary>
    /// Get module content with lessons and resources
    /// </summary>
    [HttpGet("modules/{moduleId}/content")]
    public async Task<ActionResult<ModuleContent>> GetModuleContent(
        Guid moduleId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await _learningPathService.GetModuleContentAsync(moduleId, cancellationToken);
            return Ok(content);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Start a module
    /// </summary>
    [HttpPost("modules/{moduleId}/start")]
    public async Task<ActionResult> StartModule(Guid moduleId, CancellationToken cancellationToken)
    {
        await _learningPathService.StartModuleAsync(moduleId, cancellationToken);
        return Ok(new { message = "Module started" });
    }

    /// <summary>
    /// Complete a module
    /// </summary>
    [HttpPost("modules/{moduleId}/complete")]
    public async Task<ActionResult> CompleteModule(
        Guid moduleId, 
        [FromBody] CompleteModuleRequest? request,
        CancellationToken cancellationToken)
    {
        await _learningPathService.CompleteModuleAsync(moduleId, request?.Score, cancellationToken);
        return Ok(new { message = "Module completed" });
    }

    /// <summary>
    /// Generate quiz for a module
    /// </summary>
    [HttpGet("modules/{moduleId}/quiz")]
    public async Task<ActionResult<QuizContent>> GetQuiz(
        Guid moduleId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var quiz = await _learningPathService.GenerateQuizAsync(moduleId, cancellationToken);
            return Ok(quiz);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit quiz answers
    /// </summary>
    [HttpPost("modules/{moduleId}/quiz/submit")]
    public async Task<ActionResult<QuizResult>> SubmitQuiz(
        Guid moduleId,
        [FromBody] SubmitQuizRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _learningPathService.SubmitQuizAsync(
                moduleId, request.Answers, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get quiz attempt history
    /// </summary>
    [HttpGet("modules/{moduleId}/quiz/attempts")]
    public async Task<ActionResult<List<QuizAttempt>>> GetQuizAttempts(
        Guid moduleId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var attempts = await _learningPathService.GetQuizAttemptsAsync(moduleId, userId, cancellationToken);
        return Ok(attempts);
    }

    /// <summary>
    /// Get personalized learning recommendations
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<List<LearningRecommendation>>> GetRecommendations(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var recommendations = await _learningPathService.GetRecommendationsAsync(userId, cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get user's weak areas based on quiz performance
    /// </summary>
    [HttpGet("weak-areas")]
    public async Task<ActionResult<List<string>>> GetWeakAreas(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var weakAreas = await _learningPathService.GetWeakAreasAsync(userId, cancellationToken);
        return Ok(weakAreas);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

#region Request DTOs

public record GenerateCompanyPathRequest
{
    public string Company { get; init; } = string.Empty;
    public string TargetRole { get; init; } = string.Empty;
}

public record GenerateSkillPathRequest
{
    public List<string> Skills { get; init; } = new();
    public int DifficultyLevel { get; init; } = 3;
}

public record GenerateInterviewPrepRequest
{
    public string Company { get; init; } = string.Empty;
    public List<string> FocusAreas { get; init; } = new(); // DSA, System Design, Behavioral
    public int WeeksToInterview { get; init; } = 4;
    public bool IncludeMockInterviews { get; init; } = true;
    public bool IncludeCompanySpecificQuestions { get; init; } = true;
}

public record CompleteModuleRequest
{
    public int? Score { get; init; }
}

public record SubmitQuizRequest
{
    public List<QuizAnswer> Answers { get; init; } = new();
}

#endregion
