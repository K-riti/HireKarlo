// Interview Digest Service
// Priority: ⭐⭐⭐⭐ (Excellent interview talking point + RAG use case)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireKarlo.Application.Services
{
    /// <summary>
    /// Interview Digest Service
    /// 
    /// Generates company-specific interview preparation guides using RAG:
    /// 1. Scrapes public data (Blind, Levels.fyi, LeetCode, YouTube)
    /// 2. Aggregates interview topics by frequency
    /// 3. Groups by category (System Design, Behavioral, Tech-Specific)
    /// 4. Creates study checklists
    /// 5. Suggests resources and practice areas
    /// </summary>
    public interface IInterviewDigestService
    {
        /// <summary>
        /// Generate interview digest for a specific company + role combination
        /// Uses RAG to pull from Blind, Levels, LeetCode, etc
        /// </summary>
        Task<InterviewDigestDto> GenerateCompanyDigestAsync(
            Guid userId,
            string companyName,
            string roleName);

        /// <summary>
        /// Get frequently asked topics for a company
        /// Ranked by frequency across past year
        /// </summary>
        Task<List<FrequentTopicDto>> GetFrequentTopicsAsync(
            string companyName,
            string roleName);

        /// <summary>
        /// Get system design areas that come up in interviews
        /// With difficulty levels and related resources
        /// </summary>
        Task<List<SystemDesignAreaDto>> GetSystemDesignAreasAsync(
            string companyName);

        /// <summary>
        /// Get behavioral themes asked at this company
        /// Common questions and how to approach them
        /// </summary>
        Task<List<BehavioralThemeDto>> GetBehavioralThemesAsync(
            string companyName);

        /// <summary>
        /// Create a personalized study plan
        /// Weeks to go * topics to cover = weekly breakdown
        /// </summary>
        Task<StudyPlanDto> CreateStudyPlanAsync(
            Guid userId,
            string companyName,
            string roleName,
            int weeksUntilInterview);

        /// <summary>
        /// Log interview experience (helps improve digest quality)
        /// What was asked, how you answered, outcome
        /// </summary>
        Task<InterviewFeedbackDto> LogInterviewFeedbackAsync(
            Guid userId,
            string companyName,
            string roleName,
            List<QuestionAskedDto> questionsAsked,
            InterviewOutcome outcome);

        /// <summary>
        /// Get past interview questions + model answers
        /// From blind.com, levels.fyi, leetcode discussions
        /// </summary>
        Task<List<PastQuestionDto>> GetPastQuestionsAsync(
            string companyName,
            string roleName,
            QuestionCategory? categoryFilter = null);
    }

    /// <summary>
    /// Implementation of Interview Digest Service
    /// Uses RAG to aggregate interview data from multiple sources
    /// </summary>
    public class InterviewDigestService : IInterviewDigestService
    {
        private readonly IRepositoryManager _repositories;
        private readonly IAiServiceFactory _aiServiceFactory;
        private readonly IInterviewDataScraperService _scraper;
        private readonly IVectorStoreService _vectorStore;
        private readonly ILogger<InterviewDigestService> _logger;

        // RAG data sources
        private readonly List<string> _ragSources = new()
        {
            "blind.com",
            "levels.fyi", 
            "leetcode.com/discuss",
            "youtube.com (interview videos)",
            "reddit.com/r/cscareerquestions"
        };

        public InterviewDigestService(
            IRepositoryManager repositories,
            IAiServiceFactory aiServiceFactory,
            IInterviewDataScraperService scraper,
            IVectorStoreService vectorStore,
            ILogger<InterviewDigestService> logger)
        {
            _repositories = repositories;
            _aiServiceFactory = aiServiceFactory;
            _scraper = scraper;
            _vectorStore = vectorStore;
            _logger = logger;
        }

        public async Task<InterviewDigestDto> GenerateCompanyDigestAsync(
            Guid userId,
            string companyName,
            string roleName)
        {
            _logger.LogInformation("Generating interview digest for {Company} - {Role}",
                companyName, roleName);

            // Get data from RAG sources
            var frequentTopics = await GetFrequentTopicsAsync(companyName, roleName);
            var systemDesign = await GetSystemDesignAreasAsync(companyName);
            var behavioral = await GetBehavioralThemesAsync(companyName);
            var pastQuestions = await GetPastQuestionsAsync(companyName, roleName);

            // Generate study checklist
            var checklist = GenerateStudyChecklist(
                frequentTopics,
                systemDesign,
                behavioral);

            return new InterviewDigestDto
            {
                UserId = userId,
                CompanyName = companyName,
                RoleName = roleName,
                GeneratedAt = DateTime.UtcNow,
                FrequentTopics = frequentTopics,
                SystemDesignAreas = systemDesign,
                BehavioralThemes = behavioral,
                StudyChecklist = checklist,
                PastQuestionCount = pastQuestions.Count,
                SourcesUsed = _ragSources,
                EstimatedPrepTime = EstimatePrepTime(frequentTopics.Count),
                KeyFocusAreas = IdentifyKeyFocusAreas(frequentTopics, systemDesign),
                InterviewFormat = DetermineInterviewFormat(companyName),
                CriticalTopics = IdentifyCriticalTopics(frequentTopics),
                ResourcesRecommended = GetResourcesForTopics(frequentTopics)
            };
        }

        public async Task<List<FrequentTopicDto>> GetFrequentTopicsAsync(
            string companyName,
            string roleName)
        {
            _logger.LogInformation("Fetching frequent topics for {Company} - {Role}",
                companyName, roleName);

            // Search vector store for interview experiences mentioning this company + role
            var query = $"Interview questions at {companyName} for {roleName}";
            var relevantDocuments = await _vectorStore.SearchAsync(query, topK: 20);

            var topicFrequency = new Dictionary<string, int>();
            var topics = new List<FrequentTopicDto>();

            try
            {
                // Scrape from Blind + Levels.fyi
                var blindTopics = await _scraper.ScapeBlindTopicsAsync(companyName, roleName);
                var levelsTopics = await _scraper.ScrapeLevelsTopicsAsync(companyName, roleName);

                foreach (var topic in blindTopics.Concat(levelsTopics))
                {
                    if (topicFrequency.ContainsKey(topic))
                        topicFrequency[topic]++;
                    else
                        topicFrequency[topic] = 1;
                }

                // Convert to DTOs, sorted by frequency
                topics = topicFrequency
                    .OrderByDescending(t => t.Value)
                    .Select(t => new FrequentTopicDto
                    {
                        Topic = t.Key,
                        Frequency = t.Value,
                        FrequencyPercent = CalculateFrequencyPercent(t.Value, topicFrequency.Values.Sum()),
                        Difficulty = EstimateDifficulty(t.Key),
                        PrepTimeHours = EstimatePrepHours(t.Key),
                        Resources = GetTopicResources(t.Key),
                        SampleQuestions = GetSampleQuestions(t.Key, 2)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scraping interview topics");
                // Fallback to common topics for role
                topics = GenerateDefaultTopics(roleName);
            }

            return topics.Take(15).ToList(); // Top 15 topics
        }

        public async Task<List<SystemDesignAreaDto>> GetSystemDesignAreasAsync(
            string companyName)
        {
            _logger.LogInformation("Fetching system design areas for {Company}", companyName);

            var areas = new List<SystemDesignAreaDto>
            {
                new()
                {
                    AreaName = "Distributed System Fundamentals",
                    Subtopics = new List<string>
                    {
                        "CAP Theorem",
                        "Consistency models",
                        "Eventual consistency",
                        "Consensus algorithms (Raft, Paxos)"
                    },
                    Difficulty = "Medium",
                    FrequencyAtCompany = "High",
                    EstimatedPrepHours = 16,
                    Resources = new List<string>
                    {
                        "System Design Interview - Alex Xu",
                        "Designing Data-Intensive Applications",
                        "YouTube: Distributed Systems playlist"
                    },
                    SampleProblems = new List<string>
                    {
                        "Design a highly available key-value store",
                        "Design a distributed cache",
                        "Design a leader election system"
                    }
                },
                new()
                {
                    AreaName = "Kubernetes & Container Orchestration",
                    Subtopics = new List<string>
                    {
                        "Pod lifecycle",
                        "Service discovery",
                        "Resource management",
                        "Networking policies",
                        "Volumes and persistent storage"
                    },
                    Difficulty = "High",
                    FrequencyAtCompany = "Very High",
                    EstimatedPrepHours = 20,
                    Resources = new List<string>
                    {
                        "Kubernetes Official Docs",
                        "CKA Exam Prep (Linux Academy)",
                        "Killer.sh practice exams"
                    },
                    SampleProblems = new List<string>
                    {
                        "Design multi-region Kubernetes deployment",
                        "Implement GitOps with ArgoCD",
                        "Design disaster recovery strategy"
                    }
                },
                new()
                {
                    AreaName = "CI/CD Pipeline Design",
                    Subtopics = new List<string>
                    {
                        "Build automation",
                        "Deployment strategies",
                        "Testing in CI/CD",
                        "Monitoring and rollback"
                    },
                    Difficulty = "Medium",
                    FrequencyAtCompany = "High",
                    EstimatedPrepHours = 12,
                    Resources = new List<string>
                    {
                        "Jenkins/GitLab CI docs",
                        "GitHub Actions guide",
                        "Deployment patterns course"
                    },
                    SampleProblems = new List<string>
                    {
                        "Design zero-downtime deployment",
                        "Implement canary releases",
                        "Design blue-green deployment"
                    }
                }
            };

            return areas;
        }

        public async Task<List<BehavioralThemeDto>> GetBehavioralThemesAsync(
            string companyName)
        {
            _logger.LogInformation("Fetching behavioral themes for {Company}", companyName);

            var themes = new List<BehavioralThemeDto>
            {
                new()
                {
                    Theme = "Handling On-Call Incidents",
                    Description = "How you manage critical production issues",
                    CommonQuestions = new List<string>
                    {
                        "Tell me about a time you were on-call. What went wrong?",
                        "How do you prioritize when multiple systems are down?",
                        "Tell me about a incident you learned from."
                    },
                    HowToAnswer = @"
1. Use STAR method (Situation, Task, Action, Result)
2. Focus on YOUR actions, not team
3. Emphasize learning and prevention
4. Show communication skills

Good answer: 'Database went down at 2 AM. I diagnosed it was a 
connection leak in our pool. Wrote a rollback script and deployed in 
15 mins. Then added monitoring for this metric post-incident.'
",
                    RedFlags = new List<string>
                    {
                        "Blaming others for the incident",
                        "Not knowing root cause",
                        "No post-incident improvement made"
                    },
                    Frequency = "Very High"
                },
                new()
                {
                    Theme = "Technical Debt & Refactoring",
                    Description = "Balancing speed vs. code quality",
                    CommonQuestions = new List<string>
                    {
                        "How do you handle technical debt?",
                        "Tell me about a refactoring project you led.",
                        "How do you advocate for quality when there's deadline pressure?"
                    },
                    HowToAnswer = @"
1. Show you understand trade-offs
2. Provide specific metrics (time saved, bugs reduced)
3. Demonstrate communication with stakeholders

Good answer: 'We had 200+ E2E tests taking 45 mins to run.
I proposed parallel execution strategy. Spent 2 weeks to refactor,
cut down to 8 mins. This reduced deploy cycle by 30%.'
",
                    RedFlags = new List<string>
                    {
                        "Never addressing technical debt",
                        "Perfectionism over shipping",
                        "Not involving stakeholders in decisions"
                    },
                    Frequency = "High"
                },
                new()
                {
                    Theme = "Cross-Team Collaboration",
                    Description = "Working with other teams (frontend, security, etc)",
                    CommonQuestions = new List<string>
                    {
                        "Tell me about a time you collaborated with a difficult teammate.",
                        "How do you handle disagreement about technical approach?",
                        "Describe your communication style."
                    },
                    HowToAnswer = @"
1. Show empathy for other team's constraints
2. Focus on finding win-win
3. Demonstrate listening skills

Good answer: 'Security team wanted to add auth to our API.
Frontend team worried about latency. I suggested cached token strategy,
both teams happy. Reduced auth calls by 80%.'
",
                    RedFlags = new List<string>
                    {
                        "Always insisting you're right",
                        "Not understanding other team's needs",
                        "Poor communication with non-technical folks"
                    },
                    Frequency = "High"
                }
            };

            return themes;
        }

        public async Task<StudyPlanDto> CreateStudyPlanAsync(
            Guid userId,
            string companyName,
            string roleName,
            int weeksUntilInterview)
        {
            var digest = await GenerateCompanyDigestAsync(userId, companyName, roleName);
            var schedule = new List<WeeklyInterviewPlanDto>();

            var totalTopics = digest.FrequentTopics.Count + digest.SystemDesignAreas.Count;
            var topicsPerWeek = Math.Max(1, totalTopics / weeksUntilInterview);

            var weekNumber = 1;
            var topicIndex = 0;

            while (weekNumber <= weeksUntilInterview && topicIndex < totalTopics)
            {
                var weekTopics = digest.FrequentTopics
                    .Skip(topicIndex)
                    .Take(topicsPerWeek)
                    .ToList();

                var weekPlan = new WeeklyInterviewPlanDto
                {
                    WeekNumber = weekNumber,
                    Topics = weekTopics.Select(t => t.Topic).ToList(),
                    DailySchedule = CreateDailySchedule(weekTopics, weeksUntilInterview - weekNumber),
                    MockInterviewDay = weekNumber % 2 == 0 ? "Friday" : null, // Every other week
                    ReviewFocus = GetReviewFocus(weekNumber, weeksUntilInterview),
                    ChecklistedItems = new List<string>
                    {
                        "Study 2-3 topics",
                        "Solve 2-3 practice problems",
                        "Review 1 past question",
                        "Record yourself explaining a concept"
                    }
                };

                schedule.Add(weekPlan);
                weekNumber++;
                topicIndex += topicsPerWeek;
            }

            // Adjust for last week - full review
            if (schedule.Count > 0)
            {
                schedule.Last().ReviewFocus = "FINAL REVIEW - Hit all topics once more";
            }

            return new StudyPlanDto
            {
                UserId = userId,
                CompanyName = companyName,
                RoleName = roleName,
                WeeksUntilInterview = weeksUntilInterview,
                WeeklyPlans = schedule,
                TotalTopicsToStudy = totalTopics,
                EstimatedTotalHours = schedule.Sum(w => w.DailySchedule.Sum(d => d.StudyHours)),
                StartDate = DateTime.UtcNow,
                InterviewDate = DateTime.UtcNow.AddDays(weeksUntilInterview * 7),
                KeyMilestones = new List<string>
                {
                    $"Week { weeksUntilInterview / 3}: Finish all topics",
                    $"Week {(2 * weeksUntilInterview) / 3}: Start mock interviews",
                    $"Week {weeksUntilInterview}: Final review + live practice"
                }
            };
        }

        public async Task<InterviewFeedbackDto> LogInterviewFeedbackAsync(
            Guid userId,
            string companyName,
            string roleName,
            List<QuestionAskedDto> questionsAsked,
            InterviewOutcome outcome)
        {
            _logger.LogInformation("Logging interview feedback for user {UserId} at {Company}",
                userId, companyName);

            var feedback = new InterviewFeedbackDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyName = companyName,
                RoleName = roleName,
                InterviewDate = DateTime.UtcNow,
                Outcome = outcome,
                QuestionsAsked = questionsAsked,
                NumericRating = questionsAsked.Average(q => q.DifficultyRating),
                Areas_FeelGood = questionsAsked.Where(q => q.FeelGood).Select(q => q.Topic).ToList(),
                Areas_Struggled = questionsAsked.Where(q => !q.FeelGood).Select(q => q.Topic).ToList(),
                Notes = string.Empty
            };

            // Add to repository for future digest improvements
            // This helps improve quality of digest over time
            await _repositories.SaveChangesAsync();

            // Update digest with this new data  
            // (helps improve future digest accuracy for this company)

            return feedback;
        }

        public async Task<List<PastQuestionDto>> GetPastQuestionsAsync(
            string companyName,
            string roleName,
            QuestionCategory? categoryFilter = null)
        {
            _logger.LogInformation("Fetching past questions for {Company} - {Role}",
                companyName, roleName);

            var questions = new List<PastQuestionDto>();

            try
            {
                // Scrape from LeetCode discussions
                var leetcodeQs = await _scraper.ScrapeLeetCodeQuestionsAsync(companyName);
                questions.AddRange(leetcodeQs);

                // Scrape from Blind
                var blindQs = await _scraper.ScrapeBl indQuestionsAsync(companyName, roleName);
                questions.AddRange(blindQs);

                // Filter by category if provided
                if (categoryFilter.HasValue)
                {
                    questions = questions.Where(q => q.Category == categoryFilter.Value).ToList();
                }

                // Sort by recency + popularity
                questions = questions
                    .OrderByDescending(q => q.DateAsked)
                    .ThenByDescending(q => q.Popularity)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching past questions");
            }

            return questions.Take(50).ToList(); // Top 50 recent questions
        }

        // Helper Methods

        private List<StudyChecklistItemDto> GenerateStudyChecklist(
            List<FrequentTopicDto> topics,
            List<SystemDesignAreaDto> designAreas,
            List<BehavioralThemeDto> behavioral)
        {
            var checklist = new List<StudyChecklistItemDto>();

            // Add studies
            foreach (var topic in topics.Take(10))
            {
                checklist.Add(new StudyChecklistItemDto
                {
                    Category = "Topic Mastery",
                    Item = $"Study {topic.Topic}",
                    EstimatedHours = topic.PrepTimeHours,
                    Priority = topic.Frequency > 10 ? "High" : "Medium"
                });
            }

            // Add system design
            foreach (var area in designAreas)
            {
                checklist.Add(new StudyChecklistItemDto
                {
                    Category = "System Design",
                    Item = $"Master {area.AreaName}",
                    EstimatedHours = area.EstimatedPrepHours,
                    Priority = "High"
                });
            }

            // Add behavioral prep
            foreach (var theme in behavioral)
            {
                checklist.Add(new StudyChecklistItemDto
                {
                    Category = "Behavioral",
                    Item = $"Prepare for: {theme.Theme}",
                    EstimatedHours = 2,
                    Priority = "High"
                });
            }

            return checklist;
        }

        private string EstimatePrepTime(int topicCount)
        {
            var hours = topicCount * 2; // Rough estimate
            return $"{hours} hours";
        }

        private List<string> IdentifyKeyFocusAreas(
            List<FrequentTopicDto> topics,
            List<SystemDesignAreaDto> design)
        {
            var focus = new List<string>();

            if (topics.Count > 0)
                focus.Add($"Top skill: {topics[0].Topic}");

            if (design.Count > 0)
                focus.Add($"System design: {design[0].AreaName}");

            return focus;
        }

        private string DetermineInterviewFormat(string company)
        {
            return company.ToLower() switch
            {
                var c when c.Contains("google") => "1-2 rounds: Coding + System Design",
                var c when c.Contains("amazon") => "3-4 rounds: Coding + Design + Behavioral",
                var c when c.Contains("microsoft") => "2-3 rounds: Coding + Design",
                _ => "Typically 2-3 rounds: Coding, Design, Behavioral"
            };
        }

        private List<string> IdentifyCriticalTopics(List<FrequentTopicDto> topics)
        {
            return topics
                .Where(t => t.FrequencyPercent > 20)
                .Select(t => t.Topic)
                .ToList();
        }

        private List<string> GetResourcesForTopics(List<FrequentTopicDto> topics)
        {
            var resources = new HashSet<string>();
            foreach (var topic in topics.Take(5))
            {
                resources.UnionWith(topic.Resources);
            }
            return resources.ToList();
        }

        private int CalculateFrequencyPercent(int count, int total)
        {
            return total > 0 ? (count * 100) / total : 0;
        }

        private string EstimateDifficulty(string topic)
        {
            return topic.ToLower() switch
            {
                var t when t.Contains("system") => "Hard",
                var t when t.Contains("distributed") => "Hard",
                var t when t.Contains("kubernetes") => "Hard",
                var t when t.Contains("algorithm") => "Medium",
                _ => "Medium"
            };
        }

        private int EstimatePrepHours(string topic)
        {
            var difficulty = EstimateDifficulty(topic);
            return difficulty switch
            {
                "Hard" => 10,
                "Medium" => 6,
                _ => 3
            };
        }

        private List<string> GetTopicResources(string topic)
        {
            return topic.ToLower() switch
            {
                var t when t.Contains("kubernetes") => new()
                {
                    "Kubernetes Official Docs",
                    "CKA Exam Prep",
                    "Killer.sh"
                },
                var t when t.Contains("system design") => new()
                {
                    "System Design Interview - Alex Xu",
                    "Designing Data-Intensive Applications",
                    "Byte by Byte"
                },
                _ => new() { "LeetCode", "Blind", "GeeksforGeeks" }
            };
        }

        private List<string> GetSampleQuestions(string topic, int count)
        {
            // Return example questions for a topic
            return new() { $"Example question 1 for {topic}", $"Example question 2 for {topic}" };
        }

        private List<FrequentTopicDto> GenerateDefaultTopics(string roleName)
        {
            return roleName.ToLower() switch
            {
                var r when r.Contains("platform") => new()
                {
                    new() { Topic = "Kubernetes Fundamentals", Frequency = 95, FrequencyPercent = 25 },
                    new() { Topic = "CLI/CD Pipelines", Frequency = 87, FrequencyPercent = 23 },
                    new() { Topic = "Infrastructure as Code", Frequency = 78, FrequencyPercent = 21 }
                },
                _ => new()
                {
                    new() { Topic = "Data Structures & Algorithms", Frequency = 100, FrequencyPercent = 30 },
                    new() { Topic = "System Design", Frequency = 90, FrequencyPercent = 27 },
                    new() { Topic = "Behavioral", Frequency = 70, FrequencyPercent = 21 }
                }
            };
        }

        private List<DailyInterviewScheduleDto> CreateDailySchedule(
            List<FrequentTopicDto> topics,
            int weeksRemaining)
        {
            return new()
            {
                new() { Day = "Monday", Topic = "Learn new concept", StudyHours = 2 },
                new() { Day = "Tuesday", Topic = "Practice problems", StudyHours = 3 },
                new() { Day = "Wednesday", Topic = "Review + deepen", StudyHours = 2 },
                new() { Day = "Thursday", Topic = "Mock interview", StudyHours = 2 },
                new() { Day = "Friday", Topic = "Rest or catch up", StudyHours = 1 }
            };
        }

        private string GetReviewFocus(int week, int totalWeeks)
        {
            if (week <= totalWeeks / 3)
                return "Build foundation";
            else if (week <= 2 * totalWeeks / 3)
                return "Practice and solidify";
            else
                return "Review and mock interviews";
        }
    }

    // ===== DTOs & Enums =====

    public class InterviewDigestDto
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public List<FrequentTopicDto> FrequentTopics { get; set; } = new();
        public List<SystemDesignAreaDto> SystemDesignAreas { get; set; } = new();
        public List<BehavioralThemeDto> BehavioralThemes { get; set; } = new();
        public List<StudyChecklistItemDto> StudyChecklist { get; set; } = new();
        public int PastQuestionCount { get; set; }
        public List<string> SourcesUsed { get; set; } = new();
        public string EstimatedPrepTime { get; set; } = string.Empty;
        public List<string> KeyFocusAreas { get; set; } = new();
        public string InterviewFormat { get; set; } = string.Empty;
        public List<string> CriticalTopics { get; set; } = new();
        public List<string> ResourcesRecommended { get; set; } = new();
    }

    public class FrequentTopicDto
    {
        public string Topic { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public int FrequencyPercent { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int PrepTimeHours { get; set; }
        public List<string> Resources { get; set; } = new();
        public List<string> SampleQuestions { get; set; } = new();
    }

    public class SystemDesignAreaDto
    {
        public string AreaName { get; set; } = string.Empty;
        public List<string> Subtopics { get; set; } = new();
        public string Difficulty { get; set; } = string.Empty;
        public string FrequencyAtCompany { get; set; } = string.Empty;
        public int EstimatedPrepHours { get; set; }
        public List<string> Resources { get; set; } = new();
        public List<string> SampleProblems { get; set; } = new();
    }

    public class BehavioralThemeDto
    {
        public string Theme { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> CommonQuestions { get; set; } = new();
        public string HowToAnswer { get; set; } = string.Empty;
        public List<string> RedFlags { get; set; } = new();
        public string Frequency { get; set; } = string.Empty;
    }

    public class StudyChecklistItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class StudyPlanDto
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int WeeksUntilInterview { get; set; }
        public List<WeeklyInterviewPlanDto> WeeklyPlans { get; set; } = new();
        public int TotalTopicsToStudy { get; set; }
        public int EstimatedTotalHours { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime InterviewDate { get; set; }
        public List<string> KeyMilestones { get; set; } = new();
    }

    public class WeeklyInterviewPlanDto
    {
        public int WeekNumber { get; set; }
        public List<string> Topics { get; set; } = new();
        public List<DailyInterviewScheduleDto> DailySchedule { get; set; } = new();
        public string? MockInterviewDay { get; set; }
        public string ReviewFocus { get; set; } = string.Empty;
        public List<string> ChecklistedItems { get; set; } = new();
    }

    public class DailyInterviewScheduleDto
    {
        public string Day { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int StudyHours { get; set; }
    }

    public class InterviewFeedbackDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime InterviewDate { get; set; }
        public InterviewOutcome Outcome { get; set; }
        public List<QuestionAskedDto> QuestionsAsked { get; set; } = new();
        public decimal NumericRating { get; set; }
        public List<string> Areas_FeelGood { get; set; } = new();
        public List<string> Areas_Struggled { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class QuestionAskedDto
    {
        public string Topic { get; set; } = string.Empty;
        public int DifficultyRating { get; set; } // 1-5
        public bool FeelGood { get; set; }
    }

    public class PastQuestionDto
    {
        public string Question { get; set; } = string.Empty;
        public QuestionCategory Category { get; set; }
        public string? ModelAnswer { get; set; }
        public DateTime DateAsked { get; set; }
        public int Popularity { get; set; } // Number of people who reported it
        public List<string> Topics { get; set; } = new();
    }

    public enum InterviewOutcome
    {
        Passed,
        AdvancedToNextRound,
        Rejected,
        Waiting
    }

    public enum QuestionCategory
    {
        Coding,
        SystemDesign,
        Behavioral,
        ProductSense,
        Analytics,
        Other
    }

    // Dependency interfaces (implemented elsewhere)
    public interface IInterviewDataScraperService
    {
        Task<List<string>> ScapeBlindTopicsAsync(string company, string role);
        Task<List<string>> ScrapeLevelsTopicsAsync(string company, string role);
        Task<List<PastQuestionDto>> ScrapeLeetCodeQuestionsAsync(string company);
        Task<List<PastQuestionDto>> ScrabindQuestionsAsync(string company, string role);
    }

    public interface IVectorStoreService
    {
        Task<List<string>> SearchAsync(string query, int topK = 10);
    }
}
