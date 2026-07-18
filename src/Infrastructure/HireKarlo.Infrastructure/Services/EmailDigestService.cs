using HireKarlo.Application.Interfaces.External;
using HireKarlo.Application.Interfaces.Repositories;

namespace HireKarlo.Infrastructure.Services;

public class EmailDigestService : IEmailDigestService
{
    private readonly IUserRepository _userRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IJobListingRepository _jobRepository;
    private readonly IInterviewDigestEntryRepository _interviewDigestRepository;
    private readonly IEmailService _emailService;

    public EmailDigestService(
        IUserRepository userRepository,
        IMatchRepository matchRepository,
        IJobListingRepository jobRepository,
        IInterviewDigestEntryRepository interviewDigestRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _matchRepository = matchRepository;
        _jobRepository = jobRepository;
        _interviewDigestRepository = interviewDigestRepository;
        _emailService = emailService;
    }

    public async Task SendWeeklyDigestAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetNewsletterSubscribersAsync(cancellationToken);
        foreach (var user in users)
        {
            try
            {
                var digest = await BuildDigestForUserAsync(user.Id, cancellationToken);
                if (digest.NewMatches > 0 || digest.InterviewExperiences.Any())
                {
                    await _emailService.SendWeeklyDigestAsync(user.Email, user.DisplayName, digest, cancellationToken);
                }
            }
            catch { }
        }
    }

    public async Task<WeeklyDigestContent> BuildDigestForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var topMatches = await _matchRepository.GetTopMatchesForUserAsync(userId, 5, cancellationToken);
        var newJobs = await _jobRepository.GetRecentJobsAsync(7, 100, cancellationToken);
        var interviewEntries = await _interviewDigestRepository.GetUndigestedEntriesAsync(10, cancellationToken);
        var interviewDigest = interviewEntries.Select(e => new InterviewDigestSummary
        {
            Company = e.Company ?? "Unknown",
            Summary = e.LlmSummary ?? "Interview experience",
            SourceUrl = e.SourceUrl ?? ""
        }).ToList();
        var applicationsThisWeek = await _matchRepository.GetApplicationCountForWeekAsync(userId, cancellationToken);
        return new WeeklyDigestContent
        {
            NewMatches = topMatches.Count,
            ApplicationsThisWeek = applicationsThisWeek,
            InterviewExperiences = interviewDigest,
            RoadmapReminders = GetRoadmapReminders()
        };
    }

    public async Task SubscribeToNewsletterAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.SubscribedToNewsletter = true;
            user.Email = email;
            user.DisplayName = name;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }

    public async Task UnsubscribeFromNewsletterAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.SubscribedToNewsletter = false;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }

    private List<string> GetRoadmapReminders()
    {
        var reminders = new List<string>
        {
            "Tailor your resume for each application",
            "Follow up on applications within 5-7 days",
            "Practice STAR method for behavioral interviews",
            "Update your LinkedIn headline regularly",
            "Network before applying to target companies"
        };
        var random = new Random();
        return reminders.OrderBy(_ => random.Next()).Take(3).ToList();
    }
}

public interface IEmailDigestService
{
    Task SendWeeklyDigestAsync(CancellationToken cancellationToken = default);
    Task<WeeklyDigestContent> BuildDigestForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SubscribeToNewsletterAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default);
    Task UnsubscribeFromNewsletterAsync(Guid userId, CancellationToken cancellationToken = default);
}
