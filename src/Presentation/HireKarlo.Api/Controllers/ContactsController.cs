using HireKarlo.Application.Interfaces.AI;
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
public class ContactsController : ControllerBase
{
    private readonly IContactRepository _contactRepository;
    private readonly IDreamCompanyRepository _dreamCompanyRepository;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(
        IContactRepository contactRepository,
        IDreamCompanyRepository dreamCompanyRepository,
        IOpenAIService openAIService,
        ILogger<ContactsController> logger)
    {
        _contactRepository = contactRepository;
        _dreamCompanyRepository = dreamCompanyRepository;
        _openAIService = openAIService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

    /// <summary>
    /// Get all contacts for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ContactDto>>> GetContacts(
        [FromQuery] Guid? dreamCompanyId = null,
        [FromQuery] OutreachStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var contacts = await _contactRepository.GetByUserIdAsync(userId, cancellationToken);

        if (dreamCompanyId.HasValue)
            contacts = contacts.Where(c => c.DreamCompanyId == dreamCompanyId.Value).ToList();

        if (status.HasValue)
            contacts = contacts.Where(c => c.OutreachStatus == status.Value).ToList();

        var dtos = contacts.Select(c => new ContactDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            LinkedInUrl = c.LinkedInUrl,
            Title = c.Title,
            Company = c.Company,
            Relationship = c.Relationship,
            Notes = c.Notes,
            OutreachStatus = c.OutreachStatus,
            DraftedMessage = c.DraftedMessage,
            LastContactDate = c.LastContactDate,
            FollowUpDate = c.FollowUpDate,
            DreamCompanyId = c.DreamCompanyId,
            DreamCompanyName = c.DreamCompany?.Name
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific contact by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactDto>> GetContact(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null || contact.UserId != GetUserId())
            return NotFound();

        return Ok(new ContactDto
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            LinkedInUrl = contact.LinkedInUrl,
            Title = contact.Title,
            Company = contact.Company,
            Relationship = contact.Relationship,
            Notes = contact.Notes,
            OutreachStatus = contact.OutreachStatus,
            DraftedMessage = contact.DraftedMessage,
            LastContactDate = contact.LastContactDate,
            FollowUpDate = contact.FollowUpDate,
            DreamCompanyId = contact.DreamCompanyId,
            DreamCompanyName = contact.DreamCompany?.Name
        });
    }

