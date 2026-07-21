using HireKarlo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login with Google OAuth - send Google ID token
    /// </summary>
    [HttpPost("google")]
    public async Task<ActionResult<AuthResult>> LoginWithGoogle(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginWithGoogleAsync(request.IdToken, cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    /// <summary>
    /// Login with LinkedIn OAuth - send authorization code
    /// </summary>
    [HttpPost("linkedin")]
    public async Task<ActionResult<AuthResult>> LoginWithLinkedIn(
        [FromBody] LinkedInLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginWithLinkedInAsync(
            request.Code, 
            request.RedirectUri, 
            cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    /// <summary>
    /// Register with email/password
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { error = "Email, first name, and last name are required" });
        }

        var result = await _authService.RegisterWithEmailAsync(request, cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    /// <summary>
    /// OAuth login/register - handles Google and LinkedIn OAuth
    /// </summary>
    [HttpPost("oauth")]
    public async Task<ActionResult<AuthResult>> OAuthLogin(
        [FromBody] OAuthLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        var result = await _authService.LoginOrRegisterWithOAuthAsync(
            request.Email,
            request.Name ?? request.Email,
            request.Provider ?? "OAuth",
            request.GoogleId ?? request.LinkedInId,
            cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.Success)
            return Unauthorized(new { error = result.Error });

        return Ok(result);
    }

    /// <summary>
    /// Logout - invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        await _authService.LogoutAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token, cancellationToken);

        if (user == null)
            return Unauthorized();

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Headline = user.Headline,
            LinkedInProfileUrl = user.LinkedInProfileUrl,
            TargetRole = user.TargetRole,
            TargetLocations = user.TargetLocations,
            IsOpenToRemote = user.IsOpenToRemote,
            RequiresVisa = user.RequiresVisa,
            SubscribedToNewsletter = user.SubscribedToNewsletter,
            SubscribedToMatchAlerts = user.SubscribedToMatchAlerts,
            SubscribedToWeeklyDigest = user.SubscribedToWeeklyDigest,
            LastLoginAt = user.LastLoginAt,
            LastLoginProvider = user.LastLoginProvider
        });
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement profile update
        return NoContent();
    }

    /// <summary>
    /// Update newsletter preferences
    /// </summary>
    [HttpPut("preferences/newsletter")]
    [Authorize]
    public async Task<ActionResult> UpdateNewsletterPreferences(
        [FromBody] NewsletterPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement preference update
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
        return Guid.Parse(userIdClaim);
    }
}

public record GoogleLoginRequest
{
    public string IdToken { get; init; } = string.Empty;
}

public record LinkedInLoginRequest
{
    public string Code { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public record UserProfileDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public string? Headline { get; init; }
    public string? LinkedInProfileUrl { get; init; }
    public string? TargetRole { get; init; }
    public string? TargetLocations { get; init; }
    public bool IsOpenToRemote { get; init; }
    public bool RequiresVisa { get; init; }
    public bool SubscribedToNewsletter { get; init; }
    public bool SubscribedToMatchAlerts { get; init; }
    public bool SubscribedToWeeklyDigest { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public string? LastLoginProvider { get; init; }
}

public record UpdateProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Headline { get; init; }
    public string? About { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Location { get; init; }
    public string? TargetRole { get; init; }
    public List<string>? TargetLocations { get; init; }
    public int? TargetSalaryMin { get; init; }
    public int? TargetSalaryMax { get; init; }
    public bool? IsOpenToRemote { get; init; }
    public bool? IsOpenToRelocation { get; init; }
    public bool? RequiresVisa { get; init; }
}

public record NewsletterPreferencesRequest
{
    public bool SubscribedToNewsletter { get; init; }
    public bool SubscribedToMatchAlerts { get; init; }
    public bool SubscribedToWeeklyDigest { get; init; }
}

public record OAuthLoginRequest
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? GoogleId { get; init; }
    public string? LinkedInId { get; init; }
    public string? Provider { get; init; }
}
