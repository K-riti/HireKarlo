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
        _logger = logger;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
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
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            _logger.LogWarning("Google authentication failed");
            return Redirect("/login?error=Google authentication failed");
        }

        var claims = authenticateResult.Principal?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect("/login?error=Could not retrieve email from Google");
        }

        // Call our API to register/login the user
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://hirekarlo-api.onrender.com";

        try
        {
            var requestBody = new
            {
                email,
                name,
                googleId,
                provider = "Google"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/auth/oauth", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<OAuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    // Store token in a cookie or pass to client
                    Response.Cookies.Append("auth_token", authResult.Token, new CookieOptions
                    {
                        HttpOnly = false, // Allow JavaScript access
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    Response.Cookies.Append("user_info", JsonSerializer.Serialize(new
                    {
                        authResult.UserId,
                        authResult.Email,
                        authResult.Name
                    }), new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    return Redirect("/?login=success");
                }
            }

            _logger.LogWarning("API auth failed: {Status}", response.StatusCode);
            return Redirect("/login?error=Authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google OAuth callback");
            return Redirect("/login?error=Authentication error");
        }
    }

    [HttpGet("linkedin")]
    public IActionResult LinkedInLogin()
    {
        var clientId = _configuration["LinkedIn:ClientId"];
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

        return Redirect(authUrl);
    }

    [HttpGet("linkedin/callback")]
    public async Task<IActionResult> LinkedInCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("LinkedIn OAuth error: {Error}", error);
            return Redirect("/login?error=LinkedIn authentication cancelled");
        }

        // Verify state for CSRF protection
        var storedState = Request.Cookies["linkedin_state"];
        if (string.IsNullOrEmpty(storedState) || storedState != state)
        {
            return Redirect("/login?error=Invalid state parameter");
        }

        var clientId = _configuration["LinkedIn:ClientId"];
        var clientSecret = _configuration["LinkedIn:ClientSecret"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/auth/linkedin/callback";

        try
        {
            // Exchange code for access token
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId ?? "",
                ["client_secret"] = clientSecret ?? ""
            });

            var tokenResponse = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn token exchange failed");
                return Redirect("/login?error=LinkedIn authentication failed");
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<LinkedInTokenResponse>(tokenContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Get user profile using OpenID userinfo endpoint
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData?.AccessToken);
            var userResponse = await _httpClient.GetAsync("https://api.linkedin.com/v2/userinfo");

            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn profile fetch failed");
                return Redirect("/login?error=Could not fetch LinkedIn profile");
            }

            var userContent = await userResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<LinkedInUserInfo>(userContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Call our API to register/login the user
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://hirekarlo-api.onrender.com";

            var requestBody = new
            {
                email = userData?.Email,
                name = userData?.Name,
                linkedInId = userData?.Sub,
                provider = "LinkedIn"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = null; // Clear LinkedIn auth header
            var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/auth/oauth", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<OAuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    Response.Cookies.Append("auth_token", authResult.Token, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    Response.Cookies.Append("user_info", JsonSerializer.Serialize(new
                    {
                        authResult.UserId,
                        authResult.Email,
                        authResult.Name
                    }), new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    return Redirect("/?login=success");
                }
            }

            return Redirect("/login?error=Authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LinkedIn OAuth callback");
            return Redirect("/login?error=Authentication error");
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        Response.Cookies.Delete("user_info");
        return Redirect("/");
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
