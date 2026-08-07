// Language and Localization Support for HireKarlo
// Supports 16 languages with fallback strategy

namespace HireKarlo.Localization;

using System.Collections.Concurrent;
using System.Globalization;

/// <summary>
/// Manages all localization for HireKarlo platform
/// Supports 16 languages with auto-detection and fallback
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get localized string for key in specific language
    /// </summary>
    Task<string> GetStringAsync(string key, Language language = Language.Auto);

    /// <summary>
    /// Auto-detect language from multiple sources
    /// </summary>
    Task<Language> DetectLanguageAsync();

    /// <summary>
    /// Get all strings for a language (for initialization)
    /// </summary>
    Task<Dictionary<string, string>> GetAllStringsAsync(Language language);
}

public class LocalizationService : ILocalizationService
{
    private readonly ILocalizationProvider _provider;
    private readonly ILanguageDetector _detector;
    private readonly ConcurrentDictionary<string, Dictionary<Language, string>> _cache;
    private Language _currentLanguage = Language.English;

    public LocalizationService(ILocalizationProvider provider, ILanguageDetector detector)
    {
        _provider = provider;
        _detector = detector;
        _cache = new();
    }

    public async Task<string> GetStringAsync(string key, Language language = Language.Auto)
    {
        if (language == Language.Auto)
            language = _currentLanguage;

        // Check cache first
        if (_cache.TryGetValue(key, out var translations) && translations.TryGetValue(language, out var value))
            return value;

        // Load from provider with fallback chain
        var result = await _provider.GetStringAsync(key, language);
        if (string.IsNullOrEmpty(result) && language != Language.English)
            result = await _provider.GetStringAsync(key, Language.English);

        if (!string.IsNullOrEmpty(result))
        {
            _cache.AddOrUpdate(key, new Dictionary<Language, string> { { language, result } }, 
                (_, dict) => { dict[language] = result; return dict; });
        }

        return result ?? $"[{key}]";
    }

    public async Task<Language> DetectLanguageAsync()
    {
        _currentLanguage = await _detector.DetectAsync();
        return _currentLanguage;
    }

    public async Task<Dictionary<string, string>> GetAllStringsAsync(Language language)
    {
        return await _provider.GetAllStringsAsync(language);
    }
}

/// <summary>
/// Language detection from multiple sources
/// Priority: 1) Browser/OS locale 2) User previous choice 3) API header
/// </summary>
public interface ILanguageDetector
{
    Task<Language> DetectAsync();
}

public class LanguageDetector : ILanguageDetector
{
    private readonly IExtensionStorage? _storage;
    private readonly string? _httpAcceptLanguage;

    public LanguageDetector(IExtensionStorage? storage = null, string? httpAcceptLanguage = null)
    {
        _storage = storage;
        _httpAcceptLanguage = httpAcceptLanguage;
    }

    public async Task<Language> DetectAsync()
    {
        // 1. Check previous user choice (stored)
        if (_storage != null)
        {
            var savedLanguage = await _storage.GetAsync("user_language");
            if (!string.IsNullOrEmpty(savedLanguage) && Enum.TryParse<Language>(savedLanguage, out var lang))
                return lang;
        }

        // 2. Try HTTP Accept-Language header
        if (!string.IsNullOrEmpty(_httpAcceptLanguage))
        {
            var detected = ParseAcceptLanguageHeader(_httpAcceptLanguage);
            if (detected != Language.Auto)
                return detected;
        }

        // 3. Try system/browser locale
        var systemLocale = CultureInfo.CurrentCulture.Name;
        var detectedFromCulture = DetectFromCultureName(systemLocale);
        if (detectedFromCulture != Language.Auto)
            return detectedFromCulture;

        // 4. Default to English
        return Language.English;
    }

    private Language ParseAcceptLanguageHeader(string header)
    {
        // Example: "en-US,en;q=0.9,es;q=0.8"
        var parts = header.Split(',');
        foreach (var part in parts)
        {
            var langPart = part.Split(';')[0].Trim().ToLower();
            var language = DetectFromLanguageCode(langPart);
            if (language != Language.Auto)
                return language;
        }
        return Language.Auto;
    }

    private Language DetectFromCultureName(string cultureName)
    {
        // cultureName examples: "en-US", "es-ES", "zh-CN"
        var parts = cultureName.Split('-');
        if (parts.Length > 0)
            return DetectFromLanguageCode(parts[0]);
        return Language.Auto;
    }

    private Language DetectFromLanguageCode(string code)
    {
        return code.ToLower() switch
        {
            "en" => Language.English,
            "es" => Language.Spanish,
            "fr" => Language.French,
            "de" => Language.German,
            "it" => Language.Italian,
            "pt" => Language.Portuguese,
            "nl" => Language.Dutch,
            "ru" => Language.Russian,
            "zh" => Language.ChineseSimplified,
            "ja" => Language.Japanese,
            "ko" => Language.Korean,
            "hi" => Language.Hindi,
            "ar" => Language.Arabic,
            "vi" => Language.Vietnamese,
            "tr" => Language.Turkish,
            _ => Language.Auto
        };
    }
}

