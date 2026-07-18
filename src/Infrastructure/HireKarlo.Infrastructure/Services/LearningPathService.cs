using System.Text.Json;
using HireKarlo.Application.Interfaces.AI;
using HireKarlo.Application.Interfaces.Services;
using HireKarlo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HireKarlo.Infrastructure.Services;

public class LearningPathService : ILearningPathService
{
    private readonly IOpenAIService _openAIService;
    private readonly IVectorStoreService _vectorStore;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<LearningPathService> _logger;

    // In-memory storage for demo - replace with repository
    private static readonly Dictionary<Guid, LearningPath> _paths = new();
    private static readonly Dictionary<Guid, LearningModule> _modules = new();

    public LearningPathService(
        IOpenAIService openAIService,
        IVectorStoreService vectorStore,
        IEmbeddingService embeddingService,
        ILogger<LearningPathService> logger)
    {
        _openAIService = openAIService;
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<LearningPathResult> GenerateCompanyPathAsync(
        Guid userId, 
        string company, 
        string targetRole, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating company-specific learning path for {Company} - {Role}", company, targetRole);

        // RAG: Search for company-specific interview patterns
        var companyContext = await GetCompanyInterviewContextAsync(company, targetRole, cancellationToken);

        var prompt = "Create a structured learning path for someone preparing for a " + targetRole + " interview at " + company + ".\n\n" +
            "CONTEXT FROM INTERVIEW EXPERIENCES:\n" + companyContext + "\n\n" +
            "Generate a comprehensive JSON learning path:\n" +
            "{\n" +
            "    \"title\": \"" + company + " " + targetRole + " Interview Prep\",\n" +
            "    \"description\": \"Personalized path based on " + company + " interview patterns\",\n" +
            "    \"estimatedWeeks\": 6,\n" +
            "    \"modules\": [\n" +
            "        {\n" +
            "            \"order\": 1,\n" +
            "            \"title\": \"Module Title\",\n" +
            "            \"description\": \"What you'll learn\",\n" +
            "            \"category\": \"DSA|System Design|Behavioral|Domain\",\n" +
            "            \"type\": \"Lesson|Quiz|Practice|MockInterview|SystemDesignExercise\",\n" +
            "            \"estimatedMinutes\": 60,\n" +
            "            \"topics\": [\"topic1\", \"topic2\"],\n" +
            "            \"companyRelevance\": \"Why this matters for " + company + "\"\n" +
            "        }\n" +
            "    ]\n" +
            "}\n\n" +
            "Include 10-15 modules covering:\n" +
            "1. Company culture & values (behavioral prep)\n" +
            "2. Most asked DSA patterns at " + company + "\n" +
            "3. System design specific to " + company + " scale\n" +
            "4. Role-specific technical skills\n" +
            "5. Mock interviews and practice";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<GeneratedPathDto>(prompt, null, cancellationToken: cancellationToken);

            if (result == null)
            {
                return new LearningPathResult { Success = false, Error = "Failed to generate learning path" };
            }

            var path = await CreatePathFromDto(userId, result, LearningPathType.CompanySpecific, company, targetRole, cancellationToken);

            return new LearningPathResult
            {
                Success = true,
                PathId = path.Id,
                Title = path.Title,
                TotalModules = path.TotalModules,
                EstimatedWeeks = path.EstimatedWeeks,
                Modules = path.Modules.Select(m => new ModuleSummary
                {
                    ModuleId = m.Id,
                    Order = m.Order,
                    Title = m.Title,
                    Category = m.Category ?? "",
                    Type = m.Type,
                    EstimatedMinutes = m.EstimatedMinutes,
                    Status = m.Status
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating company path");
            return new LearningPathResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<LearningPathResult> GenerateSkillPathAsync(
        Guid userId, 
        List<string> skills, 
        int difficultyLevel, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating skill-based learning path for skills: {Skills}", string.Join(", ", skills));

        var skillsText = string.Join(", ", skills);
        var difficultyText = difficultyLevel switch
        {
            1 => "Beginner",
            2 => "Elementary", 
            3 => "Intermediate",
            4 => "Advanced",
            5 => "Expert",
            _ => "Intermediate"
        };

        var prompt = "Create a structured learning path to master these skills: " + skillsText + "\n" +
            "Difficulty Level: " + difficultyText + " (Level " + difficultyLevel + "/5)\n\n" +
            "Generate a comprehensive JSON learning path:\n" +
            "{\n" +
            "    \"title\": \"Master " + skills.FirstOrDefault() + " and Related Skills\",\n" +
            "    \"description\": \"Structured path to master " + skillsText + "\",\n" +
            "    \"estimatedWeeks\": 8,\n" +
            "    \"modules\": [\n" +
            "        {\n" +
            "            \"order\": 1,\n" +
            "            \"title\": \"Module Title\",\n" +
            "            \"description\": \"Learning objectives\",\n" +
            "            \"category\": \"Fundamentals|Intermediate|Advanced|Practice\",\n" +
            "            \"type\": \"Lesson|Quiz|Practice|Project\",\n" +
            "            \"estimatedMinutes\": 90,\n" +
            "            \"prerequisites\": [],\n" +
            "            \"skills\": [\"skill being taught\"]\n" +
            "        }\n" +
            "    ]\n" +
            "}\n\n" +
            "Structure modules progressively from basics to advanced.\n" +
            "Include hands-on projects and quizzes for each skill.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<GeneratedPathDto>(prompt, null, cancellationToken: cancellationToken);

            if (result == null)
            {
                return new LearningPathResult { Success = false, Error = "Failed to generate learning path" };
            }

            var path = await CreatePathFromDto(userId, result, LearningPathType.SkillBased, null, null, cancellationToken);
            path.SkillsJson = JsonSerializer.Serialize(skills);

            return new LearningPathResult
            {
                Success = true,
                PathId = path.Id,
                Title = path.Title,
                TotalModules = path.TotalModules,
                EstimatedWeeks = path.EstimatedWeeks,
                Modules = path.Modules.Select(m => new ModuleSummary
                {
                    ModuleId = m.Id,
                    Order = m.Order,
                    Title = m.Title,
                    Category = m.Category ?? "",
                    Type = m.Type,
                    EstimatedMinutes = m.EstimatedMinutes,
                    Status = m.Status
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating skill path");
            return new LearningPathResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<LearningPathResult> GenerateInterviewPatternPathAsync(
        Guid userId, 
        string company, 
        InterviewPatternOptions options, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating interview pattern path for {Company}", company);

        // RAG: Get interview patterns from vector store
        var patterns = await GetInterviewPatternsAsync(company, options.FocusAreas, cancellationToken);

        var focusText = string.Join(", ", options.FocusAreas);
        var prompt = "Create an intensive interview prep path based on real interview patterns at " + company + ".\n\n" +
            "INTERVIEW PATTERN DATA:\n" + patterns + "\n\n" +
            "Focus Areas: " + focusText + "\n" +
            "Weeks until interview: " + options.WeeksToInterview + "\n" +
            "Include mock interviews: " + options.IncludeMockInterviews + "\n\n" +
            "Generate a JSON learning path optimized for the timeline:\n" +
            "{\n" +
            "    \"title\": \"" + company + " Interview Sprint - " + options.WeeksToInterview + " Week Plan\",\n" +
            "    \"description\": \"Intensive prep based on real " + company + " interview patterns\",\n" +
            "    \"estimatedWeeks\": " + options.WeeksToInterview + ",\n" +
            "    \"modules\": [\n" +
            "        {\n" +
            "            \"order\": 1,\n" +
            "            \"title\": \"Module Title\",\n" +
            "            \"description\": \"Focus area\",\n" +
            "            \"category\": \"DSA|System Design|Behavioral\",\n" +
            "            \"type\": \"Lesson|Quiz|Practice|MockInterview|SystemDesignExercise\",\n" +
            "            \"estimatedMinutes\": 60,\n" +
            "            \"patternMatch\": \"Which interview pattern this addresses\",\n" +
            "            \"practiceProblems\": [\"problem1\", \"problem2\"]\n" +
            "        }\n" +
            "    ]\n" +
            "}\n\n" +
            "Prioritize most-asked patterns. Daily practice problems mandatory.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<GeneratedPathDto>(prompt, null, cancellationToken: cancellationToken);

            if (result == null)
            {
                return new LearningPathResult { Success = false, Error = "Failed to generate learning path" };
            }

            var path = await CreatePathFromDto(userId, result, LearningPathType.InterviewPattern, company, null, cancellationToken);

            return new LearningPathResult
            {
                Success = true,
                PathId = path.Id,
                Title = path.Title,
                TotalModules = path.TotalModules,
                EstimatedWeeks = path.EstimatedWeeks,
                Modules = path.Modules.Select(m => new ModuleSummary
                {
                    ModuleId = m.Id,
                    Order = m.Order,
                    Title = m.Title,
                    Category = m.Category ?? "",
                    Type = m.Type,
                    EstimatedMinutes = m.EstimatedMinutes,
                    Status = m.Status
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating interview pattern path");
            return new LearningPathResult { Success = false, Error = ex.Message };
        }
    }

    public Task<LearningPath?> GetUserActivePathAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var path = _paths.Values.FirstOrDefault(p => p.UserId == userId && p.IsActive);
        return Task.FromResult(path);
    }

    public Task<List<LearningPath>> GetUserPathsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var paths = _paths.Values.Where(p => p.UserId == userId).ToList();
        return Task.FromResult(paths);
    }

    public Task<LearningPath?> GetPathWithModulesAsync(Guid pathId, CancellationToken cancellationToken = default)
    {
        _paths.TryGetValue(pathId, out var path);
        return Task.FromResult(path);
    }

    public Task StartPathAsync(Guid pathId, CancellationToken cancellationToken = default)
    {
        if (_paths.TryGetValue(pathId, out var path))
        {
            path.StartedAt = DateTime.UtcNow;
            path.IsActive = true;

            // Unlock first module
            var firstModule = path.Modules.OrderBy(m => m.Order).FirstOrDefault();
            if (firstModule != null)
            {
                firstModule.Status = LearningModuleStatus.Available;
            }
        }
        return Task.CompletedTask;
    }

    public Task PausePathAsync(Guid pathId, CancellationToken cancellationToken = default)
    {
        if (_paths.TryGetValue(pathId, out var path))
        {
            path.IsActive = false;
        }
        return Task.CompletedTask;
    }

    public async Task<ModuleContent> GetModuleContentAsync(Guid moduleId, CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleId, out var module))
        {
            throw new InvalidOperationException("Module not found: " + moduleId);
        }

        // Check if we have cached content
        if (!string.IsNullOrEmpty(module.ContentJson))
        {
            var cached = JsonSerializer.Deserialize<ModuleContent>(module.ContentJson);
            if (cached != null) return cached;
        }

        // Generate content using RAG
        var content = await GenerateModuleContentAsync(module, cancellationToken);

        // Cache it
        module.ContentJson = JsonSerializer.Serialize(content);

        return content;
    }

    public Task StartModuleAsync(Guid moduleId, CancellationToken cancellationToken = default)
    {
        if (_modules.TryGetValue(moduleId, out var module))
        {
            module.Status = LearningModuleStatus.InProgress;
            module.StartedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task CompleteModuleAsync(Guid moduleId, int? score = null, CancellationToken cancellationToken = default)
    {
        if (_modules.TryGetValue(moduleId, out var module))
        {
            module.Status = LearningModuleStatus.Completed;
            module.CompletedAt = DateTime.UtcNow;
            module.Score = score;

            // Update path progress
            if (_paths.TryGetValue(module.LearningPathId, out var path))
            {
                path.CompletedModules = path.Modules.Count(m => m.Status == LearningModuleStatus.Completed);

                // Unlock next module
                var nextModule = path.Modules
                    .Where(m => m.Order > module.Order && m.Status == LearningModuleStatus.Locked)
                    .OrderBy(m => m.Order)
                    .FirstOrDefault();

                if (nextModule != null)
                {
                    nextModule.Status = LearningModuleStatus.Available;
                }

                // Check if path is complete
                if (path.CompletedModules == path.TotalModules)
                {
                    path.CompletedAt = DateTime.UtcNow;
                    path.IsActive = false;
                }
            }
        }
        return Task.CompletedTask;
    }

    public async Task<QuizContent> GenerateQuizAsync(Guid moduleId, CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleId, out var module))
        {
            throw new InvalidOperationException("Module not found: " + moduleId);
        }

        // Check cached quiz
        if (!string.IsNullOrEmpty(module.QuizQuestionsJson))
        {
            var cached = JsonSerializer.Deserialize<QuizContent>(module.QuizQuestionsJson);
            if (cached != null) return cached;
        }

        var prompt = "Generate a quiz for the learning module: " + module.Title + "\n" +
            "Category: " + module.Category + "\n" +
            "Description: " + module.Description + "\n\n" +
            "Create a JSON quiz with 10 questions:\n" +
            "{\n" +
            "    \"moduleId\": \"" + moduleId + "\",\n" +
            "    \"title\": \"" + module.Title + " Quiz\",\n" +
            "    \"timeLimitMinutes\": 15,\n" +
            "    \"passingScore\": 70,\n" +
            "    \"questions\": [\n" +
            "        {\n" +
            "            \"questionNumber\": 1,\n" +
            "            \"question\": \"Question text\",\n" +
            "            \"type\": \"MultipleChoice|MultiSelect|TrueFalse|CodeOutput\",\n" +
            "            \"options\": [\"A) option1\", \"B) option2\", \"C) option3\", \"D) option4\"],\n" +
            "            \"codeSnippet\": null,\n" +
            "            \"hint\": \"Optional hint\",\n" +
            "            \"points\": 10,\n" +
            "            \"correctAnswer\": \"A\",\n" +
            "            \"explanation\": \"Why this is correct\"\n" +
            "        }\n" +
            "    ]\n" +
            "}\n\n" +
            "Mix question types. Include code output questions for technical topics.\n" +
            "Make questions progressively harder.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<QuizContentDto>(prompt, null, cancellationToken: cancellationToken);

            var quiz = new QuizContent
            {
                ModuleId = moduleId,
                Title = result?.Title ?? module.Title + " Quiz",
                TimeLimitMinutes = result?.TimeLimitMinutes ?? 15,
                PassingScore = result?.PassingScore ?? 70,
                Questions = result?.Questions?.Select(q => new QuizQuestion
                {
                    QuestionNumber = q.QuestionNumber,
                    Question = q.Question ?? "",
                    Type = Enum.TryParse<QuestionType>(q.Type, out var type) ? type : QuestionType.MultipleChoice,
                    Options = q.Options ?? new List<string>(),
                    CodeSnippet = q.CodeSnippet,
                    Hint = q.Hint,
                    Points = q.Points > 0 ? q.Points : 10
                }).ToList() ?? new List<QuizQuestion>()
            };

            // Cache the quiz (with answers for grading)
            module.QuizQuestionsJson = JsonSerializer.Serialize(result);

            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            return new QuizContent
            {
                ModuleId = moduleId,
                Title = module.Title + " Quiz",
                TimeLimitMinutes = 15,
                PassingScore = 70,
                Questions = GenerateDefaultQuizQuestions(module.Category ?? "General")
            };
        }
    }

    public async Task<QuizResult> SubmitQuizAsync(
        Guid moduleId, 
        List<QuizAnswer> answers, 
        CancellationToken cancellationToken = default)
    {
        if (!_modules.TryGetValue(moduleId, out var module))
        {
            throw new InvalidOperationException("Module not found");
        }

        // Get the quiz with answers
        var quizData = JsonSerializer.Deserialize<QuizContentDto>(module.QuizQuestionsJson ?? "{}");
        if (quizData?.Questions == null)
        {
            throw new InvalidOperationException("Quiz not found for this module");
        }

        var results = new List<QuestionResult>();
        var correctCount = 0;
        var weakAreas = new HashSet<string>();

        foreach (var answer in answers)
        {
            var question = quizData.Questions.FirstOrDefault(q => q.QuestionNumber == answer.QuestionNumber);
            if (question == null) continue;

            var isCorrect = string.Equals(answer.Answer?.Trim(), question.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (isCorrect) correctCount++;
            else if (!string.IsNullOrEmpty(question.Category)) weakAreas.Add(question.Category);

            results.Add(new QuestionResult
            {
                QuestionNumber = answer.QuestionNumber,
                IsCorrect = isCorrect,
                CorrectAnswer = question.CorrectAnswer ?? "",
                Explanation = question.Explanation,
                Category = question.Category
            });
        }

        var scorePercentage = answers.Count > 0 ? (correctCount * 100) / answers.Count : 0;
        var passed = scorePercentage >= (quizData.PassingScore > 0 ? quizData.PassingScore : 70);

        // Generate recommendations if failed
        var recommendations = new List<string>();
        if (!passed && weakAreas.Any())
        {
            recommendations = await GenerateQuizRecommendationsAsync(weakAreas.ToList(), cancellationToken);
        }

        // Store attempt
        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            LearningModuleId = moduleId,
            UserId = Guid.Empty, // Should come from context
            AttemptNumber = module.QuizAttempts.Count + 1,
            TotalQuestions = answers.Count,
            CorrectAnswers = correctCount,
            ScorePercentage = scorePercentage,
            AnswersJson = JsonSerializer.Serialize(results),
            AttemptedAt = DateTime.UtcNow
        };
        module.QuizAttempts.Add(attempt);

        // Update module score if passed
        if (passed)
        {
            module.Score = Math.Max(module.Score ?? 0, scorePercentage);
        }

        return new QuizResult
        {
            TotalQuestions = answers.Count,
            CorrectAnswers = correctCount,
            ScorePercentage = scorePercentage,
            Passed = passed,
            QuestionResults = results,
            WeakAreas = weakAreas.ToList(),
            Recommendations = recommendations
        };
    }

    public Task<List<QuizAttempt>> GetQuizAttemptsAsync(Guid moduleId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (_modules.TryGetValue(moduleId, out var module))
        {
            return Task.FromResult(module.QuizAttempts.Where(a => a.UserId == userId).ToList());
        }
        return Task.FromResult(new List<QuizAttempt>());
    }

    public async Task<List<LearningRecommendation>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var weakAreas = await GetWeakAreasAsync(userId, cancellationToken);
        var completedPaths = _paths.Values
            .Where(p => p.UserId == userId && p.CompletedAt.HasValue)
            .Select(p => p.Title)
            .ToList();

        var prompt = "Generate personalized learning recommendations.\n\n" +
            "User's weak areas: " + string.Join(", ", weakAreas) + "\n" +
            "Completed paths: " + string.Join(", ", completedPaths) + "\n\n" +
            "Return a JSON array of 3-5 recommendations:\n" +
            "[\n" +
            "    {\n" +
            "        \"title\": \"Path title\",\n" +
            "        \"description\": \"What they'll learn\",\n" +
            "        \"type\": \"CompanySpecific|SkillBased|RoleBased|InterviewPattern\",\n" +
            "        \"targetCompany\": null,\n" +
            "        \"skills\": [\"skill1\", \"skill2\"],\n" +
            "        \"estimatedWeeks\": 4,\n" +
            "        \"difficultyLevel\": 3,\n" +
            "        \"reason\": \"Why this is recommended based on their profile\"\n" +
            "    }\n" +
            "]";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<List<LearningRecommendation>>(prompt, cancellationToken: cancellationToken);
            return result ?? GetDefaultRecommendations();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating recommendations");
            return GetDefaultRecommendations();
        }
    }

    public Task<List<string>> GetWeakAreasAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPaths = _paths.Values.Where(p => p.UserId == userId).ToList();
        var weakAreas = new HashSet<string>();

        foreach (var path in userPaths)
        {
            foreach (var module in path.Modules)
            {
                // Check quiz attempts
                var failedAttempts = module.QuizAttempts.Where(a => a.ScorePercentage < 70);
                foreach (var attempt in failedAttempts)
                {
                    if (!string.IsNullOrEmpty(attempt.AnswersJson))
                    {
                        var results = JsonSerializer.Deserialize<List<QuestionResult>>(attempt.AnswersJson);
                        if (results != null)
                        {
                            foreach (var result in results.Where(r => !r.IsCorrect && !string.IsNullOrEmpty(r.Category)))
                            {
                                weakAreas.Add(result.Category!);
                            }
                        }
                    }
                }

                // Low scores
                if (module.Score.HasValue && module.Score < 70 && !string.IsNullOrEmpty(module.Category))
                {
                    weakAreas.Add(module.Category);
                }
            }
        }

        return Task.FromResult(weakAreas.ToList());
    }

    #region Private Helper Methods

    private async Task<string> GetCompanyInterviewContextAsync(string company, string role, CancellationToken cancellationToken)
    {
        var query = company + " " + role + " interview questions patterns experience";
        var results = await _vectorStore.SearchAsync(query, "interview_experience", topK: 5, cancellationToken: cancellationToken);

        if (!results.Any())
        {
            return "No specific interview data found. Use general best practices for FAANG-style interviews.";
        }

        return string.Join("\n\n", results.Select(r => r.Content));
    }

    private async Task<string> GetInterviewPatternsAsync(string company, List<string> focusAreas, CancellationToken cancellationToken)
    {
        var query = company + " interview " + string.Join(" ", focusAreas) + " patterns questions";
        var results = await _vectorStore.SearchAsync(query, "interview_experience", topK: 10, cancellationToken: cancellationToken);

        if (!results.Any())
        {
            return "Common patterns: Two Pointers, Sliding Window, BFS/DFS, Dynamic Programming for DSA. " +
                   "Scalability, Caching, Load Balancing, Database Sharding for System Design.";
        }

        return string.Join("\n\n", results.Select(r => r.Content));
    }

    private async Task<LearningPath> CreatePathFromDto(
        Guid userId, 
        GeneratedPathDto dto, 
        LearningPathType type,
        string? company,
        string? role,
        CancellationToken cancellationToken)
    {
        var path = new LearningPath
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title ?? "Learning Path",
            Description = dto.Description,
            Type = type,
            TargetCompany = company,
            TargetRole = role,
            TotalModules = dto.Modules?.Count ?? 0,
            EstimatedWeeks = dto.EstimatedWeeks,
            DifficultyLevel = 3,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Modules != null)
        {
            foreach (var moduleDto in dto.Modules)
            {
                var module = new LearningModule
                {
                    Id = Guid.NewGuid(),
                    LearningPathId = path.Id,
                    Order = moduleDto.Order,
                    Title = moduleDto.Title ?? "Module",
                    Description = moduleDto.Description,
                    Category = moduleDto.Category,
                    Type = ParseModuleType(moduleDto.Type),
                    EstimatedMinutes = moduleDto.EstimatedMinutes > 0 ? moduleDto.EstimatedMinutes : 60,
                    Status = moduleDto.Order == 1 ? LearningModuleStatus.Available : LearningModuleStatus.Locked,
                    CreatedAt = DateTime.UtcNow
                };

                path.Modules.Add(module);
                _modules[module.Id] = module;
            }
        }

        // Index path for RAG
        var pathContent = path.Title + " " + path.Description + " " + 
            string.Join(" ", path.Modules.Select(m => m.Title + " " + m.Category));
        path.EmbeddingId = await _vectorStore.IndexDocumentAsync(
            pathContent, 
            "learning_path",
            new Dictionary<string, string> 
            { 
                { "userId", userId.ToString() },
                { "type", type.ToString() }
            },
            cancellationToken);

        _paths[path.Id] = path;
        return path;
    }

    private async Task<ModuleContent> GenerateModuleContentAsync(LearningModule module, CancellationToken cancellationToken)
    {
        var prompt = "Generate detailed learning content for this module:\n\n" +
            "Title: " + module.Title + "\n" +
            "Category: " + module.Category + "\n" +
            "Type: " + module.Type + "\n" +
            "Description: " + module.Description + "\n\n" +
            "Return a JSON object:\n" +
            "{\n" +
            "    \"lessons\": [\n" +
            "        {\n" +
            "            \"order\": 1,\n" +
            "            \"title\": \"Lesson title\",\n" +
            "            \"content\": \"Detailed explanation with markdown\",\n" +
            "            \"codeExample\": \"Code if applicable\",\n" +
            "            \"keyTakeaways\": [\"takeaway1\", \"takeaway2\"]\n" +
            "        }\n" +
            "    ],\n" +
            "    \"resources\": [\n" +
            "        {\"title\": \"Resource name\", \"url\": \"https://...\", \"type\": \"video|article|doc\"}\n" +
            "    ],\n" +
            "    \"practiceQuestions\": [\n" +
            "        {\n" +
            "            \"question\": \"Practice problem\",\n" +
            "            \"hint\": \"Hint for solving\",\n" +
            "            \"difficulty\": \"Easy|Medium|Hard\",\n" +
            "            \"solutionApproach\": \"How to approach this\"\n" +
            "        }\n" +
            "    ]\n" +
            "}\n\n" +
            "Make content comprehensive with 3-5 lessons and 2-3 practice questions.";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<ModuleContentDto>(prompt, null, cancellationToken: cancellationToken);

            return new ModuleContent
            {
                ModuleId = module.Id,
                Title = module.Title,
                Description = module.Description ?? "",
                Type = module.Type,
                Category = module.Category ?? "",
                EstimatedMinutes = module.EstimatedMinutes,
                Lessons = result?.Lessons?.Select(l => new Lesson
                {
                    Order = l.Order,
                    Title = l.Title ?? "",
                    Content = l.Content ?? "",
                    CodeExample = l.CodeExample,
                    KeyTakeaways = l.KeyTakeaways ?? new List<string>()
                }).ToList() ?? new List<Lesson>(),
                Resources = result?.Resources?.Select(r => new Resource
                {
                    Title = r.Title ?? "",
                    Url = r.Url ?? "",
                    Type = r.Type ?? "article"
                }).ToList() ?? new List<Resource>(),
                PracticeQuestions = result?.PracticeQuestions?.Select(p => new PracticeQuestion
                {
                    Question = p.Question ?? "",
                    Hint = p.Hint,
                    Difficulty = p.Difficulty ?? "Medium",
                    SolutionApproach = p.SolutionApproach
                }).ToList() ?? new List<PracticeQuestion>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating module content");
            return new ModuleContent
            {
                ModuleId = module.Id,
                Title = module.Title,
                Description = module.Description ?? "",
                Type = module.Type,
                Category = module.Category ?? "",
                EstimatedMinutes = module.EstimatedMinutes
            };
        }
    }

    private async Task<List<string>> GenerateQuizRecommendationsAsync(List<string> weakAreas, CancellationToken cancellationToken)
    {
        var prompt = "Generate specific study recommendations for these weak areas: " + string.Join(", ", weakAreas) + "\n\n" +
            "Return a JSON array of 3-5 actionable recommendations:\n" +
            "[\"recommendation 1\", \"recommendation 2\"]";

        try
        {
            var result = await _openAIService.CompleteAsJsonAsync<List<string>>(prompt, cancellationToken: cancellationToken);
            return result ?? new List<string> { "Review the failed topics", "Practice more problems" };
        }
        catch
        {
            return new List<string> { "Review the concepts you missed", "Practice similar problems" };
        }
    }

    private ModuleType ParseModuleType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "lesson" => ModuleType.Lesson,
            "quiz" => ModuleType.Quiz,
            "practice" => ModuleType.Practice,
            "project" => ModuleType.Project,
            "mockinterview" => ModuleType.MockInterview,
            "systemdesignexercise" => ModuleType.SystemDesignExercise,
            _ => ModuleType.Lesson
        };
    }

