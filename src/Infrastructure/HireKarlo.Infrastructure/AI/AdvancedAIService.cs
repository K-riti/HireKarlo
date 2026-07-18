using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.AI;

public interface IAdvancedAIService
{
    // Application Outcome Predictor
    Task<ApplicationPrediction> PredictApplicationOutcomeAsync(
        string resumeText, 
        string jobDescription, 
        List<HistoricalApplication> userHistory,
        CancellationToken cancellationToken = default);

    // Explainable ATS Score
    Task<ExplainableAtsScore> GetExplainableAtsScoreAsync(
        string resumeText, 
        string jobDescription,
        CancellationToken cancellationToken = default);

    // Keyword Radar
    Task<KeywordRadarResult> AnalyzeKeywordRadarAsync(
        string resumeText,
        List<string> targetJobDescriptions,
        CancellationToken cancellationToken = default);

    // Skill Trajectory Simulator
    Task<SkillTrajectory> SimulateSkillTrajectoryAsync(
        string currentSkills,
        string targetRole,
        int monthsToProject,
        CancellationToken cancellationToken = default);

    // 6-Month Career Roadmap (Pure LLM)
    Task<CareerRoadmap> GenerateCareerRoadmapAsync(
        string resumeText,
        string targetRole,
        string targetCompany,
        CancellationToken cancellationToken = default);

    // Resume Tailoring per JD (Pure LLM)
    Task<TailoredResume> TailorResumeForJobAsync(
        string resumeText,
        string jobDescription,
        CancellationToken cancellationToken = default);

    // RAG-based Interview Question Bank
    Task<List<InterviewQuestionWithContext>> GetContextualInterviewQuestionsAsync(
        string company,
        string role,
        string questionType,
        CancellationToken cancellationToken = default);
}

public class AdvancedAIService : IAdvancedAIService
{
    private readonly IOpenAIService _openAI;
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStoreService _vectorStore;
    private readonly ILogger<AdvancedAIService> _logger;

