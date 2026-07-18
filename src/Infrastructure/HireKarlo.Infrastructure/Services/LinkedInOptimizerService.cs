using System.Text.Json;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.Services;

public class LinkedInOptimizerService : ILinkedInOptimizerService
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<LinkedInOptimizerService> _logger;

    public LinkedInOptimizerService(
        IOpenAIService openAIService,
        ILogger<LinkedInOptimizerService> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<ProfileOptimizationResult> OptimizeProfileAsync(
        LinkedInProfileInput input,
        OptimizationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Optimizing LinkedIn profile for roles: {Roles}", 
            string.Join(", ", options.TargetRoles));

        var headlineResult = await OptimizeHeadlineAsync(
            input.Headline ?? "", 
            options.TargetRoles, 
            cancellationToken);

        var aboutResult = await OptimizeAboutAsync(
            input.About ?? "",
            options.TargetRoles,
            options.TargetKeywords,
            cancellationToken);

        var experienceResults = new List<ExperienceOptimizationResult>();
        foreach (var exp in input.Experiences)
        {
            var expResult = await OptimizeExperienceAsync(exp, options, cancellationToken);
            experienceResults.Add(expResult);
        }

        var currentScore = CalculateProfileScore(input, options.TargetKeywords);
        var projectedScore = CalculateProjectedScore(
            headlineResult, aboutResult, experienceResults, options.TargetKeywords);

        var allCurrentText = input.Headline + " " + input.About + " " +
            string.Join(" ", input.Experiences.Select(e => e.Description)) +
            string.Join(" ", input.Skills);
        var missingKeywords = options.TargetKeywords
            .Where(k => !allCurrentText.Contains(k, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var recommendations = await GenerateRecommendationsAsync(input, options, cancellationToken);

        return new ProfileOptimizationResult
        {
            CurrentScore = currentScore,
            ProjectedScore = projectedScore,
            Headline = headlineResult,
            About = aboutResult,
            Experiences = experienceResults,
            MissingKeywords = missingKeywords,
            GeneralRecommendations = recommendations
        };
    }

    public async Task<HeadlineOptimizationResult> OptimizeHeadlineAsync(
        string currentHeadline,
        List<string> targetRoles,
        CancellationToken cancellationToken = default)
    {
        var rolesText = string.Join(", ", targetRoles);
        var prompt = "You are a LinkedIn optimization expert. Analyze and improve this headline for someone targeting these roles: " + rolesText + "\n\n" +
            "CURRENT HEADLINE: \"" + currentHeadline + "\"\n\n" +
            "Return a JSON object with:\n" +
            "{\n" +
            "    \"suggestions\": [\"headline option 1\", \"headline option 2\", \"headline option 3\"],\n" +
            "    \"keywordsToInclude\": [\"keyword1\", \"keyword2\"],\n" +
            "    \"keywordScore\": 50,\n" +
            "    \"analysis\": \"Brief explanation\"\n" +
            "}\n\n" +
            "Guidelines: Keep under 220 characters, include relevant job titles, add key skills.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<HeadlineOptimizationDto>(prompt, null, cancellationToken);

            return new HeadlineOptimizationResult
            {
                Original = currentHeadline,
                Suggestions = result?.Suggestions ?? new List<string>(),
                KeywordsToInclude = result?.KeywordsToInclude ?? new List<string>(),
                KeywordScore = result?.KeywordScore ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing headline");
            return new HeadlineOptimizationResult
            {
                Original = currentHeadline,
                Suggestions = new List<string> { GenerateDefaultHeadline(targetRoles) },
                KeywordsToInclude = targetRoles,
                KeywordScore = 0
            };
        }
    }

    public async Task<AboutOptimizationResult> OptimizeAboutAsync(
        string currentAbout,
        List<string> targetRoles,
        List<string> targetKeywords,
        CancellationToken cancellationToken = default)
    {
        var rolesText = string.Join(", ", targetRoles);
        var keywordsText = string.Join(", ", targetKeywords);
        var prompt = "You are a LinkedIn optimization expert. Rewrite this About section for someone targeting: " + rolesText + "\n\n" +
            "TARGET KEYWORDS: " + keywordsText + "\n\n" +
            "CURRENT ABOUT SECTION:\n\"" + currentAbout + "\"\n\n" +
            "Return a JSON object with:\n" +
            "{\n" +
            "    \"optimized\": \"The rewritten About section\",\n" +
            "    \"addedKeywords\": [\"keyword1\"],\n" +
            "    \"keywordDensityBefore\": 30,\n" +
            "    \"keywordDensityAfter\": 70,\n" +
            "    \"improvementNotes\": [\"note1\"]\n" +
            "}\n\n" +
            "Keep authentic voice, add keywords naturally, 200-400 words optimal.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<AboutOptimizationDto>(prompt, null, cancellationToken);

            return new AboutOptimizationResult
            {
                Original = currentAbout,
                Optimized = result?.Optimized ?? currentAbout,
                AddedKeywords = result?.AddedKeywords ?? new List<string>(),
                KeywordDensityBefore = result?.KeywordDensityBefore ?? CalculateKeywordDensity(currentAbout, targetKeywords),
                KeywordDensityAfter = result?.KeywordDensityAfter ?? 0,
                ImprovementNotes = result?.ImprovementNotes ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing about section");
            return new AboutOptimizationResult
            {
                Original = currentAbout,
                Optimized = currentAbout,
                KeywordDensityBefore = CalculateKeywordDensity(currentAbout, targetKeywords),
                KeywordDensityAfter = CalculateKeywordDensity(currentAbout, targetKeywords)
            };
        }
    }

    private async Task<ExperienceOptimizationResult> OptimizeExperienceAsync(
        LinkedInExperience experience,
        OptimizationOptions options,
        CancellationToken cancellationToken)
    {
        var rolesText = string.Join(", ", options.TargetRoles);
        var keywordsText = string.Join(", ", options.TargetKeywords);
        var prompt = "Optimize this LinkedIn experience entry for someone targeting: " + rolesText + "\n\n" +
            "TARGET KEYWORDS: " + keywordsText + "\n\n" +
            "CURRENT EXPERIENCE:\n" +
            "Title: " + experience.Title + "\n" +
            "Company: " + experience.Company + "\n" +
            "Description: \"" + (experience.Description ?? "") + "\"\n\n" +
            "Return a JSON object with:\n" +
            "{\n" +
            "    \"optimizedDescription\": \"Rewritten description with keywords and metrics\",\n" +
            "    \"suggestedBullets\": [\"Achievement with metrics\"],\n" +
            "    \"addedKeywords\": [\"keyword\"],\n" +
            "    \"improvementNotes\": [\"what was improved\"]\n" +
            "}";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<ExperienceOptimizationDto>(prompt, null, cancellationToken);

            return new ExperienceOptimizationResult
            {
                Title = experience.Title,
                Company = experience.Company,
                OriginalDescription = experience.Description,
                OptimizedDescription = result?.OptimizedDescription ?? experience.Description,
                SuggestedBullets = result?.SuggestedBullets ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error optimizing experience for {Company}", experience.Company);
            return new ExperienceOptimizationResult
            {
                Title = experience.Title,
                Company = experience.Company,
                OriginalDescription = experience.Description,
                OptimizedDescription = experience.Description
            };
        }
    }

    private async Task<List<string>> GenerateRecommendationsAsync(
        LinkedInProfileInput input,
        OptimizationOptions options,
        CancellationToken cancellationToken)
    {
        var rolesText = string.Join(", ", options.TargetRoles);
        var keywordsText = string.Join(", ", options.TargetKeywords);
        var skillsText = string.Join(", ", input.Skills.Take(10));
        var prompt = "Create specific recommendations for a LinkedIn profile targeting: " + rolesText + "\n\n" +
            "Profile Summary:\n" +
            "- Headline: " + input.Headline + "\n" +
            "- Skills: " + skillsText + "\n" +
            "- Experience Count: " + input.Experiences.Count + "\n\n" +
            "Target Keywords: " + keywordsText + "\n\n" +
            "Return a JSON array of 5-7 specific recommendations:\n" +
            "[\"recommendation 1\", \"recommendation 2\"]";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<List<string>>(prompt, null, cancellationToken);
            return result ?? GetDefaultRecommendations();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating recommendations");
            return GetDefaultRecommendations();
        }
    }

    private int CalculateProfileScore(LinkedInProfileInput input, List<string> targetKeywords)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(input.Headline))
        {
            score += 10;
            if (input.Headline.Length > 50) score += 5;
            if (targetKeywords.Any(k => input.Headline.Contains(k, StringComparison.OrdinalIgnoreCase)))
                score += 5;
        }

        if (!string.IsNullOrWhiteSpace(input.About))
        {
            score += 10;
            if (input.About.Length > 200) score += 5;
            var keywordsInAbout = targetKeywords.Count(k => 
                input.About.Contains(k, StringComparison.OrdinalIgnoreCase));
            score += Math.Min(10, keywordsInAbout * 2);
        }

        if (input.Experiences.Any())
        {
            score += 10;
            var expWithDescriptions = input.Experiences.Count(e => 
                !string.IsNullOrWhiteSpace(e.Description) && e.Description.Length > 100);
            score += Math.Min(15, expWithDescriptions * 5);
        }

        if (input.Skills.Any())
        {
            score += 5;
            var relevantSkills = input.Skills.Count(s => 
                targetKeywords.Any(k => s.Contains(k, StringComparison.OrdinalIgnoreCase)));
            score += Math.Min(10, relevantSkills * 2);
        }

        var allText = input.Headline + " " + input.About + " " +
            string.Join(" ", input.Experiences.Select(e => e.Description));
        var density = CalculateKeywordDensity(allText, targetKeywords);
        score += (int)(density * 0.15);

        return Math.Min(score, 100);
    }

    private int CalculateProjectedScore(
        HeadlineOptimizationResult headline,
        AboutOptimizationResult about,
        List<ExperienceOptimizationResult> experiences,
        List<string> targetKeywords)
    {
        var score = 0;

        if (headline.Suggestions.Any()) score += 20;

        score += Math.Min(25, about.KeywordDensityAfter / 4);

        var optimizedExpCount = experiences.Count(e => 
            !string.IsNullOrWhiteSpace(e.OptimizedDescription) && 
            e.OptimizedDescription != e.OriginalDescription);
        score += Math.Min(25, optimizedExpCount * 8);

        var totalAddedKeywords = headline.KeywordsToInclude.Count +
            about.AddedKeywords.Count;
        score += Math.Min(15, totalAddedKeywords * 2);

        return Math.Min(score + 15, 100);
    }

    private int CalculateKeywordDensity(string text, List<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(text) || !keywords.Any())
            return 0;

        var foundCount = keywords.Count(k => 
            text.Contains(k, StringComparison.OrdinalIgnoreCase));
        return (int)((double)foundCount / keywords.Count * 100);
    }

    private string GenerateDefaultHeadline(List<string> targetRoles)
    {
        if (targetRoles.Any())
        {
            return targetRoles.First() + " | Open to Opportunities";
        }
        return "Software Professional | Open to Opportunities";
    }

    private List<string> GetDefaultRecommendations()
    {
        return new List<string>
        {
            "Add a professional headshot - profiles with photos get 21x more views",
            "Add a custom banner image that reflects your professional brand",
            "Request recommendations from colleagues and managers",
            "Reorder your skills to put most relevant ones first",
            "Customize your LinkedIn URL for better personal branding",
            "Add certifications and courses to show continuous learning",
            "Engage regularly by posting and commenting to increase visibility"
        };
    }
}

internal class HeadlineOptimizationDto
{
    public List<string>? Suggestions { get; set; }
    public List<string>? KeywordsToInclude { get; set; }
    public int KeywordScore { get; set; }
    public string? Analysis { get; set; }
}

internal class AboutOptimizationDto
{
    public string? Optimized { get; set; }
    public List<string>? AddedKeywords { get; set; }
    public int KeywordDensityBefore { get; set; }
    public int KeywordDensityAfter { get; set; }
    public List<string>? ImprovementNotes { get; set; }
}

internal class ExperienceOptimizationDto
{
    public string? OptimizedDescription { get; set; }
    public List<string>? SuggestedBullets { get; set; }
    public List<string>? AddedKeywords { get; set; }
    public List<string>? ImprovementNotes { get; set; }
}
