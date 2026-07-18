using Blazored.LocalStorage;
using System.Security.Claims;

namespace HireKarlo.Web.Client.Services;

public class AuthStateProvider : IDisposable
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public event Action? OnAuthStateChanged;
    public UserInfo? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public AuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public async Task InitializeAsync()
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
                OnAuthStateChanged?.Invoke();
            }
        }
    }

    public async Task LoginAsync(string token, UserInfo user)
    {
        await _localStorage.SetItemAsStringAsync("authToken", token);
        await _localStorage.SetItemAsync("user", user);

        _http.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CurrentUser = user;
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("user");

        _http.DefaultRequestHeaders.Authorization = null;
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    public void Dispose()
    {
        OnAuthStateChanged = null;
    }
}
