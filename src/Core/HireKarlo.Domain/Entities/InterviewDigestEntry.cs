using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

/// <summary>
/// USP #4: Interview Digest Engine - RAG-based interview preparation content
/// Collects interview questions and experiences from Glassdoor, LeetCode, GFG, etc.
/// Uses vector embeddings for semantic search and relevance ranking
/// </summary>
public class InterviewDigestEntry : BaseEntity
{
    /// <summary>
    /// User this digest entry is useful for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Which dream company is this interview prep for
    /// </summary>
    public Guid? DreamCompanyId { get; set; }

    /// <summary>
    /// Company name (denormalized for quick display)
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Source URL where this came from
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Source platform (LeetCode, Glassdoor, GeeksForGeeks, CareerCup, Blind, Reddit)
    /// </summary>
    public string SourcePlatform { get; set; } = string.Empty;

    /// <summary>
    /// Original title/question text
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Original snippet from source
    /// </summary>
    public string? Snippet { get; set; }

    /// <summary>
    /// AI-generated summary for quick reading
    /// </summary>
    public string? LlmSummary { get; set; }

    /// <summary>
    /// Role title (e.g., "Backend Engineer", "Senior SDE")
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Interview type (Phone, Online Assessment, Onsite, System Design, Behavioral)
    /// </summary>
    public string? InterviewType { get; set; }

    /// <summary>
    /// Difficulty level (Easy, Medium, Hard)
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// Category of interview content
    /// </summary>
    public DigestCategory? ContentCategory { get; set; }

    /// <summary>
    /// Topics covered (JSON array)
    /// Example: ["distributed-systems", "rate-limiting", "caching"]
    /// </summary>
    public string? Topics { get; set; }

    /// <summary>
    /// Key takeaways from this interview (JSON array)
    /// </summary>
    public string? KeyTakeaways { get; set; }

    /// <summary>
    /// Vector embedding for semantic search
    /// Stored as JSON serialized array or pgvector type
    /// </summary>
    public string? ContentEmbedding { get; set; }

    /// <summary>
    /// Relevance score to user's target (0-100)
    /// Auto-calculated based on skills and role match
    /// </summary>
    public int Relevance { get; set; }

    /// <summary>
    /// How many times this topic appears across sources
    /// Higher frequency = more likely to appear
    /// </summary>
    public int? Frequency { get; set; }

    /// <summary>
    /// When this content was originally published
    /// </summary>
    public DateTime PublishedDate { get; set; }

    /// <summary>
    /// When we fetched this content
    /// </summary>
    public DateTime FetchedDate { get; set; }

    /// <summary>
    /// Whether to include in digest generation
    /// </summary>
    public bool IncludedInDigest { get; set; }

    /// <summary>
    /// When digest was last sent to user
    /// </summary>
    public DateTime? DigestSentDate { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual DreamCompany? DreamCompany { get; set; }
}
