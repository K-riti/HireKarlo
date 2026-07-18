using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HireKarlo.JobIngestion.Functions;

public class JobIngestionFunction
{
    private readonly ILogger<JobIngestionFunction> _logger;

    public JobIngestionFunction(ILogger<JobIngestionFunction> logger)
    {
        _logger = logger;
    }

    [Function("DailyJobIngestion")]
    public async Task RunDailyIngestion([TimerTrigger("0 0 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Daily job ingestion started at: {Time}", DateTime.UtcNow);
        // TODO: Implement job fetching from APIs
        await Task.CompletedTask;
    }

    [Function("DreamCompanyJobTracking")]
    public async Task RunDreamCompanyTracking([TimerTrigger("0 0 */4 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Dream company job tracking started at: {Time}", DateTime.UtcNow);
        // TODO: Implement dream company job tracking
        await Task.CompletedTask;
    }
}