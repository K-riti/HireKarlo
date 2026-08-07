# Job Application Automation Feature

## Overview

The Job Application Automation feature automates the repetitive task of applying to jobs, inspired by the blog post about using Claude AI for job hunting automation. Instead of relying on external services, this automation is built directly into HireKarlo.

## How It Works

### Daily Workflow

The automation runs on a fixed schedule:

1. **6:00 AM UTC** - Resume Upload Automation
   - Marks the user's latest master resume as the preferred resume for that day
   - Ensures your profile is always "fresh"

2. **12:00 PM UTC** - Job Application Automation
   - Fetches new unscored jobs matching your profile
   - Filters jobs by match score (only applies if score >= configured threshold)
   - Auto-tailors resume to job description (optional)
   - Automatically applies to up to N jobs (default: 5)
   - Logs all automation runs for tracking

## Features

### ✅ Key Capabilities

- **Smart Matching**: Only applies to jobs with match score >= 70% (configurable)
- **Auto Tailoring**: Automatically tailors resume for each job application
- **Rate Limiting**: Conservative approach - applies to 5 jobs per day by default
- **Preference Respect**: Honors your target role and location preferences
- **Duplicate Prevention**: Won't apply twice to the same job
- **Detailed Logging**: Every automation run is logged with results
- **User Control**: Can enable/disable at any time via UI or API
- **Resume Selection**: Use preferred resume or auto-select latest master resume

### ⚙️ Configuration Options

Users can configure automation via the UI or API endpoints:

```csharp
{
  "enabled": true,
  "dailyApplicationTarget": 5,        // Apply to 5 jobs per day
  "minimumMatchScore": 70.0,          // Only apply if score >= 70%
  "autoTailorResume": true,           // Tailor each resume per job
  "preferredResumeId": "guid-here"    // Optional: specific resume to use
}
```

## API Endpoints

### Get Automation Settings

```http
GET /api/automation/settings
Authorization: Bearer {token}
```

**Response:**
```json
{
  "enabled": true,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": "00000000-0000-0000-0000-000000000000"
}
```

### Update Automation Settings

```http
PUT /api/automation/settings
Authorization: Bearer {token}
Content-Type: application/json

{
  "enabled": true,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

### Enable Automation

```http
POST /api/automation/enable
Authorization: Bearer {token}
```

### Disable Automation

```http
POST /api/automation/disable
Authorization: Bearer {token}
```

### Manually Trigger Applications (Testing)

```http
POST /api/automation/apply
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Automation completed. Applied to 3 jobs.",
  "applicationsSubmitted": 3,
  "applications": [
	{
	  "jobListingId": "guid-1",
	  "jobTitle": "Senior DevOps Engineer",
	  "company": "Tech Corp",
	  "matchScore": 92.5,
	  "applied": true,
	  "applicationId": "app-guid-1",
	  "usedResumeId": "resume-guid-1"
	}
  ],
  "executedAt": "2026-01-15T12:00:00Z",
  "error": null
}
```

### Manually Trigger Resume Upload (Testing)

```http
POST /api/automation/upload-resume
Authorization: Bearer {token}
```

## Client SDK Usage

### Getting Settings

```csharp
var settings = await apiClient.GetAutomationSettingsAsync();
Console.WriteLine($"Automation enabled: {settings?.Enabled}");
Console.WriteLine($"Daily target: {settings?.DailyApplicationTarget} jobs");
```

### Enabling Automation

```csharp
var updated = await apiClient.EnableAutomationAsync();
if (updated?.Enabled == true)
	Console.WriteLine("Automation enabled successfully!");
```

### Running Applications Manually

```csharp
var result = await apiClient.RunApplicationAutomationAsync();
Console.WriteLine($"Applied to {result?.ApplicationsSubmitted} jobs");

foreach (var app in result?.Applications ?? new List<ApplicationResultDto>())
{
	if (app.Applied)
		Console.WriteLine($"✓ {app.Company} - {app.JobTitle}");
	else
		Console.WriteLine($"✗ {app.Company} - {app.JobTitle}: {app.Reason}");
}
```

### Updating Settings

```csharp
var request = new UpdateAutomationSettingsRequest
{
	Enabled = true,
	DailyApplicationTarget = 10,
	MinimumMatchScore = 75.0,
	AutoTailorResume = true,
	PreferredResumeId = null  // Auto-select
};

