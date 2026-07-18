using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class JobListing : BaseEntity
{
    public string ExternalId { get; set; } = string.Empty; // ID from source API
    public JobSource Source { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public string? Location { get; set; }
    public bool IsRemote { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? SalaryRange { get; set; }
    public int? SalaryMin { get; set; }
    public int? SalaryMax { get; set; }
    public string? Currency { get; set; }
    public string? JobType { get; set; } // Full-time, Part-time, Contract
    public string? ExperienceLevel { get; set; } // Entry, Mid, Senior
    public string? ApplyUrl { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime FetchedDate { get; set; }
    public DateTime? ExpiresDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool SponsorsVisa { get; set; }

    // NLP extracted fields
    public string? ExtractedSkills { get; set; } // JSON array
    public string? ExtractedKeywords { get; set; } // JSON array
    public string? EmbeddingId { get; set; } // Reference to vector store embedding

    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<Resume> TailoredResumes { get; set; } = new List<Resume>();
}
