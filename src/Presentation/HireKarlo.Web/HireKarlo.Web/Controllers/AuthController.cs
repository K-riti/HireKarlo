using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace HireKarlo.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Set reasonable timeout
        _logger = logger;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var clientId = _configuration["Google:ClientId"];

        // Check if Google OAuth is configured
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("Google OAuth not configured - using demo auth");
            return RedirectToAction(nameof(DemoAuth), new { provider = "Google" });
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback"),
            Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("Google authentication failed: {Error}", authenticateResult.Failure?.Message);
                return Redirect("/login?error=Google+authentication+failed");
            }

            var claims = authenticateResult.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Redirect("/login?error=Could+not+retrieve+email+from+Google");
            }

            // Try to call API, but fallback to demo mode if it fails
            return await ProcessOAuthLogin(email, name ?? email.Split('@')[0], googleId ?? "", "Google");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google OAuth callback");
            return Redirect("/login?error=Authentication+error");
        }
    }

    [HttpGet("linkedin")]
    public IActionResult LinkedInLogin()
    {
        var clientId = _configuration["LinkedIn:ClientId"];

        // Check if LinkedIn OAuth is configured
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("LinkedIn OAuth not configured - ClientId is empty. Using demo auth.");
            return RedirectToAction(nameof(DemoAuth), new { provider = "LinkedIn" });
        }

        var redirectUri = $"{Request.Scheme}://{Request.Host}/auth/linkedin/callback";
        var state = Guid.NewGuid().ToString("N");

        // Store state in cookie for CSRF protection
        Response.Cookies.Append("linkedin_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var authUrl = $"https://www.linkedin.com/oauth/v2/authorization?" +
            $"response_type=code&" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"state={state}&" +
            $"scope=openid%20profile%20email";

        _logger.LogInformation("Redirecting to LinkedIn with clientId: {ClientId}", clientId.Substring(0, Math.Min(8, clientId.Length)) + "...");
        return Redirect(authUrl);
    }

    [HttpGet("linkedin/callback")]
    public async Task<IActionResult> LinkedInCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("LinkedIn OAuth error: {Error}", error);
            return Redirect("/login?error=LinkedIn+authentication+cancelled");
        }

        // If code is missing, redirect to demo auth
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("LinkedIn callback received without code parameter");
            return RedirectToAction(nameof(DemoAuth), new { provider = "LinkedIn" });
        }

        // Verify state for CSRF protection
        var storedState = Request.Cookies["linkedin_state"];
        if (string.IsNullOrEmpty(storedState) || storedState != state)
        {
            _logger.LogWarning("LinkedIn state mismatch: stored={StoredState}, received={State}", storedState, state);
            return Redirect("/login?error=Invalid+state+parameter");
        }

        var clientId = _configuration["LinkedIn:ClientId"];
        var clientSecret = _configuration["LinkedIn:ClientSecret"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/auth/linkedin/callback";

        // Check if LinkedIn credentials are configured
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("LinkedIn credentials not configured, falling back to demo auth");
            return RedirectToAction(nameof(DemoAuth), new { provider = "LinkedIn" });
        }

        try
        {
            // Exchange code for access token
            using var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            });

            var tokenResponse = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn token exchange failed: {StatusCode} - {Content}", tokenResponse.StatusCode, tokenContent);
                return Redirect("/login?error=LinkedIn+authentication+failed");
            }

            var tokenData = JsonSerializer.Deserialize<LinkedInTokenResponse>(tokenContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrEmpty(tokenData?.AccessToken))
            {
                _logger.LogWarning("LinkedIn token response missing access_token");
                return Redirect("/login?error=LinkedIn+authentication+failed");
            }

            // Get user profile using OpenID userinfo endpoint
            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
            userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var userResponse = await _httpClient.SendAsync(userRequest);
            var userContent = await userResponse.Content.ReadAsStringAsync();

            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn profile fetch failed: {StatusCode} - {Content}", userResponse.StatusCode, userContent);
                return Redirect("/login?error=Could+not+fetch+LinkedIn+profile");
            }

            var userData = JsonSerializer.Deserialize<LinkedInUserInfo>(userContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return await ProcessOAuthLogin(userData?.Email ?? "", userData?.Name ?? "", userData?.Sub ?? "", "LinkedIn");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LinkedIn OAuth callback");
            return Redirect("/login?error=Authentication+error");
        }
    }

    /// <summary>
    /// Demo authentication endpoint - used when OAuth providers are not configured
    /// Creates a demo session for testing the app
    /// </summary>
    [HttpGet("demo")]
    public IActionResult DemoAuth([FromQuery] string provider = "Demo")
    {
        _logger.LogInformation("Demo auth requested for provider: {Provider}", provider);

        var demoUserId = Guid.NewGuid().ToString();
        var demoEmail = $"demo-{DateTime.Now.Ticks}@hirekarlo.com";
        var demoName = $"Demo User ({provider})";
        var demoToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        // Set demo auth cookies
        SetAuthCookies(demoToken, demoUserId, demoEmail, demoName, isNewUser: true);

        // Redirect to onboarding for new demo users
        return Redirect("/onboarding");
    }

    /// <summary>
    /// Process OAuth login - call API or fallback to demo mode
    /// </summary>
    private async Task<IActionResult> ProcessOAuthLogin(string email, string name, string providerId, string provider)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Redirect($"/login?error=Could+not+retrieve+email+from+{provider}");
        }

        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://hirekarlo-api.onrender.com";

        try
        {
            var requestBody = new
            {
                email,
                name,
                providerId,
                provider
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/auth/oauth", content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<OAuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    SetAuthCookies(authResult.Token, authResult.UserId ?? "", authResult.Email ?? email, authResult.Name ?? name, isNewUser: false);
                    return Redirect("/auth-callback");
                }
            }

            _logger.LogWarning("API auth failed with status: {Status}, falling back to demo mode", response.StatusCode);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("API auth timed out, falling back to demo mode");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API auth, falling back to demo mode");
        }

        // Fallback: Create local auth session
        var fallbackToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{DateTime.UtcNow.Ticks}"));
        SetAuthCookies(fallbackToken, Guid.NewGuid().ToString(), email, name, isNewUser: true);

        // Send new users to onboarding
        return Redirect("/onboarding");
    }

    /// <summary>
    /// Set authentication cookies
    /// </summary>
    private void SetAuthCookies(string token, string userId, string email, string name, bool isNewUser)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false, // Allow JavaScript access
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("auth_token", token, cookieOptions);
        Response.Cookies.Append("user_info", JsonSerializer.Serialize(new
        {
            UserId = userId,
            Email = email,
            Name = name,
            IsNewUser = isNewUser
        }), cookieOptions);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        Response.Cookies.Delete("user_info");
        return Redirect("/");
    }

    /// <summary>
    /// Check auth configuration status
    /// </summary>
    [HttpGet("status")]
    public IActionResult AuthStatus()
    {
        return Ok(new
        {
            GoogleConfigured = !string.IsNullOrEmpty(_configuration["Google:ClientId"]),
            LinkedInConfigured = !string.IsNullOrEmpty(_configuration["LinkedIn:ClientId"]),
            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://hirekarlo-api.onrender.com"
        });
    }
}

// Response models
public class OAuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public class LinkedInTokenResponse
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}

public class LinkedInUserInfo
{
    public string? Sub { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Picture { get; set; }
}
