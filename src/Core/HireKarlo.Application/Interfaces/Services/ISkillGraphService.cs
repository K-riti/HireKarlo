namespace HireKarlo.Application.Interfaces.Services;

using HireKarlo.Domain.Entities;
using HireKarlo.Domain.Enums;

/// <summary>
/// Manages user's skill graph and proficiency tracking
/// Part of Career Operating System foundation
/// </summary>
public interface ISkillGraphService
{
    /// <summary>
    /// Add a skill to user's skill graph
    /// </summary>
    Task<SkillGraph> AddSkillAsync(Guid userId, string skillName, SkillLevel level, string category);

    /// <summary>
    /// Update skill proficiency
    /// </summary>
    Task<SkillGraph> UpdateSkillProficiencyAsync(Guid userId, Guid skillId, int newProficiency);

    /// <summary>
    /// Get all skills for user
    /// </summary>
    Task<List<SkillGraph>> GetUserSkillsAsync(Guid userId);

    /// <summary>
    /// Get skills by category
    /// </summary>
    Task<List<SkillGraph>> GetSkillsByCategoryAsync(Guid userId, string category);

    /// <summary>
    /// Generate embedding vector for a skill
    /// Used for semantic similarity search
    /// </summary>
    Task<float[]> GenerateSkillEmbeddingAsync(string skillName, string? context = null);

    /// <summary>
    /// Generate skill gap recommendations based on dream companies
    /// This is the core of the Skill ROI Engine (USP #2)
    /// </summary>
    Task<List<SkillGapRecommendation>> GenerateSkillRecommendationsAsync(Guid userId);

    /// <summary>
    /// Delete a skill
    /// </summary>
    Task DeleteSkillAsync(Guid userId, Guid skillId);

    /// <summary>
    /// Mark skill as evidence (e.g., certificate, project link)
    /// </summary>
    Task<SkillGraph> AddSkillEvidenceAsync(Guid userId, Guid skillId, string evidenceUrl);

    /// <summary>
    /// Get skill impact metrics (how much it improves match % for each company)
    /// </summary>
    Task<Dictionary<string, double>> GetSkillImpactAsync(Guid userId, Guid skillId);
}
