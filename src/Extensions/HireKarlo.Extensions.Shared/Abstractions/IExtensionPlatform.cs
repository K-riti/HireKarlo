// HireKarlo Extension Architecture
// Enable multi-language, multi-platform extension delivery

namespace HireKarlo.Extensions.Shared.Abstractions;

/// <summary>
/// Represents supported platforms for HireKarlo
/// </summary>
public enum Platform
{
    VsCode,           // VS Code Extension
    Chrome,           // Chrome Web Extension
    Firefox,          // Firefox Web Extension
    Edge,             // Microsoft Edge Extension
    Safari,           // Safari App Extension
    WebStandalone     // Standalone Web Version
}

/// <summary>
/// Supported languages with language codes
/// </summary>
public enum Language
{
    Auto,             // Auto-detect from browser/OS
    English,          // en
    Spanish,          // es
    French,           // fr
    German,           // de
    Italian,          // it
    Portuguese,       // pt
    Dutch,            // nl
    Russian,          // ru
    ChineseSimplified,  // zh-CN
    ChineseTraditional, // zh-TW
    Japanese,         // ja
    Korean,           // ko
    Hindi,            // hi
    Arabic,           // ar
    Vietnamese,       // vi
    Turkish           // tr
}

/// <summary>
/// Extension configuration for initialization
/// </summary>
public interface IExtensionConfig
{
    Platform Platform { get; }
    Language Language { get; }
    bool EnableCloudSync { get; }
    bool EnableOfflineMode { get; }
    string? ApiBaseUrl { get; }
    string? ApiKey { get; }
}

/// <summary>
/// Extension context - provides access to APIs and storage
/// </summary>
public interface IExtensionContext
{
    // Platform detection
    Platform GetCurrentPlatform();
    string GetExtensionVersion();

    // Language management
    Language GetDetectedLanguage();
    Task<string> GetLocalizedStringAsync(string key, Language language);

    // Storage
    IExtensionStorage Storage { get; }

    // Network
    IExtensionHttpClient Http { get; }

    // Events
    IExtensionEventBus Events { get; }
}

/// <summary>
/// Cross-platform storage abstraction
/// Vscode: globalState + workspaceState
/// Browser: localStorage + IndexedDB
/// </summary>
public interface IExtensionStorage
{
    // Key-value storage (sync)
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task DeleteAsync(string key);

    // Encrypted storage (for sensitive data)
    Task SetEncryptedAsync(string key, string value);
    Task<string?> GetEncryptedAsync(string key);

    // Large data (IndexedDB/application data)
    Task SetBlobAsync(string key, byte[] data);
    Task<byte[]?> GetBlobAsync(string key);
}

/// <summary>
/// Network client for extension
/// </summary>
public interface IExtensionHttpClient
{
    Task<T?> GetAsync<T>(string url);
    Task<T?> PostAsync<T>(string url, object? body = null);
    Task<T?> PutAsync<T>(string url, object? body);
    Task DeleteAsync(string url);

    // Raw response
    Task<(int statusCode, string body)> GetRawAsync(string url);
}

/// <summary>
/// Event bus for extension communication
/// </summary>
public interface IExtensionEventBus
{
    void Subscribe<T>(Action<T> handler) where T : IExtensionEvent;
    void Publish<T>(T @event) where T : IExtensionEvent;
    void Unsubscribe<T>(Action<T> handler) where T : IExtensionEvent;
}

public interface IExtensionEvent { }

/// <summary>
/// Extension service registry
/// </summary>
public class ExtensionServiceCollection
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly IExtensionConfig _config;

    public ExtensionServiceCollection(IExtensionConfig config)
    {
        _config = config;
    }

    public void AddSingleton<TInterface, TImplementation>(TImplementation instance)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _services[typeof(TInterface)] = instance;
    }

    public void AddScoped<TInterface, TImplementation>(Func<IExtensionServiceProvider, TImplementation> factory)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _services[typeof(TInterface)] = factory;
    }

    public IExtensionServiceProvider Build()
    {
        return new DefaultExtensionServiceProvider(_services);
    }
}

public interface IExtensionServiceProvider
{
    T GetService<T>() where T : class;
}

public class DefaultExtensionServiceProvider : IExtensionServiceProvider
{
    private readonly Dictionary<Type, object> _services;

    public DefaultExtensionServiceProvider(Dictionary<Type, object> services)
    {
        _services = services;
    }

    public T GetService<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            if (service is Delegate factory)
            {
                return (T)factory.DynamicInvoke(this)!;
            }
            return (T)service;
        }

        throw new InvalidOperationException($"Service {type.Name} not registered");
    }
}

/// <summary>
/// Career engine tailored for extensions  
/// Lightweight, offline-capable
/// </summary>
public interface IExtensionCareerEngine
{
    Task<JobMatchResult> AnalyzeJobAsync(string jobDescription);
    Task<ResumeAnalysisResult> AnalyzeResumeAsync(string resumeText);
    Task<SkillProfileResult> GetSkillProfileAsync();
    Task SyncWithCloudAsync();
}

public class JobMatchResult
{
    public int MatchPercentage { get; set; }
    public List<string> MatchingSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
}

public class ResumeAnalysisResult
{
    public List<string> ExtractedSkills { get; set; } = new();
    public string Role { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
}

public class SkillProfileResult
{
    public List<(string skill, float proficiency)> Skills { get; set; } = new();
    public float AverageProficiency { get; set; }
}
