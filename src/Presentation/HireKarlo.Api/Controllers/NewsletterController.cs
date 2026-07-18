using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HireKarlo.Infrastructure.Services;
using HireKarlo.Application.Interfaces.External;
using System.Security.Claims;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly IEmailDigestService _emailDigestService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(IEmailDigestService emailDigestService, ILogger<NewsletterController> logger)
    {
        _emailDigestService = emailDigestService;
        _logger = logger;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        if (userId == null && string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { error = "Email is required for anonymous subscription" });
        }

        try
        {
            if (userId.HasValue)
            {
                await _emailDigestService.SubscribeToNewsletterAsync(
                    userId.Value, 
                    request.Email, 
                    request.Name ?? "Subscriber", 
                    cancellationToken);
            }
            else
            {
                // For anonymous users, just store email for future (would need separate email list)
                _logger.LogInformation("Anonymous newsletter subscription: {Email}", request.Email);
            }

            return Ok(new { message = "Successfully subscribed to newsletter!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to newsletter");
            return StatusCode(500, new { error = "Failed to subscribe. Please try again." });
        }
    }

    [HttpPost("unsubscribe")]
    [Authorize]
    public async Task<IActionResult> Unsubscribe(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            await _emailDigestService.UnsubscribeFromNewsletterAsync(userId.Value, cancellationToken);
            return Ok(new { message = "Successfully unsubscribed from newsletter" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from newsletter");
            return StatusCode(500, new { error = "Failed to unsubscribe. Please try again." });
        }
    }

    [HttpGet("preview")]
    [Authorize]
    public async Task<ActionResult<WeeklyDigestContent>> PreviewDigest(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var digest = await _emailDigestService.BuildDigestForUserAsync(userId.Value, cancellationToken);
            return Ok(digest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building digest preview");
            return StatusCode(500, new { error = "Failed to build digest preview" });
        }
    }

    [HttpPost("send-test")]
    [Authorize]
    public async Task<IActionResult> SendTestDigest(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            // In a real implementation, this would send an email to the user
            var digest = await _emailDigestService.BuildDigestForUserAsync(userId.Value, cancellationToken);
            return Ok(new { message = "Test digest sent!", preview = digest });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test digest");
            return StatusCode(500, new { error = "Failed to send test digest" });
        }
    }

    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}

public record NewsletterSubscribeRequest(string Email, string? Name);
