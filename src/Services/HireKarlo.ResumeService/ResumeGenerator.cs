using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HireKarlo.ResumeService;

public class ResumeGenerator : IResumeGenerator
{
    private readonly IResumeParser _parser;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ResumeGenerator> _logger;

    public ResumeGenerator(IResumeParser parser, IOpenAIService openAIService, ILogger<ResumeGenerator> logger)
    {
        _parser = parser;
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<byte[]> GenerateTailoredResumeAsync(Resume resume, JobListing job, TailoringOptions? options = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating tailored resume for job: {JobTitle}", job.Title);

        var parsed = await GetParsedResume(resume, ct);
        var tailored = await TailorWithAI(parsed, job, options, ct);

        return await GenerateDocxAsync(tailored, ct);
    }

    private async Task<ParsedResume> GetParsedResume(Resume resume, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(resume.RawText))
            return await _parser.ParseFromTextAsync(resume.RawText, ct);

        return new ParsedResume
        {
            Summary = resume.Summary,
            Skills = ParseJsonList(resume.Skills),
            Experience = ParseExperience(resume.Experience),
            Education = ParseEducation(resume.Education),
            Certifications = ParseJsonList(resume.Certifications),
            Projects = ParseProjects(resume.Projects)
        };
    }

    private async Task<ParsedResume> TailorWithAI(ParsedResume parsed, JobListing job, TailoringOptions? options, CancellationToken ct)
    {
        var prompt = "Tailor this resume for the job. Focus on relevant skills and achievements.\n\n" +
                     "JOB TITLE: " + job.Title + "\nCOMPANY: " + job.Company + "\nDESCRIPTION: " + job.Description + "\n\n" +
                     "CURRENT RESUME:\n" + JsonSerializer.Serialize(parsed) + "\n\n" +
                     "Return a JSON object with the same structure but optimized for this job.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<TailoredResumeDto>(prompt, null, ct);
            if (result != null)
            {
                return new ParsedResume
                {
                    Summary = result.Summary ?? parsed.Summary,
                    Skills = result.Skills ?? parsed.Skills,
                    Experience = result.Experience?.Select(e => new ExperienceEntry
                    {
                        Company = e.Company ?? "", Title = e.Title ?? "",
                        Description = e.Description, Achievements = e.Achievements ?? new()
                    }).ToList() ?? parsed.Experience,
                    Education = parsed.Education,
                    Certifications = parsed.Certifications,
                    Projects = parsed.Projects,
                    Contact = parsed.Contact
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI tailoring failed, using original");
        }
        return parsed;
    }

    private async Task<byte[]> GenerateDocxAsync(ParsedResume resume, CancellationToken ct)
    {
        using var ms = new MemoryStream();

        await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            var body = mainPart.Document.Body!;

            // Contact Section
            if (resume.Contact != null)
            {
                AddParagraph(body, resume.Contact.Name ?? "Your Name", true, 24);
                var contact = string.Join(" | ", new[] { resume.Contact.Email, resume.Contact.Phone, resume.Contact.LinkedIn }
                    .Where(s => !string.IsNullOrEmpty(s)));
                if (!string.IsNullOrEmpty(contact))
                    AddParagraph(body, contact, false, 10);
            }

            // Summary
            if (!string.IsNullOrEmpty(resume.Summary))
            {
                AddParagraph(body, "PROFESSIONAL SUMMARY", true, 12);
                AddParagraph(body, resume.Summary, false, 11);
            }

            // Skills
            if (resume.Skills.Any())
            {
                AddParagraph(body, "SKILLS", true, 12);
                AddParagraph(body, string.Join(" | ", resume.Skills), false, 11);
            }

            // Experience
            if (resume.Experience.Any())
            {
                AddParagraph(body, "EXPERIENCE", true, 12);
                foreach (var exp in resume.Experience)
                {
                    AddParagraph(body, exp.Title + " - " + exp.Company, true, 11);
                    if (!string.IsNullOrEmpty(exp.Description))
                        AddParagraph(body, exp.Description, false, 10);
                    foreach (var ach in exp.Achievements)
                        AddParagraph(body, "• " + ach, false, 10);
                }
            }

            // Education
            if (resume.Education.Any())
            {
                AddParagraph(body, "EDUCATION", true, 12);
                foreach (var edu in resume.Education)
                    AddParagraph(body, edu.Degree + " - " + edu.Institution, false, 11);
            }

            // Projects
            if (resume.Projects.Any())
            {
                AddParagraph(body, "PROJECTS", true, 12);
                foreach (var proj in resume.Projects)
                {
                    AddParagraph(body, proj.Name, true, 11);
                    if (!string.IsNullOrEmpty(proj.Description))
                        AddParagraph(body, proj.Description, false, 10);
                }
            }

            doc.Save();
        }, ct);

        return ms.ToArray();
    }

    private void AddParagraph(Body body, string text, bool bold, int fontSize)
    {
        var props = new RunProperties();
        if (bold) props.Append(new Bold());
        props.Append(new FontSize { Val = (fontSize * 2).ToString() });

        var run = new Run(props, new Text(text));
        body.Append(new Paragraph(run));
    }

    private List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }

    private List<ExperienceEntry> ParseExperience(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<ExperienceEntry>>(json) ?? new(); }
        catch { return new(); }
    }

    private List<EducationEntry> ParseEducation(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<EducationEntry>>(json) ?? new(); }
        catch { return new(); }
    }

    private List<ProjectEntry> ParseProjects(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<ProjectEntry>>(json) ?? new(); }
        catch { return new(); }
    }
}

internal class TailoredResumeDto
{
    public string? Summary { get; set; }
    public List<string>? Skills { get; set; }
    public List<TailoredExpDto>? Experience { get; set; }
}

internal class TailoredExpDto
{
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Achievements { get; set; }
}