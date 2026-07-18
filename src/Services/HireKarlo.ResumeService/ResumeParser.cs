using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging;

namespace HireKarlo.ResumeService;

public class ResumeParser : IResumeParser
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ResumeParser> _logger;

    public ResumeParser(IOpenAIService openAIService, ILogger<ResumeParser> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ExtractFromPdfAsync(fileStream, cancellationToken),
            ".docx" => await ExtractFromDocxAsync(fileStream, cancellationToken),
            ".txt" => await ExtractFromTxtAsync(fileStream, cancellationToken),
            _ => throw new NotSupportedException("Unsupported file format: " + extension)
        };
    }

    public async Task<ParsedResume> ParseAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var rawText = await ExtractTextAsync(fileStream, fileName, cancellationToken);
        if (string.IsNullOrWhiteSpace(rawText))
            throw new InvalidOperationException("No text content found in resume");

        return await ParseWithAIAsync(rawText, cancellationToken);
    }

    public async Task<ParsedResume> ParseFromTextAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resumeText))
            throw new ArgumentException("Resume text cannot be empty", nameof(resumeText));

        return await ParseWithAIAsync(resumeText, cancellationToken);
    }

    private async Task<string> ExtractFromPdfAsync(Stream stream, CancellationToken ct)
    {
        var text = new StringBuilder();
        await Task.Run(() =>
        {
            using var reader = new PdfReader(stream);
            using var doc = new PdfDocument(reader);
            for (int i = 1; i <= doc.GetNumberOfPages(); i++)
                text.AppendLine(PdfTextExtractor.GetTextFromPage(doc.GetPage(i)));
        }, ct);
        return text.ToString();
    }

    private async Task<string> ExtractFromDocxAsync(Stream stream, CancellationToken ct)
    {
        var text = new StringBuilder();
        await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(stream, false);
            text.Append(doc.MainDocumentPart?.Document.Body?.InnerText ?? "");
        }, ct);
        return text.ToString();
    }

    private async Task<string> ExtractFromTxtAsync(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(ct);
    }

    private async Task<ParsedResume> ParseWithAIAsync(string rawText, CancellationToken ct)
    {
        var prompt = "Parse this resume into JSON format with fields: summary, skills[], experience[], education[], certifications[], projects[], contact{}. Resume:\n" + rawText;

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<ParsedResumeDto>(prompt, null, ct);
            return result != null ? MapToResult(result) : ParseFallback(rawText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI parsing failed, using fallback");
            return ParseFallback(rawText);
        }
    }

    private ParsedResume ParseFallback(string text)
    {
        var skills = new[] { "C#", ".NET", "JavaScript", "Python", "Azure", "AWS", "SQL", "React", "Docker" }
            .Where(s => text.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
        var email = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}").Value;
        var phone = Regex.Match(text, @"[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}").Value;

        return new ParsedResume
        {
            Skills = skills,
            Contact = new ContactInfo { Email = email, Phone = phone }
        };
    }

    private ParsedResume MapToResult(ParsedResumeDto dto) => new()
    {
        Summary = dto.Summary,
        Skills = dto.Skills ?? new(),
        Experience = dto.Experience?.Select(e => new ExperienceEntry
        {
            Company = e.Company ?? "", Title = e.Title ?? "", Location = e.Location,
            StartDate = e.StartDate, EndDate = e.EndDate, Description = e.Description,
            Achievements = e.Achievements ?? new()
        }).ToList() ?? new(),
        Education = dto.Education?.Select(e => new EducationEntry
        {
            Institution = e.Institution ?? "", Degree = e.Degree ?? "", Field = e.Field,
            Location = e.Location, GraduationDate = e.GraduationDate, GPA = e.Gpa
        }).ToList() ?? new(),
        Certifications = dto.Certifications ?? new(),
        Projects = dto.Projects?.Select(p => new ProjectEntry
        {
            Name = p.Name ?? "", Description = p.Description,
            Technologies = p.Technologies ?? new(), Url = p.Url
        }).ToList() ?? new(),
        Contact = dto.Contact != null ? new ContactInfo
        {
            Name = dto.Contact.Name, Email = dto.Contact.Email, Phone = dto.Contact.Phone,
            LinkedIn = dto.Contact.Linkedin, GitHub = dto.Contact.Github
        } : new()
    };
}

internal class ParsedResumeDto
{
    public string? Summary { get; set; }
    public List<string>? Skills { get; set; }
    public List<ExpDto>? Experience { get; set; }
    public List<EduDto>? Education { get; set; }
    public List<string>? Certifications { get; set; }
    public List<ProjDto>? Projects { get; set; }
    public ContactDto? Contact { get; set; }
}

internal class ExpDto
{
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string? Location { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Description { get; set; }
    public List<string>? Achievements { get; set; }
}

internal class EduDto
{
    public string? Institution { get; set; }
    public string? Degree { get; set; }
    public string? Field { get; set; }
    public string? Location { get; set; }
    public string? GraduationDate { get; set; }
    public string? Gpa { get; set; }
}

internal class ProjDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Technologies { get; set; }
    public string? Url { get; set; }
}

internal class ContactDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Linkedin { get; set; }
    public string? Github { get; set; }
}