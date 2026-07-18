using HireKarlo.Domain.Entities;

namespace HireKarlo.Application.Interfaces.Services;

public interface IResumeParser
{
    Task<ParsedResume> ParseAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<ParsedResume> ParseFromTextAsync(string resumeText, CancellationToken cancellationToken = default);
}

public interface IResumeGenerator
{
    Task<byte[]> GenerateTailoredResumeAsync(Resume baseResume, JobListing targetJob, TailoringOptions? options = null, CancellationToken cancellationToken = default);
}

public record ParsedResume
{
    public string RawText { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public List<string> Skills { get; init; } = new();
    public List<ExperienceEntry> Experience { get; init; } = new();
    public List<EducationEntry> Education { get; init; } = new();
    public List<string> Certifications { get; init; } = new();
    public List<ProjectEntry> Projects { get; init; } = new();
    public ContactInfo? Contact { get; init; }
}

public record ExperienceEntry
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public string? Description { get; init; }
    public List<string> Achievements { get; init; } = new();
    public List<string> Bullets { get; init; } = new();
}

public record EducationEntry
{
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public string? Field { get; init; }
    public string? Location { get; init; }
    public string? GraduationDate { get; init; }
    public string? GPA { get; init; }
}

public record ProjectEntry
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Technologies { get; init; } = new();
    public string? Url { get; init; }
}

public record ContactInfo
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? LinkedIn { get; init; }
    public string? GitHub { get; init; }
    public string? Portfolio { get; init; }
    public string? Location { get; init; }
}

public record TailoringOptions
{
    public bool RewriteSummary { get; init; } = true;
    public bool ReorderSkills { get; init; } = true;
    public bool EnhanceBullets { get; init; } = true;
    public bool AddMissingKeywords { get; init; } = true;
    public double KeywordDensityTarget { get; init; } = 0.7;
}
