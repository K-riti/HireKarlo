using System.Text.RegularExpressions;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HireKarlo.AtsEngine;

public class AtsScorer : IAtsScorer
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<AtsScorer> _logger;

    private static readonly string[] RequiredSections = 
        { "summary", "experience", "education", "skills" };

    private static readonly string[] FormattingRedFlags = 
        { "table", "text box", "column", "header", "footer", "image" };

    public AtsScorer(IOpenAIService openAIService, ILogger<AtsScorer> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<AtsReport> AnalyzeAsync(
        string resumeText, 
        string jobDescription, 
        string jobTitle, 
        CancellationToken cancellationToken = default)
    {
        // Extract keywords from JD using LLM
        var keywords = await ExtractKeywordsAsync(jobDescription, cancellationToken);

        // Run all analyses in parallel
        var titleMatchTask = AnalyzeTitleMatchAsync(
            ExtractResumeTitle(resumeText), jobTitle, cancellationToken);
        var keywordScoreTask = CalculateKeywordDensityScoreAsync(
            resumeText, keywords, cancellationToken);
        var formattingTask = DetectFormattingIssuesAsync(resumeText, cancellationToken);
        var sectionsTask = DetectMissingSectionsAsync(resumeText, cancellationToken);
        var dateTask = ValidateDateFormatsAsync(resumeText, cancellationToken);

        await Task.WhenAll(titleMatchTask, keywordScoreTask, formattingTask, sectionsTask, dateTask);

        var titleMatch = await titleMatchTask;
        var keywordScore = await keywordScoreTask;
        var formattingIssues = await formattingTask;
        var missingSections = await sectionsTask;
        var dateIssues = await dateTask;

        // Calculate section score
        var sectionScore = CalculateSectionScore(missingSections);

        // Calculate formatting score
        var formattingScore = Math.Max(0, 100 - (formattingIssues.Count * 15));

        // Calculate date format score
        var dateScore = Math.Max(0, 100 - (dateIssues.Count * 20));

        var score = AtsScore.Create(
            (int)(titleMatch.Similarity * 100),
            keywordScore,
            sectionScore,
            formattingScore,
            dateScore);

        // Generate recommendations
        var recommendations = GenerateRecommendations(
            titleMatch, keywords, formattingIssues, missingSections, dateIssues, resumeText);

        // Find matching and missing keywords
        var matchingKeywords = FindMatchingKeywords(resumeText.ToLower(), keywords);
        var missingKeywords = keywords.Except(matchingKeywords, StringComparer.OrdinalIgnoreCase).ToList();

        return new AtsReport
        {
            Score = score,
            MissingKeywords = missingKeywords,
            MatchingKeywords = matchingKeywords,
            FormattingIssues = formattingIssues,
            MissingSections = missingSections,
            Recommendations = recommendations,
            TitleMatch = titleMatch,
            DateIssues = dateIssues
        };
    }

    public async Task<int> CalculateKeywordDensityScoreAsync(
        string resumeText, 
        IEnumerable<string> requiredKeywords, 
        CancellationToken cancellationToken = default)
    {
        var resumeLower = resumeText.ToLower();
        var keywordList = requiredKeywords.ToList();

        if (!keywordList.Any()) return 50;

        var matchedCount = keywordList.Count(keyword => 
            resumeLower.Contains(keyword.ToLower()));

        var matchPercentage = (double)matchedCount / keywordList.Count;

        // Weight by keyword importance (could be enhanced with TF-IDF)
        return (int)(matchPercentage * 100);
    }

    public async Task<TitleMatchResult> AnalyzeTitleMatchAsync(
        string resumeTitle, 
        string jobTitle, 
        CancellationToken cancellationToken = default)
    {
        var normalizedResumeTitle = NormalizeTitle(resumeTitle);
        var normalizedJobTitle = NormalizeTitle(jobTitle);

        var isExactMatch = string.Equals(
            normalizedResumeTitle, normalizedJobTitle, StringComparison.OrdinalIgnoreCase);

        var similarity = CalculateTitleSimilarity(normalizedResumeTitle, normalizedJobTitle);
        var isFuzzyMatch = similarity >= 0.7 && !isExactMatch;

        string? suggestedTitle = null;
        if (!isExactMatch && !isFuzzyMatch)
        {
            suggestedTitle = await GenerateSuggestedTitleAsync(
                resumeTitle, jobTitle, cancellationToken);
        }

        return new TitleMatchResult
        {
            ResumeTitle = resumeTitle,
            JobTitle = jobTitle,
            Similarity = similarity,
            IsExactMatch = isExactMatch,
            IsFuzzyMatch = isFuzzyMatch,
            SuggestedTitle = suggestedTitle
        };
    }

    public Task<List<string>> DetectFormattingIssuesAsync(
        string resumeText, 
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var textLower = resumeText.ToLower();

        // Check for problematic patterns
        if (Regex.IsMatch(resumeText, @"\t.*\t.*\t"))
            issues.Add("Multiple tab characters detected - may indicate table formatting");

        if (Regex.IsMatch(resumeText, @"[│┃┆┇┊┋]"))
            issues.Add("Unicode box-drawing characters detected - indicates visual columns");

        if (Regex.IsMatch(resumeText, @"[●○■□▪▫•‣⁃]"))
        {
            // This is actually okay for bullets, but check for consistency
            var bulletTypes = Regex.Matches(resumeText, @"[●○■□▪▫•‣⁃]")
                .Select(m => m.Value)
                .Distinct()
                .Count();
            if (bulletTypes > 2)
                issues.Add("Inconsistent bullet point characters - use standard bullets (• or -)");
        }

        if (Regex.IsMatch(resumeText, @"\s{4,}"))
            issues.Add("Excessive whitespace detected - may indicate column layout");

        // Check for common ATS-unfriendly patterns in the text
        if (textLower.Contains("text box"))
            issues.Add("Text boxes are not ATS-friendly - use standard paragraphs");

        return Task.FromResult(issues);
    }

    public Task<List<string>> DetectMissingSectionsAsync(
        string resumeText, 
        CancellationToken cancellationToken = default)
    {
        var textLower = resumeText.ToLower();
        var missing = new List<string>();

        var sectionPatterns = new Dictionary<string, string[]>
        {
            ["Summary"] = new[] { "summary", "objective", "profile", "about" },
            ["Experience"] = new[] { "experience", "employment", "work history", "professional experience" },
            ["Education"] = new[] { "education", "academic", "degree", "university", "college" },
            ["Skills"] = new[] { "skills", "technical skills", "competencies", "technologies" }
        };

        foreach (var section in sectionPatterns)
        {
            var found = section.Value.Any(pattern => 
                Regex.IsMatch(textLower, $@"\b{pattern}\b"));

            if (!found)
                missing.Add(section.Key);
        }

        return Task.FromResult(missing);
    }

    public Task<List<DateFormatIssue>> ValidateDateFormatsAsync(
        string resumeText, 
        CancellationToken cancellationToken = default)
    {
        var issues = new List<DateFormatIssue>();

        // Standard date formats: "Jan 2020", "January 2020", "01/2020", "2020"
        var standardDatePattern = @"\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+\d{4}\b|\b\d{1,2}/\d{4}\b|\b\d{4}\b";

        // Problematic formats
        var problematicPatterns = new[]
        {
            (@"\b\d{1,2}/\d{1,2}/\d{2,4}\b", "Full date with day is unnecessary - use 'Month Year' format"),
            (@"\b\d{4}\s*-\s*\d{4}\b", "Year ranges should use 'Month Year - Month Year' for clarity"),
            (@"\b(present|current|now)\b", ""),  // This is actually okay
        };

        // Find date-like patterns in experience section
        var experienceMatch = Regex.Match(resumeText, 
            @"(?:experience|employment).*?(?=education|skills|$)", 
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (experienceMatch.Success)
        {
            var experienceText = experienceMatch.Value;

            foreach (var (pattern, message) in problematicPatterns)
            {
                if (string.IsNullOrEmpty(message)) continue;

                var matches = Regex.Matches(experienceText, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    issues.Add(new DateFormatIssue
                    {
                        OriginalText = match.Value,
                        Location = "Experience section",
                        Issue = message,
                        SuggestedFormat = "Jan 2020 - Present"
                    });
                }
            }
        }

        return Task.FromResult(issues);
    }

    private async Task<List<string>> ExtractKeywordsAsync(
        string jobDescription, 
        CancellationToken cancellationToken)
    {
        var prompt = $@"Extract the most important keywords and skills from this job description. 
Include technical skills, tools, soft skills, and required qualifications.
Return as a JSON array of strings.

Job Description:
{jobDescription}";

        var response = await _openAIService.CompleteAsJsonAsync<List<string>>(
            prompt, 
            new CompletionOptions { Temperature = 0.2 },
            cancellationToken);

        return response ?? new List<string>();
    }

    private string ExtractResumeTitle(string resumeText)
    {
        // Try to find the title near the top of the resume
        var lines = resumeText.Split('\n').Take(10);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 5 && trimmed.Length < 100 && 
                !trimmed.Contains('@') && !trimmed.Contains("http"))
            {
                // Skip name line (usually first non-empty line)
                if (Regex.IsMatch(trimmed, @"^[A-Z][a-z]+\s+[A-Z][a-z]+$"))
                    continue;

                // Look for title patterns
                if (Regex.IsMatch(trimmed, @"(?:engineer|developer|manager|analyst|designer|consultant|specialist)", 
                    RegexOptions.IgnoreCase))
                {
                    return trimmed;
                }
            }
        }
        return string.Empty;
    }

    private static string NormalizeTitle(string title)
    {
        return Regex.Replace(title.ToLower(), @"[^a-z0-9\s]", "").Trim();
    }

    private static double CalculateTitleSimilarity(string title1, string title2)
    {
        var words1 = title1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = title2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (!words1.Any() || !words2.Any()) return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union; // Jaccard similarity
    }

    private async Task<string> GenerateSuggestedTitleAsync(
        string currentTitle, 
        string targetTitle,
        CancellationToken cancellationToken)
    {
        var prompt = $@"The resume has title ""{currentTitle}"" but the job title is ""{targetTitle}"".
Suggest a better resume title that aligns with the target while remaining honest.
Return only the suggested title, nothing else.";

        return await _openAIService.CompleteAsync(
            prompt, 
            new CompletionOptions { Temperature = 0.3, MaxTokens = 50 },
            cancellationToken);
    }

    private static int CalculateSectionScore(List<string> missingSections)
    {
        var deduction = missingSections.Count switch
        {
            0 => 0,
            1 => 15,
            2 => 35,
            3 => 60,
            _ => 80
        };
        return 100 - deduction;
    }

    private static List<string> FindMatchingKeywords(string resumeLower, List<string> keywords)
    {
        return keywords
            .Where(k => resumeLower.Contains(k.ToLower()))
            .ToList();
    }

    private static List<string> GenerateRecommendations(
        TitleMatchResult titleMatch,
        List<string> keywords,
        List<string> formattingIssues,
        List<string> missingSections,
        List<DateFormatIssue> dateIssues,
        string resumeText)
    {
        var recommendations = new List<string>();

        if (!titleMatch.IsExactMatch && !titleMatch.IsFuzzyMatch)
        {
            recommendations.Add($"Consider updating your title to better match '{titleMatch.JobTitle}'");
            if (!string.IsNullOrEmpty(titleMatch.SuggestedTitle))
                recommendations.Add($"Suggested title: {titleMatch.SuggestedTitle}");
        }

        if (missingSections.Any())
        {
            recommendations.Add($"Add clearly labeled sections for: {string.Join(", ", missingSections)}");
        }

        if (formattingIssues.Any())
        {
            recommendations.Add("Fix formatting issues to improve ATS parsing");
            recommendations.AddRange(formattingIssues.Take(3));
        }

        if (dateIssues.Any())
        {
            recommendations.Add("Standardize date formats to 'Month Year' (e.g., 'Jan 2020 - Present')");
        }

        return recommendations;
    }
}
