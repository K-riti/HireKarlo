using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.ValueObjects;

namespace HireKarlo.Infrastructure.Services;

public class MatchingEngine : IMatchingEngine
{
    public Task<MatchReport> CalculateMatchAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default)
    {
        // Calculate keyword overlap
        var resumeWords = resumeText.ToLower().Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var jobWords = jobDescription.ToLower().Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        var commonWords = resumeWords.Intersect(jobWords).ToList();
        var missingWords = jobWords.Except(resumeWords).Take(10).ToList();
        var keywordScore = (double)commonWords.Count / Math.Max(jobWords.Count, 1) * 100;

        var report = new MatchReport
        {
            OverallScore = Math.Min(95, keywordScore + 20),
            KeywordScore = Math.Min(100, keywordScore),
            SemanticScore = 75, // Would use embeddings in production
            TitleScore = 70,
            GapAnalysis = new GapAnalysis
            {
                SkillGaps = new List<SkillGap>(),
                ExperienceGaps = new List<ExperienceGap>(),
                MissingKeywords = missingWords,
                MatchingKeywords = commonWords.Take(10).ToList(),
                PartialMatches = new List<string>()
            },
            Strengths = new List<string> { "Relevant technical background", "Good keyword match" },
            Weaknesses = new List<string> { "Consider adding more specific metrics" },
            Recommendations = new List<string> 
            { 
                "Tailor your resume to highlight relevant experience",
                "Add specific metrics and achievements"
            }
        };

        return Task.FromResult(report);
    }

    public Task<double> CalculateSemanticSimilarityAsync(string text1, string text2, CancellationToken cancellationToken = default)
    {
        // Simple word overlap similarity - use embeddings in production
        var words1 = text1.ToLower().Split().ToHashSet();
        var words2 = text2.ToLower().Split().ToHashSet();

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        var jaccardSimilarity = union > 0 ? (double)intersection / union : 0;
        return Task.FromResult(jaccardSimilarity * 100);
    }

    public Task<GapAnalysis> AnalyzeGapsAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var resumeWords = resumeText.ToLower().Split().ToHashSet();
        var jobWords = jobDescription.ToLower().Split().ToHashSet();

        var analysis = new GapAnalysis
        {
            SkillGaps = new List<SkillGap>
            {
                new SkillGap { RequiredSkill = "Review job requirements", RequiredLevel = "High", CurrentLevel = "Medium", Severity = GapSeverity.Moderate }
            },
            ExperienceGaps = new List<ExperienceGap>
            {
                new ExperienceGap { Requirement = "Industry Experience", CurrentExperience = "2 years", Severity = GapSeverity.Minor, Recommendation = "Highlight transferable experience" }
            },
            MissingKeywords = jobWords.Except(resumeWords).Take(10).ToList(),
            MatchingKeywords = resumeWords.Intersect(jobWords).Take(10).ToList(),
            PartialMatches = new List<string>()
        };

        return Task.FromResult(analysis);
    }

    public Task<List<MatchResult>> FindMatchingJobsAsync(Guid userId, double minScore = 80, int limit = 50, CancellationToken cancellationToken = default)
    {
        // In production, query jobs and calculate matches
        var results = new List<MatchResult>();
        return Task.FromResult(results);
    }
}

