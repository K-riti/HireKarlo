using HireKarlo.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvancedAIController : ControllerBase
{
    private readonly IAdvancedAIService _advancedAI;
    private readonly ILogger<AdvancedAIController> _logger;

    public AdvancedAIController(IAdvancedAIService advancedAI, ILogger<AdvancedAIController> logger)
    {
        _advancedAI = advancedAI;
        _logger = logger;
    }

    /// <summary>
    /// Predicts application outcome based on user's historical data
    /// </summary>
    [HttpPost("predict-outcome")]
    public async Task<ActionResult<ApplicationPrediction>> PredictOutcome(
        [FromBody] PredictOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var prediction = await _advancedAI.PredictApplicationOutcomeAsync(
            request.ResumeText,
            request.JobDescription,
            request.History,
            cancellationToken);

        return Ok(prediction);
    }

    /// <summary>
    /// Gets explainable ATS score with detailed breakdown
    /// </summary>
    [HttpPost("explainable-ats")]
    public async Task<ActionResult<ExplainableAtsScore>> GetExplainableAts(
        [FromBody] AtsScoreRequest request,
        CancellationToken cancellationToken)
    {
        var score = await _advancedAI.GetExplainableAtsScoreAsync(
            request.ResumeText,
            request.JobDescription,
            cancellationToken);

        return Ok(score);
    }

    /// <summary>
    /// Analyzes keyword trends across multiple job descriptions
    /// </summary>
    [HttpPost("keyword-radar")]
    public async Task<ActionResult<KeywordRadarResult>> AnalyzeKeywordRadar(
        [FromBody] KeywordRadarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _advancedAI.AnalyzeKeywordRadarAsync(
            request.ResumeText,
            request.JobDescriptions,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Simulates skill trajectory over time
    /// </summary>
    [HttpPost("skill-trajectory")]
    public async Task<ActionResult<SkillTrajectory>> SimulateTrajectory(
        [FromBody] TrajectoryRequest request,
        CancellationToken cancellationToken)
    {
        var trajectory = await _advancedAI.SimulateSkillTrajectoryAsync(
            request.CurrentSkills,
            request.TargetRole,
            request.MonthsToProject,
            cancellationToken);

        return Ok(trajectory);
    }

    /// <summary>
    /// Generates 6-month career roadmap
    /// </summary>
    [HttpPost("career-roadmap")]
    public async Task<ActionResult<CareerRoadmap>> GenerateRoadmap(
        [FromBody] RoadmapRequest request,
        CancellationToken cancellationToken)
    {
        var roadmap = await _advancedAI.GenerateCareerRoadmapAsync(
            request.ResumeText,
            request.TargetRole,
            request.TargetCompany,
            cancellationToken);

        return Ok(roadmap);
    }

    /// <summary>
    /// Tailors resume for specific job description
    /// </summary>
    [HttpPost("tailor-resume")]
    public async Task<ActionResult<TailoredResume>> TailorResume(
        [FromBody] AdvancedTailorResumeRequest request,
        CancellationToken cancellationToken)
    {
        var tailored = await _advancedAI.TailorResumeForJobAsync(
            request.ResumeText,
            request.JobDescription,
            cancellationToken);

        return Ok(tailored);
    }

    /// <summary>
    /// Gets contextual interview questions using RAG
    /// </summary>
    [HttpPost("interview-questions")]
    public async Task<ActionResult<List<InterviewQuestionWithContext>>> GetInterviewQuestions(
        [FromBody] InterviewQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var questions = await _advancedAI.GetContextualInterviewQuestionsAsync(
            request.Company,
            request.Role,
            request.QuestionType,
            cancellationToken);

        return Ok(questions);
    }
}

#region Request DTOs

public record PredictOutcomeRequest(
    string ResumeText,
    string JobDescription,
    List<HistoricalApplication> History
);

public record AtsScoreRequest(
    string ResumeText,
    string JobDescription
);

public record KeywordRadarRequest(
    string ResumeText,
    List<string> JobDescriptions
);

public record TrajectoryRequest(
    string CurrentSkills,
    string TargetRole,
    int MonthsToProject = 6
);

public record RoadmapRequest(
    string ResumeText,
    string TargetRole,
    string TargetCompany
);

public record AdvancedTailorResumeRequest(
    string ResumeText,
    string JobDescription
);

public record InterviewQuestionsRequest(
    string Company,
    string Role,
    string QuestionType = "Behavioral"
);

#endregion
