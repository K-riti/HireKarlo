# Quick Start Guide - Job Application Automation

## 🚀 Get Started in 5 Minutes

### Step 1: Apply Database Migration

```powershell
cd C:\Users\BhaskarK\source\repos\HireKarlo
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api
```

This adds the automation columns to your database.

### Step 2: Verify Build

```powershell
dotnet build
```

Should complete with: **Build successful**

### Step 3: Start the API

```powershell
dotnet run -p src/Presentation/HireKarlo.Api
```

API starts at: `https://localhost:7001`

### Step 4: Get Your Auth Token

Log in via the Blazor frontend or use your existing JWT token.

### Step 5: Enable Automation

**Option A: Using curl**

```powershell
$token = "YOUR_JWT_TOKEN"
curl -X POST https://localhost:7001/api/automation/enable `
  -H "Authorization: Bearer $token"
```

**Option B: Using the client SDK**

```csharp
var settings = await apiClient.EnableAutomationAsync();
Console.WriteLine("Automation enabled:" + settings?.Enabled);
```

### Step 6: Manually Test

Trigger application automation immediately (don't wait until 12 PM):

```powershell
curl -X POST https://localhost:7001/api/automation/apply `
  -H "Authorization: Bearer $token"
```

**Response:**
```json
{
  "success": true,
  "message": "Automation completed. Applied to 3 jobs.",
  "applicationsSubmitted": 3,
  "applications": [
	{
	  "jobTitle": "Senior DevOps Engineer",
	  "company": "Tech Company",
	  "matchScore": 92.5,
	  "applied": true
	}
  ]
}
```

---

## ✅ Verify It's Working

### Check Settings

```powershell
curl -X GET https://localhost:7001/api/automation/settings `
  -H "Authorization: Bearer $token"
```

### Manual Resume Upload Test

```powershell
curl -X POST https://localhost:7001/api/automation/upload-resume `
  -H "Authorization: Bearer $token"
