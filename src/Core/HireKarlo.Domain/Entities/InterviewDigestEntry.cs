using HireKarlo.Domain.Common;

namespace HireKarlo.Domain.Entities;

public class InterviewDigestEntry : BaseEntity
{
    public string Company { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string SourcePlatform { get; set; } = string.Empty; // Reddit, Blind, LeetCode, Glassdoor
    public string? OriginalTitle { get; set; }
    public string? Snippet { get; set; } // Original snippet from search
    public string? LlmSummary { get; set; } // AI-generated summary
    public string? Role { get; set; } // What role was being interviewed for
    public string? InterviewType { get; set; } // Phone, Onsite, System Design, etc.
    public string? Difficulty { get; set; }
    public string? Topics { get; set; } // JSON array of topics covered
    public string? KeyTakeaways { get; set; } // JSON array
    public DateTime PublishedDate { get; set; }
    public DateTime FetchedDate { get; set; }
    public bool IncludedInDigest { get; set; }
    public DateTime? DigestSentDate { get; set; }
}
