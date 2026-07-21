using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace HireKarlo.Web.Client.Services;

public class AuthStateProvider : IDisposable
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public event Action? OnAuthStateChanged;
    public UserInfo? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsPrerendering { get; private set; } = true;

    public AuthStateProvider(ILocalStorageService localStorage, HttpClient http, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _http = http;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        // Prevent multiple initializations and server-side execution
        if (_isInitialized) return;

        // Check if we're in a browser context
        try
        {
            // This will throw during server-side prerendering
            if (_jsRuntime is IJSInProcessRuntime)
            {
                IsPrerendering = false;
            }
            else
            {
                // Try a simple JS interop call to detect if we're in browser
                await _jsRuntime.InvokeVoidAsync("eval", "");
                IsPrerendering = false;
            }
        }
        catch
        {
            // We're prerendering on server, skip localStorage access
            IsPrerendering = true;
            return;
        }

        try
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var user = await _localStorage.GetItemAsync<UserInfo>("user");
                if (user != null)
                {
                    CurrentUser = user;
                }
            }
            _isInitialized = true;
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthStateProvider init error: {ex.Message}");
        }
    }

    public async Task LoginAsync(string token, UserInfo user)
    {
        if (IsPrerendering) return;

        try
        {
            await _localStorage.SetItemAsStringAsync("authToken", token);
            await _localStorage.SetItemAsync("user", user);

            _http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            CurrentUser = user;
            _isInitialized = true;
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        if (IsPrerendering) return;

        try
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");

            _http.DefaultRequestHeaders.Authorization = null;
            CurrentUser = null;
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        OnAuthStateChanged = null;
    }
}