```

---

## ⚙️ Configure Automation

### Default Settings

```json
{
  "enabled": false,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

### Customize Settings

```powershell
$body = @{
  enabled = $true
  dailyApplicationTarget = 10  # Apply to 10 jobs/day
  minimumMatchScore = 75.0      # Only if score >= 75%
  autoTailorResume = $true
  preferredResumeId = $null
} | ConvertTo-Json

curl -X PUT https://localhost:7001/api/automation/settings `
  -H "Authorization: Bearer $token" `
  -H "Content-Type: application/json" `
  -d $body
```

---

## 📋 What Happens Daily

### 6:00 AM UTC

✅ **Automation runs resume upload task**
- Finds your latest master resume
- Marks it as preferred for automation
- Your profile is now "fresh" for the day

### 12:00 PM UTC

✅ **Automation runs job application task**
- Finds new jobs matching your target role
- Scores them based on resume match
- Filters by your minimum score threshold
- Applies to top 5 jobs (or configured amount)
- Logs all results

### Result: Jobs Applied Automatically!

---

## 🧪 API Testing Cheat Sheet

### Get Settings
```powershell
curl -X GET https://localhost:7001/api/automation/settings -H "Authorization: Bearer $token"
```

### Enable Automation
```powershell
curl -X POST https://localhost:7001/api/automation/enable -H "Authorization: Bearer $token"
```

### Disable Automation
```powershell
curl -X POST https://localhost:7001/api/automation/disable -H "Authorization: Bearer $token"
```

### Manually Trigger Applications
```powershell
curl -X POST https://localhost:7001/api/automation/apply -H "Authorization: Bearer $token"
```

### Manually Trigger Resume Upload
```powershell
curl -X POST https://localhost:7001/api/automation/upload-resume -H "Authorization: Bearer $token"
```

### Update Settings
```powershell
curl -X PUT https://localhost:7001/api/automation/settings `
  -H "Authorization: Bearer $token" `
  -H "Content-Type: application/json" `
  -d '{"enabled":true,"dailyApplicationTarget":5,"minimumMatchScore":70.0,"autoTailorResume":true,"preferredResumeId":null}'
```

---

## 📚 Important Files

| File | Purpose |
|------|---------|
| `src/Infrastructure/HireKarlo.Infrastructure/Services/JobApplicationAutomationService.cs` | Core automation logic |
| `src/Infrastructure/HireKarlo.Infrastructure/BackgroundServices/JobApplicationAutomationBackgroundService.cs` | Scheduler (runs at 6 AM & 12 PM) |
| `src/Presentation/HireKarlo.Api/Controllers/AutomationController.cs` | API endpoints |
| `src/Core/HireKarlo.Application/Interfaces/Services/IJobApplicationAutomationService.cs` | Service interface |
| `src/Infrastructure/HireKarlo.Infrastructure/Services/AUTOMATION_FEATURE.md` | Detailed documentation |

---

## 🔍 Monitor Automation

### Check Recent Applications
```powershell
curl -X GET https://localhost:7001/api/applications `
  -H "Authorization: Bearer $token"
```

### View Automation History
History is stored in the `AutomationHistory` JSON field on the User entity. Access via database or API.

---

## 💡 Tips & Tricks

### Tip 1: Resume Requirements
- Must have at least one **master resume**
- Resume must have parsed content (summary, skills, experience)
- Auto-tailored versions won't interfere

### Tip 2: Job Requirements
- Must be marked as active (`IsActive = true`)
- Must have a match score calculated
- User must not have already applied

### Tip 3: Testing Before Production
1. Set `minimumMatchScore` high (e.g., 90%) to test safely
2. Set `dailyApplicationTarget` low (e.g., 1) to limit applications
3. Use manual trigger to test immediately
4. Once comfortable, adjust to desired values

### Tip 4: Timezone Note
- Scheduled tasks use UTC times (6:00 AM and 12:00 PM UTC)
- If you need local times, cloudplatform may offer timezone config
- For now, times are fixed UTC

---

## ❓ FAQ

**Q: What if I don't want to apply to all high-scoring jobs?**
A: Increase your `minimumMatchScore` threshold to filter more strictly.

**Q: Can I apply to more than 5 jobs?**
A: Yes! Increase `dailyApplicationTarget` to any number.

**Q: What if a job application fails?**
A: Automation logs the failure reason. Check the result for details. Other jobs still apply.

**Q: Can I turn off automation temporarily?**
A: Yes! Use the `disable` endpoint. You can re-enable anytime.

**Q: Does automation apply twice to same job?**
A: No! It checks existing applications first and skips already-applied jobs.

**Q: When does automation run if my laptop is off?**
A: The API server handles automation, regardless of your laptop. No laptop power management required!

---

## 🐛 Troubleshooting

### Automation Not Running

**Check 1**: Is automation enabled?
```powershell
curl -X GET https://localhost:7001/api/automation/settings -H "Authorization: Bearer $token"
```
Look for `"enabled": true`

**Check 2**: Do you have a master resume?
```powershell
curl -X GET https://localhost:7001/api/resumes -H "Authorization: Bearer $token"
```
Must have at least one with `"isMaster": true`

**Check 3**: Are there unscored jobs?
Automation only applies to jobs that have already been matched/scored.

### No Jobs Applied

**Reason 1**: All suitable jobs already applied to
- Check your applications list

**Reason 2**: No jobs meet minimum score
- Lower your `minimumMatchScore` threshold
- Or add more jobs via job search

**Reason 3**: Jobs don't match your target role
- Update your `targetRole` preference
- Or disable role filtering by clearing it

### Manual Trigger Not Working

**Check 1**: Is your token valid?
- Re-login to get fresh token

**Check 2**: Is API running?
- Verify API startup: `dotnet run -p src/Presentation/HireKarlo.Api`

**Check 3**: Check the error response
- Full error details in API response

---

## 📞 Getting Help

1. **API Documentation**: See `AUTOMATION_FEATURE.md`
2. **Implementation Details**: See `AUTOMATION_IMPLEMENTATION_SUMMARY.md`
3. **Code Comments**: Check source files for detailed comments
4. **Logging**: Enable DEBUG logging to trace execution

---

## 🎉 You're All Set!

Your automated job application system is ready. Start by:

1. ✅ Running the migration
2. ✅ Enabling automation
3. ✅ Manually testing with one manual trigger
4. ✅ Observing the scheduled runs (6 AM & 12 PM)
5. ✅ Reviewing applications created

**Happy automating!** 🚀
