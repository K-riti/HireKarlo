using HireKarlo.Domain.Entities;

namespace HireKarlo.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginWithLinkedInAsync(string authorizationCode, string redirectUri, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginWithEmailAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult> RegisterWithEmailAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginOrRegisterWithOAuthAsync(string email, string name, string provider, string? providerId, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default);
}

public record AuthResult
{
    public bool Success { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public UserInfo? User { get; init; }
    public string? Error { get; init; }
    public bool IsNewUser { get; init; }
}

public record UserInfo
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public string? LoginProvider { get; init; }
}

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

public record GoogleUserInfo
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string GivenName { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string Picture { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
}

public record LinkedInUserInfo
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public string? Headline { get; init; }
    public string? ProfileUrl { get; init; }
}