/// <summary>
/// Loads localization strings from files or database
/// </summary>
public interface ILocalizationProvider
{
    Task<string?> GetStringAsync(string key, Language language);
    Task<Dictionary<string, string>> GetAllStringsAsync(Language language);
}

public class JsonLocalizationProvider : ILocalizationProvider
{
    private readonly Dictionary<Language, Dictionary<string, string>> _resources;

    public JsonLocalizationProvider()
    {
        _resources = new();
        // Initialize with embedded resources
        InitializeResources();
    }

    private void InitializeResources()
    {
        // In production, load from JSON files in Resources/Localization/{language}.json
        // For now, seed with common keys in English
        _resources[Language.English] = new Dictionary<string, string>
        {
            ["ext.title"] = "HireKarlo",
            ["ext.description"] = "AI Career Copilot",
            ["job.analysis.title"] = "Job Analysis",
            ["job.match.percentage"] = "Match: {0}%",
            ["skills.missing"] = "Missing Skills",
            ["skills.have"] = "Have Skills",
            ["action.analyze"] = "Analyze",
            ["action.save"] = "Save",
            ["settings.language"] = "Language",
            ["settings.offline"] = "Offline Mode",
            ["error.network"] = "Network Error",
            ["error.invalid_resume"] = "Invalid Resume",
            ["success.analyzed"] = "Job analyzed successfully"
        };

        // Spanish
        _resources[Language.Spanish] = new Dictionary<string, string>
        {
            ["ext.title"] = "HireKarlo",
            ["ext.description"] = "Copiloto de Carrera IA",
            ["job.analysis.title"] = "Análisis de Trabajo",
            ["job.match.percentage"] = "Coincidencia: {0}%",
            ["skills.missing"] = "Habilidades Faltantes",
            ["skills.have"] = "Habilidades Disponibles"
        };

        // Add more languages similarly
        // This would be expanded to all 16 languages
    }

    public Task<string?> GetStringAsync(string key, Language language)
    {
        if (_resources.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value))
            return Task.FromResult<string?>(value);

        return Task.FromResult<string?>(null);
    }

    public Task<Dictionary<string, string>> GetAllStringsAsync(Language language)
    {
        if (_resources.TryGetValue(language, out var dict))
            return Task.FromResult(new Dictionary<string, string>(dict));

        return Task.FromResult(new Dictionary<string, string>());
    }
}

/// <summary>
/// Translation helper for UI strings
/// </summary>
public static class TranslationExtensions
{
    public static async Task<string> T(this ILocalizationService service, string key)
    {
        return await service.GetStringAsync(key);
    }

    public static async Task<string> T(this ILocalizationService service, string key, params object?[] args)
    {
        var template = await service.GetStringAsync(key);
        return string.Format(template, args);
    }
}

/// <summary>
/// Language-specific formatting (numbers, dates, currency)
/// </summary>
public class FormattingService
{
    private readonly Language _language;

    public FormattingService(Language language)
    {
        _language = language;
    }

    public string FormatNumber(decimal number)
    {
        var culture = GetCulture(_language);
        return number.ToString("N0", culture);
    }

    public string FormatCurrency(decimal amount)
    {
        var culture = GetCulture(_language);
        return amount.ToString("C", culture);
    }

    public string FormatDate(DateTime date)
    {
        var culture = GetCulture(_language);
        return date.ToString("d", culture);
    }

    private CultureInfo GetCulture(Language language) => language switch
    {
        Language.English => new CultureInfo("en-US"),
        Language.Spanish => new CultureInfo("es-ES"),
        Language.French => new CultureInfo("fr-FR"),
        Language.German => new CultureInfo("de-DE"),
        Language.Italian => new CultureInfo("it-IT"),
        Language.Portuguese => new CultureInfo("pt-PT"),
        Language.Dutch => new CultureInfo("nl-NL"),
        Language.Russian => new CultureInfo("ru-RU"),
        Language.ChineseSimplified => new CultureInfo("zh-CN"),
        Language.ChineseTraditional => new CultureInfo("zh-TW"),
        Language.Japanese => new CultureInfo("ja-JP"),
        Language.Korean => new CultureInfo("ko-KR"),
        Language.Hindi => new CultureInfo("hi-IN"),
        Language.Arabic => new CultureInfo("ar-SA"),
        Language.Vietnamese => new CultureInfo("vi-VN"),
        Language.Turkish => new CultureInfo("tr-TR"),
        _ => CultureInfo.InvariantCulture
    };
}