    private List<QuizQuestion> GenerateDefaultQuizQuestions(string category)
    {
        return new List<QuizQuestion>
        {
            new QuizQuestion
            {
                QuestionNumber = 1,
                Question = "What is the time complexity of binary search?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string> { "A) O(n)", "B) O(log n)", "C) O(n^2)", "D) O(1)" },
                Points = 10
            },
            new QuizQuestion
            {
                QuestionNumber = 2,
                Question = "Hash tables provide O(1) average case lookup time.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                Points = 10
            }
        };
    }

    private List<LearningRecommendation> GetDefaultRecommendations()
    {
        return new List<LearningRecommendation>
        {
            new LearningRecommendation
            {
                Title = "Master System Design",
                Description = "Learn to design scalable distributed systems",
                Type = LearningPathType.SkillBased,
                Skills = new List<string> { "System Design", "Scalability", "Distributed Systems" },
                EstimatedWeeks = 6,
                DifficultyLevel = 4,
                Reason = "Essential for senior engineering roles"
            },
            new LearningRecommendation
            {
                Title = "DSA Fundamentals",
                Description = "Core data structures and algorithms",
                Type = LearningPathType.SkillBased,
                Skills = new List<string> { "Arrays", "Trees", "Graphs", "Dynamic Programming" },
                EstimatedWeeks = 8,
                DifficultyLevel = 3,
                Reason = "Foundation for technical interviews"
            }
        };
    }

    #endregion
}

