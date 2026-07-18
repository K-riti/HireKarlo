using HireKarlo.Application.Features.Applications.Commands;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Enums;
using MediatR;

namespace HireKarlo.Application.Features.Applications.Handlers;

public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Guid>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IResumeRepository _resumeRepository;

    public CreateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IJobListingRepository jobListingRepository,
        IResumeRepository resumeRepository)
    {
        _applicationRepository = applicationRepository;
        _jobListingRepository = jobListingRepository;
        _resumeRepository = resumeRepository;
    }

    public async Task<Guid> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        var jobListing = await _jobListingRepository.GetByIdAsync(request.JobListingId, cancellationToken)
            ?? throw new ArgumentException($"Job listing {request.JobListingId} not found");

        var application = new Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            JobListingId = request.JobListingId,
            ResumeId = request.ResumeId,
            Stage = request.InitialStage,
            Notes = request.Notes ?? string.Empty,
            AppliedDate = request.InitialStage == ApplicationStage.Applied ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        return application.Id;
    }
}

public class UpdateApplicationStageCommandHandler : IRequestHandler<UpdateApplicationStageCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;

    public UpdateApplicationStageCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<bool> Handle(UpdateApplicationStageCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application == null)
            return false;

        var previousStage = application.Stage;
        application.Stage = request.NewStage;

        if (!string.IsNullOrEmpty(request.Notes))
            application.Notes = request.Notes;

        // Track when actually applied
        if (previousStage != ApplicationStage.Applied && request.NewStage == ApplicationStage.Applied)
            application.AppliedDate = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(application, cancellationToken);
        return true;
    }
}

/// <summary>
/// Generates a draft application including tailored cover letter and application message.
/// This is the "auto-draft" feature - drafts for human review, never auto-submits.
/// </summary>
public class DraftApplicationCommandHandler : IRequestHandler<DraftApplicationCommand, DraftApplicationResult>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IOpenAIService _openAIService;
    private readonly IAtsScorer _atsScorer;

    public DraftApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IJobListingRepository jobListingRepository,
        IResumeRepository resumeRepository,
        IOpenAIService openAIService,
        IAtsScorer atsScorer)
    {
        _applicationRepository = applicationRepository;
        _jobListingRepository = jobListingRepository;
        _resumeRepository = resumeRepository;
        _openAIService = openAIService;
        _atsScorer = atsScorer;
    }

    public async Task<DraftApplicationResult> Handle(DraftApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new ArgumentException($"Application {request.ApplicationId} not found");

        var jobListing = await _jobListingRepository.GetByIdAsync(application.JobListingId, cancellationToken)
            ?? throw new ArgumentException($"Job listing {application.JobListingId} not found");

        var warnings = new List<string>();
        string? resumeContent = null;

        if (application.ResumeId.HasValue)
        {
            var resume = await _resumeRepository.GetByIdAsync(application.ResumeId.Value, cancellationToken);
            resumeContent = resume?.ParsedContent;
        }

        if (string.IsNullOrEmpty(resumeContent))
        {
            warnings.Add("No resume attached - draft generated without resume context. Attach a resume for better results.");
        }

        // Generate application message (for application forms)
        var applicationMessage = await GenerateApplicationMessageAsync(jobListing, resumeContent, cancellationToken);

        // Generate cover letter if requested
        string? coverLetter = null;
        if (request.IncludeCoverLetter)
        {
            coverLetter = await GenerateCoverLetterAsync(jobListing, resumeContent, cancellationToken);
        }

        // Calculate ATS score if resume available
        var atsScore = 0;
        if (!string.IsNullOrEmpty(resumeContent) && !string.IsNullOrEmpty(jobListing.Description))
        {
            var atsResult = await _atsScorer.AnalyzeAsync(resumeContent, jobListing.Description, jobListing.Title, cancellationToken);
            atsScore = atsResult.Score.OverallScore;

            if (atsScore < 60)
                warnings.Add($"ATS score is {atsScore}% - consider tailoring your resume before applying.");
        }

        // Store draft in application notes for review
        application.DraftedCoverLetter = coverLetter;
        application.DraftedMessage = applicationMessage;
        application.DraftGeneratedAt = DateTime.UtcNow;
        await _applicationRepository.UpdateAsync(application, cancellationToken);

        return new DraftApplicationResult
        {
            CoverLetter = coverLetter,
            ApplicationMessage = applicationMessage,
            AtsScore = atsScore,
            Warnings = warnings
        };
    }

    private async Task<string> GenerateApplicationMessageAsync(
        Domain.Entities.JobListing job, 
        string? resumeContent, 
        CancellationToken cancellationToken)
    {
        var prompt = $@"Generate a professional, concise application message for this job. 
This message is for application form 'Why do you want to work here?' or similar fields.

JOB DETAILS:
Title: {job.Title}
Company: {job.Company}
Description: {job.Description}

{(string.IsNullOrEmpty(resumeContent) ? "" : $"CANDIDATE RESUME:\n{resumeContent}")}

REQUIREMENTS:
- 150-250 words maximum
- Professional but personable tone
- Highlight 2-3 relevant qualifications
- Show genuine interest in the company/role
- No generic phrases like 'I am excited to apply'
- End with a forward-looking statement

Generate ONLY the message text, no headers or labels.";

        return await _openAIService.CompleteAsync(prompt, new CompletionOptions { Temperature = 0.7 }, cancellationToken);
    }

    private async Task<string> GenerateCoverLetterAsync(
        Domain.Entities.JobListing job, 
        string? resumeContent, 
        CancellationToken cancellationToken)
    {
        var prompt = $@"Generate a professional cover letter for this job application.

JOB DETAILS:
Title: {job.Title}
Company: {job.Company}
Location: {job.Location}
Description: {job.Description}

{(string.IsNullOrEmpty(resumeContent) ? "" : $"CANDIDATE RESUME:\n{resumeContent}")}

REQUIREMENTS:
- Professional business letter format
- 3-4 paragraphs
- Opening: Hook + specific interest in this role
- Body: 2-3 concrete examples matching job requirements
- Closing: Call to action + enthusiasm
- No placeholder text like [Your Name] - leave those parts out
- Match keywords from the job description naturally
- Confident but not arrogant tone

Generate ONLY the cover letter body (no addresses/headers).";

        return await _openAIService.CompleteAsync(prompt, new CompletionOptions { Temperature = 0.7 }, cancellationToken);
    }
}

public class AddReferralToApplicationCommandHandler : IRequestHandler<AddReferralToApplicationCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IContactRepository _contactRepository;

    public AddReferralToApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IContactRepository contactRepository)
    {
        _applicationRepository = applicationRepository;
        _contactRepository = contactRepository;
    }

    public async Task<bool> Handle(AddReferralToApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application == null)
            return false;

        var contact = await _contactRepository.GetByIdAsync(request.ContactId, cancellationToken);
        if (contact == null)
            return false;

        application.ReferredById = request.ContactId;
        await _applicationRepository.UpdateAsync(application, cancellationToken);
        return true;
    }
}
