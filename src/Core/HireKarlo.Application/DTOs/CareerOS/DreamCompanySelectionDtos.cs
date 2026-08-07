namespace HireKarlo.Application.DTOs.CareerOS;

/// <summary>
/// Request for onboarding step 2: Select dream companies
/// </summary>
public class DreamCompanySelectionRequest
{
    public List<string> CompanyNames { get; set; } = new();
}

/// <summary>
/// Response after dream companies are selected
/// </summary>
public class DreamCompanySelectionResponse
{
    public List<DreamCompanyDto> CreatedCompanies { get; set; } = new();
    public string NextStepMessage { get; set; } = "Analyzing your profile against dream companies...";
}

/// <summary>
/// Simple DTO for dream company
/// </summary>
public class DreamCompanyDto
{
    public Guid DreamCompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}
