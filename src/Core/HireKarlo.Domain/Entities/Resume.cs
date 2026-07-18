using HireKarlo.Domain.Common;
using HireKarlo.Domain.Enums;

namespace HireKarlo.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public ResumeFileType FileType { get; set; }
    public string? ParsedContent { get; set; } // JSON structured content
    public string? RawText { get; set; }
    public bool IsMaster { get; set; } // Is this the main/master resume
    public Guid? TailoredForJobId { get; set; } // If tailored, which job
    public Guid? ParentResumeId { get; set; } // Parent resume if this is a tailored version
    public int Version { get; set; } = 1;
    public string? EmbeddingId { get; set; } // Reference to vector store embedding

    // Parsed sections
    public string? Summary { get; set; }
    public string? Skills { get; set; } // JSON array
    public string? Experience { get; set; } // JSON array
    public string? Education { get; set; } // JSON array
    public string? Certifications { get; set; } // JSON array
    public string? Projects { get; set; } // JSON array

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual JobListing? TailoredForJob { get; set; }
    public virtual Resume? ParentResume { get; set; }
    public virtual ICollection<Resume> TailoredVersions { get; set; } = new List<Resume>();
}
