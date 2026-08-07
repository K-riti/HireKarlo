namespace HireKarlo.Application.DTOs.CareerOS;

/// <summary>
/// Request for uploading resume in onboarding step 1
/// </summary>
public class ResumeUploadRequest
{
    public Stream ResumeStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Response after resume is processed
/// </summary>
public class ResumeUploadResponse
{
    public Guid ResumeId { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public ResumeAnalysis Analysis { get; set; } = new();
    public string NextMessage { get; set; } = "Great! Now let's find your dream companies...";
}

/// <summary>
/// Analysis of uploaded resume
/// </summary>
public class ResumeAnalysis
{
    public List<string> Skills { get; set; } = new();
    public List<string> Experience { get; set; } = new();
    public List<string> Education { get; set; } = new();
    public string? CurrentRole { get; set; }
    public int? YearsOfExperience { get; set; }
    public List<string> Certifications { get; set; } = new();
}
