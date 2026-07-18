using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HireKarlo.Application.Interfaces.AI;

namespace HireKarlo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOpenAIService _openAI;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IOpenAIService openAI, ILogger<ChatController> logger)
    {
        _openAI = openAI;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = @"You are an AI career assistant for HireKarlo, a job search and career development platform. 
You help users with:
- Resume writing and optimization tips
- Interview preparation and practice strategies  
- Salary negotiation advice
- Career path guidance and planning
- Job search strategies
- Networking tips
- LinkedIn profile optimization
- Technical and behavioral interview questions

Be helpful, encouraging, and provide actionable advice. Keep responses concise but informative.
When appropriate, suggest specific actions users can take on the HireKarlo platform.";

            if (!string.IsNullOrEmpty(request.Context))
            {
                systemPrompt += $"\n\nUser context: {request.Context}";
            }

            var prompt = $"{systemPrompt}\n\nUser: {request.Message}\n\nAssistant:";

            var response = await _openAI.CompleteAsync(prompt, new CompletionOptions 
            { 
                MaxTokens = 500,
                Temperature = 0.7f
            }, cancellationToken);

            // Generate follow-up suggestions
            var suggestions = GenerateSuggestions(request.Message, response);
            var actionType = DetermineActionType(request.Message);

            return Ok(new ChatResponse(response, suggestions, actionType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return Ok(new ChatResponse(
                "I apologize, but I'm having trouble processing your request right now. Please try again in a moment.",
                new List<string> { "Try again", "Ask something else" },
                null
            ));
        }
    }

    private List<string> GenerateSuggestions(string message, string response)
    {
        var suggestions = new List<string>();
        var lowerMessage = message.ToLower();

        if (lowerMessage.Contains("resume"))
        {
            suggestions.Add("Upload resume for ATS check");
            suggestions.Add("How to tailor resume for specific jobs?");
        }
        else if (lowerMessage.Contains("interview"))
        {
            suggestions.Add("Start a mock interview");
            suggestions.Add("Common behavioral questions");
        }
        else if (lowerMessage.Contains("salary") || lowerMessage.Contains("negotiate"))
        {
            suggestions.Add("What's my market value?");
            suggestions.Add("How to counter an offer");
        }
        else if (lowerMessage.Contains("linkedin"))
        {
            suggestions.Add("Optimize my LinkedIn profile");
            suggestions.Add("LinkedIn headline examples");
        }
        else if (lowerMessage.Contains("skill") || lowerMessage.Contains("learn"))
        {
            suggestions.Add("Create a learning path");
            suggestions.Add("Most in-demand skills for 2024");
        }
        else
        {
            suggestions.Add("Help with my resume");
            suggestions.Add("Prepare for interviews");
            suggestions.Add("Career advice");
        }

        return suggestions.Take(3).ToList();
    }

    private string? DetermineActionType(string message)
    {
        var lowerMessage = message.ToLower();

        if (lowerMessage.Contains("upload") && lowerMessage.Contains("resume"))
            return "navigate:/resumes";
        if (lowerMessage.Contains("mock interview") || lowerMessage.Contains("practice interview"))
            return "navigate:/mock-interview";
        if (lowerMessage.Contains("learning path") || lowerMessage.Contains("study plan"))
            return "navigate:/learning";
        if (lowerMessage.Contains("linkedin") && lowerMessage.Contains("optimi"))
            return "navigate:/linkedin";
        if (lowerMessage.Contains("find job") || lowerMessage.Contains("search job"))
            return "navigate:/jobs";

        return null;
    }
}

public record ChatRequest(string Message, string? Context);
public record ChatResponse(string Message, List<string>? Suggestions, string? ActionType);
