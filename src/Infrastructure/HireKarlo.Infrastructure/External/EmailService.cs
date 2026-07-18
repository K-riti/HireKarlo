using HireKarlo.Application.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.External;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendMatchAlertAsync(string toEmail, string userName, List<MatchAlertItem> matches, CancellationToken cancellationToken = default)
    {
        // In production, use SendGrid or similar
        _logger.LogInformation("Sending match alert to {Email} with {Count} matches", toEmail, matches.Count);
        await Task.CompletedTask;
    }

    public async Task SendWeeklyDigestAsync(string toEmail, string userName, WeeklyDigestContent content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending weekly digest to {Email}: {Matches} new matches, {Apps} applications", 
            toEmail, content.NewMatches, content.ApplicationsThisWeek);
        await Task.CompletedTask;
    }

    public async Task SendReferralDraftReadyAsync(string toEmail, string userName, string contactName, string company, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending referral draft ready notification to {Email} for {Contact} at {Company}", 
            toEmail, contactName, company);
        await Task.CompletedTask;
    }
}
