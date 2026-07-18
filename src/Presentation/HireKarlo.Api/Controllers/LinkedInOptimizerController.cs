using HireKarlo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LinkedInOptimizerController : ControllerBase
{
    private readonly ILinkedInOptimizerService _linkedInService;
    private readonly ILogger<LinkedInOptimizerController> _logger;

    public LinkedInOptimizerController(
        ILinkedInOptimizerService linkedInService,
        ILogger<LinkedInOptimizerController> logger)
    {
        _linkedInService = linkedInService;
        _logger = logger;
    }

    /// <summary>
    /// Optimize entire LinkedIn profile
    /// </summary>
    [HttpPost("optimize")]
    public async Task<ActionResult<ProfileOptimizationResponse>> OptimizeProfile(
        [FromBody] OptimizeProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = new LinkedInProfileInput
        {
            Headline = request.Headline,
            About = request.About,
            Experiences = request.Experiences?.Select(e => new LinkedInExperience
            {
                Title = e.Title,
                Company = e.Company,
                Description = e.Description
            }).ToList() ?? new List<LinkedInExperience>(),
            Skills = request.Skills ?? new List<string>()
        };

        var options = new OptimizationOptions
        {
            TargetRoles = request.TargetRoles ?? new List<string>(),
            TargetKeywords = request.TargetKeywords ?? new List<string>(),
            Tone = request.Tone ?? "Professional",
            IncludeMetrics = request.IncludeMetrics
        };

        var result = await _linkedInService.OptimizeProfileAsync(input, options, cancellationToken);

        return Ok(new ProfileOptimizationResponse
        {
            CurrentScore = result.CurrentScore,
            ProjectedScore = result.ProjectedScore,
            ScoreImprovement = result.ProjectedScore - result.CurrentScore,
            Headline = new HeadlineResponse
            {
                Original = result.Headline.Original,
                Suggestions = result.Headline.Suggestions,
                KeywordsToInclude = result.Headline.KeywordsToInclude,
                KeywordScore = result.Headline.KeywordScore
            },
            About = new AboutResponse
            {
                Original = result.About.Original,
                Optimized = result.About.Optimized,
                AddedKeywords = result.About.AddedKeywords,
                KeywordDensityBefore = result.About.KeywordDensityBefore,
                KeywordDensityAfter = result.About.KeywordDensityAfter,
                ImprovementNotes = result.About.ImprovementNotes
            },
            Experiences = result.Experiences.Select(e => new ExperienceResponse
            {
                OriginalTitle = e.Title,
                OriginalCompany = e.Company,
                OptimizedDescription = e.OptimizedDescription,
                SuggestedBullets = e.SuggestedBullets
            }).ToList(),
            MissingKeywords = result.MissingKeywords,
            GeneralRecommendations = result.GeneralRecommendations
        });
    }

    /// <summary>
    /// Optimize just the headline
    /// </summary>
    [HttpPost("headline")]
    public async Task<ActionResult<HeadlineResponse>> OptimizeHeadline(
        [FromBody] OptimizeHeadlineRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _linkedInService.OptimizeHeadlineAsync(
            request.CurrentHeadline ?? "",
            request.TargetRoles ?? new List<string>(),
            cancellationToken);

        return Ok(new HeadlineResponse
        {
            Original = result.Original,
            Suggestions = result.Suggestions,
            KeywordsToInclude = result.KeywordsToInclude,
            KeywordScore = result.KeywordScore
        });
    }

    /// <summary>
    /// Optimize just the About section
    /// </summary>
    [HttpPost("about")]
    public async Task<ActionResult<AboutResponse>> OptimizeAbout(
        [FromBody] OptimizeAboutRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _linkedInService.OptimizeAboutAsync(
            request.CurrentAbout ?? "",
            request.TargetRoles ?? new List<string>(),
            request.TargetKeywords ?? new List<string>(),
            cancellationToken);

        return Ok(new AboutResponse
        {
            Original = result.Original,
            Optimized = result.Optimized,
            AddedKeywords = result.AddedKeywords,
            KeywordDensityBefore = result.KeywordDensityBefore,
            KeywordDensityAfter = result.KeywordDensityAfter,
            ImprovementNotes = result.ImprovementNotes
        });
    }
}

// Request/Response DTOs
public record OptimizeProfileRequest
{
    public string? Headline { get; init; }
    public string? About { get; init; }
    public List<ExperienceInput>? Experiences { get; init; }
    public List<string>? Skills { get; init; }
    public List<string>? TargetRoles { get; init; }
    public List<string>? TargetKeywords { get; init; }
    public string? Tone { get; init; } = "Professional";
    public bool IncludeMetrics { get; init; } = true;
}

public record ExperienceInput
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record OptimizeHeadlineRequest
{
    public string? CurrentHeadline { get; init; }
    public List<string>? TargetRoles { get; init; }
}

public record OptimizeAboutRequest
{
    public string? CurrentAbout { get; init; }
    public List<string>? TargetRoles { get; init; }
    public List<string>? TargetKeywords { get; init; }
}

public record ProfileOptimizationResponse
{
    public int CurrentScore { get; init; }
    public int ProjectedScore { get; init; }
    public int ScoreImprovement { get; init; }
    public HeadlineResponse Headline { get; init; } = null!;
    public AboutResponse About { get; init; } = null!;
    public List<ExperienceResponse> Experiences { get; init; } = new();
    public List<string> MissingKeywords { get; init; } = new();
    public List<string> GeneralRecommendations { get; init; } = new();
}

public record HeadlineResponse
{
    public string Original { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public List<string> KeywordsToInclude { get; init; } = new();
    public int KeywordScore { get; init; }
}

public record AboutResponse
{
    public string Original { get; init; } = string.Empty;
    public string Optimized { get; init; } = string.Empty;
    public List<string> AddedKeywords { get; init; } = new();
    public int KeywordDensityBefore { get; init; }
    public int KeywordDensityAfter { get; init; }
    public List<string> ImprovementNotes { get; init; } = new();
}

public record ExperienceResponse
{
    public string OriginalTitle { get; init; } = string.Empty;
    public string OriginalCompany { get; init; } = string.Empty;
    public string? OptimizedDescription { get; init; }
    public List<string> SuggestedBullets { get; init; } = new();
}
