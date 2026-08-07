using HireKarlo.Application.DTOs.CareerOS;

namespace HireKarlo.Application.Interfaces.Services;

/// <summary>
/// Main orchestrator for the Career Operating System
/// Handles the 3-step onboarding and career dashboard
/// </summary>
public interface ICareerDashboardService
{
    /// <summary>
    /// STEP 1: Upload and process resume
    /// Extracts skills, experience, education from resume
    /// </summary>
    Task<ResumeUploadResponse> ProcessResumeAsync(Guid userId, Stream resumeStream, string fileName);

    /// <summary>
    /// Extract and build skill graph from resume
    /// Creates initial SkillGraph entities from parsed resume
    /// </summary>
    Task<UserSkillProfile> ExtractSkillGraphAsync(Guid userId);

    /// <summary>
    /// STEP 2: Setup dream companies
    /// Creates DreamCompany entities and triggers initial match calculation
    /// </summary>
    Task<List<DreamCompanyStatusDto>> SetupDreamCompaniesAsync(Guid userId, List<string> companyNames);

    /// <summary>
    /// STEP 3: Get career dashboard
    /// The "wow" moment - shows match %, gaps, recommendations, etc.
    /// </summary>
    Task<CareerDashboardResponse> GetCareerDashboardAsync(Guid userId);

    /// <summary>
    /// Recalculate match for a specific dream company
    /// Called when user completes a skill or milestone
    /// </summary>
    Task<DreamCompanyStatusDto> RecalculateMatchAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Get current onboarding step for user
    /// Returns which step they're on or if already completed
    /// </summary>
    Task<OnboardingStepDto> GetCurrentOnboardingStepAsync(Guid userId);

    /// <summary>
    /// Mark onboarding as complete
    /// </summary>
    Task CompleteOnboardingAsync(Guid userId);
}

/// <summary>
/// Onboarding step information
/// </summary>
public class OnboardingStepDto
{
    public int CurrentStep { get; set; } // 1, 2, 3
    public string StepName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
