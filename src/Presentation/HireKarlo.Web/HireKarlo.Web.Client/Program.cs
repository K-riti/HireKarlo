using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Blazored.Toast;
using HireKarlo.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Add Blazored services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();

// Add application services
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthStateProvider>();

var host = builder.Build();

// Initialize auth state
var authState = host.Services.GetRequiredService<AuthStateProvider>();
await authState.InitializeAsync();

await host.RunAsync();
