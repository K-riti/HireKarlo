using HireKarlo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MockInterviewController : ControllerBase
{
    private readonly IMockInterviewService _mockInterviewService;
    private readonly ILogger<MockInterviewController> _logger;

    public MockInterviewController(
        IMockInterviewService mockInterviewService,
        ILogger<MockInterviewController> logger)
    {
        _mockInterviewService = mockInterviewService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<MockInterviewSession>> StartSession(
        [FromBody] StartSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = new MockInterviewOptions
        {
            TargetCompany = request.Company,
            TargetRole = request.Role,
            Type = request.Type,
            Difficulty = request.Difficulty,
            NumberOfQuestions = request.NumberOfQuestions,
            UseVoice = request.UseVoice
        };

        var session = await _mockInterviewService.StartSessionAsync(
            GetUserId(), options, cancellationToken);

        return Ok(session);
    }

    [HttpGet("{sessionId:guid}/question")]
    public async Task<ActionResult<InterviewQuestion>> GetNextQuestion(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _mockInterviewService.GetNextQuestionAsync(
            sessionId, cancellationToken);

        return Ok(question);
    }

    [HttpPost("{sessionId:guid}/answer")]
    public async Task<ActionResult<AnswerFeedback>> SubmitAnswer(
        Guid sessionId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var feedback = await _mockInterviewService.SubmitAnswerAsync(
            sessionId, request.Answer, cancellationToken);

        return Ok(feedback);
    }

    [HttpPost("{sessionId:guid}/end")]
    public async Task<ActionResult<SessionSummary>> EndSession(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _mockInterviewService.EndSessionAsync(
            sessionId, cancellationToken);

        return Ok(summary);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("oid")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}

public record StartSessionRequest
{
    public string? Company { get; init; }
    public string? Role { get; init; }
    public InterviewType Type { get; init; } = InterviewType.Behavioral;
    public string Difficulty { get; init; } = "Medium";
    public int NumberOfQuestions { get; init; } = 5;
    public bool UseVoice { get; init; } = false;
}

public record SubmitAnswerRequest
{
    public string Answer { get; init; } = string.Empty;
}