var updated = await apiClient.UpdateAutomationSettingsAsync(request);
```

## Architecture

### Components

1. **IJobApplicationAutomationService**
   - Core business logic for automation
   - Scores jobs and filters by match threshold
   - Handles resume tailoring coordination
   - Tracks automation history

2. **JobApplicationAutomationBackgroundService**
   - Runs scheduled tasks at 6:00 AM and 12:00 PM UTC
   - Discovers users with automation enabled
   - Ensures resilience and error handling

3. **AutomationController**
   - Exposes HTTP endpoints for UI interaction
   - Handles manual test triggers
   - Provides settings management

4. **User Entity Extensions**
   - Stores automation preferences
   - Tracks automation history (JSON)
   - Monitors monthly application count

## Safety Features

1. **Match Score Filtering**: Won't apply to jobs below your threshold
2. **Duplicate Prevention**: Checks if already applied before submitting
3. **Audit Trail**: Every run logged with results
4. **User Override**: Can disable anytime
5. **Conservative Defaults**: 5 applications/day, 70% match minimum
6. **Error Isolation**: Failure on one job doesn't break entire run

## How to Use

### Step 1: Setup (One-time)

1. Upload your master resume
2. Create job matches (via job search or manual scoring)
3. Configure automation preferences

### Step 2: Enable Automation

Via UI or API:
- Set `enabled = true`
- Adjust daily target (default: 5)
- Set minimum match score (default: 70%)
- Enable/disable resume tailoring

### Step 3: Let It Run

**6:00 AM**: Automation marks your latest resume as active

**12:00 PM**: Automation applies to eligible jobs

### Step 4: Monitor

- Check the dashboard for recent applications
- Review automation history in your account settings
- Manually trigger to test anytime

## Implementation Details

### User Model Changes

```csharp
// New properties added to User entity
public bool AutomationEnabled { get; set; } = false;
public int DailyApplicationTarget { get; set; } = 5;
public double MinimumMatchScoreForAutomation { get; set; } = 70.0;
public bool AutoTailorResume { get; set; } = true;
public Guid? PreferredResumeIdForAutomation { get; set; }
public DateTime? LastAutomationRunAt { get; set; }
public int AutomationApplicationsThisMonth { get; set; } = 0;
public string? AutomationHistory { get; set; }  // JSON
```

### Automation History Format

```json
[
  {
	"executedAt": "2026-01-15T12:00:00Z",
	"applicationsSubmitted": 3,
	"success": true,
	"message": "Applied to 3 jobs"
  }
]
```

## Performance Considerations

- **Scheduled Tasks**: Run at fixed times (6 AM, 12 PM UTC)
- **Batch Processing**: All users with automation enabled run sequentially
- **Database Queries**: Optimized with repository methods
- **Resume Tailoring**: Currently returns base resume (placeholder for full implementation)
- **Error Handling**: Graceful failure - one job failure doesn't stop others

## Future Enhancements

1. **Email Notifications**: Send daily automation results
2. **AI Resume Tailoring**: Implement actual resume customization per job
3. **Skill-based Filtering**: Additional filtering beyond match score
4. **Company Whitelist/Blacklist**: Allow users to exclude certain companies
5. **Application Delay**: Random delays between applications to appear more natural
6. **Webhook Integration**: Notify external systems when automation runs
7. **Analytics Dashboard**: Detailed automation performance metrics
8. **Smart Scheduling**: Learn optimal application times per user

## Troubleshooting

### Automation Not Running

1. Check if automation is enabled
2. Verify at least one master resume exists
3. Check if there are eligible jobs (unscored, matching role preference)
4. Review logs in the application

### No Jobs Applied

Possible reasons:
- All suitable jobs already applied to
- Match scores below threshold
- No jobs match your target role
- Application failed due to missing information

### Resume Not Updating

1. Upload a new master resume
2. Verify `PreferredResumeIdForAutomation` is set correctly
3. Check resume has required sections (summary, skills, experience)

## Testing

### Manual Trigger

Use the `/api/automation/apply` endpoint to manually run automation without waiting for scheduled time:

```bash
curl -X POST https://your-api.com/api/automation/apply \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Monitor Automation

Check your automation history:
```bash
curl -X GET https://your-api.com/api/automation/settings \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Security Considerations

- ✅ Requires authentication (JWT token)
- ✅ User-specific automation (won't run for other users)
- ✅ Respects user preferences strictly
- ✅ Audit trail of all automations
- ✅ No sensitive data exposed in responses

## Cost & Resource Usage

- **Minimal Cost**: Runs only for enabled users
- **Low Latency**: Background service model
- **Database**: Minimal additional storage for history
- **API Calls**: No external API calls needed (all internal)

## References

This feature was inspired by:
- Blog post: Using Claude AI for automated job applications
- Medium article: "I Automated My Job Hunt"
- Problem: Repetitive daily job application tasks

---

**Questions?** Check the [main README](../../README.md) or review the API documentation.
