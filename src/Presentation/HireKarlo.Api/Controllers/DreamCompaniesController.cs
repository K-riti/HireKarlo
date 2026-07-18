using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DreamCompaniesController : ControllerBase
{
    private readonly IDreamCompanyRepository _dreamCompanyRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly ILogger<DreamCompaniesController> _logger;

    public DreamCompaniesController(
        IDreamCompanyRepository dreamCompanyRepository,
        IJobListingRepository jobListingRepository,
        ILogger<DreamCompaniesController> logger)
    {
        _dreamCompanyRepository = dreamCompanyRepository;
        _jobListingRepository = jobListingRepository;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

    /// <summary>
    /// Get all dream companies for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DreamCompanyDto>>> GetDreamCompanies(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var companies = await _dreamCompanyRepository.GetByUserIdAsync(userId, cancellationToken);

        var dtos = companies.Select(c => new DreamCompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            LogoUrl = c.LogoUrl,
            Website = c.Website,
            CareersPageUrl = c.CareersPageUrl,
            SponsorsVisa = c.SponsorsVisa,
            Priority = c.Priority,
            Notes = c.Notes,
            TargetRoles = c.TargetRoles,
            IsTrackingJobs = c.IsTrackingJobs,
            LastJobFetch = c.LastJobFetch,
            ContactCount = c.Contacts?.Count ?? 0
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific dream company by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DreamCompanyDetailDto>> GetDreamCompany(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _dreamCompanyRepository.GetByIdAsync(id, cancellationToken);

        if (company == null || company.UserId != GetUserId())
            return NotFound();

        // Get recent jobs from this company
        var recentJobs = await _jobListingRepository.GetByCompanyAsync(company.Name, 10, cancellationToken);

        var dto = new DreamCompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            LogoUrl = company.LogoUrl,
            Website = company.Website,
            CareersPageUrl = company.CareersPageUrl,
            GreenhouseBoardToken = company.GreenhouseBoardToken,
            LeverCompanyId = company.LeverCompanyId,
            SponsorsVisa = company.SponsorsVisa,
            Priority = company.Priority,
            Notes = company.Notes,
            TargetRoles = company.TargetRoles,
            IsTrackingJobs = company.IsTrackingJobs,
            LastJobFetch = company.LastJobFetch,
            Contacts = company.Contacts?.Select(c => new ContactSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Title = c.Title,
                OutreachStatus = c.OutreachStatus
            }).ToList() ?? new(),
            RecentJobs = recentJobs.Select(j => new JobSummaryDto
            {
                Id = j.Id,
                Title = j.Title,
                PostedDate = j.PostedDate
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Add a new dream company to track
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateDreamCompany(
        [FromBody] CreateDreamCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = new DreamCompany
        {
            UserId = GetUserId(),
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Website = request.Website,
            CareersPageUrl = request.CareersPageUrl,
            GreenhouseBoardToken = request.GreenhouseBoardToken,
            LeverCompanyId = request.LeverCompanyId,
            SponsorsVisa = request.SponsorsVisa,
            Priority = request.Priority,
            Notes = request.Notes,
            TargetRoles = request.TargetRoles,
            IsTrackingJobs = request.IsTrackingJobs
        };

        await _dreamCompanyRepository.AddAsync(company, cancellationToken);

        _logger.LogInformation("User {UserId} added dream company {CompanyName}", GetUserId(), company.Name);

        return CreatedAtAction(nameof(GetDreamCompany), new { id = company.Id }, company.Id);
    }

    /// <summary>
    /// Update a dream company
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateDreamCompany(
        Guid id,
        [FromBody] UpdateDreamCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await _dreamCompanyRepository.GetByIdAsync(id, cancellationToken);

        if (company == null || company.UserId != GetUserId())
            return NotFound();

        company.Name = request.Name ?? company.Name;
        company.LogoUrl = request.LogoUrl ?? company.LogoUrl;
        company.Website = request.Website ?? company.Website;
        company.CareersPageUrl = request.CareersPageUrl ?? company.CareersPageUrl;
        company.GreenhouseBoardToken = request.GreenhouseBoardToken ?? company.GreenhouseBoardToken;
        company.LeverCompanyId = request.LeverCompanyId ?? company.LeverCompanyId;
        company.SponsorsVisa = request.SponsorsVisa ?? company.SponsorsVisa;
        company.Priority = request.Priority ?? company.Priority;
        company.Notes = request.Notes ?? company.Notes;
        company.TargetRoles = request.TargetRoles ?? company.TargetRoles;
        company.IsTrackingJobs = request.IsTrackingJobs ?? company.IsTrackingJobs;

        await _dreamCompanyRepository.UpdateAsync(company, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete a dream company
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteDreamCompany(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _dreamCompanyRepository.GetByIdAsync(id, cancellationToken);

        if (company == null || company.UserId != GetUserId())
            return NotFound();

        await _dreamCompanyRepository.DeleteAsync(company, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Update priority for a dream company
    /// </summary>
    [HttpPatch("{id:guid}/priority")]
    public async Task<ActionResult> UpdatePriority(
        Guid id,
        [FromBody] UpdatePriorityRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await _dreamCompanyRepository.GetByIdAsync(id, cancellationToken);

        if (company == null || company.UserId != GetUserId())
            return NotFound();

        company.Priority = request.Priority;
        await _dreamCompanyRepository.UpdateAsync(company, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Toggle job tracking for a company
    /// </summary>
    [HttpPatch("{id:guid}/tracking")]
    public async Task<ActionResult> ToggleJobTracking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _dreamCompanyRepository.GetByIdAsync(id, cancellationToken);

        if (company == null || company.UserId != GetUserId())
            return NotFound();

        company.IsTrackingJobs = !company.IsTrackingJobs;
        await _dreamCompanyRepository.UpdateAsync(company, cancellationToken);

        return Ok(new { IsTrackingJobs = company.IsTrackingJobs });
    }
}

// DTOs
public record DreamCompanyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Website { get; init; }
    public string? CareersPageUrl { get; init; }
    public bool SponsorsVisa { get; init; }
    public Priority Priority { get; init; }
    public string? Notes { get; init; }
    public string? TargetRoles { get; init; }
    public bool IsTrackingJobs { get; init; }
    public DateTime? LastJobFetch { get; init; }
    public int ContactCount { get; init; }
}

public record DreamCompanyDetailDto : DreamCompanyDto
{
    public string? GreenhouseBoardToken { get; init; }
    public string? LeverCompanyId { get; init; }
    public List<ContactSummaryDto> Contacts { get; init; } = new();
    public List<JobSummaryDto> RecentJobs { get; init; } = new();
}

public record ContactSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Title { get; init; }
    public OutreachStatus OutreachStatus { get; init; }
}

public record JobSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? PostedDate { get; init; }
}

public record CreateDreamCompanyRequest
{
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Website { get; init; }
    public string? CareersPageUrl { get; init; }
    public string? GreenhouseBoardToken { get; init; }
    public string? LeverCompanyId { get; init; }
    public bool SponsorsVisa { get; init; }
    public Priority Priority { get; init; } = Priority.Medium;
    public string? Notes { get; init; }
    public string? TargetRoles { get; init; }
    public bool IsTrackingJobs { get; init; } = true;
}

public record UpdateDreamCompanyRequest
{
    public string? Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? Website { get; init; }
    public string? CareersPageUrl { get; init; }
    public string? GreenhouseBoardToken { get; init; }
    public string? LeverCompanyId { get; init; }
    public bool? SponsorsVisa { get; init; }
    public Priority? Priority { get; init; }
    public string? Notes { get; init; }
    public string? TargetRoles { get; init; }
    public bool? IsTrackingJobs { get; init; }
}

public record UpdatePriorityRequest
{
    public Priority Priority { get; init; }
}
