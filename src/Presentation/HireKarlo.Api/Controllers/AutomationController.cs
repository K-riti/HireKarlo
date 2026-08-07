using HireKarlo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AutomationController : ControllerBase
{
    private readonly IJobApplicationAutomationService _automationService;
    private readonly ILogger<AutomationController> _logger;

    public AutomationController(
        IJobApplicationAutomationService automationService,
        ILogger<AutomationController> logger)
    {
        _automationService = automationService;
        _logger = logger;
    }

    /// <summary>
    /// Get current automation settings for the user
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<AutomationSettingsResponse>> GetSettings(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var settings = await _automationService.GetAutomationSettingsAsync(userId, cancellationToken);

        if (settings == null)
            return NotFound("Automation settings not found");

        return Ok(new AutomationSettingsResponse
        {
            Enabled = settings.Enabled,
            DailyApplicationTarget = settings.DailyApplicationTarget,
            MinimumMatchScore = settings.MinimumMatchScore,
            AutoTailorResume = settings.AutoTailorResume,
            PreferredResumeId = settings.PreferredResumeId
        });
    }

    /// <summary>
    /// Update automation settings for the user
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult<AutomationSettingsResponse>> UpdateSettings(
        [FromBody] UpdateAutomationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var settings = new AutomationSettings
        {
            Enabled = request.Enabled,
            DailyApplicationTarget = request.DailyApplicationTarget ?? 5,
            MinimumMatchScore = request.MinimumMatchScore ?? 70.0,
            AutoTailorResume = request.AutoTailorResume ?? true,
            PreferredResumeId = request.PreferredResumeId
        };

        var success = await _automationService.UpdateAutomationSettingsAsync(userId, settings, cancellationToken);

        if (!success)
            return BadRequest("Failed to update automation settings");

        return Ok(new AutomationSettingsResponse
        {
            Enabled = settings.Enabled,
            DailyApplicationTarget = settings.DailyApplicationTarget,
            MinimumMatchScore = settings.MinimumMatchScore,
            AutoTailorResume = settings.AutoTailorResume,
            PreferredResumeId = settings.PreferredResumeId
        });
    }

    /// <summary>
    /// Enable automation for the user
    /// </summary>
    [HttpPost("enable")]
    public async Task<ActionResult<AutomationSettingsResponse>> EnableAutomation(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var currentSettings = await _automationService.GetAutomationSettingsAsync(userId, cancellationToken);

        if (currentSettings == null)
            return BadRequest("Automation settings not found");

        currentSettings.Enabled = true;
        var success = await _automationService.UpdateAutomationSettingsAsync(userId, currentSettings, cancellationToken);

        if (!success)
            return BadRequest("Failed to enable automation");

        return Ok(new AutomationSettingsResponse
        {
            Enabled = true,
            DailyApplicationTarget = currentSettings.DailyApplicationTarget,
            MinimumMatchScore = currentSettings.MinimumMatchScore,
            AutoTailorResume = currentSettings.AutoTailorResume,
            PreferredResumeId = currentSettings.PreferredResumeId
        });
    }

    /// <summary>
    /// Disable automation for the user
    /// </summary>
    [HttpPost("disable")]
    public async Task<ActionResult<AutomationSettingsResponse>> DisableAutomation(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var currentSettings = await _automationService.GetAutomationSettingsAsync(userId, cancellationToken);

        if (currentSettings == null)
            return BadRequest("Automation settings not found");

        currentSettings.Enabled = false;
        var success = await _automationService.UpdateAutomationSettingsAsync(userId, currentSettings, cancellationToken);

        if (!success)
            return BadRequest("Failed to disable automation");

        return Ok(new AutomationSettingsResponse
        {
            Enabled = false,
            DailyApplicationTarget = currentSettings.DailyApplicationTarget,
            MinimumMatchScore = currentSettings.MinimumMatchScore,
            AutoTailorResume = currentSettings.AutoTailorResume,
            PreferredResumeId = currentSettings.PreferredResumeId
        });
    }

    /// <summary>
    /// Manually trigger automated job applications (for testing)
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<AutomationRunResponse>> RunApplicationAutomation(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        _logger.LogInformation("Manually triggering application automation for user {UserId}", userId);

        var result = await _automationService.ExecuteAutomatedApplicationsAsync(userId, cancellationToken);

        return Ok(new AutomationRunResponse
        {
            Success = result.Success,
            Message = result.Message,
            ApplicationsSubmitted = result.ApplicationsSubmitted,
            Applications = result.Applications.Select(a => new ApplicationResultDto
            {
                JobListingId = a.JobListingId,
                JobTitle = a.JobTitle,
                Company = a.Company,
                MatchScore = a.MatchScore,
                Applied = a.Applied,
                Reason = a.Reason,
                ApplicationId = a.ApplicationId
            }).ToList(),
            ExecutedAt = result.ExecutedAt,
            Error = result.Error
        });
    }

    /// <summary>
    /// Manually trigger automated resume upload (for testing)
    /// </summary>
    [HttpPost("upload-resume")]
    public async Task<ActionResult<MessageResponse>> RunResumeUploadAutomation(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        _logger.LogInformation("Manually triggering resume upload automation for user {UserId}", userId);

        var success = await _automationService.ExecuteAutomatedResumeUploadAsync(userId, cancellationToken);

        if (!success)
            return BadRequest(new MessageResponse { Message = "Failed to execute resume upload automation" });

        return Ok(new MessageResponse
        {
            Message = "Resume upload automation executed successfully"
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier") ??
                         User.FindFirst("sub") ??
                         User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User ID not found in token");
    }
}

#region DTOs

public record UpdateAutomationSettingsRequest
{
    public bool Enabled { get; init; }
    public int? DailyApplicationTarget { get; init; }
    public double? MinimumMatchScore { get; init; }
    public bool? AutoTailorResume { get; init; }
    public Guid? PreferredResumeId { get; init; }
}

public record AutomationSettingsResponse
{
    public bool Enabled { get; init; }
    public int DailyApplicationTarget { get; init; }
    public double MinimumMatchScore { get; init; }
    public bool AutoTailorResume { get; init; }
    public Guid? PreferredResumeId { get; init; }
}

public record AutomationRunResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int ApplicationsSubmitted { get; init; }
    public List<ApplicationResultDto> Applications { get; init; } = new();
    public DateTime ExecutedAt { get; init; }
    public string? Error { get; init; }
}

public record ApplicationResultDto
{
    public Guid JobListingId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public double MatchScore { get; init; }
    public bool Applied { get; init; }
    public string? Reason { get; init; }
    public Guid? ApplicationId { get; init; }
}

public record MessageResponse
{
    public string Message { get; init; } = string.Empty;
}

#endregion
