using HireKarlo.Application.Features.Applications.Commands;
using HireKarlo.Application.Features.Applications.Queries;
using HireKarlo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IMediator mediator, ILogger<ApplicationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get applications - returns demo data for unauthenticated users
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ApplicationDto>>> GetApplications(
        [FromQuery] ApplicationStage? stage = null,
        CancellationToken cancellationToken = default)
    {
        // Return demo data for testing
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(GetDemoApplications());
        }

        var userId = GetUserId();
        var query = new GetUserApplicationsQuery
        {
            UserId = userId,
            FilterByStage = stage
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("kanban")]
    [Authorize]
    public async Task<ActionResult<Dictionary<ApplicationStage, List<ApplicationDto>>>> GetKanbanBoard(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetApplicationsByStageQuery { UserId = userId };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApplicationDetailDto>> GetApplication(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationByIdQuery { ApplicationId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateApplication(
        [FromBody] CreateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateApplicationCommand
        {
            UserId = GetUserId(),
            JobListingId = request.JobListingId,
            ResumeId = request.ResumeId,
            InitialStage = request.Stage,
            Notes = request.Notes
        };

        var applicationId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetApplication), new { id = applicationId }, applicationId);
    }

    [HttpPut("{id:guid}/stage")]
    [Authorize]
    public async Task<ActionResult> UpdateStage(
        Guid id,
        [FromBody] UpdateStageRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateApplicationStageCommand
        {
            ApplicationId = id,
            NewStage = request.Stage,
            Notes = request.Notes
        };

        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:guid}/draft")]
    public async Task<ActionResult<DraftApplicationResult>> GenerateDraft(
        Guid id,
        [FromQuery] bool includeCoverLetter = true,
        CancellationToken cancellationToken = default)
    {
        var command = new DraftApplicationCommand
        {
            ApplicationId = id,
            IncludeCoverLetter = includeCoverLetter
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/referral")]
    public async Task<ActionResult> AddReferral(
        Guid id,
        [FromBody] AddReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddReferralToApplicationCommand
        {
            ApplicationId = id,
            ContactId = request.ContactId
        };

        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }

    private static List<DemoApplicationDto> GetDemoApplications() =>
    [
        new() { Id = Guid.NewGuid(), Company = "Google", Position = "Senior Software Engineer", Status = "Interview", Location = "Mountain View, CA", AppliedDate = DateTime.Now.AddDays(-5), MatchScore = 92 },
        new() { Id = Guid.NewGuid(), Company = "Microsoft", Position = "Full Stack Developer", Status = "Applied", Location = "Seattle, WA", AppliedDate = DateTime.Now.AddDays(-3), MatchScore = 88 },
        new() { Id = Guid.NewGuid(), Company = "Amazon", Position = "Backend Engineer", Status = "Applied", Location = "San Francisco, CA", AppliedDate = DateTime.Now.AddDays(-7), MatchScore = 85 },
        new() { Id = Guid.NewGuid(), Company = "Meta", Position = "Frontend Developer", Status = "Offer", Location = "Menlo Park, CA", AppliedDate = DateTime.Now.AddDays(-14), MatchScore = 90 },
        new() { Id = Guid.NewGuid(), Company = "Netflix", Position = "DevOps Engineer", Status = "Rejected", Location = "Los Gatos, CA", AppliedDate = DateTime.Now.AddDays(-20), MatchScore = 75 },
        new() { Id = Guid.NewGuid(), Company = "Stripe", Position = "Platform Engineer", Status = "Interview", Location = "San Francisco, CA", AppliedDate = DateTime.Now.AddDays(-10), MatchScore = 95 },
    ];
}

public class DemoApplicationDto
{
    public Guid Id { get; set; }
    public string Company { get; set; } = "";
    public string Position { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Location { get; set; }
    public DateTime AppliedDate { get; set; }
    public int MatchScore { get; set; }
}

public record CreateApplicationRequest
{
    public Guid JobListingId { get; init; }
    public Guid? ResumeId { get; init; }
    public ApplicationStage Stage { get; init; } = ApplicationStage.Saved;
    public string? Notes { get; init; }
}

public record UpdateStageRequest
{
    public ApplicationStage Stage { get; init; }
    public string? Notes { get; init; }
}

public record AddReferralRequest
{
    public Guid ContactId { get; init; }
}
