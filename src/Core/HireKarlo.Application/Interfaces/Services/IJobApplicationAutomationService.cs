namespace HireKarlo.Application.Interfaces.Services;

/// <summary>
/// Service for automating job applications based on user preferences and match scores
/// </summary>
public interface IJobApplicationAutomationService
{
    /// <summary>
    /// Executes automated job applications for a user
    /// </summary>
    /// <param name="userId">User ID to run automation for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing number of applications submitted and details</returns>
    Task<AutomationRunResult> ExecuteAutomatedApplicationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates automation preferences for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="settings">New automation settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> UpdateAutomationSettingsAsync(
        Guid userId,
        AutomationSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current automation settings for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current automation settings</returns>
    Task<AutomationSettings?> GetAutomationSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes automated resume upload (marks latest resume as active for the day)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> ExecuteAutomatedResumeUploadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public class AutomationSettings
{
    public bool Enabled { get; set; } = false;
    public int DailyApplicationTarget { get; set; } = 5;
    public double MinimumMatchScore { get; set; } = 70.0;
    public bool AutoTailorResume { get; set; } = true;
    public Guid? PreferredResumeId { get; set; }
}

public class AutomationRunResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ApplicationsSubmitted { get; set; }
    public List<AutomationApplicationResult> Applications { get; set; } = new();
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
}

public class AutomationApplicationResult
{
    public Guid JobListingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public double MatchScore { get; set; }
    public bool Applied { get; set; }
    public string? Reason { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid? UsedResumeId { get; set; }
}
