using HireKarlo.Application.DTOs.CareerOS;
using HireKarlo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HireKarlo.Application.Services;

/// <summary>
/// Phase 2A Implementation: Resume Parsing Service
/// Extracts skills, experience, education from resume document
/// Currently supports text-based extraction; can be enhanced with AI
/// </summary>
public class ResumeParsingService : IResumeParsingService
{
    private readonly ILogger<ResumeParsingService> _logger;

    public ResumeParsingService(ILogger<ResumeParsingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse resume and extract structured information
    /// Phase 2A: Replace with actual PDF/DOCX parsing + AI extraction
    /// </summary>
    public async Task<ResumeAnalysis> ParseResumeAsync(Stream resumeStream, string fileName)
    {
        _logger.LogInformation("Parsing resume: {FileName}", fileName);

        try
        {
            // Read resume content
            using var reader = new StreamReader(resumeStream, Encoding.UTF8);
            var content = await reader.ReadToEndAsync();

            // Phase 2A: Use actual resume parsing library (e.g., iTextSharp for PDF, DocumentFormat.OpenXml for DOCX)
            // For now, do simple text extraction
            var analysis = new ResumeAnalysis
            {
                Skills = ExtractSkills(content),
                Experience = ExtractExperience(content),
                Education = ExtractEducation(content),
                CurrentRole = ExtractCurrentRole(content),
                YearsOfExperience = EstimateYearsOfExperience(content),
                Certifications = ExtractCertifications(content)
            };

            _logger.LogInformation(
                "Resume parsed: {SkillCount} skills, {ExperienceCount} experiences, {EducationCount} education, {YearsExperience} years",
                analysis.Skills?.Count ?? 0,
                analysis.Experience?.Count ?? 0,
                analysis.Education?.Count ?? 0,
                analysis.YearsOfExperience ?? 0);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing resume {FileName}", fileName);
            return new ResumeAnalysis
            {
                Skills = new List<string>(),
                Experience = new List<string>(),
                Education = new List<string>(),
                CurrentRole = "Unable to parse",
                YearsOfExperience = 0,
                Certifications = new List<string>()
            };
        }
    }

    // Private extraction methods

    private List<string> ExtractSkills(string content)
    {
        var skills = new List<string>();
        var commonSkillKeywords = new[]
        {
            // Programming
            "python", "java", "csharp", "c#", "rust", "go", "typescript", "javascript", "kotlin",
            // Cloud
            "aws", "azure", "gcp", "kubernetes", "docker",
            // Databases
            "postgresql", "mongodb", "dynamodb", "redis", "sql server",
            // Frontend
            "react", "angular", "vue", "blazor", "asp.net",
            // DevOps/Tools
            "git", "jenkins", "github actions", "ci/cd", "terraform", "ansible",
            // Soft Skills
            "leadership", "communication", "project management", "agile", "scrum"
        };

        var lower = content.ToLower();
        foreach (var skill in commonSkillKeywords)
        {
            if (lower.Contains(skill))
                skills.Add(skill);
        }

        return skills.Distinct().ToList();
    }

    private List<string> ExtractExperience(string content)
    {
        var experience = new List<string>();

        // Look for common job title patterns
        var jobTitles = new[]
        {
            "Software Engineer", "Senior Engineer", "Lead", "Manager",
            "Developer", "Architect", "DevOps", "SRE",
            "Data Engineer", "Solutions Architect"
        };

        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            foreach (var title in jobTitles)
            {
                if (line.Contains(title, StringComparison.OrdinalIgnoreCase))
                {
                    experience.Add(line.Trim());
                    break;
                }
            }
        }

        return experience.Take(5).ToList(); // Limit to 5 items
    }

    private List<string> ExtractEducation(string content)
    {
        var education = new List<string>();
        var degrees = new[] { "bachelor", "master", "phd", "diploma", "certification" };

        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            foreach (var degree in degrees)
            {
                if (line.Contains(degree, StringComparison.OrdinalIgnoreCase))
                {
                    education.Add(line.Trim());
                    break;
                }
            }
        }

        return education.Take(3).ToList();
    }

    private string ExtractCurrentRole(string content)
    {
        // Phase 2A: Use AI to identify current role
        // For now, look for common patterns
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (line.Contains("current", StringComparison.OrdinalIgnoreCase) ||
                lines.IndexOf(line) < 10) // Assume role is mentioned early
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 5 && trimmed.Length < 100)
                    return trimmed;
            }
        }

        return "Professional";
    }

    private int EstimateYearsOfExperience(string content)
    {
        // Look for year patterns like "2020-2023"
        var yearPattern = @"(\d{4})\s*[-–]\s*(\d{4}|Present|present)";
        var matches = System.Text.RegularExpressions.Regex.Matches(content, yearPattern);

        if (matches.Count == 0)
            return 0;

        int totalYears = 0;
        int currentYear = DateTime.Now.Year;

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int startYear))
            {
                int endYear = match.Groups[2].Value.Contains("Present", StringComparison.OrdinalIgnoreCase)
                    ? currentYear
                    : int.Parse(match.Groups[2].Value);

                totalYears += (endYear - startYear);
            }
        }

        return Math.Max(0, Math.Min(totalYears, 50)); // Cap at 50 years
    }

    private List<string> ExtractCertifications(string content)
    {
        var certifications = new List<string>();
        var certKeywords = new[]
        {
            "aws certified", "gcp certified", "azure certified",
            "kubernetes certification", "docker certification",
            "certified scrum", "pmp", "cissp"
        };

        var lower = content.ToLower();
        foreach (var cert in certKeywords)
        {
            if (lower.Contains(cert))
                certifications.Add(cert);
        }

        return certifications.Distinct().ToList();
    }
}
