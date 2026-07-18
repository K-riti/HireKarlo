using HireKarlo.Domain.Enums;
using MediatR;

namespace HireKarlo.Application.Features.Applications.Queries;

public record GetUserApplicationsQuery : IRequest<List<ApplicationDto>>
{
    public Guid UserId { get; init; }
    public ApplicationStage? FilterByStage { get; init; }
}

public record GetApplicationByIdQuery : IRequest<ApplicationDetailDto?>
{
    public Guid ApplicationId { get; init; }
}

public record GetApplicationsByStageQuery : IRequest<Dictionary<ApplicationStage, List<ApplicationDto>>>
{
    public Guid UserId { get; init; }
}

public record ApplicationDto
{
    public Guid Id { get; init; }
    public Guid JobListingId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? Location { get; init; }
    public ApplicationStage Stage { get; init; }
    public DateTime? AppliedDate { get; init; }
    public int? AtsScore { get; init; }
    public bool UsedReferral { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ApplicationDetailDto : ApplicationDto
{
    public string? Notes { get; init; }
    public string? CoverLetter { get; init; }
    public string? DraftedMessage { get; init; }
    public string? AtsReport { get; init; }
    public List<StageHistoryEntry> StageHistory { get; init; } = new();
    public ResumeDto? Resume { get; init; }
    public ContactDto? ReferralContact { get; init; }
}

public record StageHistoryEntry
{
    public ApplicationStage Stage { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Notes { get; init; }
}

public record ContactDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Company { get; init; }
}

public record ResumeDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public bool IsTailored { get; init; }
}
