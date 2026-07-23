using HireKarlo.Application.Features.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IMediator mediator, ILogger<JobsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Search jobs - publicly accessible demo endpoint with mock data
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public ActionResult<List<JobDto>> SearchJobsDemo(
        [FromQuery] string? query,
        [FromQuery] string? location,
        [FromQuery] bool remote = false)
    {
        _logger.LogInformation("Searching jobs: query={Query}, location={Location}, remote={Remote}", 
            query, location, remote);

        // Return demo jobs for testing the UI
        var demoJobs = GetDemoJobs();

        // Apply filters
        var filtered = demoJobs.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(j => 
                j.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                j.Company.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                j.Skills.Any(s => s.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(location))
        {
            filtered = filtered.Where(j => 
                j.Location.Contains(location, StringComparison.OrdinalIgnoreCase) ||
                (location.Equals("remote", StringComparison.OrdinalIgnoreCase) && j.IsRemote));
        }

        if (remote)
        {
            filtered = filtered.Where(j => j.IsRemote);
        }

        return Ok(filtered.ToList());
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<JobSearchResult>> SearchJobs(
        [FromQuery] string? query,
        [FromQuery] string? location,
        [FromQuery] bool? remoteOnly,
        [FromQuery] bool? visaSponsorship,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchJobsQuery
        {
            Query = query,
            Location = location,
            RemoteOnly = remoteOnly,
            VisaSponsorship = visaSponsorship,
            Page = page,
            PageSize = Math.Min(pageSize, 50)
        };

        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> GetJob(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetJobByIdQuery { JobListingId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("matched")]
    public async Task<ActionResult<List<MatchedJobDto>>> GetMatchedJobs(
        [FromQuery] double minScore = 80,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetMatchedJobsQuery
        {
            UserId = userId,
            MinScore = minScore,
            Limit = Math.Min(limit, 100)
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/match")]
    public async Task<ActionResult<MatchedJobDto>> GetJobMatch(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // TODO: Calculate match for specific job
        return Ok();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }

    private static List<JobDto> GetDemoJobs() =>
    [
        new() { Id = Guid.NewGuid(), Title = "Senior Software Engineer", Company = "Google", Location = "Mountain View, CA", IsRemote = true, SalaryRange = "$180,000 - $250,000", MatchScore = 92, PostedDate = DateTime.Now.AddDays(-1), Skills = ["C#", ".NET", "Azure", "Microservices", "Kubernetes"], Description = "Join our team building next-generation cloud infrastructure. Work with cutting-edge technologies and solve complex distributed systems challenges." },
        new() { Id = Guid.NewGuid(), Title = "Full Stack Developer", Company = "Microsoft", Location = "Seattle, WA", IsRemote = true, SalaryRange = "$150,000 - $200,000", MatchScore = 88, PostedDate = DateTime.Now.AddDays(-2), Skills = ["React", "TypeScript", "Node.js", "Azure", "SQL"], Description = "Build innovative web applications as part of the Azure team. Collaborate with designers and product managers to deliver exceptional user experiences." },
        new() { Id = Guid.NewGuid(), Title = "Backend Engineer", Company = "Amazon", Location = "San Francisco, CA", IsRemote = false, SalaryRange = "$160,000 - $220,000", MatchScore = 85, PostedDate = DateTime.Now.AddDays(-3), Skills = ["Java", "AWS", "DynamoDB", "Python", "Microservices"], Description = "Design and implement scalable backend services for millions of customers worldwide. Strong focus on reliability and performance." },
        new() { Id = Guid.NewGuid(), Title = "DevOps Engineer", Company = "Netflix", Location = "Los Gatos, CA", IsRemote = true, SalaryRange = "$170,000 - $230,000", MatchScore = 78, PostedDate = DateTime.Now.AddDays(-4), Skills = ["Kubernetes", "Docker", "Terraform", "AWS", "CI/CD"], Description = "Help us scale our streaming platform to reach viewers globally. Work on automation, deployment pipelines, and infrastructure as code." },
        new() { Id = Guid.NewGuid(), Title = "Frontend Developer", Company = "Meta", Location = "Menlo Park, CA", IsRemote = true, SalaryRange = "$165,000 - $215,000", MatchScore = 82, PostedDate = DateTime.Now.AddDays(-5), Skills = ["React", "JavaScript", "TypeScript", "GraphQL", "CSS"], Description = "Create beautiful, performant user interfaces for billions of users. Focus on accessibility and cross-platform compatibility." },
        new() { Id = Guid.NewGuid(), Title = "Machine Learning Engineer", Company = "OpenAI", Location = "San Francisco, CA", IsRemote = false, SalaryRange = "$200,000 - $300,000", MatchScore = 65, PostedDate = DateTime.Now.AddDays(-1), Skills = ["Python", "PyTorch", "TensorFlow", "ML", "Deep Learning"], Description = "Push the boundaries of AI research and build systems that will shape the future. Work on large language models and advanced AI architectures." },
        new() { Id = Guid.NewGuid(), Title = "Site Reliability Engineer", Company = "Apple", Location = "Cupertino, CA", IsRemote = false, SalaryRange = "$175,000 - $240,000", MatchScore = 75, PostedDate = DateTime.Now.AddDays(-6), Skills = ["Linux", "Python", "Kubernetes", "Monitoring", "Incident Response"], Description = "Keep Apple services running 24/7 for millions of users. Build tools for better observability and incident management." },
        new() { Id = Guid.NewGuid(), Title = "Platform Engineer", Company = "Stripe", Location = "San Francisco, CA", IsRemote = true, SalaryRange = "$190,000 - $260,000", MatchScore = 90, PostedDate = DateTime.Now.AddDays(-2), Skills = ["Ruby", "Go", "PostgreSQL", "APIs", "Payments"], Description = "Build the infrastructure that powers online payments. Work on distributed systems, APIs, and financial technology." },
        new() { Id = Guid.NewGuid(), Title = "Cloud Architect", Company = "Salesforce", Location = "San Francisco, CA", IsRemote = true, SalaryRange = "$185,000 - $250,000", MatchScore = 86, PostedDate = DateTime.Now.AddDays(-7), Skills = ["AWS", "Azure", "GCP", "Terraform", "Architecture"], Description = "Design and implement multi-cloud strategies for enterprise customers. Lead cloud migration and modernization initiatives." },
        new() { Id = Guid.NewGuid(), Title = "Software Engineer II", Company = "Uber", Location = "Chicago, IL", IsRemote = true, SalaryRange = "$140,000 - $180,000", MatchScore = 79, PostedDate = DateTime.Now.AddDays(-3), Skills = ["Go", "Python", "Kafka", "Redis", "Microservices"], Description = "Work on real-time systems that power millions of rides daily. Build reliable, scalable services that handle massive traffic." },
        new() { Id = Guid.NewGuid(), Title = "Data Engineer", Company = "Airbnb", Location = "San Francisco, CA", IsRemote = true, SalaryRange = "$155,000 - $210,000", MatchScore = 72, PostedDate = DateTime.Now.AddDays(-4), Skills = ["Python", "Spark", "Airflow", "SQL", "Data Pipelines"], Description = "Build data infrastructure that enables data-driven decisions. Work with petabytes of data and cutting-edge analytics tools." },
        new() { Id = Guid.NewGuid(), Title = "Security Engineer", Company = "Cloudflare", Location = "Austin, TX", IsRemote = true, SalaryRange = "$160,000 - $220,000", MatchScore = 68, PostedDate = DateTime.Now.AddDays(-8), Skills = ["Security", "Networking", "Go", "Rust", "Cryptography"], Description = "Protect the internet. Build security solutions that defend websites and applications from attacks." },
    ];
}

public class JobDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string? SalaryRange { get; set; }
    public bool IsRemote { get; set; }
    public DateTime PostedDate { get; set; }
    public int MatchScore { get; set; }
    public List<string> Skills { get; set; } = [];
    public string? Description { get; set; }
    public bool IsSaved { get; set; }
}
