using HireKarlo.Application.DTOs.CareerOS;

namespace HireKarlo.Application.Interfaces.Services;

/// <summary>
/// Resume parsing service - extracts structured data from resume files
/// Phase 2A: Will integrate with AI/ML service for resume parsing
/// </summary>
public interface IResumeParsingService
{
    /// <summary>
    /// Parse a resume file and extract structured data
    /// </summary>
    Task<ResumeAnalysis> ParseResumeAsync(Stream resumeStream, string fileName);
}

/// <summary>
/// Notification service - sends notifications to users
/// Phase 2B: Will integrate with email/push notification service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a user
    /// </summary>
    Task SendNotificationAsync(Guid userId, string subject, string message);

    /// <summary>
    /// Send email notification
    /// </summary>
    Task SendEmailAsync(string email, string subject, string message);

    /// <summary>
    /// Send push notification
    /// </summary>
    Task SendPushNotificationAsync(Guid userId, string title, string message);
}
