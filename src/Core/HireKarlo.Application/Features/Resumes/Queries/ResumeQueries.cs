using MediatR;

namespace HireKarlo.Application.Features.Resumes.Queries;

public record GetUserResumesQuery : IRequest<List<ResumeDto>>
{
    public Guid UserId { get; init; }
    public bool IncludeTailored { get; init; } = true;
}

public record GetResumeByIdQuery : IRequest<ResumeDto?>
{
    public Guid ResumeId { get; init; }
}

public record ResumeDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string BlobUrl { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
    public bool IsMaster { get; init; }
    public int Version { get; init; }
    public Guid? TailoredForJobId { get; init; }
    public string? TailoredForJobTitle { get; init; }
    public DateTime CreatedAt { get; init; }
    public ParsedResumeDto? ParsedContent { get; init; }
}

public record ParsedResumeDto
{
    public string? Summary { get; init; }
    public List<string> Skills { get; init; } = new();
    public List<ExperienceDto> Experience { get; init; } = new();
    public List<EducationDto> Education { get; init; } = new();
}

public record ExperienceDto
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public List<string> Bullets { get; init; } = new();
}

public record EducationDto
{
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public string? GraduationDate { get; init; }
}
