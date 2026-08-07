using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs scheduled job application automation tasks
/// Runs at fixed times: 6:00 AM for resume upload, 12:00 PM for applications
/// </summary>
public class JobApplicationAutomationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobApplicationAutomationBackgroundService> _logger;
    private Timer? _timer;

    // Schedule times (UTC)
    private static readonly TimeSpan ResumeUploadTime = new(6, 0, 0);      // 6:00 AM
    private static readonly TimeSpan ApplicationTime = new(12, 0, 0);      // 12:00 PM

    public JobApplicationAutomationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<JobApplicationAutomationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Application Automation Background Service started");

        // Calculate initial delay to next scheduled time
        var delay = CalculateDelayToNextRun();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait until next scheduled time
                await Task.Delay(delay, stoppingToken);

                // Execute the appropriate scheduled task
                await ExecuteScheduledTasksAsync(stoppingToken);

                // Recalculate delay for next run (24 hours from current execution)
                delay = TimeSpan.FromDays(1);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Job Application Automation Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automation background service");
                // Wait 5 minutes before retrying on error
                delay = TimeSpan.FromMinutes(5);
            }
        }
    }

    private async Task ExecuteScheduledTasksAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.TimeOfDay;

        // Check if we're near resume upload time (within 5 minute window)
        if (IsNearTime(now, ResumeUploadTime, TimeSpan.FromMinutes(5)))
        {
            _logger.LogInformation("Executing scheduled resume upload automation");
            await ExecuteResumeUploadAutomationAsync(cancellationToken);
        }

        // Check if we're near application time (within 5 minute window)
        if (IsNearTime(now, ApplicationTime, TimeSpan.FromMinutes(5)))
        {
            _logger.LogInformation("Executing scheduled job application automation");
            await ExecuteApplicationAutomationAsync(cancellationToken);
        }
    }

    private async Task ExecuteResumeUploadAutomationAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var automationService = scope.ServiceProvider.GetRequiredService<IJobApplicationAutomationService>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                // Get all users with automation enabled
                var allUsers = await userRepository.GetAllAsync(cancellationToken);
                var enabledUsers = allUsers.Where(u => u.AutomationEnabled).ToList();

                foreach (var user in enabledUsers)
                {
                    try
                    {
                        var success = await automationService.ExecuteAutomatedResumeUploadAsync(user.Id, cancellationToken);
                        if (success)
                        {
                            _logger.LogInformation("Resume upload automation completed for user {UserId}", user.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Resume upload automation failed for user {UserId}", user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing resume upload automation for user {UserId}", user.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled resume upload automation");
            }
        }
    }

    private async Task ExecuteApplicationAutomationAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var automationService = scope.ServiceProvider.GetRequiredService<IJobApplicationAutomationService>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                // Get all users with automation enabled
                var allUsers = await userRepository.GetAllAsync(cancellationToken);
                var enabledUsers = allUsers.Where(u => u.AutomationEnabled).ToList();

                foreach (var user in enabledUsers)
                {
                    try
                    {
                        var result = await automationService.ExecuteAutomatedApplicationsAsync(user.Id, cancellationToken);
                        if (result.Success)
                        {
                            _logger.LogInformation("Job application automation completed for user {UserId}. Applied to {Count} jobs",
                                user.Id, result.ApplicationsSubmitted);
                        }
                        else
                        {
                            _logger.LogWarning("Job application automation failed for user {UserId}: {Message}",
                                user.Id, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing job application automation for user {UserId}", user.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled job application automation");
            }
        }
    }

    private TimeSpan CalculateDelayToNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRunTime = GetNextScheduledTime(now);
        return nextRunTime - now;
    }

    private DateTime GetNextScheduledTime(DateTime now)
    {
        var times = new[] { ResumeUploadTime, ApplicationTime };
        var nextTime = times
            .Where(t => t > now.TimeOfDay)
            .OrderBy(t => t)
            .FirstOrDefault();

        if (nextTime != TimeSpan.Zero)
        {
            return now.Date.Add(nextTime);
        }

        // If no more scheduled times today, schedule for tomorrow's first time
        return now.Date.AddDays(1).Add(ResumeUploadTime);
    }

    private bool IsNearTime(TimeSpan current, TimeSpan target, TimeSpan window)
    {
        var diff = current - target;
        return diff >= TimeSpan.Zero && diff <= window;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Application Automation Background Service stopping");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