#region DTOs

internal class GeneratedPathDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int EstimatedWeeks { get; set; }
    public List<GeneratedModuleDto>? Modules { get; set; }
}

internal class GeneratedModuleDto
{
    public int Order { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Type { get; set; }
    public int EstimatedMinutes { get; set; }
    public List<string>? Topics { get; set; }
}

internal class ModuleContentDto
{
    public List<LessonDto>? Lessons { get; set; }
    public List<ResourceDto>? Resources { get; set; }
    public List<PracticeQuestionDto>? PracticeQuestions { get; set; }
}

internal class LessonDto
{
    public int Order { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CodeExample { get; set; }
    public List<string>? KeyTakeaways { get; set; }
}

internal class ResourceDto
{
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Type { get; set; }
}

internal class PracticeQuestionDto
{
    public string? Question { get; set; }
    public string? Hint { get; set; }
    public string? Difficulty { get; set; }
    public string? SolutionApproach { get; set; }
}

internal class QuizContentDto
{
    public string? Title { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public List<QuizQuestionDto>? Questions { get; set; }
}

internal class QuizQuestionDto
{
    public int QuestionNumber { get; set; }
    public string? Question { get; set; }
    public string? Type { get; set; }
    public List<string>? Options { get; set; }
    public string? CodeSnippet { get; set; }
    public string? Hint { get; set; }
    public int Points { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public string? Category { get; set; }
}

#endregion
