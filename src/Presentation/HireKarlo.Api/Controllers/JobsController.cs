using HireKarlo.Application.Features.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IMediator mediator, ILogger<JobsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<JobSearchResult>> SearchJobs(
        [FromQuery] string? query,
        [FromQuery] string? location,
        [FromQuery] bool? remoteOnly,
        [FromQuery] bool? visaSponsorship,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchJobsQuery
        {
            Query = query,
            Location = location,
            RemoteOnly = remoteOnly,
            VisaSponsorship = visaSponsorship,
            Page = page,
            PageSize = Math.Min(pageSize, 50)
        };

        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> GetJob(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetJobByIdQuery { JobListingId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("matched")]
    public async Task<ActionResult<List<MatchedJobDto>>> GetMatchedJobs(
        [FromQuery] double minScore = 80,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetMatchedJobsQuery
        {
            UserId = userId,
            MinScore = minScore,
            Limit = Math.Min(limit, 100)
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/match")]
    public async Task<ActionResult<MatchedJobDto>> GetJobMatch(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // TODO: Calculate match for specific job
        return Ok();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}
