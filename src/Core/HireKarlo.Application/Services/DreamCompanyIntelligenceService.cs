// Dream Company Intelligence Service
// Priority: ⭐⭐⭐⭐⭐ (Biggest Product Differentiator)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireKarlo.Application.Services
{
    /// <summary>
    /// Dream Company Intelligence Service
    /// 
    /// Analyzes user's fit against target companies and generates:
    /// 1. Match percentage (0-100%)
    /// 2. Matched skills (with user's current level)
    /// 3. Missing skills (with importance ranking)
    /// 4. Recommended projects to build
    /// 5. Required certifications
    /// 6. Suggested learning path
    /// </summary>
    public interface IDreamCompanyIntelligenceService
    {
        /// <summary>
        /// Add a target company for the user
        /// </summary>
        Task<DreamCompanyDto> AddTargetCompanyAsync(
            Guid userId, 
            string companyName, 
            string? targetRole = null);

        /// <summary>
        /// Calculate user's match % against a dream company
        /// Returns match score + detailed breakdown
        /// </summary>
        Task<DreamCompanyMatchDto> CalculateCompanyMatchAsync(
            Guid userId, 
            Guid companyId);

        /// <summary>
        /// Get all target companies for user with match percentages
        /// Ordered by match % descending
        /// </summary>
        Task<List<DreamCompanyMatchDto>> GetAllCompanyMatchesAsync(Guid userId);

        /// <summary>
        /// Get skills user is missing for a target company
        /// Ordered by impact (how much they reduce match %)
        /// </summary>
        Task<List<SkillGapDto>> GetSkillGapsAsync(
            Guid userId, 
            Guid companyId);

        /// <summary>
        /// Get recommended projects to build to improve match
        /// Example: "Build a Kubernetes multi-region orchestrator"
        /// </summary>
        Task<List<ProjectRecommendationDto>> GetProjectRecommendationsAsync(
            Guid userId, 
            Guid companyId);

        /// <summary>
        /// Get required certifications for target company's common roles
        /// Example: CKA (Kubernetes), AWS Solutions Architect
        /// </summary>
        Task<List<CertificationDto>> GetRequiredCertificationsAsync(
            Guid userId, 
            Guid companyId);

        /// <summary>
        /// Generate a personalized learning path to reach target match %
        /// Week-by-week breakdown of what to learn
        /// </summary>
        Task<LearningPathDto> GenerateLearningPathAsync(
            Guid userId, 
            Guid companyId,
            int targetMatchPercentage = 85);
    }

    /// <summary>
    /// Implementation of Dream Company Intelligence
    /// </summary>
    public class DreamCompanyIntelligenceService : IDreamCompanyIntelligenceService
    {
        private readonly IRepositoryManager _repositories;
        private readonly ICareerEngineService _careerEngine;
        private readonly IAiServiceFactory _aiServiceFactory;
        private readonly ILogger<DreamCompanyIntelligenceService> _logger;

        public DreamCompanyIntelligenceService(
            IRepositoryManager repositories,
            ICareerEngineService careerEngine,
            IAiServiceFactory aiServiceFactory,
            ILogger<DreamCompanyIntelligenceService> logger)
        {
            _repositories = repositories;
            _careerEngine = careerEngine;
            _aiServiceFactory = aiServiceFactory;
            _logger = logger;
        }

        public async Task<DreamCompanyDto> AddTargetCompanyAsync(
            Guid userId, 
            string companyName, 
            string? targetRole = null)
        {
            _logger.LogInformation("Adding target company {CompanyName} for user {UserId}", 
                companyName, userId);

            // Check if company exists in our DB
            var company = await _repositories.Companies
                .FindAsync(c => c.Name.ToLower() == companyName.ToLower());

            if (company == null)
            {
                // Create new company profile
                company = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = companyName,
                    CreatedAt = DateTime.UtcNow
                };

                _repositories.Companies.Add(company);

                // Fetch and store company public info
                // - LinkedIn company page
                // - Tech stack (from public sources)
                // - Recent job postings
                // - Employee count
                // - Funding/Revenue (for context)
                await FetchCompanyPublicDataAsync(company);
            }

            // Add to user's target companies
            var user = await _repositories.Users.FindAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException($"User {userId} not found");

            var dreamCompany = new DreamCompany
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = company.Id,
                TargetRole = targetRole,
                AddedAt = DateTime.UtcNow,
                LastMatchCalculatedAt = null
            };

            user.DreamCompanies.Add(dreamCompany);
            await _repositories.SaveChangesAsync();

            _logger.LogInformation("Successfully added target company {CompanyName}", companyName);

            return new DreamCompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                TargetRole = targetRole,
                AddedAt = dreamCompany.AddedAt
            };
        }

        public async Task<DreamCompanyMatchDto> CalculateCompanyMatchAsync(
            Guid userId, 
            Guid companyId)
        {
            _logger.LogInformation("Calculating match score for user {UserId} vs company {CompanyId}",
                userId, companyId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                throw new InvalidOperationException($"User {userId} has no resume");

            var company = await _repositories.Companies
                .Include(c => c.CommonSkills)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                throw new InvalidOperationException($"Company {companyId} not found");

            // Use AI to analyze company's tech stack and requirements
            var companyProfile = await _careerEngine.AnalyzeCompanyAsync(company);

            // Compare user's skills with company requirements
            var userSkills = ParseUserSkills(user.Resume);
            var matchedSkills = CalculateMatchedSkills(userSkills, companyProfile.RequiredSkills);
            var missingSkills = companyProfile.RequiredSkills
                .Where(s => !matchedSkills.Any(m => m.SkillName == s.Name))
                .ToList();

            // Calculate overall match percentage (weighted)
            var matchPercentage = CalculateWeightedMatch(
                matchedSkills,
                missingSkills,
                companyProfile);

            _logger.LogInformation("Match calculated: {MatchPercentage}% for user {UserId}",
                matchPercentage, userId);

            return new DreamCompanyMatchDto
            {
                UserId = userId,
                CompanyId = companyId,
                CompanyName = company.Name,
                MatchPercentage = matchPercentage,
                MatchLevel = GetMatchLevel(matchPercentage),
                MatchedSkills = matchedSkills,
                MissingSkills = missingSkills.Select(s => new SkillDto
                {
                    SkillName = s.Name,
                    Importance = s.Importance
                }).ToList(),
                LastCalculatedAt = DateTime.UtcNow,
                NextRecommendedRole = companyProfile.RecommendedRoles.FirstOrDefault()
            };
        }

        public async Task<List<DreamCompanyMatchDto>> GetAllCompanyMatchesAsync(Guid userId)
        {
            var user = await _repositories.Users
                .Include(u => u.DreamCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<DreamCompanyMatchDto>();

            var matches = new List<DreamCompanyMatchDto>();

            foreach (var dreamCompany in user.DreamCompanies)
            {
                var match = await CalculateCompanyMatchAsync(userId, dreamCompany.CompanyId);
                matches.Add(match);
            }

            // Sort by match % descending (highest matches first)
            return matches.OrderByDescending(m => m.MatchPercentage).ToList();
        }

        public async Task<List<SkillGapDto>> GetSkillGapsAsync(
            Guid userId, 
            Guid companyId)
        {
            var match = await CalculateCompanyMatchAsync(userId, companyId);

            // Sort by impact (how much adding this skill improves match %)
            var gaps = match.MissingSkills
                .OrderByDescending(s => s.Importance)
                .Select(s => new SkillGapDto
                {
                    SkillName = s.SkillName,
                    Importance = s.Importance,
                    LearningTimeWeeks = EstimateLearningTime(s.SkillName),
                    ExpectedImprovementPercent = EstimateMatchImprovement(s),
                    ResourcesAvailable = GetLearningResources(s.SkillName)
                })
                .ToList();

            return gaps;
        }

        public async Task<List<ProjectRecommendationDto>> GetProjectRecommendationsAsync(
            Guid userId, 
            Guid companyId)
        {
            var gaps = await GetSkillGapsAsync(userId, companyId);
            var projects = new List<ProjectRecommendationDto>();

            // Use AI to generate project ideas that address skill gaps
            foreach (var gap in gaps.Take(3)) // Top 3 gaps
            {
                var aiService = _aiServiceFactory.CreateCompletionService();

                var prompt = $@"Generate a portfolio project idea that teaches {gap.SkillName}.

Requirements:
- Should take 4-8 weeks to complete
- Relevant to job search
- Practical and implementable
- Impressive for interviews

Format:
PROJECT_NAME
DESCRIPTION
TECH_STACK
ESTIMATED_TIME
LEARNING_OUTCOMES";

                var response = await aiService.CompleteAsync(prompt);

                projects.Add(new ProjectRecommendationDto
                {
                    ProjectName = ExtractProjectName(response),
                    Description = response,
                    RelevantSkills = new List<string> { gap.SkillName },
                    EstimatedWeeks = 6,
                    MatchImprovementPercent = gap.ExpectedImprovementPercent,
                    Difficulty = gap.Importance > 8 ? "Hard" : "Medium"
                });
            }

            return projects;
        }

        public async Task<List<CertificationDto>> GetRequiredCertificationsAsync(
            Guid userId, 
            Guid companyId)
        {
            var company = await _repositories.Companies
                .FindAsync(c => c.Id == companyId);

            if (company == null)
                return new List<CertificationDto>();

            // Common certifications by company + role
            var certifications = new List<CertificationDto>
            {
                // Examples - populated from company profile
                new()
                {
                    Name = "Certified Kubernetes Administrator (CKA)",
                    Issuer = "CNCF",
                    ExamCost = "$395",
                    StudyTimeWeeks = 8,
                    PassRate = "70%",
                    Relevance = "Very High",
                    PrepResources = new List<string>
                    {
                        "Linux Academy",
                        "Killer.sh",
                        "Official CNCF docs"
                    }
                },
                new()
                {
                    Name = "AWS Solutions Architect Professional",
                    Issuer = "Amazon",
                    ExamCost = "$300",
                    StudyTimeWeeks = 12,
                    PassRate = "65%",
                    Relevance = "High",
                    PrepResources = new List<string>
                    {
                        "A Cloud Guru",
                        "Linux Academy",
                        "AWS Whitepapers"
                    }
                }
            };

            return certifications;
        }

        public async Task<LearningPathDto> GenerateLearningPathAsync(
            Guid userId, 
            Guid companyId,
            int targetMatchPercentage = 85)
        {
            var match = await CalculateCompanyMatchAsync(userId, companyId);
            var gaps = await GetSkillGapsAsync(userId, companyId);

            var weeks = new List<WeeklyPlanDto>();
            var weekNumber = 1;
            var matchGain = 0;

            // Sort gaps by ROI (impact / time)
            var sortedGaps = gaps
                .OrderByDescending(g => g.ExpectedImprovementPercent / (decimal)g.LearningTimeWeeks)
                .ToList();

            foreach (var gap in sortedGaps)
            {
                if (matchGain >= targetMatchPercentage - match.MatchPercentage)
                    break;

                // Create weekly plan for this skill
                for (int i = 0; i < gap.LearningTimeWeeks; i++)
                {
                    var weekPlan = new WeeklyPlanDto
                    {
                        WeekNumber = weekNumber++,
                        Skill = gap.SkillName,
                        Focus = GetWeeklyFocus(gap.SkillName, i, gap.LearningTimeWeeks),
                        Resources = gap.ResourcesAvailable,
                        ProjectMilestone = GetProjectMilestone(gap.SkillName, i, gap.LearningTimeWeeks),
                        TimeCommitmentHours = 15 // Per week
                    };

                    weeks.Add(weekPlan);
                }

                matchGain += (int)gap.ExpectedImprovementPercent;
            }

            var projectedMatch = Math.Min(100, match.MatchPercentage + matchGain);

            return new LearningPathDto
            {
                UserId = userId,
                CompanyId = companyId,
                CurrentMatchPercentage = match.MatchPercentage,
                TargetMatchPercentage = targetMatchPercentage,
                ProjectedMatchPercentage = projectedMatch,
                TotalWeeks = weeks.Count,
                WeeklyPlans = weeks,
                KeyMilestones = ExtractMilestones(weeks),
                EstimatedReadyDate = DateTime.UtcNow.AddDays(weeks.Count * 7),
                SuccessProbability = CalculateSuccessProbability(weeks.Count, sortedGaps.Count)
            };
        }

        // Helper Methods

        private async Task FetchCompanyPublicDataAsync(Company company)
        {
            // Fetch from:
            // 1. LinkedIn company page
            // 2. GitHub (if public repos)
            // 3. Tech stack databases (StackShare, Crunchbase)
            // 4. Recent job postings (LinkedIn, Indeed)

            _logger.LogInformation("Fetching public data for company {CompanyName}", company.Name);
        }

        private List<SkillMatch> ParseUserSkills(Resume resume)
        {
            // Parse resume and extract skills with proficiency levels
            return new List<SkillMatch>();
        }

        private List<SkillMatch> CalculateMatchedSkills(
            List<SkillMatch> userSkills, 
            List<RequiredSkill> companySkills)
        {
            // Return skills that exist in both lists
            return userSkills
                .Where(u => companySkills.Any(c => c.Name.ToLower() == u.SkillName.ToLower()))
                .ToList();
        }

        private int CalculateWeightedMatch(
            List<SkillMatch> matched,
            List<RequiredSkill> missing,
            CompanyProfile profile)
        {
            // Weighted match calculation
            // Skills: 60%
            // Experience: 20%
            // Certifications: 10%
            // Industry background: 10%

            var skillScore = matched.Any() ? 
                (matched.Count * 100) / (matched.Count + missing.Count) : 0;

            return (int)(skillScore * 0.6); // Example: simplified calculation
        }

        private string GetMatchLevel(int percentage)
        {
            return percentage switch
            {
                >= 90 => "Excellent",
                >= 75 => "Very Good",
                >= 60 => "Good",
                >= 45 => "Fair",
                _ => "Needs Work"
            };
        }

        private int EstimateLearningTime(string skillName)
        {
            // Based on skill complexity and market data
            return skillName.ToLower() switch
            {
                var s when s.Contains("kubernetes") => 8,
                var s when s.Contains("python") => 4,
                var s when s.Contains("terraform") => 3,
                var s when s.Contains("rust") => 12,
                _ => 6
            };
        }

        private int EstimateMatchImprovement(SkillDto skill)
        {
            // Importance maps to match improvement
            return (int)(skill.Importance * 1.5);
        }

        private List<string> GetLearningResources(string skillName)
        {
            return skillName.ToLower() switch
            {
                var s when s.Contains("kubernetes") => new()
                {
                    "Linux Academy CKA course",
                    "Killer.sh CKA practice",
                    "Kubernetes official docs"
                },
                _ => new() { "Udemy", "Coursera", "Official docs" }
            };
        }

        private string ExtractProjectName(string aiResponse)
        {
            var lines = aiResponse.Split('\n');
            return lines.FirstOrDefault()?.Trim() ?? "Unnamed Project";
        }

        private string GetWeeklyFocus(string skill, int weekIndex, int totalWeeks)
        {
            if (weekIndex < totalWeeks / 3)
                return $"Master fundamentals of {skill}";
            else if (weekIndex < 2 * totalWeeks / 3)
                return $"Intermediate projects with {skill}";
            else
                return $"Advanced patterns and production {skill}";
        }

        private string GetProjectMilestone(string skill, int weekIndex, int totalWeeks)
        {
            if (weekIndex == 0)
                return $"Complete first {skill} tutorial";
            else if (weekIndex == totalWeeks / 2)
                return $"Build mini-project with {skill}";
            else if (weekIndex == totalWeeks - 1)
                return $"Complete production {skill} project";
            return "";
        }

        private List<string> ExtractMilestones(List<WeeklyPlanDto> weeks)
        {
            return weeks
                .Where(w => !string.IsNullOrEmpty(w.ProjectMilestone))
                .Select(w => w.ProjectMilestone)
                .ToList();
        }

        private decimal CalculateSuccessProbability(int weeks, int skillCount)
        {
            // Higher complexity = lower probability
            // But realistic timeline increases it
            return Math.Min(1m, 1m - (skillCount * 0.1m) + (weeks > 0 ? 0.15m : 0m));
        }
    }

    // ===== DTOs =====

    public class DreamCompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TargetRole { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class DreamCompanyMatchDto
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int MatchPercentage { get; set; }
        public string MatchLevel { get; set; } = string.Empty;
        public List<SkillMatch> MatchedSkills { get; set; } = new();
        public List<SkillDto> MissingSkills { get; set; } = new();
        public DateTime LastCalculatedAt { get; set; }
        public string? NextRecommendedRole { get; set; }
    }

    public class SkillGapDto
    {
        public string SkillName { get; set; } = string.Empty;
        public int Importance { get; set; } // 1-10
        public int LearningTimeWeeks { get; set; }
        public int ExpectedImprovementPercent { get; set; }
        public List<string> ResourcesAvailable { get; set; } = new();
    }

    public class ProjectRecommendationDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> RelevantSkills { get; set; } = new();
        public int EstimatedWeeks { get; set; }
        public int MatchImprovementPercent { get; set; }
        public string Difficulty { get; set; } = string.Empty;
    }

    public class CertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string ExamCost { get; set; } = string.Empty;
        public int StudyTimeWeeks { get; set; }
        public string PassRate { get; set; } = string.Empty;
        public string Relevance { get; set; } = string.Empty;
        public List<string> PrepResources { get; set; } = new();
    }

    public class LearningPathDto
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public int CurrentMatchPercentage { get; set; }
        public int TargetMatchPercentage { get; set; }
        public int ProjectedMatchPercentage { get; set; }
        public int TotalWeeks { get; set; }
        public List<WeeklyPlanDto> WeeklyPlans { get; set; } = new();
        public List<string> KeyMilestones { get; set; } = new();
        public DateTime EstimatedReadyDate { get; set; }
        public decimal SuccessProbability { get; set; }
    }

    public class WeeklyPlanDto
    {
        public int WeekNumber { get; set; }
        public string Skill { get; set; } = string.Empty;
        public string Focus { get; set; } = string.Empty;
        public List<string> Resources { get; set; } = new();
        public string ProjectMilestone { get; set; } = string.Empty;
        public int TimeCommitmentHours { get; set; }
    }

    public class SkillMatch
    {
        public string SkillName { get; set; } = string.Empty;
        public int UserLevel { get; set; } // 1-5
        public int CompanyRequiredLevel { get; set; } // 1-5
    }

    public class SkillDto
    {
        public string SkillName { get; set; } = string.Empty;
        public int Importance { get; set; } // 1-10
    }

    public class CompanyProfile
    {
        public string Name { get; set; } = string.Empty;
        public List<RequiredSkill> RequiredSkills { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public string TechStack { get; set; } = string.Empty;
    }

    public class RequiredSkill
    {
        public string Name { get; set; } = string.Empty;
        public int Importance { get; set; } // 1-10
    }
}