    public AdvancedAIService(
        IOpenAIService openAI,
        IEmbeddingService embeddings,
        IVectorStoreService vectorStore,
        ILogger<AdvancedAIService> logger)
    {
        _openAI = openAI;
        _embeddings = embeddings;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    #region Application Outcome Predictor

    public async Task<ApplicationPrediction> PredictApplicationOutcomeAsync(
        string resumeText,
        string jobDescription,
        List<HistoricalApplication> userHistory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Predicting application outcome based on {Count} historical applications", userHistory.Count);

        var historyPatterns = AnalyzeHistoryPatterns(userHistory);
        var semanticScore = await _embeddings.CalculateCosineSimilarityAsync(resumeText, jobDescription, cancellationToken);

        var prompt = $@"You are an expert career analyst. Predict the outcome of a job application based on the candidate's resume, target job, and their historical application patterns.

RESUME:
{resumeText}

JOB DESCRIPTION:
{jobDescription}

HISTORICAL APPLICATION PATTERNS:
- Total Applications: {historyPatterns.TotalApplications}
- Interview Rate: {historyPatterns.InterviewRate:P0}
- Offer Rate: {historyPatterns.OfferRate:P0}
- Average Response Time: {historyPatterns.AvgResponseDays} days
- Best Performing Job Types: {string.Join(", ", historyPatterns.SuccessfulJobTypes)}
- Weakest Areas: {string.Join(", ", historyPatterns.WeakAreas)}

SEMANTIC MATCH SCORE: {semanticScore * 100:F0}%

Provide prediction in JSON:
{{
    ""successProbability"": <0-100>,
    ""predictedOutcome"": ""Likely Offer|Likely Interview|Likely Rejection|Uncertain"",
    ""timeToResponseDays"": <estimated days>,
    ""confidenceLevel"": ""High|Medium|Low"",
    ""keyFactors"": [
        {{""factor"": ""factor name"", ""impact"": ""Positive|Negative|Neutral"", ""weight"": <1-10>}}
    ],
    ""improvementActions"": [""action1"", ""action2""],
    ""similarSuccessfulApplications"": <count from history>,
    ""riskFactors"": [""risk1"", ""risk2""]
}}";

        var result = await _openAI.CompleteAsJsonAsync<ApplicationPredictionResponse>(
            prompt, new CompletionOptions { Temperature = 0.3 }, cancellationToken);

        return new ApplicationPrediction
        {
            SuccessProbability = result?.SuccessProbability ?? 50,
            PredictedOutcome = result?.PredictedOutcome ?? "Uncertain",
            TimeToResponseDays = result?.TimeToResponseDays ?? 14,
            ConfidenceLevel = result?.ConfidenceLevel ?? "Medium",
            KeyFactors = result?.KeyFactors ?? new(),
            ImprovementActions = result?.ImprovementActions ?? new(),
            RiskFactors = result?.RiskFactors ?? new(),
            BasedOnHistoricalCount = userHistory.Count
        };
    }

    private HistoryPatterns AnalyzeHistoryPatterns(List<HistoricalApplication> history)
    {
        if (!history.Any())
            return new HistoryPatterns();

        var interviews = history.Count(h => h.ReachedInterview);
        var offers = history.Count(h => h.ReceivedOffer);
        var responseDays = history.Where(h => h.ResponseDays.HasValue).Select(h => h.ResponseDays!.Value).ToList();

        var successfulTypes = history
            .Where(h => h.ReachedInterview || h.ReceivedOffer)
            .GroupBy(h => h.JobType)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        var rejectedTypes = history
            .Where(h => !h.ReachedInterview && !h.ReceivedOffer)
            .GroupBy(h => h.JobType)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        return new HistoryPatterns
        {
            TotalApplications = history.Count,
            InterviewRate = history.Count > 0 ? (double)interviews / history.Count : 0,
            OfferRate = interviews > 0 ? (double)offers / interviews : 0,
            AvgResponseDays = responseDays.Any() ? (int)responseDays.Average() : 14,
            SuccessfulJobTypes = successfulTypes,
            WeakAreas = rejectedTypes
        };
    }

    #endregion

    #region Explainable ATS Score

    public async Task<ExplainableAtsScore> GetExplainableAtsScoreAsync(
        string resumeText,
        string jobDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating explainable ATS score");

        var prompt = $@"You are an ATS (Applicant Tracking System) expert. Analyze this resume against the job description and provide a detailed, explainable ATS score.

RESUME:
{resumeText}

JOB DESCRIPTION:
{jobDescription}

Provide detailed analysis in JSON:
{{
    ""overallScore"": <0-100>,
    ""scoreBreakdown"": {{
        ""keywordMatch"": {{""score"": <0-100>, ""weight"": 30, ""details"": ""explanation""}},
        ""experienceRelevance"": {{""score"": <0-100>, ""weight"": 25, ""details"": ""explanation""}},
        ""skillsAlignment"": {{""score"": <0-100>, ""weight"": 20, ""details"": ""explanation""}},
        ""educationMatch"": {{""score"": <0-100>, ""weight"": 10, ""details"": ""explanation""}},
        ""formatCompliance"": {{""score"": <0-100>, ""weight"": 10, ""details"": ""explanation""}},
        ""actionVerbs"": {{""score"": <0-100>, ""weight"": 5, ""details"": ""explanation""}}
    }},
    ""keywordAnalysis"": {{
        ""requiredFound"": [""keyword1"", ""keyword2""],
        ""requiredMissing"": [""keyword1"", ""keyword2""],
        ""bonusFound"": [""keyword1""],
        ""densityScore"": <0-100>
    }},
    ""passLikelihood"": ""High|Medium|Low"",
    ""humanReviewLikelihood"": <0-100>,
    ""specificIssues"": [
        {{""issue"": ""description"", ""location"": ""section/line"", ""severity"": ""Critical|Warning|Info"", ""fix"": ""how to fix""}}
    ],
    ""quickWins"": [""immediate improvement 1"", ""immediate improvement 2""],
    ""scoreExplanation"": ""natural language summary of why this score""
}}";

        var result = await _openAI.CompleteAsJsonAsync<ExplainableAtsScoreResponse>(
            prompt, new CompletionOptions { Temperature = 0.2 }, cancellationToken);

        return new ExplainableAtsScore
        {
            OverallScore = result?.OverallScore ?? 50,
            ScoreBreakdown = result?.ScoreBreakdown ?? new(),
            KeywordAnalysis = result?.KeywordAnalysis ?? new(),
            PassLikelihood = result?.PassLikelihood ?? "Medium",
            HumanReviewLikelihood = result?.HumanReviewLikelihood ?? 50,
            SpecificIssues = result?.SpecificIssues ?? new(),
            QuickWins = result?.QuickWins ?? new(),
            ScoreExplanation = result?.ScoreExplanation ?? "Unable to generate explanation"
        };
    }

    #endregion

    #region Keyword Radar

    public async Task<KeywordRadarResult> AnalyzeKeywordRadarAsync(
        string resumeText,
        List<string> targetJobDescriptions,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing keyword radar across {Count} job descriptions", targetJobDescriptions.Count);

        var combinedJds = string.Join("\n\n---\n\n", targetJobDescriptions.Take(5));

        var prompt = $@"Analyze keyword trends across multiple job descriptions and compare against the resume to create a ""keyword radar"".

RESUME:
{resumeText}

TARGET JOB DESCRIPTIONS:
{combinedJds}

Create keyword radar in JSON:
{{
    ""trendingKeywords"": [
        {{""keyword"": ""keyword"", ""frequency"": <count across JDs>, ""inResume"": true/false, ""importance"": ""Critical|High|Medium|Low""}}
    ],
    ""categoryRadar"": {{
        ""technicalSkills"": {{""coverage"": <0-100>, ""missing"": [""skill1""], ""strong"": [""skill1""]}},
        ""softSkills"": {{""coverage"": <0-100>, ""missing"": [""skill1""], ""strong"": [""skill1""]}},
        ""tools"": {{""coverage"": <0-100>, ""missing"": [""tool1""], ""strong"": [""tool1""]}},
        ""methodologies"": {{""coverage"": <0-100>, ""missing"": [""method1""], ""strong"": [""method1""]}},
        ""certifications"": {{""coverage"": <0-100>, ""missing"": [""cert1""], ""strong"": [""cert1""]}}
    }},
    ""industryBuzzwords"": [""word1"", ""word2""],
    ""emergingTrends"": [""trend1"", ""trend2""],
    ""overallReadiness"": <0-100>,
    ""prioritizedActions"": [
        {{""action"": ""what to add/learn"", ""impact"": <1-10>, ""effort"": ""Low|Medium|High""}}
    ]
}}";

        var result = await _openAI.CompleteAsJsonAsync<KeywordRadarResponse>(
            prompt, new CompletionOptions { Temperature = 0.3 }, cancellationToken);

        return new KeywordRadarResult
        {
            TrendingKeywords = result?.TrendingKeywords ?? new(),
            CategoryRadar = result?.CategoryRadar ?? new(),
            IndustryBuzzwords = result?.IndustryBuzzwords ?? new(),
            EmergingTrends = result?.EmergingTrends ?? new(),
            OverallReadiness = result?.OverallReadiness ?? 50,
            PrioritizedActions = result?.PrioritizedActions ?? new()
        };
    }

    #endregion

    #region Skill Trajectory Simulator

    public async Task<SkillTrajectory> SimulateSkillTrajectoryAsync(
        string currentSkills,
        string targetRole,
        int monthsToProject,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Simulating skill trajectory for {Months} months towards {Role}", monthsToProject, targetRole);

        var prompt = $@"Simulate a skill development trajectory to reach a target role.

CURRENT SKILLS/EXPERIENCE:
{currentSkills}

TARGET ROLE: {targetRole}
TIME HORIZON: {monthsToProject} months

Create trajectory simulation in JSON:
{{
    ""currentLevel"": ""Junior|Mid|Senior|Lead"",
    ""targetLevel"": ""Junior|Mid|Senior|Lead"",
    ""feasibility"": ""Highly Achievable|Achievable|Challenging|Very Challenging"",
    ""monthlyMilestones"": [
        {{
            ""month"": 1,
            ""focus"": ""primary focus area"",
            ""skillsToAcquire"": [""skill1""],
            ""projects"": [""project idea""],
            ""resources"": [""course/book""],
            ""expectedLevel"": <0-100>
        }}
    ],
    ""criticalPath"": [""milestone1"", ""milestone2""],
    ""riskFactors"": [""risk1""],
    ""accelerators"": [""how to speed up""],
    ""plateauWarnings"": [{{""month"": 3, ""reason"": ""why might stall""}}],
    ""projectedConfidence"": <0-100>,
    ""alternativePaths"": [
        {{""role"": ""alternative role"", ""reachability"": <0-100>}}
    ]
}}";

        var result = await _openAI.CompleteAsJsonAsync<SkillTrajectoryResponse>(
            prompt, new CompletionOptions { Temperature = 0.5 }, cancellationToken);

        return new SkillTrajectory
        {
            CurrentLevel = result?.CurrentLevel ?? "Mid",
            TargetLevel = result?.TargetLevel ?? "Senior",
            Feasibility = result?.Feasibility ?? "Achievable",
            MonthlyMilestones = result?.MonthlyMilestones ?? new(),
            CriticalPath = result?.CriticalPath ?? new(),
            RiskFactors = result?.RiskFactors ?? new(),
            Accelerators = result?.Accelerators ?? new(),
            ProjectedConfidence = result?.ProjectedConfidence ?? 50
        };
    }

    #endregion

    #region Career Roadmap (Pure LLM)

    public async Task<CareerRoadmap> GenerateCareerRoadmapAsync(
        string resumeText,
        string targetRole,
        string targetCompany,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating 6-month career roadmap for {Role} at {Company}", targetRole, targetCompany);

        var prompt = $@"Create a detailed 6-month career roadmap to help this candidate land a {targetRole} position at {targetCompany}.

CANDIDATE'S CURRENT PROFILE:
{resumeText}

Generate roadmap in JSON:
{{
    ""executiveSummary"": ""brief overview of the plan"",
    ""currentGapAssessment"": {{
        ""skillGaps"": [""gap1"", ""gap2""],
        ""experienceGaps"": [""gap1""],
        ""estimatedReadiness"": <0-100>
    }},
    ""monthlyPlan"": [
        {{
            ""month"": 1,
            ""theme"": ""Foundation Building"",
            ""primaryGoals"": [""goal1"", ""goal2""],
            ""learningActivities"": [
                {{""activity"": ""name"", ""timeHours"": 10, ""resource"": ""link/name"", ""priority"": ""High|Medium|Low""}}
            ],
            ""projectWork"": {{""name"": ""project"", ""description"": ""what to build"", ""skills"": [""skill1""]}},
            ""networkingActions"": [""action1""],
            ""milestoneCheckpoint"": ""what success looks like""
        }}
    ],
    ""weeklyScheduleTemplate"": {{
        ""learningHours"": 10,
        ""projectHours"": 5,
        ""networkingHours"": 2,
        ""applicationHours"": 3
    }},
    ""keyResources"": [
        {{""type"": ""Course|Book|Project|Community"", ""name"": ""name"", ""url"": ""link"", ""priority"": 1}}
    ],
    ""successMetrics"": [
        {{""metric"": ""name"", ""target"": ""value"", ""measurementMethod"": ""how to measure""}}
    ],
    ""contingencyPlans"": [
        {{""scenario"": ""if this happens"", ""action"": ""do this""}}
    ],
    ""estimatedReadinessAtEnd"": <0-100>
}}";

        var result = await _openAI.CompleteAsJsonAsync<CareerRoadmapResponse>(
            prompt, new CompletionOptions { Temperature = 0.5, MaxTokens = 3000 }, cancellationToken);

        return new CareerRoadmap
        {
            ExecutiveSummary = result?.ExecutiveSummary ?? "",
            CurrentGapAssessment = result?.CurrentGapAssessment ?? new(),
            MonthlyPlan = result?.MonthlyPlan ?? new(),
            WeeklyScheduleTemplate = result?.WeeklyScheduleTemplate ?? new(),
            KeyResources = result?.KeyResources ?? new(),
            SuccessMetrics = result?.SuccessMetrics ?? new(),
            EstimatedReadinessAtEnd = result?.EstimatedReadinessAtEnd ?? 70
        };
    }

    #endregion

    #region Resume Tailoring (Pure LLM)

    public async Task<TailoredResume> TailorResumeForJobAsync(
        string resumeText,
        string jobDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tailoring resume for specific job description");

        var prompt = $@"Tailor this resume for the specific job description. Rewrite sections to better match while keeping facts truthful.

ORIGINAL RESUME:
{resumeText}

JOB DESCRIPTION:
{jobDescription}

Provide tailored version in JSON:
{{
    ""tailoredSummary"": ""rewritten professional summary"",
    ""tailoredExperience"": [
        {{
            ""company"": ""company name"",
            ""role"": ""optimized role title"",
            ""bullets"": [""rewritten bullet with metrics and keywords""]
        }}
    ],
    ""tailoredSkills"": {{
        ""primary"": [""skill1""],
        ""secondary"": [""skill2""],
        ""tools"": [""tool1""]
    }},
    ""keywordsAdded"": [""keyword1""],
    ""changesSummary"": [
        {{""section"": ""Experience"", ""change"": ""what was changed"", ""reason"": ""why""}}
    ],
    ""beforeAfterScore"": {{
        ""before"": <estimated ATS score>,
        ""after"": <estimated ATS score>
    }},
    ""warningsIfAny"": [""potential concern about the changes""]
}}";

        var result = await _openAI.CompleteAsJsonAsync<TailoredResumeResponse>(
            prompt, new CompletionOptions { Temperature = 0.4, MaxTokens = 2500 }, cancellationToken);

        return new TailoredResume
        {
            TailoredSummary = result?.TailoredSummary ?? "",
            TailoredExperience = result?.TailoredExperience ?? new(),
            TailoredSkills = result?.TailoredSkills ?? new(),
            KeywordsAdded = result?.KeywordsAdded ?? new(),
            ChangesSummary = result?.ChangesSummary ?? new(),
            BeforeScore = result?.BeforeAfterScore?.Before ?? 50,
            AfterScore = result?.BeforeAfterScore?.After ?? 75
        };
    }

    #endregion

    #region RAG Interview Question Bank

    public async Task<List<InterviewQuestionWithContext>> GetContextualInterviewQuestionsAsync(
        string company,
        string role,
        string questionType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving contextual interview questions for {Company} {Role}", company, role);

        // Step 1: Retrieve from vector store
        var searchQuery = $"{company} {role} {questionType} interview questions experience";
        var retrievedContent = await _vectorStore.SearchAsync(
            searchQuery, 
            "interview_experiences", 
            10, 
            0.5, 
            cancellationToken);

        if (!retrievedContent.Any())
        {
            _logger.LogWarning("No interview content found in vector store for {Company}", company);
            retrievedContent = new List<VectorSearchResult>
            {
                new() { Content = $"Generic {questionType} questions for {role} positions", Score = 0.5 }
            };
        }

        // Step 2: Ground LLM generation in retrieved content
        var context = string.Join("\n\n---\n\n", retrievedContent.Select(r => r.Content));

        var prompt = $@"Based ONLY on the following real interview experiences and questions, generate {questionType} interview questions for a {role} position at {company}.

REAL INTERVIEW DATA (use this as your source - do not make up information not present here):
{context}

Generate questions in JSON (only include questions that can be grounded in the above data):
{{
    ""questions"": [
        {{
            ""question"": ""the question"",
            ""type"": ""{questionType}"",
            ""difficulty"": ""Easy|Medium|Hard"",
            ""source"": ""where this came from in the data"",
            ""frequency"": ""Common|Occasional|Rare"",
            ""sampleAnswer"": ""key points for a good answer"",
            ""followUps"": [""likely follow-up question""],
            ""tips"": [""how to prepare for this specific question""],
            ""groundedIn"": ""quote from the source data that supports this question""
        }}
    ],
    ""dataQuality"": ""High|Medium|Low"",
    ""dataRecency"": ""estimate when data is from"",
    ""caveat"": ""any limitations of this data""
}}";

        var result = await _openAI.CompleteAsJsonAsync<InterviewQuestionsResponse>(
            prompt, new CompletionOptions { Temperature = 0.3 }, cancellationToken);

        return result?.Questions?.Select(q => new InterviewQuestionWithContext
        {
            Question = q.Question,
            Type = q.Type,
            Difficulty = q.Difficulty,
            Source = q.Source,
            Frequency = q.Frequency,
            SampleAnswer = q.SampleAnswer,
            FollowUps = q.FollowUps,
            Tips = q.Tips,
            IsGrounded = !string.IsNullOrEmpty(q.GroundedIn)
        }).ToList() ?? new();
    }

    #endregion
}

#region Response DTOs

public class ApplicationPredictionResponse
{
    public int SuccessProbability { get; set; }
    public string PredictedOutcome { get; set; } = "";
    public int TimeToResponseDays { get; set; }
    public string ConfidenceLevel { get; set; } = "";
    public List<PredictionFactor> KeyFactors { get; set; } = new();
    public List<string> ImprovementActions { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
}

public class ExplainableAtsScoreResponse
{
    public int OverallScore { get; set; }
    public Dictionary<string, ScoreComponent> ScoreBreakdown { get; set; } = new();
    public KeywordAnalysisResult KeywordAnalysis { get; set; } = new();
    public string PassLikelihood { get; set; } = "";
    public int HumanReviewLikelihood { get; set; }
    public List<AtsIssue> SpecificIssues { get; set; } = new();
    public List<string> QuickWins { get; set; } = new();
    public string ScoreExplanation { get; set; } = "";
}

public class KeywordRadarResponse
{
    public List<TrendingKeyword> TrendingKeywords { get; set; } = new();
    public Dictionary<string, CategoryCoverage> CategoryRadar { get; set; } = new();
    public List<string> IndustryBuzzwords { get; set; } = new();
    public List<string> EmergingTrends { get; set; } = new();
    public int OverallReadiness { get; set; }
    public List<PrioritizedAction> PrioritizedActions { get; set; } = new();
}

public class SkillTrajectoryResponse
{
    public string CurrentLevel { get; set; } = "";
    public string TargetLevel { get; set; } = "";
    public string Feasibility { get; set; } = "";
    public List<MonthlyMilestone> MonthlyMilestones { get; set; } = new();
    public List<string> CriticalPath { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Accelerators { get; set; } = new();
    public int ProjectedConfidence { get; set; }
}

public class CareerRoadmapResponse
{
    public string ExecutiveSummary { get; set; } = "";
    public GapAssessment CurrentGapAssessment { get; set; } = new();
    public List<MonthlyPlanItem> MonthlyPlan { get; set; } = new();
    public WeeklySchedule WeeklyScheduleTemplate { get; set; } = new();
    public List<Resource> KeyResources { get; set; } = new();
    public List<SuccessMetric> SuccessMetrics { get; set; } = new();
    public int EstimatedReadinessAtEnd { get; set; }
}

public class TailoredResumeResponse
{
    public string TailoredSummary { get; set; } = "";
    public List<TailoredExperienceItem> TailoredExperience { get; set; } = new();
    public TailoredSkillsGroup TailoredSkills { get; set; } = new();
    public List<string> KeywordsAdded { get; set; } = new();
    public List<ChangeItem> ChangesSummary { get; set; } = new();
    public BeforeAfterScore BeforeAfterScore { get; set; } = new();
}

public class InterviewQuestionsResponse
{
    public List<InterviewQuestionItem> Questions { get; set; } = new();
    public string DataQuality { get; set; } = "";
    public string Caveat { get; set; } = "";
}

#endregion

#region Domain Models

public class ApplicationPrediction
{
    public int SuccessProbability { get; set; }
    public string PredictedOutcome { get; set; } = "";
    public int TimeToResponseDays { get; set; }
    public string ConfidenceLevel { get; set; } = "";
    public List<PredictionFactor> KeyFactors { get; set; } = new();
    public List<string> ImprovementActions { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    public int BasedOnHistoricalCount { get; set; }
}

public class HistoricalApplication
{
    public string JobType { get; set; } = "";
    public bool ReachedInterview { get; set; }
    public bool ReceivedOffer { get; set; }
    public int? ResponseDays { get; set; }
    public DateTime AppliedDate { get; set; }
}

public class HistoryPatterns
{
    public int TotalApplications { get; set; }
    public double InterviewRate { get; set; }
    public double OfferRate { get; set; }
    public int AvgResponseDays { get; set; }
    public List<string> SuccessfulJobTypes { get; set; } = new();
    public List<string> WeakAreas { get; set; } = new();
}

public class PredictionFactor
{
    public string Factor { get; set; } = "";
    public string Impact { get; set; } = "";
    public int Weight { get; set; }
}

public class ExplainableAtsScore
{
    public int OverallScore { get; set; }
    public Dictionary<string, ScoreComponent> ScoreBreakdown { get; set; } = new();
    public KeywordAnalysisResult KeywordAnalysis { get; set; } = new();
    public string PassLikelihood { get; set; } = "";
    public int HumanReviewLikelihood { get; set; }
    public List<AtsIssue> SpecificIssues { get; set; } = new();
    public List<string> QuickWins { get; set; } = new();
    public string ScoreExplanation { get; set; } = "";
}

public class ScoreComponent
{
    public int Score { get; set; }
    public int Weight { get; set; }
    public string Details { get; set; } = "";
}

public class KeywordAnalysisResult
{
    public List<string> RequiredFound { get; set; } = new();
    public List<string> RequiredMissing { get; set; } = new();
    public List<string> BonusFound { get; set; } = new();
    public int DensityScore { get; set; }
}

public class AtsIssue
{
    public string Issue { get; set; } = "";
    public string Location { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Fix { get; set; } = "";
}

public class KeywordRadarResult
{
    public List<TrendingKeyword> TrendingKeywords { get; set; } = new();
    public Dictionary<string, CategoryCoverage> CategoryRadar { get; set; } = new();
    public List<string> IndustryBuzzwords { get; set; } = new();
    public List<string> EmergingTrends { get; set; } = new();
    public int OverallReadiness { get; set; }
    public List<PrioritizedAction> PrioritizedActions { get; set; } = new();
}

public class TrendingKeyword
{
    public string Keyword { get; set; } = "";
    public int Frequency { get; set; }
    public bool InResume { get; set; }
    public string Importance { get; set; } = "";
}

public class CategoryCoverage
{
    public int Coverage { get; set; }
    public List<string> Missing { get; set; } = new();
    public List<string> Strong { get; set; } = new();
}

public class PrioritizedAction
{
    public string Action { get; set; } = "";
    public int Impact { get; set; }
    public string Effort { get; set; } = "";
}

public class SkillTrajectory
{
    public string CurrentLevel { get; set; } = "";
    public string TargetLevel { get; set; } = "";
    public string Feasibility { get; set; } = "";
    public List<MonthlyMilestone> MonthlyMilestones { get; set; } = new();
    public List<string> CriticalPath { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Accelerators { get; set; } = new();
    public int ProjectedConfidence { get; set; }
}

public class MonthlyMilestone
{
    public int Month { get; set; }
    public string Focus { get; set; } = "";
    public List<string> SkillsToAcquire { get; set; } = new();
    public int ExpectedLevel { get; set; }
}

public class CareerRoadmap
{
    public string ExecutiveSummary { get; set; } = "";
    public GapAssessment CurrentGapAssessment { get; set; } = new();
    public List<MonthlyPlanItem> MonthlyPlan { get; set; } = new();
    public WeeklySchedule WeeklyScheduleTemplate { get; set; } = new();
    public List<Resource> KeyResources { get; set; } = new();
    public List<SuccessMetric> SuccessMetrics { get; set; } = new();
    public int EstimatedReadinessAtEnd { get; set; }
}

public class GapAssessment
{
    public List<string> SkillGaps { get; set; } = new();
    public List<string> ExperienceGaps { get; set; } = new();
    public int EstimatedReadiness { get; set; }
}

public class MonthlyPlanItem
{
    public int Month { get; set; }
    public string Theme { get; set; } = "";
    public List<string> PrimaryGoals { get; set; } = new();
    public string MilestoneCheckpoint { get; set; } = "";
}

public class WeeklySchedule
{
    public int LearningHours { get; set; }
    public int ProjectHours { get; set; }
    public int NetworkingHours { get; set; }
    public int ApplicationHours { get; set; }
}

public class Resource
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public int Priority { get; set; }
}

public class SuccessMetric
{
    public string Metric { get; set; } = "";
    public string Target { get; set; } = "";
    public string MeasurementMethod { get; set; } = "";
}

public class TailoredResume
{
    public string TailoredSummary { get; set; } = "";
    public List<TailoredExperienceItem> TailoredExperience { get; set; } = new();
    public TailoredSkillsGroup TailoredSkills { get; set; } = new();
    public List<string> KeywordsAdded { get; set; } = new();
    public List<ChangeItem> ChangesSummary { get; set; } = new();
    public int BeforeScore { get; set; }
    public int AfterScore { get; set; }
}

public class TailoredExperienceItem
{
    public string Company { get; set; } = "";
    public string Role { get; set; } = "";
    public List<string> Bullets { get; set; } = new();
}

public class TailoredSkillsGroup
{
    public List<string> Primary { get; set; } = new();
    public List<string> Secondary { get; set; } = new();
    public List<string> Tools { get; set; } = new();
}

public class ChangeItem
{
    public string Section { get; set; } = "";
    public string Change { get; set; } = "";
    public string Reason { get; set; } = "";
}

public class BeforeAfterScore
{
    public int Before { get; set; }
    public int After { get; set; }
}

public class InterviewQuestionWithContext
{
    public string Question { get; set; } = "";
    public string Type { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Source { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string SampleAnswer { get; set; } = "";
    public List<string> FollowUps { get; set; } = new();
    public List<string> Tips { get; set; } = new();
    public bool IsGrounded { get; set; }
}

public class InterviewQuestionItem
{
    public string Question { get; set; } = "";
    public string Type { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Source { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string SampleAnswer { get; set; } = "";
    public List<string> FollowUps { get; set; } = new();
    public List<string> Tips { get; set; } = new();
    public string GroundedIn { get; set; } = "";
}

#endregion
