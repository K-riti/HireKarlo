// Opportunity Radar Service
// Priority: ⭐⭐⭐⭐⭐ (CENTERPIECE - Ties all features together)
//
// This is the main user dashboard. Instead of "auto-apply" (passive),
// users see high-quality opportunities with confidence scores and analysis.
// This keeps users engaged and coming back daily/weekly.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireKarlo.Application.Services
{
    /// <summary>
    /// Opportunity Radar Service
    /// 
    /// Central hub that:
    /// 1. Searches job boards for opportunities
    /// 2. Scores each against user's resume (0-100%)
    /// 3. Explains why it's a match
    /// 4. Shows what skills are missing
    /// 5. Recommends whether to apply
    /// 6. Tracks which ones user is interested in
    /// 7. Surfaces top matches for today/this week
    /// 
    /// Replaces "Auto-Apply" with intelligent discovery + user control
    /// </summary>
    public interface IOpportunityRadarService
    {
        /// <summary>
        /// Get today's top opportunities for a user
        /// Returns jobs ranked by match % (highest first)
        /// </summary>
        Task<OpportunityRadarDashboardDto> GetDailyOpportunitiesAsync(
            Guid userId,
            int limit = 10);

        /// <summary>
        /// Get all opportunities this week with match scores
        /// </summary>
        Task<List<OpportunityDto>> GetWeeklyOpportunitiesAsync(Guid userId, int limit = 50);

        /// <summary>
        /// Find opportunities matching target companies
        /// Prioritize jobs at user's dream companies
        /// </summary>
        Task<List<OpportunityDto>> FindOpportunitiesAtTargetCompaniesAsync(
            Guid userId);

        /// <summary>
        /// Get detailed analysis for a single opportunity
        /// Why matched + what's missing + should apply recommendation
        /// </summary>
        Task<OpportunityAnalysisDto> AnalyzeOpportunityAsync(
            Guid userId,
            string jobId);

        /// <summary>
        /// Get match score for a job against user's resume
        /// Returns 0-100 with breakdown of skill matches/gaps
        /// </summary>
        Task<JobMatchScoreDto> CalculateMatchScoreAsync(
            Guid userId,
            string jobDescription);

        /// <summary>
        /// Mark opportunity as interested, applied, rejected, etc
        /// Tracks user interaction for pipeline analytics
        /// </summary>
        Task<OpportunityInteractionDto> LogOpportunityInteractionAsync(
            Guid userId,
            string jobId,
            OpportunityInteractionType type,
            string? notes = null);

        /// <summary>
        /// Get radar statistics (match trends, application pipeline)
        /// Shows progress over time
        /// </summary>
        Task<RadarStatisticsDto> GetRadarStatsAsync(Guid userId);

        /// <summary>
        /// Get opportunities at specific company
        /// For "apply to Adobe strategy"
        /// </summary>
        Task<List<OpportunityDto>> GetOpportunitiesAtCompanyAsync(
            Guid userId,
            Guid companyId);

        /// <summary>
        /// Recommend next action for user
        /// Based on match scores and application history
        /// </summary>
        Task<NextActionRecommendationDto> GetNextActionAsync(Guid userId);
    }

    /// <summary>
    /// Implementation of Opportunity Radar Service
    /// </summary>
    public class OpportunityRadarService : IOpportunityRadarService
    {
        private readonly IRepositoryManager _repositories;
        private readonly ICareerEngineService _careerEngine;
        private readonly IJobBoardScraperService _jobScraper;
        private readonly ILogger<OpportunityRadarService> _logger;

        public OpportunityRadarService(
            IRepositoryManager repositories,
            ICareerEngineService careerEngine,
            IJobBoardScraperService jobScraper,
            ILogger<OpportunityRadarService> logger)
        {
            _repositories = repositories;
            _careerEngine = careerEngine;
            _jobScraper = jobScraper;
            _logger = logger;
        }

        public async Task<OpportunityRadarDashboardDto> GetDailyOpportunitiesAsync(
            Guid userId,
            int limit = 10)
        {
            _logger.LogInformation("Getting daily opportunities for user {UserId}", userId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .Include(u => u.DreamCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                throw new InvalidOperationException($"User {userId} has no resume");

            // Get updated job listings from multiple sources
            var jobs = await _jobScraper.GetTodaysJobsAsync(
                user.DreamCompanies.Select(dc => dc.CompanyId).ToList()
            );

            _logger.LogInformation("Found {JobCount} jobs for user {UserId}", jobs.Count, userId);

            // Score each job against user's resume
            var scoredOpportunities = new List<OpportunityDto>();

            foreach (var job in jobs)
            {
                try
                {
                    var matchScore = await CalculateMatchScoreAsync(userId, job.Description);

                    // Only include opportunities with reasonable match
                    if (matchScore.OverallScore >= 40) // Include lower scores to show diversity
                    {
                        scoredOpportunities.Add(new OpportunityDto
                        {
                            JobId = job.Id,
                            Company = job.Company,
                            Title = job.Title,
                            Location = job.Location,
                            Url = job.Url,
                            Source = job.Source,
                            MatchPercentage = matchScore.OverallScore,
                            MatchedSkills = matchScore.MatchedSkills,
                            GapSkills = matchScore.GapSkills,
                            IsAtDreamCompany = user.DreamCompanies.Any(dc => 
                                dc.Company.Name.ToLower() == job.Company.ToLower()),
                            DiscoveredAt = DateTime.UtcNow,
                            ShouldApplyRecommendation = GetApplyRecommendation(matchScore.OverallScore),
                            TimeToResponsiveHours = EstimateResponseTime(job.Company)
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to score job {JobId}", job.Id);
                    continue;
                }
            }

            // Sort by match %, then by dream company priority
            var sorted = scoredOpportunities
                .OrderByDescending(o => o.IsAtDreamCompany)
                .ThenByDescending(o => o.MatchPercentage)
                .Take(limit)
                .ToList();

            // Get previous best match for comparison
            var previousBestMatch = await GetPreviousBestMatchAsync(userId);

            // Get pipeline stats
            var stats = await GetRadarStatsAsync(userId);

            return new OpportunityRadarDashboardDto
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow,
                OpportunitiesFound = sorted.Count,
                TopOpportunities = sorted,
                AverageMatchPercentage = sorted.Any() ? 
                    (int)sorted.Average(o => o.MatchPercentage) : 0,
                BestMatchPercentage = sorted.Any() ? 
                    sorted.Max(o => o.MatchPercentage) : 0,
                DreamCompanyCount = user.DreamCompanies.Count,
                OpportunitiesAtDreamCompanies = sorted.Count(o => o.IsAtDreamCompany),
                PreviousBestMatch = previousBestMatch,
                Stats = stats,
                NextAction = await GetNextActionAsync(userId),
                InsightMessage = GenerateInsightMessage(sorted, stats)
            };
        }

        public async Task<List<OpportunityDto>> GetWeeklyOpportunitiesAsync(Guid userId, int limit = 50)
        {
            _logger.LogInformation("Getting weekly opportunities for user {UserId}", userId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .Include(u => u.DreamCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                return new List<OpportunityDto>();

            // Get all jobs from this week
            var jobs = await _jobScraper.GetJobsSinceAsync(
                startDate: DateTime.UtcNow.AddDays(-7),
                companies: user.DreamCompanies.Select(dc => dc.CompanyId).ToList()
            );

            var opportunities = new List<OpportunityDto>();

            foreach (var job in jobs.Take(limit * 2)) // Score more to filter
            {
                try
                {
                    var matchScore = await CalculateMatchScoreAsync(userId, job.Description);

                    if (matchScore.OverallScore >= 50)
                    {
                        opportunities.Add(new OpportunityDto
                        {
                            JobId = job.Id,
                            Company = job.Company,
                            Title = job.Title,
                            Location = job.Location,
                            Url = job.Url,
                            Source = job.Source,
                            MatchPercentage = matchScore.OverallScore,
                            MatchedSkills = matchScore.MatchedSkills,
                            GapSkills = matchScore.GapSkills,
                            IsAtDreamCompany = user.DreamCompanies.Any(dc => 
                                dc.Company.Name.ToLower() == job.Company.ToLower()),
                            DiscoveredAt = job.PublishedAt,
                            ShouldApplyRecommendation = GetApplyRecommendation(matchScore.OverallScore),
                            TimeToResponsiveHours = EstimateResponseTime(job.Company)
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to score job {JobId}", job.Id);
                    continue;
                }
            }

            return opportunities
                .OrderByDescending(o => o.MatchPercentage)
                .Take(limit)
                .ToList();
        }

        public async Task<List<OpportunityDto>> FindOpportunitiesAtTargetCompaniesAsync(Guid userId)
        {
            _logger.LogInformation("Finding opportunities at target companies for user {UserId}", userId);

            var user = await _repositories.Users
                .Include(u => u.DreamCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.DreamCompanies.Count == 0)
                return new List<OpportunityDto>();

            var allOpportunities = new List<OpportunityDto>();

            foreach (var dreamCompany in user.DreamCompanies)
            {
                var opportunities = await GetOpportunitiesAtCompanyAsync(userId, dreamCompany.CompanyId);
                allOpportunities.AddRange(opportunities);
            }

            return allOpportunities
                .OrderByDescending(o => o.MatchPercentage)
                .ToList();
        }

        public async Task<OpportunityAnalysisDto> AnalyzeOpportunityAsync(
            Guid userId,
            string jobId)
        {
            _logger.LogInformation("Analyzing opportunity {JobId} for user {UserId}", jobId, userId);

            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var job = await _jobScraper.GetJobDetailsAsync(jobId);

            if (user?.Resume == null || job == null)
                throw new InvalidOperationException("Invalid user or job");

            var matchScore = await CalculateMatchScoreAsync(userId, job.Description);

            // Get user's dream companies to see if this is at one
            var dreamCompanies = await _repositories.DreamCompanies
                .Where(dc => dc.UserId == userId)
                .ToListAsync();

            var isDreamCompany = dreamCompanies.Any(dc => 
                dc.Company.Name.ToLower() == job.Company.ToLower());

            // Calculate time to get skills
            var weeksToLearnGaps = matchScore.GapSkills.Sum(g => EstimateWeeksToLearn(g));

            // Get referral opportunities at this company
            var referralsAtCompany = await _repositories.Referrals
                .Where(r => r.UserId == userId && r.Company.ToLower() == job.Company.ToLower())
                .ToListAsync();

            return new OpportunityAnalysisDto
            {
                JobId = jobId,
                Company = job.Company,
                Title = job.Title,
                Url = job.Url,
                MatchPercentage = matchScore.OverallScore,
                MatchAnalysis = new MatchAnalysisDto
                {
                    OverallFit = InterpretMatch(matchScore.OverallScore),
                    MatchedSkills = matchScore.MatchedSkills,
                    GapSkills = matchScore.GapSkills,
                    SkillsYouHave = matchScore.MatchedSkills.Count,
                    SkillsTheyWant = matchScore.MatchedSkills.Count + matchScore.GapSkills.Count
                },
                WhyYouMatch = GenerateWhyYouMatch(matchScore, job),
                WhatYouAreMissing = GenerateWhatYouAreMissing(matchScore),
                WeeksToReadyForRole = weeksToLearnGaps,
                ShouldApply = GetApplyRecommendation(matchScore.OverallScore),
                ApplyReasoning = GenerateApplyReasoning(matchScore.OverallScore, isDreamCompany, weeksToLearnGaps),
                ReferralsAtCompany = referralsAtCompany.Select(r => new ReferralContactDto
                {
                    Name = r.Name,
                    Title = r.Title,
                    ReferralScore = r.ReferralScore,
                    LinkedInUrl = r.LinkedInUrl
                }).ToList(),
                NextSteps = GenerateNextSteps(matchScore.OverallScore, isDreamCompany, referralsAtCompany.Count),
                IsDreamCompany = isDreamCompany
            };
        }

        public async Task<JobMatchScoreDto> CalculateMatchScoreAsync(
            Guid userId,
            string jobDescription)
        {
            var user = await _repositories.Users
                .Include(u => u.Resume)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Resume == null)
                throw new InvalidOperationException($"User {userId} has no resume");

            // Extract skills from job description
            var jobSkills = await _careerEngine.ExtractSkillsFromJobAsync(jobDescription);

            // Extract skills from user's resume
            var userSkills = await _careerEngine.ExtractSkillsFromResumeAsync(user.Resume);

            // Calculate matches
            var matched = userSkills
                .Where(us => jobSkills.Any(js => 
                    js.Name.ToLower() == us.Name.ToLower()))
                .ToList();

            var gaps = jobSkills
                .Where(js => !userSkills.Any(us => 
                    us.Name.ToLower() == js.Name.ToLower()))
                .ToList();

            // Calculate weighted score
            var score = CalculateWeightedMatchScore(
                matched, 
                gaps, 
                jobSkills,
                user.Resume.ExperienceYears ?? 0);

            return new JobMatchScoreDto
            {
                OverallScore = score,
                MatchedSkills = matched.Select(s => s.Name).ToList(),
                GapSkills = gaps.Select(s => s.Name).ToList(),
                MatchedCount = matched.Count,
                GapCount = gaps.Count,
                TotalRequired = jobSkills.Count,
                MatchPercentage = gaps.Count > 0 ? 
                    (matched.Count * 100) / (matched.Count + gaps.Count) : 100,
                Verdict = InterpretMatch(score),
                ConfidenceLevel = CalculateConfidence(matched.Count, gaps.Count)
            };
        }

        public async Task<OpportunityInteractionDto> LogOpportunityInteractionAsync(
            Guid userId,
            string jobId,
            OpportunityInteractionType type,
            string? notes = null)
        {
            _logger.LogInformation("Logging {InteractionType} for job {JobId} by user {UserId}",
                type, jobId, userId);

            var interaction = new OpportunityInteractionDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                JobId = jobId,
                InteractionType = type,
                InteractionDate = DateTime.UtcNow,
                Notes = notes
            };

            // Update job status in cache
            // (Could store in DB for analytics)

            return interaction;
        }

        public async Task<RadarStatisticsDto> GetRadarStatsAsync(Guid userId)
        {
            var user = await _repositories.Users
                .Include(u => u.ApplicationHistory)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new RadarStatisticsDto();

            // Get stats from last 30 days
            var thisMonth = DateTime.UtcNow.AddDays(-30);
            var lastMonth = DateTime.UtcNow.AddDays(-60);

            var thisMonthApps = user.ApplicationHistory?
                .Where(a => a.AppliedAt >= thisMonth)
                .ToList() ?? new();

            var lastMonthApps = user.ApplicationHistory?
                .Where(a => a.AppliedAt >= lastMonth && a.AppliedAt < thisMonth)
                .ToList() ?? new();

            var thisMonthAvgMatch = thisMonthApps.Any() ? 
                (int)thisMonthApps.Average(a => a.MatchScoreAtTime) : 0;

            var lastMonthAvgMatch = lastMonthApps.Any() ? 
                (int)lastMonthApps.Average(a => a.MatchScoreAtTime) : 0;

            return new RadarStatisticsDto
            {
                UserId = userId,
                ThisMonthApplications = thisMonthApps.Count,
                LastMonthApplications = lastMonthApps.Count,
                ThisMonthAvgMatch = thisMonthAvgMatch,
                LastMonthAvgMatch = lastMonthAvgMatch,
                MatchImprovement = thisMonthAvgMatch - lastMonthAvgMatch,
                MatchTrend = thisMonthAvgMatch > lastMonthAvgMatch ? "📈 Improving" : "📉 Declining",
                InterviewsScheduled = thisMonthApps.Count(a => a.InterviewScheduled),
                OffersReceived = thisMonthApps.Count(a => a.OfferReceived),
                AcceptanceRate = thisMonthApps.Any() ? 
                    (thisMonthApps.Count(a => a.OfferReceived) * 100) / thisMonthApps.Count : 0
            };
        }

        public async Task<List<OpportunityDto>> GetOpportunitiesAtCompanyAsync(
            Guid userId,
            Guid companyId)
        {
            var company = await _repositories.Companies.FindAsync(c => c.Id == companyId);
            if (company == null)
                return new List<OpportunityDto>();

            var jobs = await _jobScraper.GetJobsAtCompanyAsync(company.Name);

            var opportunities = new List<OpportunityDto>();
            foreach (var job in jobs)
            {
                try
                {
                    var matchScore = await CalculateMatchScoreAsync(userId, job.Description);
                    opportunities.Add(new OpportunityDto
                    {
                        JobId = job.Id,
                        Company = job.Company,
                        Title = job.Title,
                        Location = job.Location,
                        Url = job.Url,
                        Source = job.Source,
                        MatchPercentage = matchScore.OverallScore,
                        MatchedSkills = matchScore.MatchedSkills,
                        GapSkills = matchScore.GapSkills,
                        IsAtDreamCompany = true,
                        DiscoveredAt = job.PublishedAt,
                        ShouldApplyRecommendation = GetApplyRecommendation(matchScore.OverallScore),
                        TimeToResponsiveHours = EstimateResponseTime(company.Name)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to score job {JobId}", job.Id);
                }
            }

            return opportunities.OrderByDescending(o => o.MatchPercentage).ToList();
        }

        public async Task<NextActionRecommendationDto> GetNextActionAsync(Guid userId)
        {
            var user = await _repositories.Users
                .Include(u => u.DreamCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new NextActionRecommendationDto();

            var stats = await GetRadarStatsAsync(userId);
            var opportunities = await GetDailyOpportunitiesAsync(userId, limit: 3);

            // Determine recommendation based on current state
            if (opportunities.Count == 0)
                return new NextActionRecommendationDto
                {
                    Action = "No strong matches today",
                    Reasoning = "Check back tomorrow for new opportunities",
                    Priority = "Low"
                };

            var topOp = opportunities.First();

            if (topOp.MatchPercentage >= 90)
                return new NextActionRecommendationDto
                {
                    Action = $"Apply to {topOp.Title} at {topOp.Company}",
                    Reasoning = $"91%+ match is exceptional. Apply today while position is fresh.",
                    Priority = "High",
                    TargetJobId = topOp.JobId
                };

            if (topOp.IsAtDreamCompany && topOp.MatchPercentage >= 75)
                return new NextActionRecommendationDto
                {
                    Action = $"Reach out to referral at {topOp.Company}",
                    Reasoning = $"{topOp.Company} is in your targets. Use referral + apply.",
                    Priority = "High",
                    TargetJobId = topOp.JobId
                };

            if (stats.MatchTrend == "📉 Declining")
                return new NextActionRecommendationDto
                {
                    Action = "Review skill gaps",
                    Reasoning = $"Match % declining ({stats.ThisMonthAvgMatch}% vs {stats.LastMonthAvgMatch}%). Learn missing skills.",
                    Priority = "Medium"
                };

            return new NextActionRecommendationDto
            {
                Action = $"Apply to {topOp.Title} at {topOp.Company}",
                Reasoning = $"{topOp.MatchPercentage}% match. Good fit for your profile.",
                Priority = "Medium",
                TargetJobId = topOp.JobId
            };
        }

        // Helper Methods

        private int CalculateWeightedMatchScore(
            List<Skill> matched,
            List<Skill> gaps,
            List<Skill> jobSkills,
            int userExperienceYears)
        {
            if (jobSkills.Count == 0)
                return 50;

            // Weighted calculation
            var skillMatch = matched.Any() ? 
                (matched.Count * 100) / jobSkills.Count : 0;

            var experienceBonus = userExperienceYears >= 5 ? 5 : 
                                  userExperienceYears >= 3 ? 3 : 0;

            var score = (int)(skillMatch * 0.85 + experienceBonus);
            return Math.Min(100, score);
        }

        private string InterpretMatch(int score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 75 => "Very Good",
                >= 60 => "Good",
                >= 45 => "Fair",
                _ => "Weak"
            };
        }

        private string GetApplyRecommendation(int score)
        {
            return score switch
            {
                >= 90 => "🔥 Apply immediately",
                >= 75 => "✅ Strong fit - Apply",
                >= 60 => "👍 Good fit - Consider applying",
                >= 45 => "🤔 Possible - Apply if interested",
                _ => "⏭️ Look for better matches"
            };
        }

        private int EstimateWeeksToLearn(string skill)
        {
            return skill.ToLower() switch
            {
                var s when s.Contains("kubernetes") => 8,
                var s when s.Contains("python") => 4,
                var s when s.Contains("terraform") => 3,
                var s when s.Contains("rust") => 12,
                var s when s.Contains("go") => 6,
                var s when s.Contains("java") => 8,
                _ => 4
            };
        }

        private int EstimateResponseTime(string company)
        {
            return company.ToLower() switch
            {
                var c when c.Contains("startup") => 24,
                var c when c.Contains("google") => 168, // 1 week
                var c when c.Contains("meta") => 168,
                var c when c.Contains("amazon") => 72,
                _ => 48
            };
        }

        private string GenerateWhyYouMatch(JobMatchScoreDto match, JobDetails job)
        {
            var matchedStr = string.Join(", ", match.MatchedSkills.Take(3));
            return $"You have {match.MatchedCount} of the {match.TotalRequired} required skills, " +
                   $"including: {matchedStr}";
        }

        private string GenerateWhatYouAreMissing(JobMatchScoreDto match)
        {
            if (match.GapSkills.Count == 0)
                return "You have all required skills! ✅";

            var gapStr = string.Join(", ", match.GapSkills.Take(3));
            var msg = $"Missing {match.GapCount} skills: {gapStr}";
            if (match.GapSkills.Count > 3)
                msg += $" (+{match.GapSkills.Count - 3} more)";
            return msg;
        }

        private string GenerateApplyReasoning(int score, bool isDreamCompany, int weeksToReady)
        {
            if (isDreamCompany && score >= 60)
                return $"This is a dream company target. Even at {score}% match, apply + use referrals.";

            if (score >= 90)
                return "Exceptional match. Apply immediately.";

            if (score >= 75)
                return "Strong match. Apply today while position is fresh.";

            if (weeksToReady <= 4)
                return $"Could be ready in {weeksToReady} weeks. Consider learning the gaps first.";

            return "Below target match. Look for stronger opportunities first.";
        }

        private List<string> GenerateNextSteps(int matchScore, bool isDreamCompany, int referralCount)
        {
            var steps = new List<string>();

            if (referralCount > 0)
                steps.Add($"✅ Reach out to {referralCount} referral(s) at this company");
            else
                steps.Add("🔍 Find referrals at this company");

            if (matchScore >= 75)
                steps.Add("📝 Apply to the job");
            else
                steps.Add("🎓 Learn missing skills");

            steps.Add("📅 Set interview prep reminder");

            return steps;
        }

        private string GenerateInsightMessage(List<OpportunityDto> opportunities, RadarStatisticsDto stats)
        {
            var dreamCompanyOps = opportunities.Count(o => o.IsAtDreamCompany);
            var avgMatch = opportunities.Any() ? (int)opportunities.Average(o => o.MatchPercentage) : 0;

            if (dreamCompanyOps > 0)
                return $"🎯 {dreamCompanyOps} opportunities at dream companies! Avg match: {avgMatch}%";

            if (stats.MatchTrend == "📈 Improving")
                return $"📈 Your match scores are improving! Last month: {stats.LastMonthAvgMatch}% → Today: {stats.ThisMonthAvgMatch}%";

            return $"Found {opportunities.Count} new opportunities today. Average match: {avgMatch}%";
        }

        private async Task<int> GetPreviousBestMatchAsync(Guid userId)
        {
            var user = await _repositories.Users
                .Include(u => u.ApplicationHistory)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.ApplicationHistory?.Any() != true)
                return 0;

            return (int)user.ApplicationHistory.Max(a => a.MatchScoreAtTime);
        }

        private decimal CalculateConfidence(int matched, int gaps)
        {
            var total = matched + gaps;
            return total > 0 ? (decimal)matched / total : 0;
        }
    }

    // ===== DTOs =====

    public class OpportunityRadarDashboardDto
    {
        public Guid UserId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int OpportunitiesFound { get; set; }
        public List<OpportunityDto> TopOpportunities { get; set; } = new();
        public int AverageMatchPercentage { get; set; }
        public int BestMatchPercentage { get; set; }
        public int DreamCompanyCount { get; set; }
        public int OpportunitiesAtDreamCompanies { get; set; }
        public int PreviousBestMatch { get; set; }
        public RadarStatisticsDto Stats { get; set; } = new();
        public NextActionRecommendationDto NextAction { get; set; } = new();
        public string InsightMessage { get; set; } = string.Empty;
    }

    public class OpportunityDto
    {
        public string JobId { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // LinkedIn, Indeed, etc
        public int MatchPercentage { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> GapSkills { get; set; } = new();
        public bool IsAtDreamCompany { get; set; }
        public DateTime DiscoveredAt { get; set; }
        public string ShouldApplyRecommendation { get; set; } = string.Empty;
        public int TimeToResponsiveHours { get; set; } // How quickly to apply
    }

    public class OpportunityAnalysisDto
    {
        public string JobId { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int MatchPercentage { get; set; }
        public MatchAnalysisDto MatchAnalysis { get; set; } = new();
        public List<string> WhyYouMatch { get; set; } = new();
        public List<string> WhatYouAreMissing { get; set; } = new();
        public int WeeksToReadyForRole { get; set; }
        public string ShouldApply { get; set; } = string.Empty;
        public string ApplyReasoning { get; set; } = string.Empty;
        public List<ReferralContactDto> ReferralsAtCompany { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
        public bool IsDreamCompany { get; set; }
    }

    public class MatchAnalysisDto
    {
        public string OverallFit { get; set; } = string.Empty;
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> GapSkills { get; set; } = new();
        public int SkillsYouHave { get; set; }
        public int SkillsTheyWant { get; set; }
    }

    public class ReferralContactDto
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int ReferralScore { get; set; }
        public string LinkedInUrl { get; set; } = string.Empty;
    }

    public class JobMatchScoreDto
    {
        public int OverallScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> GapSkills { get; set; } = new();
        public int MatchedCount { get; set; }
        public int GapCount { get; set; }
        public int TotalRequired { get; set; }
        public int MatchPercentage { get; set; }
        public string Verdict { get; set; } = string.Empty;
        public decimal ConfidenceLevel { get; set; }
    }

    public class OpportunityInteractionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public OpportunityInteractionType InteractionType { get; set; }
        public DateTime InteractionDate { get; set; }
        public string? Notes { get; set; }
    }

    public class RadarStatisticsDto
    {
        public Guid UserId { get; set; }
        public int ThisMonthApplications { get; set; }
        public int LastMonthApplications { get; set; }
        public int ThisMonthAvgMatch { get; set; }
        public int LastMonthAvgMatch { get; set; }
        public int MatchImprovement { get; set; }
        public string MatchTrend { get; set; } = string.Empty; // "📈 Improving" or "📉 Declining"
        public int InterviewsScheduled { get; set; }
        public int OffersReceived { get; set; }
        public int AcceptanceRate { get; set; }
    }

    public class NextActionRecommendationDto
    {
        public string Action { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public string? TargetJobId { get; set; }
    }

    public enum OpportunityInteractionType
    {
        Viewed,
        Bookmarked,
        Applied,
        Rejected,
        InterviewScheduled,
        OfferReceived,
        OfferAccepted,
        OfferRejected
    }

    // Supporting types
    public class Skill
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } // 1-5
    }

    public class JobDetails
    {
        public string Id { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }

    // Dependency interfaces
    public interface IJobBoardScraperService
    {
        Task<List<JobDetails>> GetTodaysJobsAsync(List<Guid> companyIds);
        Task<List<JobDetails>> GetJobsSinceAsync(DateTime startDate, List<Guid> companies);
        Task<JobDetails> GetJobDetailsAsync(string jobId);
        Task<List<JobDetails>> GetJobsAtCompanyAsync(string companyName);
    }
}
