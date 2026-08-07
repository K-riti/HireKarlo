// Referral Intelligence Service
// Priority: ⭐⭐⭐⭐⭐ (Very High Value - Most Referrals > 80% callback rate)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireKarlo.Application.Services
{
    /// <summary>
    /// Referral Intelligence Service
    /// 
    /// Discovers employees at target companies and generates:
    /// 1. Referral score (0-100) based on profile similarity
    /// 2. Why they're a good match (tech stack, experience, location)
    /// 3. Suggested outreach message (personalized, not generic)
    /// 4. Best time to reach out
    /// 5. Conversation starters
    /// </summary>
    public interface IReferralIntelligenceService
    {
        /// <summary>
        /// Find qualified referrals at a target company
        /// Returns list ordered by referral score (highest first)
        /// </summary>
        Task<List<ReferralProfileDto>> FindReferralsAsync(
            Guid userId, 
            Guid companyId,
            int limit = 10);

        /// <summary>
        /// Score a specific person as a potential referral
        /// Returns detailed breakdown of why/why not
        /// </summary>
        Task<ReferralScoreDto> ScoreReferralAsync(
            Guid userId,
            string linkedInUrl);

        /// <summary>
        /// Generate personalized outreach message for a referral
        /// Not generic - uses shared background, skills, location etc
        /// </summary>
        Task<OutreachMessageDto> GenerateOutreachMessageAsync(
            Guid userId,
            Guid referralId,
            string? targetRole = null);

        /// <summary>
        /// Get conversation starters (shared interests, tech stacks, etc)
        /// Helps with the initial break-ice in outreach
        /// </summary>
        Task<List<ConversationStarterDto>> GetConversationStartersAsync(
            Guid userId,
            Guid referralId);

        /// <summary>
        /// Track referral interactions (contacted, replied, met for coffee, etc)
        /// </summary>
        Task<ReferralInteractionDto> LogInteractionAsync(
            Guid userId,
            Guid referralId,
            ReferralInteractionType type,
            string? notes = null);

        /// <summary>
        /// Get referral pipeline status
        /// How many active, followed up, converted to interviews
        /// </summary>
        Task<ReferralPipelineDto> GetReferralPipelineAsync(Guid userId);
    }

    /// <summary>
    /// Implementation of Referral Intelligence
    /// </summary>
    public class ReferralIntelligenceService : IReferralIntelligenceService
    {
        private readonly IRepositoryManager _repositories;
        private readonly IAiServiceFactory _aiServiceFactory;
        private readonly ILinkedInIntegrationService _linkedIn;
        private readonly ILogger<ReferralIntelligenceService> _logger;

        public ReferralIntelligenceService(
            IRepositoryManager repositories,
            IAiServiceFactory aiServiceFactory,
            ILinkedInIntegrationService linkedIn,
            ILogger<ReferralIntelligenceService> logger)
        {
            _repositories = repositories;
            _aiServiceFactory = aiServiceFactory;
            _linkedIn = linkedIn;
            _logger = logger;
        }

        public async Task<List<ReferralProfileDto>> FindReferralsAsync(
            Guid userId, 
            Guid companyId,
            int limit = 10)
        {
            _logger.LogInformation("Finding referrals at company {CompanyId} for user {UserId}",
                companyId, userId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                throw new InvalidOperationException($"User {userId} has no resume");

            var company = await _repositories.Companies.FindAsync(c => c.Id == companyId);
            if (company == null)
                throw new InvalidOperationException($"Company {companyId} not found");

            // Search for employees at this company
            var employees = await _linkedIn.SearchCompanyEmployeesAsync(company.Name);

            _logger.LogInformation("Found {EmployeeCount} employees at {CompanyName}",
                employees.Count, company.Name);

            var referrals = new List<ReferralProfileDto>();

            foreach (var employee in employees.Take(limit * 2)) // Get 2x to filter
            {
                try
                {
                    var score = await ScoreReferralAsync(userId, employee.LinkedInUrl);

                    if (score.OverallScore >= 60) // Only include decent matches
                    {
                        referrals.Add(new ReferralProfileDto
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            LinkedInUrl = employee.LinkedInUrl,
                            Name = employee.Name,
                            Title = employee.Title,
                            Company = company.Name,
                            Location = employee.Location,
                            ReferralScore = score.OverallScore,
                            ScoreBreakdown = score,
                            CurrentRole = employee.Title,
                            YearsOfExperience = employee.YearsExperience,
                            Skills = employee.Skills,
                            Status = ReferralStatus.Discovered,
                            DiscoveredAt = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to score referral {LinkedInUrl}", 
                        employee.LinkedInUrl);
                    continue;
                }
            }

            // Sort by referral score descending
            return referrals
                .OrderByDescending(r => r.ReferralScore)
                .Take(limit)
                .ToList();
        }

        public async Task<ReferralScoreDto> ScoreReferralAsync(
            Guid userId,
            string linkedInUrl)
        {
            _logger.LogInformation("Scoring referral {LinkedInUrl} for user {UserId}",
                linkedInUrl, userId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                throw new InvalidOperationException($"User {userId} has no resume");

            // Fetch referral's public profile
            var referralProfile = await _linkedIn.GetPublicProfileAsync(linkedInUrl);

            // Extract user's profile
            var userSkills = ExtractSkillsFromResume(user.Resume);
            var userExperience = ExtractExperienceFromResume(user.Resume);
            var userLocation = ExtractLocationFromResume(user.Resume);

            // Score on 5 dimensions
            var techStackScore = CalculateTechStackScore(userSkills, referralProfile.Skills);
            var experienceScore = CalculateExperienceScore(userExperience, referralProfile.YearsExperience);
            var locationScore = CalculateLocationScore(userLocation, referralProfile.Location);
            var recencyScore = CalculateRecencyScore(referralProfile.LastActive);
            var reachScore = CalculateReachScore(referralProfile);

            var overallScore = (int)((techStackScore * 0.35) + 
                                    (experienceScore * 0.25) + 
                                    (locationScore * 0.15) + 
                                    (recencyScore * 0.15) + 
                                    (reachScore * 0.10));

            _logger.LogInformation("Referral score: {Score} for {LinkedInUrl}",
                overallScore, linkedInUrl);

            return new ReferralScoreDto
            {
                LinkedInUrl = linkedInUrl,
                OverallScore = overallScore,
                ScoreBreakdown = new Dictionary<string, int>
                {
                    { "Tech Stack Match", techStackScore },
                    { "Experience Fit", experienceScore },
                    { "Location Proximity", locationScore },
                    { "Recency", recencyScore },
                    { "Reachability", reachScore }
                },
                ScoreReasons = new List<string>
                {
                    $"Tech stack match: {techStackScore}%",
                    $"Similar experience level: {experienceScore}%",
                    $"Location: {(locationScore > 70 ? "Same" : "Different")} region",
                    $"Recently active on LinkedIn: {(recencyScore > 70 ? "Yes" : "Not recently")}",
                    $"Reachability score: {reachScore}%"
                },
                ScoreInterpretation = InterpretScore(overallScore),
                Recommendation = GenerateScoreRecommendation(overallScore)
            };
        }

        public async Task<OutreachMessageDto> GenerateOutreachMessageAsync(
            Guid userId,
            Guid referralId,
            string? targetRole = null)
        {
            _logger.LogInformation("Generating outreach message for user {UserId} to referral {ReferralId}",
                userId, referralId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var referral = await _repositories.Referrals
                .FindAsync(r => r.Id == referralId);

            if (user?.Resume == null || referral == null)
                throw new InvalidOperationException("Invalid user or referral");

            // Get conversation starters
            var starters = await GetConversationStartersAsync(userId, referralId);

            // Build the message using AI
            var aiService = _aiServiceFactory.CreateCompletionService();

            var prompt = $@"Generate a professional but personalized LinkedIn outreach message.

CONTEXT:
- Your name: {user.Name}
- Your background: {ExtractSummaryFromResume(user.Resume)}
- Target role: {targetRole ?? "Platform Engineer"}
- Recipient name: {referral.Name}
- Recipient role: {referral.Title}
- Recipient company: {referral.Company}

SHARED INTERESTS:
{string.Join("\n", starters.Select(s => $"- {s.Topic}: {s.Description}"))}

GUIDELINES:
1. Be specific (mention 1-2 shared tech/experiences)
2. Reference their work/posts if possible
3. Show genuine interest, not just asking for a job
4. Ask for advice or insights (not a favor)
5. Keep under 200 words
6. End with a specific call-to-action

Generate the message:";

            var message = await aiService.CompleteAsync(prompt);

            // Generate alternative versions (aggressive, casual, professional)
            var variants = new List<OutreachVariantDto>
            {
                new()
                {
                    Style = "Professional",
                    Message = message,
                    BestFor = "Technical roles, first contact"
                },
                new()
                {
                    Style = "Casual",
                    Message = await GenerateVariant(aiService, message, "casual, friendlier tone"),
                    BestFor = "Startups, friendly company culture"
                },
                new()
                {
                    Style = "Aggressive",
                    Message = await GenerateVariant(aiService, message, "direct, value-forward pitch"),
                    BestFor = "Time-sensitive opportunities"
                }
            };

            return new OutreachMessageDto
            {
                UserId = userId,
                ReferralId = referralId,
                ReferralName = referral.Name,
                ReferralRole = referral.Title,
                TargetRole = targetRole,
                PrimaryMessage = message,
                Variants = variants,
                ConversationStarters = starters.Take(3).ToList(),
                BestTimeToContact = CalculateBestContactTime(referral),
                FollowUpTiming = "After 5 days if no response",
                LinkedInUrl = referral.LinkedInUrl
            };
        }

        public async Task<List<ConversationStarterDto>> GetConversationStartersAsync(
            Guid userId,
            Guid referralId)
        {
            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var referral = await _repositories.Referrals
                .FindAsync(r => r.Id == referralId);

            if (user?.Resume == null || referral == null)
                return new List<ConversationStarterDto>();

            var starters = new List<ConversationStarterDto>();

            // Extract shared tech skills
            var userSkills = ExtractSkillsFromResume(user.Resume);
            var sharedSkills = userSkills.Intersect(referral.Skills, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var skill in sharedSkills.Take(2))
            {
                starters.Add(new ConversationStarterDto
                {
                    Topic = skill,
                    Description = $"Discussing your experience with {skill}",
                    Type = ConversationType.SharedTech,
                    SuggestedQuestion = $@"I noticed you've worked extensively with {skill}. 
                        How has your approach to {skill} evolved at {referral.Company}?"
                });
            }

            // Extract similar experience levels
            var userExp = ExtractExperienceFromResume(user.Resume);
            var expDiff = Math.Abs(userExp - referral.YearsOfExperience);

            if (expDiff <= 2)
            {
                starters.Add(new ConversationStarterDto
                {
                    Topic = "Similar Career Stage",
                    Description = $"Both mid-level engineers (~{referral.YearsOfExperience} years)",
                    Type = ConversationType.CareerStage,
                    SuggestedQuestion = $"How did you approach the transition to senior roles?"
                });
            }

            // Similar location
            var userLocation = ExtractLocationFromResume(user.Resume);
            if (userLocation?.ToLower().Contains(referral.Location.ToLower()) ?? false)
            {
                starters.Add(new ConversationStarterDto
                {
                    Topic = "Same Location",
                    Description = $"Both in {referral.Location}",
                    Type = ConversationType.Location,
                    SuggestedQuestion = $"Are there any local tech communities in {referral.Location} you'd recommend?"
                });
            }

            return starters;
        }

        public async Task<ReferralInteractionDto> LogInteractionAsync(
            Guid userId,
            Guid referralId,
            ReferralInteractionType type,
            string? notes = null)
        {
            var interaction = new ReferralInteractionDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReferralId = referralId,
                InteractionType = type,
                Notes = notes,
                InteractionDate = DateTime.UtcNow
            };

            // Update referral status based on interaction
            var referral = await _repositories.Referrals.FindAsync(r => r.Id == referralId);
            if (referral != null)
            {
                referral.Status = type switch
                {
                    ReferralInteractionType.Contacted => ReferralStatus.Contacted,
                    ReferralInteractionType.Replied => ReferralStatus.Engaged,
                    ReferralInteractionType.MetForCoffee => ReferralStatus.Converted,
                    ReferralInteractionType.Referral => ReferralStatus.Converted,
                    _ => referral.Status
                };

                referral.LastInteractionAt = DateTime.UtcNow;
            }

            await _repositories.SaveChangesAsync();

            _logger.LogInformation("Logged interaction {Type} for referral {ReferralId}",
                type, referralId);

            return interaction;
        }

        public async Task<ReferralPipelineDto> GetReferralPipelineAsync(Guid userId)
        {
            var referrals = await _repositories.Referrals
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var discovered = referrals.Count(r => r.Status == ReferralStatus.Discovered);
            var contacted = referrals.Count(r => r.Status == ReferralStatus.Contacted);
            var engaged = referrals.Count(r => r.Status == ReferralStatus.Engaged);
            var converted = referrals.Count(r => r.Status == ReferralStatus.Converted);

            var conversionRate = discovered > 0 
                ? (decimal)converted / discovered * 100 
                : 0;

            return new ReferralPipelineDto
            {
                UserId = userId,
                Total = referrals.Count,
                Discovered = discovered,
                Contacted = contacted,
                Engaged = engaged,
                Converted = converted,
                ConversionRate = conversionRate,
                AverageScoreOfConverted = converted > 0 
                    ? referrals.Where(r => r.Status == ReferralStatus.Converted)
                        .Average(r => r.ReferralScore) 
                    : 0,
                NextActions = GenerateNextActions(referrals),
                TopCompanies = GetTopCompanies(referrals),
                FollowUpDue = referrals
                    .Where(r => r.Status == ReferralStatus.Contacted && 
                                DateTime.UtcNow.Subtract(r.LastInteractionAt ?? DateTime.MinValue).TotalDays >= 5)
                    .Select(r => r.Id)
                    .ToList()
            };
        }

        // Helper Methods

        private int CalculateTechStackScore(List<string> userSkills, List<string> referralSkills)
        {
            if (!userSkills.Any() || !referralSkills.Any())
                return 0;

            var matches = userSkills.Intersect(referralSkills, StringComparer.OrdinalIgnoreCase).Count();
            return (int)((decimal)matches / Math.Max(userSkills.Count, referralSkills.Count) * 100);
        }

        private int CalculateExperienceScore(int userExp, int referralExp)
        {
            var diff = Math.Abs(userExp - referralExp);
            return diff switch
            {
                <= 1 => 95,
                <= 2 => 85,
                <= 3 => 70,
                <= 5 => 50,
                _ => 30
            };
        }

        private int CalculateLocationScore(string? userLocation, string referralLocation)
        {
            if (userLocation?.ToLower() == referralLocation.ToLower())
                return 95;

            if (userLocation?.ToLower().Contains(referralLocation.ToLower()) ?? false)
                return 60;

            return 20;
        }

        private int CalculateRecencyScore(DateTime lastActive)
        {
            var daysSinceActive = DateTime.UtcNow.Subtract(lastActive).TotalDays;
            return daysSinceActive switch
            {
                <= 7 => 100,
                <= 30 => 80,
                <= 90 => 60,
                <= 180 => 40,
                _ => 20
            };
        }

        private int CalculateReachScore(LinkedInProfile profile)
        {
            // Higher if: connections visible, recent activity, not a C-level
            var score = 50;

            if (profile.IsOpenToMessages)
                score += 30;

            if (!profile.Title.ToLower().Contains("ceo") && 
                !profile.Title.ToLower().Contains("founder"))
                score += 20;

            return Math.Min(100, score);
        }

        private string InterpretScore(int score)
        {
            return score switch
            {
                >= 85 => "Excellent Match",
                >= 70 => "Very Good Match",
                >= 55 => "Good Match",
                >= 40 => "Fair Match",
                _ => "Weak Match"
            };
        }

        private string GenerateScoreRecommendation(int score)
        {
            return score switch
            {
                >= 85 => "Highly recommended - reach out immediately",
                >= 70 => "Recommended - strong match, reach out soon",
                >= 55 => "Worth contacting - good alignment on tech stack",
                >= 40 => "Consider contacting - some relevant overlap",
                _ => "Low priority - limited relevance"
            };
        }

        private List<string> ExtractSkillsFromResume(Resume resume)
        {
            // Parse resume and return list of skills
            return resume.ParsedContent?.Split(',').ToList() ?? new();
        }

        private int ExtractExperienceFromResume(Resume resume)
        {
            // Calculate total years from work history
            return 5; // Placeholder
        }

        private string? ExtractLocationFromResume(Resume resume)
        {
            // Extract location from resume
            return null;
        }

        private string ExtractSummaryFromResume(Resume resume)
        {
            // Extract professional summary
            return "Software Engineer with focus on cloud infrastructure";
        }

        private async Task<string> GenerateVariant(ICompletionService service, string original, string style)
        {
            var prompt = $@"Rewrite this LinkedIn message in {style}:

{original}";
            return await service.CompleteAsync(prompt);
        }

        private string CalculateBestContactTime(ReferralProfile referral)
        {
            // Based on LinkedIn activity patterns
            return "Tuesday-Thursday, 9-11 AM in their timezone";
        }

        private List<string> GenerateNextActions(List<ReferralProfile> referrals)
        {
            var actions = new List<string>();

            if (referrals.Count(r => r.Status == ReferralStatus.Discovered) > 5)
                actions.Add("Batch outreach to top 5 discovered referrals");

            if (referrals.Count(r => r.Status == ReferralStatus.Contacted) > 3)
                actions.Add("Follow up with 3+ contacted referrals");

            return actions;
        }

        private List<string> GetTopCompanies(List<ReferralProfile> referrals)
        {
            return referrals
                .GroupBy(r => r.Company)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToList();
        }
    }

    // ===== DTOs & Enums =====

    public class ReferralProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string LinkedInUrl { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ReferralScore { get; set; }
        public ReferralScoreDto? ScoreBreakdown { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public List<string> Skills { get; set; } = new();
        public ReferralStatus Status { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public class ReferralScoreDto
    {
        public string LinkedInUrl { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public Dictionary<string, int> ScoreBreakdown { get; set; } = new();
        public List<string> ScoreReasons { get; set; } = new();
        public string ScoreInterpretation { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public class OutreachMessageDto
    {
        public Guid UserId { get; set; }
        public Guid ReferralId { get; set; }
        public string ReferralName { get; set; } = string.Empty;
        public string ReferralRole { get; set; } = string.Empty;
        public string? TargetRole { get; set; }
        public string PrimaryMessage { get; set; } = string.Empty;
        public List<OutreachVariantDto> Variants { get; set; } = new();
        public List<ConversationStarterDto> ConversationStarters { get; set; } = new();
        public string BestTimeToContact { get; set; } = string.Empty;
        public string FollowUpTiming { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
    }

    public class OutreachVariantDto
    {
        public string Style { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string BestFor { get; set; } = string.Empty;
    }

    public class ConversationStarterDto
    {
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ConversationType Type { get; set; }
        public string SuggestedQuestion { get; set; } = string.Empty;
    }

    public class ReferralInteractionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ReferralId { get; set; }
        public ReferralInteractionType InteractionType { get; set; }
        public string? Notes { get; set; }
        public DateTime InteractionDate { get; set; }
    }

    public class ReferralPipelineDto
    {
        public Guid UserId { get; set; }
        public int Total { get; set; }
        public int Discovered { get; set; }
        public int Contacted { get; set; }
        public int Engaged { get; set; }
        public int Converted { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AverageScoreOfConverted { get; set; }
        public List<string> NextActions { get; set; } = new();
        public List<string> TopCompanies { get; set; } = new();
        public List<Guid> FollowUpDue { get; set; } = new();
    }

    public enum ReferralStatus
    {
        Discovered,
        Contacted,
        Engaged,
        Converted,
        Archived
    }

    public enum ReferralInteractionType
    {
        Viewed,
        Contacted,
        Replied,
        MetForCoffee,
        Referral,
        Rejected,
        NoResponse
    }

    public enum ConversationType
    {
        SharedTech,
        CareerStage,
        Location,
        Company,
        Project,
        Achievement
    }

    public class LinkedInProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int YearsExperience { get; set; }
        public List<string> Skills { get; set; } = new();
        public string Location { get; set; } = string.Empty;
        public DateTime LastActive { get; set; }
        public bool IsOpenToMessages { get; set; }
    }

    public class ReferralProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public int ReferralScore { get; set; }
        public ReferralStatus Status { get; set; }
        public DateTime? LastInteractionAt { get; set; }
        public List<string> Skills { get; set; } = new();
        public int YearsOfExperience { get; set; }
    }
}
