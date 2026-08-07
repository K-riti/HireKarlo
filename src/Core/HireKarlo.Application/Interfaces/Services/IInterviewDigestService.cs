namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Application.DTOs.CareerOS;
using HireKarlo.Domain.Entities;

/// <summary>
/// USP #4: Interview Digest Engine
/// RAG-based (Retrieval Augmented Generation) interview preparation
/// Collects questions from Glassdoor, LeetCode, GFG, etc.
/// Generates personalized digest based on user's target role and companies
/// </summary>
public interface IInterviewDigestService
{
    /// <summary>
    /// Ingest interview data from external sources
    /// Stores content with vector embeddings for semantic search
    /// </summary>
    Task IngestInterviewDataAsync(string source, string companyName, string content, string? role = null);

    /// <summary>
    /// Generate and return embedding vector for content
    /// Used for semantic similarity and relevance ranking
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string content);

    /// <summary>
    /// Generate interview digest for a specific role at a company
    /// Returns top interview questions grouped by type
    /// </summary>
    Task<InterviewDigestResponse> GenerateDigestForRoleAsync(Guid userId, Guid dreamCompanyId, string role);

    /// <summary>
    /// Search relevant interview questions using semantic similarity
    /// </summary>
    Task<List<InterviewDigestEntry>> SearchRelevantQuestionsAsync(Guid userId, string query, int limit = 10);

    /// <summary>
    /// Get all interview topics for a company/role sorted by frequency
    /// </summary>
    Task<List<InterviewTopicDto>> GetTopicsForRoleAsync(Guid dreamCompanyId, string role);

    /// <summary>
    /// Update relevance score for interview entries
    /// Called periodically to rank based on user's skills
    /// </summary>
    Task UpdateRelevanceScoresAsync(Guid userId);

    /// <summary>
    /// Get digest for all dream companies
    /// Called from dashboard to show interview prep
    /// </summary>
    Task<List<InterviewPrepDto>> GetCompleteDiestAsync(Guid userId);

    /// <summary>
    /// Send interview digest notification
    /// </summary>
    Task SendDigestNotificationAsync(Guid userId, Guid dreamCompanyId);

    /// <summary>
    /// Mark interview content as viewed
    /// </summary>
    Task MarkAsViewedAsync(Guid entryId);
}

/// <summary>
/// Interview digest response
/// </summary>
public class InterviewDigestResponse
{
    public string CompanyName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<InterviewTopicGroupDto> TopicalBreakdown { get; set; } = new();
    public List<InterviewResourceDto> TopResources { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string SummaryInsights { get; set; } = string.Empty;
}

/// <summary>
/// Group of interview topics
/// </summary>
public class InterviewTopicGroupDto
{
    public string Category { get; set; } = string.Empty;
    public List<InterviewTopicDto> Topics { get; set; } = new();
    public int TotalQuestions { get; set; }
}

/// <summary>
/// Interview resource or learning material
/// </summary>
public class InterviewResourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string Relevance { get; set; } = string.Empty;
}
