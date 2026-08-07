using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HireKarlo.Infrastructure.Services;

public class JobApplicationAutomationService : IJobApplicationAutomationService
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly JobApplicationService _jobApplicationService;
    private readonly ILogger<JobApplicationAutomationService> _logger;

    public JobApplicationAutomationService(
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IJobListingRepository jobListingRepository,
        IResumeRepository resumeRepository,
        IMatchRepository matchRepository,
        JobApplicationService jobApplicationService,
        ILogger<JobApplicationAutomationService> logger)
    {
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _jobListingRepository = jobListingRepository;
        _resumeRepository = resumeRepository;
        _matchRepository = matchRepository;
        _jobApplicationService = jobApplicationService;
        _logger = logger;
    }

    public async Task<AutomationRunResult> ExecuteAutomatedApplicationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = new AutomationRunResult();

        try
        {
            _logger.LogInformation("Starting automated job applications for user {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not found";
                result.Error = "User not found";
                return result;
            }

            if (!user.AutomationEnabled)
            {
                result.Success = false;
                result.Message = "Automation is disabled for this user";
                return result;
            }

            // Get the resume to use
            var resumeId = user.PreferredResumeIdForAutomation;
            if (resumeId == Guid.Empty || resumeId == null)
            {
                // Find the most recent master resume
                var latestResume = await _resumeRepository.GetMasterResumeAsync(userId, cancellationToken);
                if (latestResume == null)
                {
                    result.Success = false;
                    result.Message = "No resume found for automation";
                    result.Error = "No suitable resume found";
                    return result;
                }
                resumeId = latestResume.Id;
            }

            // Find unprocessed jobs that match user's preferences
            var unprocessedJobs = await GetUnprocessedJobsAsync(userId, cancellationToken);
            _logger.LogInformation("Found {Count} unprocessed jobs for user {UserId}", unprocessedJobs.Count, userId);

            // Score and filter jobs
            var eligibleJobs = new List<(JobListing job, double score)>();
            foreach (var job in unprocessedJobs)
            {
                var matches = await _matchRepository.GetByUserIdAsync(userId, cancellationToken);
                var existingMatch = matches.FirstOrDefault(m => m.JobListingId == job.Id);

                if (existingMatch != null && existingMatch.OverallScore >= user.MinimumMatchScoreForAutomation)
                {
                    eligibleJobs.Add((job, existingMatch.OverallScore));
                }
            }

            // Sort by match score (highest first)
            var sortedJobs = eligibleJobs.OrderByDescending(x => x.score).ToList();
            _logger.LogInformation("Found {Count} eligible jobs with score >= {MinScore}", 
                sortedJobs.Count, user.MinimumMatchScoreForAutomation);

            // Apply to top N jobs
            var applicationsTarget = user.DailyApplicationTarget;
            var applicationsAttempted = 0;

            foreach (var (job, matchScore) in sortedJobs.Take(applicationsTarget))
            {
                if (applicationsAttempted >= applicationsTarget)
                    break;

                try
                {
                    // Check if already applied
                    var userApplications = await _applicationRepository.GetByUserIdAsync(userId, cancellationToken);
                    var existingApplication = userApplications.FirstOrDefault(a => a.JobListingId == job.Id);

                    if (existingApplication != null)
                    {
                        _logger.LogInformation("User {UserId} already applied to job {JobId}", userId, job.Id);
                        result.Applications.Add(new AutomationApplicationResult
                        {
                            JobListingId = job.Id,
                            JobTitle = job.Title,
                            Company = job.Company,
                            MatchScore = matchScore,
                            Applied = false,
                            Reason = "Already applied to this job"
                        });
                        continue;
                    }

                    // Tailor resume if enabled
                    Guid resumeToUse = resumeId.Value;
                    if (user.AutoTailorResume)
                    {
                        resumeToUse = await TailorResumeForJobAsync(userId, resumeId.Value, job, cancellationToken);
                    }

                    // Apply to job with force flag since we already scored >= 70%
                    var applicationResult = await _jobApplicationService.ApplyToJobAsync(
                        userId,
                        job.Id,
                        resumeToUse,
                        forceApply: true,
                        cancellationToken);

                    if (applicationResult.Success)
                    {
                        result.Applications.Add(new AutomationApplicationResult
                        {
                            JobListingId = job.Id,
                            JobTitle = job.Title,
                            Company = job.Company,
                            MatchScore = matchScore,
                            Applied = true,
                            ApplicationId = applicationResult.ApplicationId,
                            UsedResumeId = resumeToUse
                        });
                        applicationsAttempted++;
                        _logger.LogInformation("Successfully applied to job {JobId} ({Title}) for user {UserId}", 
                            job.Id, job.Title, userId);
                    }
                    else
                    {
                        result.Applications.Add(new AutomationApplicationResult
                        {
                            JobListingId = job.Id,
                            JobTitle = job.Title,
                            Company = job.Company,
                            MatchScore = matchScore,
                            Applied = false,
                            Reason = applicationResult.Error ?? "Application failed"
                        });
                        _logger.LogWarning("Failed to apply to job {JobId}: {Error}", job.Id, applicationResult.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying to job {JobId} for user {UserId}", job.Id, userId);
                    result.Applications.Add(new AutomationApplicationResult
                    {
                        JobListingId = job.Id,
                        JobTitle = job.Title,
                        Company = job.Company,
                        MatchScore = matchScore,
                        Applied = false,
                        Reason = $"Exception: {ex.Message}"
                    });
                }
            }

            // Update user's automation stats
            user.LastAutomationRunAt = DateTime.UtcNow;
            user.AutomationApplicationsThisMonth += applicationsAttempted;

            // Update automation history
            UpdateAutomationHistory(user, result);

            await _userRepository.UpdateAsync(user, cancellationToken);

            result.Success = true;
            result.ApplicationsSubmitted = applicationsAttempted;
            result.Message = $"Automation completed. Applied to {applicationsAttempted} jobs.";

            _logger.LogInformation("Automation completed for user {UserId}. Applied to {Count} jobs", 
                userId, applicationsAttempted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated job applications for user {UserId}", userId);
            result.Success = false;
            result.Message = "Automation failed with an error";
            result.Error = ex.Message;
        }

        return result;
    }

    public async Task<bool> ExecuteAutomatedResumeUploadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing automated resume upload for user {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // Get the most recent master resume
            var latestResume = await _resumeRepository.GetMasterResumeAsync(userId, cancellationToken);
            if (latestResume == null)
            {
                _logger.LogWarning("No master resume found for user {UserId}", userId);
                return false;
            }

            // Mark as the preferred resume for automation
            user.PreferredResumeIdForAutomation = latestResume.Id;
            user.LastAutomationRunAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Automated resume upload completed for user {UserId}. Resume ID: {ResumeId}", 
                userId, latestResume.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated resume upload for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateAutomationSettingsAsync(
        Guid userId,
        AutomationSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.AutomationEnabled = settings.Enabled;
            user.DailyApplicationTarget = settings.DailyApplicationTarget;
            user.MinimumMatchScoreForAutomation = settings.MinimumMatchScore;
            user.AutoTailorResume = settings.AutoTailorResume;
            user.PreferredResumeIdForAutomation = settings.PreferredResumeId;

            await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("Updated automation settings for user {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating automation settings for user {UserId}", userId);
            return false;
        }
    }

    public async Task<AutomationSettings?> GetAutomationSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return null;

            return new AutomationSettings
            {
                Enabled = user.AutomationEnabled,
                DailyApplicationTarget = user.DailyApplicationTarget,
                MinimumMatchScore = user.MinimumMatchScoreForAutomation,
                AutoTailorResume = user.AutoTailorResume,
                PreferredResumeId = user.PreferredResumeIdForAutomation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting automation settings for user {UserId}", userId);
            return null;
        }
    }

    private async Task<List<JobListing>> GetUnprocessedJobsAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Get all active jobs
        var allJobs = (await _jobListingRepository.GetActiveJobsAsync(cancellationToken)).ToList();

        // Filter out jobs user has already applied to
        var userApplications = await _applicationRepository.GetByUserIdAsync(userId, cancellationToken);
        var appliedJobIds = userApplications.Select(a => a.JobListingId).ToHashSet();

        // Get user's preferences for filtering
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var targetRole = user?.TargetRole ?? string.Empty;

        // Filter by role and location preferences
        var unprocessedJobs = allJobs
            .Where(j => !appliedJobIds.Contains(j.Id))
            .Where(j => string.IsNullOrEmpty(targetRole) || 
                       j.Title.Contains(targetRole, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return unprocessedJobs;
    }

    private async Task<Guid> TailorResumeForJobAsync(
        Guid userId,
        Guid baseResumeId,
        JobListing jobListing,
        CancellationToken cancellationToken)
    {
        try
        {
            // For now, return the base resume ID
            // In a full implementation, this would call a resume tailoring service
            // that uses AI to customize the resume for the specific job

            _logger.LogInformation("Tailoring resume for job {JobId}", jobListing.Id);

            // TODO: Implement actual resume tailoring using ResumeService or AI service
            // This would create a new resume variant tailored to the job description

            return baseResumeId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error tailoring resume for job {JobId}, using base resume", jobListing.Id);
            return baseResumeId;
        }
    }

    private void UpdateAutomationHistory(User user, AutomationRunResult result)
    {
        try
        {
            var history = string.IsNullOrEmpty(user.AutomationHistory)
                ? new List<AutomationHistoryEntry>()
                : JsonSerializer.Deserialize<List<AutomationHistoryEntry>>(user.AutomationHistory) ?? new List<AutomationHistoryEntry>();

            history.Add(new AutomationHistoryEntry
            {
                ExecutedAt = result.ExecutedAt,
                ApplicationsSubmitted = result.ApplicationsSubmitted,
                Success = result.Success,
                Message = result.Message
            });

            // Keep only last 30 runs
            if (history.Count > 30)
                history = history.TakeLast(30).ToList();

            user.AutomationHistory = JsonSerializer.Serialize(history);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating automation history");
        }
    }
}

public class AutomationHistoryEntry
{
    public DateTime ExecutedAt { get; set; }
    public int ApplicationsSubmitted { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
