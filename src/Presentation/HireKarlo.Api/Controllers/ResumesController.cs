using HireKarlo.Application.Features.Resumes.Commands;
using HireKarlo.Application.Features.Resumes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResumesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ResumesController> _logger;

    public ResumesController(IMediator mediator, ILogger<ResumesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResumeDto>>> GetUserResumes(
        [FromQuery] bool includeTailored = true,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserResumesQuery 
        { 
            UserId = userId, 
            IncludeTailored = includeTailored 
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResumeDto>> GetResume(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetResumeByIdQuery { ResumeId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<UploadResumeResult>> UploadResume(
        IFormFile file,
        [FromQuery] bool setAsMaster = false,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var allowedTypes = new[] { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest("Only PDF and DOCX files are supported");

        using var stream = file.OpenReadStream();
        var command = new UploadResumeCommand
        {
            UserId = GetUserId(),
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SetAsMaster = setAsMaster
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/tailor")]
    public async Task<ActionResult<TailorResumeResult>> TailorResume(
        Guid id,
        [FromBody] TailorResumeRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new TailorResumeCommand
        {
            UserId = GetUserId(),
            ResumeId = id,
            JobListingId = request.JobListingId,
            RewriteSummary = request.RewriteSummary,
            ReorderSkills = request.ReorderSkills,
            EnhanceBullets = request.EnhanceBullets
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<ActionResult<AnalyzeResumeResult>> AnalyzeResume(
        Guid id,
        [FromQuery] Guid? jobListingId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new AnalyzeResumeCommand
        {
            ResumeId = id,
            JobListingId = jobListingId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteResume(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement delete command
        return NoContent();
    }

    private Guid GetUserId()
    {
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}

public record TailorResumeRequest
{
    public Guid JobListingId { get; init; }
    public bool RewriteSummary { get; init; } = true;
    public bool ReorderSkills { get; init; } = true;
    public bool EnhanceBullets { get; init; } = true;
}
