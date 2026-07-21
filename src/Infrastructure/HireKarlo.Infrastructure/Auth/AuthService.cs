using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HireKarlo.Infrastructure.Auth;

public class AuthSettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "HireKarlo";
    public string JwtAudience { get; set; } = "HireKarlo";
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 30;

    // Google OAuth
    public string GoogleClientId { get; set; } = string.Empty;
    public string GoogleClientSecret { get; set; } = string.Empty;

    // LinkedIn OAuth
    public string LinkedInClientId { get; set; } = string.Empty;
    public string LinkedInClientSecret { get; set; } = string.Empty;
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthSettings> settings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AuthResult> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify Google ID token
            var googleUser = await VerifyGoogleTokenAsync(idToken, cancellationToken);
            if (googleUser == null)
                return new AuthResult { Success = false, Error = "Invalid Google token" };

            // Find or create user
            var user = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);
            var isNewUser = user == null;

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = googleUser.Email,
                    FirstName = googleUser.GivenName,
                    LastName = googleUser.FamilyName,
                    DisplayName = googleUser.Name,
                    ProfilePictureUrl = googleUser.Picture,
                    GoogleId = googleUser.Id,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginProvider = "Google",
                    SubscribedToNewsletter = true,
                    SubscribedToMatchAlerts = true,
                    SubscribedToWeeklyDigest = true
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Update Google ID if not set, update last login
                if (string.IsNullOrEmpty(user.GoogleId))
                    user.GoogleId = googleUser.Id;
                if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                    user.ProfilePictureUrl = googleUser.Picture;
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginProvider = "Google";
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            return GenerateAuthResult(user, isNewUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return new AuthResult { Success = false, Error = "Authentication failed" };
        }
    }

    public async Task<AuthResult> LoginWithLinkedInAsync(string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
    {
        try
        {
            // Exchange authorization code for access token
            var tokenResponse = await ExchangeLinkedInCodeAsync(authorizationCode, redirectUri, cancellationToken);
            if (tokenResponse == null)
                return new AuthResult { Success = false, Error = "Failed to exchange LinkedIn code" };

            // Get user profile from LinkedIn
            var linkedInUser = await GetLinkedInUserInfoAsync(tokenResponse.AccessToken, cancellationToken);
            if (linkedInUser == null)
                return new AuthResult { Success = false, Error = "Failed to get LinkedIn profile" };

            // Find or create user
            var user = await _userRepository.GetByEmailAsync(linkedInUser.Email, cancellationToken);
            var isNewUser = user == null;

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = linkedInUser.Email,
                    FirstName = linkedInUser.FirstName,
                    LastName = linkedInUser.LastName,
                    DisplayName = $"{linkedInUser.FirstName} {linkedInUser.LastName}",
                    ProfilePictureUrl = linkedInUser.ProfilePictureUrl,
                    LinkedInId = linkedInUser.Id,
                    LinkedInProfileUrl = linkedInUser.ProfileUrl,
                    Headline = linkedInUser.Headline,
                    LinkedInAccessToken = tokenResponse.AccessToken,
                    LinkedInTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginProvider = "LinkedIn",
                    SubscribedToNewsletter = true,
                    SubscribedToMatchAlerts = true,
                    SubscribedToWeeklyDigest = true
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Update LinkedIn data
                if (string.IsNullOrEmpty(user.LinkedInId))
                    user.LinkedInId = linkedInUser.Id;
                user.LinkedInProfileUrl = linkedInUser.ProfileUrl;
                user.LinkedInAccessToken = tokenResponse.AccessToken;
                user.LinkedInTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                if (string.IsNullOrEmpty(user.Headline))
                    user.Headline = linkedInUser.Headline;
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginProvider = "LinkedIn";
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            return GenerateAuthResult(user, isNewUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LinkedIn login");
            return new AuthResult { Success = false, Error = "Authentication failed" };
        }
    }

    public async Task<AuthResult> LoginWithEmailAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return new AuthResult { Success = false, Error = "No account found with this email. Please sign up first." };
            }

            // Check if user has a password hash (registered with email)
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new AuthResult { Success = false, Error = "This account was created with Google or LinkedIn. Please use social login." };
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult { Success = false, Error = "Invalid password" };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginProvider = "Email";
            await _userRepository.UpdateAsync(user, cancellationToken);

            return GenerateAuthResult(user, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email login");
            return new AuthResult { Success = false, Error = "Login failed. Please try again." };
        }
    }

    public async Task<AuthResult> RegisterWithEmailAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
                return new AuthResult { Success = false, Error = "Email already registered. Please log in instead." };

            // Hash the password
            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DisplayName = $"{request.FirstName} {request.LastName}",
                PasswordHash = passwordHash,
                LastLoginAt = DateTime.UtcNow,
                LastLoginProvider = "Email",
                SubscribedToNewsletter = true,
                SubscribedToMatchAlerts = true,
                SubscribedToWeeklyDigest = true
            };

            await _userRepository.AddAsync(user, cancellationToken);
            return GenerateAuthResult(user, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email registration");
            return new AuthResult { Success = false, Error = "Registration failed. Please try again." };
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = $"{salt}:{password}";
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var hash = Convert.ToBase64String(hashBytes);
        return $"{salt}:{hash}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = parts[0];
        var hash = parts[1];

        using var sha256 = SHA256.Create();
        var saltedPassword = $"{salt}:{password}";
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var computedHash = Convert.ToBase64String(hashBytes);

        return hash == computedHash;
    }

    public async Task<AuthResult> LoginOrRegisterWithOAuthAsync(string email, string name, string provider, string? providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find or create user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            var isNewUser = user == null;

            if (user == null)
            {
                // Parse name into first/last
                var nameParts = (name ?? email).Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = name ?? email,
                    GoogleId = provider == "Google" ? providerId : null,
                    LinkedInId = provider == "LinkedIn" ? providerId : null,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginProvider = provider,
                    SubscribedToNewsletter = true,
                    SubscribedToMatchAlerts = true,
                    SubscribedToWeeklyDigest = true
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Update last login info
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginProvider = provider;

                // Link provider ID if not already set
                if (provider == "Google" && string.IsNullOrEmpty(user.GoogleId))
                    user.GoogleId = providerId;
                if (provider == "LinkedIn" && string.IsNullOrEmpty(user.LinkedInId))
                    user.LinkedInId = providerId;

                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            return GenerateAuthResult(user, isNewUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth login for {Email}", email);
            return new AuthResult { Success = false, Error = "Authentication failed" };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Validate refresh token and generate new access token
        // This would require storing refresh tokens - simplified for now
        return new AuthResult { Success = false, Error = "Refresh token expired" };
    }

    public Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Invalidate refresh tokens
        return Task.FromResult(true);
    }

    public Task<User?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.JwtSecret);

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _settings.JwtIssuer,
                ValidateAudience = true,
                ValidAudience = _settings.JwtAudience,
                ValidateLifetime = true
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "sub").Value);

            return _userRepository.GetByIdAsync(userId, cancellationToken);
        }
        catch
        {
            return Task.FromResult<User?>(null);
        }
    }

    private AuthResult GenerateAuthResult(User user, bool isNewUser)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            IsNewUser = isNewUser,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                LoginProvider = user.LastLoginProvider
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("name", user.DisplayName ?? $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.JwtIssuer,
            audience: _settings.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}", 
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenInfo = JsonSerializer.Deserialize<JsonElement>(json);

        // Verify audience matches our client ID
        var aud = tokenInfo.GetProperty("aud").GetString();
        if (aud != _settings.GoogleClientId)
            return null;

        return new GoogleUserInfo
        {
            Id = tokenInfo.GetProperty("sub").GetString() ?? "",
            Email = tokenInfo.GetProperty("email").GetString() ?? "",
            Name = tokenInfo.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            GivenName = tokenInfo.TryGetProperty("given_name", out var given) ? given.GetString() ?? "" : "",
            FamilyName = tokenInfo.TryGetProperty("family_name", out var family) ? family.GetString() ?? "" : "",
            Picture = tokenInfo.TryGetProperty("picture", out var pic) ? pic.GetString() ?? "" : "",
            EmailVerified = tokenInfo.TryGetProperty("email_verified", out var verified) && verified.GetString() == "true"
        };
    }

    private async Task<LinkedInTokenResponse?> ExchangeLinkedInCodeAsync(
        string code, string redirectUri, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = _settings.LinkedInClientId,
            ["client_secret"] = _settings.LinkedInClientSecret
        });

        var response = await client.PostAsync(
            "https://www.linkedin.com/oauth/v2/accessToken", 
            content, 
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>(cancellationToken: cancellationToken);
    }

    private async Task<LinkedInUserInfo?> GetLinkedInUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Get profile info using OpenID Connect userinfo endpoint
        var response = await client.GetAsync(
            "https://api.linkedin.com/v2/userinfo", 
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var userInfo = JsonSerializer.Deserialize<JsonElement>(json);

        return new LinkedInUserInfo
        {
            Id = userInfo.GetProperty("sub").GetString() ?? "",
            Email = userInfo.TryGetProperty("email", out var email) ? email.GetString() ?? "" : "",
            FirstName = userInfo.TryGetProperty("given_name", out var given) ? given.GetString() ?? "" : "",
            LastName = userInfo.TryGetProperty("family_name", out var family) ? family.GetString() ?? "" : "",
            ProfilePictureUrl = userInfo.TryGetProperty("picture", out var pic) ? pic.GetString() : null,
            ProfileUrl = null // Would need additional API call
        };
    }

    private record LinkedInTokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public int ExpiresIn { get; init; }
        public string Scope { get; init; } = string.Empty;
    }
}
