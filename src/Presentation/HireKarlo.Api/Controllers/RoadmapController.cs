using HireKarlo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;
    private readonly ILogger<RoadmapController> _logger;

    public RoadmapController(IRoadmapService roadmapService, ILogger<RoadmapController> logger)
    {
        _roadmapService = roadmapService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoadmapItemDto>>> GetRoadmap(
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement get roadmap query
        return Ok(new List<RoadmapItemDto>());
    }

    [HttpPost("generate")]
    public async Task<ActionResult> GenerateRoadmap(
        [FromBody] GenerateRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = new RoadmapGenerationOptions
        {
            WeeksToGenerate = request.Weeks,
            TargetRoles = request.TargetRoles,
            TargetCompanies = request.TargetCompanies,
            HoursPerWeek = request.HoursPerWeek,
            IncludeDSA = request.IncludeDSA,
            IncludeSystemDesign = request.IncludeSystemDesign,
            IncludeBehavioral = request.IncludeBehavioral,
            IncludeProjects = request.IncludeProjects
        };

        var roadmap = await _roadmapService.GenerateRoadmapAsync(
            GetUserId(), options, cancellationToken);

        return Ok(roadmap);
    }

    [HttpGet("projects/recommendations")]
    public async Task<ActionResult<List<ProjectRecommendation>>> GetProjectRecommendations(
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _roadmapService.GetProjectRecommendationsAsync(
            GetUserId(), cancellationToken);

        return Ok(recommendations);
    }

    [HttpPost("trajectory/simulate")]
    public async Task<ActionResult<SkillTrajectoryResult>> SimulateTrajectory(
        [FromBody] SimulateTrajectoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _roadmapService.SimulateSkillTrajectoryAsync(
            GetUserId(), request.SkillsToLearn, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult> UpdateItemStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement status update
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}

public record RoadmapItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int WeekNumber { get; init; }
    public string? Category { get; init; }
    public List<ResourceLinkDto> Resources { get; init; } = new();
}

public record ResourceLinkDto
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}

public record GenerateRoadmapRequest
{
    public int Weeks { get; init; } = 24;
    public List<string> TargetRoles { get; init; } = new();
    public List<string> TargetCompanies { get; init; } = new();
    public int HoursPerWeek { get; init; } = 20;
    public bool IncludeDSA { get; init; } = true;
    public bool IncludeSystemDesign { get; init; } = true;
    public bool IncludeBehavioral { get; init; } = true;
    public bool IncludeProjects { get; init; } = true;
}

public record SimulateTrajectoryRequest
{
    public List<string> SkillsToLearn { get; init; } = new();
}

public record UpdateStatusRequest
{
    public string Status { get; init; } = string.Empty;
}
