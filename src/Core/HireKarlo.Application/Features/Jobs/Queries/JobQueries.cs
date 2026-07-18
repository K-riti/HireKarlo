using MediatR;

namespace HireKarlo.Application.Features.Jobs.Queries;

public record SearchJobsQuery : IRequest<JobSearchResult>
{
    public string? Query { get; init; }
    public string? Location { get; init; }
    public bool? RemoteOnly { get; init; }
    public bool? VisaSponsorship { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record GetJobByIdQuery : IRequest<JobDetailDto?>
{
    public Guid JobListingId { get; init; }
}

public record GetMatchedJobsQuery : IRequest<List<MatchedJobDto>>
{
    public Guid UserId { get; init; }
    public double MinScore { get; init; } = 80;
    public int Limit { get; init; } = 50;
}

public record JobSearchResult
{
    public List<JobDto> Jobs { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public record JobDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? CompanyLogoUrl { get; init; }
    public string? Location { get; init; }
    public bool IsRemote { get; init; }
    public string? SalaryRange { get; init; }
    public string? JobType { get; init; }
    public string? ExperienceLevel { get; init; }
    public bool SponsorsVisa { get; init; }
    public DateTime PostedDate { get; init; }
    public string Source { get; init; } = string.Empty;
}

public record JobDetailDto : JobDto
{
    public string? Description { get; init; }
    public string? Requirements { get; init; }
    public string? ApplyUrl { get; init; }
    public List<string> ExtractedSkills { get; init; } = new();
    public List<string> ExtractedKeywords { get; init; } = new();
}

public record MatchedJobDto : JobDto
{
    public double MatchScore { get; init; }
    public double SemanticScore { get; init; }
    public double KeywordScore { get; init; }
    public List<string> MatchingSkills { get; init; } = new();
    public List<string> MissingSkills { get; init; } = new();
    public string MatchStatus { get; init; } = string.Empty;
}
