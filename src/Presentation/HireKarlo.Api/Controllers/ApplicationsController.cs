using HireKarlo.Application.Features.Applications.Commands;
using HireKarlo.Application.Features.Applications.Queries;
using HireKarlo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IMediator mediator, ILogger<ApplicationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationDto>>> GetApplications(
        [FromQuery] ApplicationStage? stage = null,
        CancellationToken cancellationToken = default)
    {
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
    public async Task<ActionResult<Dictionary<ApplicationStage, List<ApplicationDto>>>> GetKanbanBoard(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetApplicationsByStageQuery { UserId = userId };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
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
