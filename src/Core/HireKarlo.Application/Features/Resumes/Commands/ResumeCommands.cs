using MediatR;

namespace HireKarlo.Application.Features.Resumes.Commands;

public record UploadResumeCommand : IRequest<UploadResumeResult>
{
    public Guid UserId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public bool SetAsMaster { get; init; }
}

public record UploadResumeResult
{
    public Guid ResumeId { get; init; }
    public string BlobUrl { get; init; } = string.Empty;
    public bool ParsedSuccessfully { get; init; }
    public string? ParseError { get; init; }
}

public record TailorResumeCommand : IRequest<TailorResumeResult>
{
    public Guid UserId { get; init; }
    public Guid ResumeId { get; init; }
    public Guid JobListingId { get; init; }
    public bool RewriteSummary { get; init; } = true;
    public bool ReorderSkills { get; init; } = true;
    public bool EnhanceBullets { get; init; } = true;
}

public record TailorResumeResult
{
    public Guid TailoredResumeId { get; init; }
    public string BlobUrl { get; init; } = string.Empty;
    public List<string> ChangesMade { get; init; } = new();
    public int OriginalAtsScore { get; init; }
    public int NewAtsScore { get; init; }
}

public record AnalyzeResumeCommand : IRequest<AnalyzeResumeResult>
{
    public Guid ResumeId { get; init; }
    public Guid? JobListingId { get; init; } // Optional: analyze against specific job
}

public record AnalyzeResumeResult
{
    public Guid ResumeId { get; init; }
    public Domain.ValueObjects.AtsReport? AtsReport { get; init; }
    public Domain.ValueObjects.MatchReport? MatchReport { get; init; }
    public List<string> GeneralRecommendations { get; init; } = new();
}
