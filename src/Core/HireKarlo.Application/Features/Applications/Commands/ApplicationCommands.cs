using HireKarlo.Domain.Enums;
using MediatR;

namespace HireKarlo.Application.Features.Applications.Commands;

public record CreateApplicationCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public Guid JobListingId { get; init; }
    public Guid? ResumeId { get; init; }
    public ApplicationStage InitialStage { get; init; } = ApplicationStage.Saved;
    public string? Notes { get; init; }
}

public record UpdateApplicationStageCommand : IRequest<bool>
{
    public Guid ApplicationId { get; init; }
    public ApplicationStage NewStage { get; init; }
    public string? Notes { get; init; }
}

public record DraftApplicationCommand : IRequest<DraftApplicationResult>
{
    public Guid ApplicationId { get; init; }
    public bool IncludeCoverLetter { get; init; } = true;
}

public record DraftApplicationResult
{
    public string? CoverLetter { get; init; }
    public string? ApplicationMessage { get; init; }
    public int AtsScore { get; init; }
    public List<string> Warnings { get; init; } = new();
}

public record AddReferralToApplicationCommand : IRequest<bool>
{
    public Guid ApplicationId { get; init; }
    public Guid ContactId { get; init; }
}