    /// <summary>
    /// Add a new contact
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateContact(
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var contact = new Contact
        {
            UserId = GetUserId(),
            DreamCompanyId = request.DreamCompanyId,
            Name = request.Name,
            Email = request.Email,
            LinkedInUrl = request.LinkedInUrl,
            Title = request.Title,
            Company = request.Company,
            Relationship = request.Relationship,
            Notes = request.Notes,
            OutreachStatus = OutreachStatus.Draft
        };

        await _contactRepository.AddAsync(contact, cancellationToken);

        _logger.LogInformation("User {UserId} added contact {ContactName}", GetUserId(), contact.Name);

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact.Id);
    }

    /// <summary>
    /// Update a contact
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateContact(
        Guid id,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null || contact.UserId != GetUserId())
            return NotFound();

        contact.Name = request.Name ?? contact.Name;
        contact.Email = request.Email ?? contact.Email;
        contact.LinkedInUrl = request.LinkedInUrl ?? contact.LinkedInUrl;
        contact.Title = request.Title ?? contact.Title;
        contact.Company = request.Company ?? contact.Company;
        contact.Relationship = request.Relationship ?? contact.Relationship;
        contact.Notes = request.Notes ?? contact.Notes;
        contact.DreamCompanyId = request.DreamCompanyId ?? contact.DreamCompanyId;

        await _contactRepository.UpdateAsync(contact, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteContact(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null || contact.UserId != GetUserId())
            return NotFound();

        await _contactRepository.DeleteAsync(contact, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Generate AI-drafted outreach message for a contact
    /// </summary>
    [HttpPost("{id:guid}/draft-message")]
    public async Task<ActionResult<DraftMessageResponse>> GenerateDraftMessage(
        Guid id,
        [FromBody] DraftMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null || contact.UserId != GetUserId())
            return NotFound();

        var prompt = $@"Generate a professional, personalized outreach message for a referral request.

CONTACT INFORMATION:
- Name: {contact.Name}
- Title: {contact.Title ?? "Unknown"}
- Company: {contact.Company ?? contact.DreamCompany?.Name ?? "Unknown"}
- Relationship: {contact.Relationship ?? "Professional connection"}

SENDER CONTEXT:
- Target Role: {request.TargetRole}
- Sender Background: {request.SenderBackground}
- Why This Company: {request.WhyThisCompany}

MESSAGE TYPE: {request.MessageType} (LinkedIn/Email/Cold)

Generate a concise, genuine message that:
1. Opens with a relevant connection point
2. Briefly mentions the sender's relevant background
3. Asks for a referral or informational conversation
4. Is respectful of their time
5. Ends with a clear call to action

Keep it under 200 words. Be genuine, not salesy.";

        var draftedMessage = await _openAIService.CompleteAsync(prompt, new CompletionOptions 
        { 
            Temperature = 0.7,
            MaxTokens = 500 
        }, cancellationToken);

        // Save the drafted message
        contact.DraftedMessage = draftedMessage;
        await _contactRepository.UpdateAsync(contact, cancellationToken);

        return Ok(new DraftMessageResponse
        {
            DraftedMessage = draftedMessage,
            ContactId = contact.Id
        });
    }

    /// <summary>
    /// Update outreach status for a contact
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> UpdateOutreachStatus(
        Guid id,
        [FromBody] UpdateOutreachStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null || contact.UserId != GetUserId())
            return NotFound();

        contact.OutreachStatus = request.Status;

        if (request.Status == OutreachStatus.Sent || request.Status == OutreachStatus.Responded)
            contact.LastContactDate = DateTime.UtcNow;

        if (request.FollowUpDate.HasValue)
            contact.FollowUpDate = request.FollowUpDate;

        await _contactRepository.UpdateAsync(contact, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Get contacts that need follow-up
    /// </summary>
    [HttpGet("follow-ups")]
    public async Task<ActionResult<List<ContactDto>>> GetFollowUps(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var contacts = await _contactRepository.GetByUserIdAsync(userId, cancellationToken);

        var followUps = contacts
            .Where(c => c.FollowUpDate.HasValue && c.FollowUpDate.Value.Date <= DateTime.UtcNow.Date)
            .OrderBy(c => c.FollowUpDate)
            .Select(c => new ContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                LinkedInUrl = c.LinkedInUrl,
                Title = c.Title,
                Company = c.Company,
                Relationship = c.Relationship,
                Notes = c.Notes,
                OutreachStatus = c.OutreachStatus,
                DraftedMessage = c.DraftedMessage,
                LastContactDate = c.LastContactDate,
                FollowUpDate = c.FollowUpDate,
                DreamCompanyId = c.DreamCompanyId,
                DreamCompanyName = c.DreamCompany?.Name
            }).ToList();

        return Ok(followUps);
    }
}

// DTOs
public record ContactDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? Title { get; init; }
    public string? Company { get; init; }
    public string? Relationship { get; init; }
    public string? Notes { get; init; }
    public OutreachStatus OutreachStatus { get; init; }
    public string? DraftedMessage { get; init; }
    public DateTime? LastContactDate { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public Guid? DreamCompanyId { get; init; }
    public string? DreamCompanyName { get; init; }
}

public record CreateContactRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? Title { get; init; }
    public string? Company { get; init; }
    public string? Relationship { get; init; }
    public string? Notes { get; init; }
    public Guid? DreamCompanyId { get; init; }
}

public record UpdateContactRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? Title { get; init; }
    public string? Company { get; init; }
    public string? Relationship { get; init; }
    public string? Notes { get; init; }
    public Guid? DreamCompanyId { get; init; }
}

public record DraftMessageRequest
{
    public string TargetRole { get; init; } = string.Empty;
    public string SenderBackground { get; init; } = string.Empty;
    public string WhyThisCompany { get; init; } = string.Empty;
    public string MessageType { get; init; } = "LinkedIn"; // LinkedIn, Email, Cold
}

public record DraftMessageResponse
{
    public string DraftedMessage { get; init; } = string.Empty;
    public Guid ContactId { get; init; }
}

public record UpdateOutreachStatusRequest
{
    public OutreachStatus Status { get; init; }
    public DateTime? FollowUpDate { get; init; }
}
