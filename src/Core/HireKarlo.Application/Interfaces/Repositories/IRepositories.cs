using HireKarlo.Domain.Entities;

namespace HireKarlo.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByAzureAdB2CIdAsync(string azureId, CancellationToken cancellationToken = default);
    Task<User?> GetWithResumesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetWithApplicationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetNewsletterSubscribersAsync(CancellationToken cancellationToken = default);
}

public interface IResumeRepository : IRepository<Resume>
{
    Task<IReadOnlyList<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Resume?> GetMasterResumeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Resume>> GetTailoredVersionsAsync(Guid parentResumeId, CancellationToken cancellationToken = default);
}

public interface IJobListingRepository : IRepository<JobListing>
{
    Task<JobListing?> GetByExternalIdAsync(string externalId, string source, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobListing>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobListing>> SearchAsync(string? query, string? location, bool? remote, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobListing>> GetByCompanyAsync(string company, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobListing>> GetByCompanyAsync(string company, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobListing>> GetRecentJobsAsync(int days, int limit, CancellationToken cancellationToken = default);
}

public interface IMatchRepository : IRepository<Match>
{
    Task<IReadOnlyList<Match>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetHighScoreMatchesAsync(Guid userId, double minScore = 90, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetTopMatchesForUserAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<int> GetApplicationCountForWeekAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IApplicationRepository : IRepository<Domain.Entities.Application>
{
    Task<IReadOnlyList<Domain.Entities.Application>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Application>> GetByStageAsync(Guid userId, Domain.Enums.ApplicationStage stage, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Application?> GetWithDetailsAsync(Guid applicationId, CancellationToken cancellationToken = default);
}

public interface IDreamCompanyRepository : IRepository<DreamCompany>
{
    Task<IReadOnlyList<DreamCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DreamCompany>> GetTrackingEnabledAsync(CancellationToken cancellationToken = default);
}

public interface IContactRepository : IRepository<Contact>
{
    Task<IReadOnlyList<Contact>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contact>> GetByCompanyAsync(Guid userId, Guid dreamCompanyId, CancellationToken cancellationToken = default);
}

public interface IRoadmapItemRepository : IRepository<RoadmapItem>
{
    Task<IReadOnlyList<RoadmapItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoadmapItem>> GetByWeekAsync(Guid userId, int weekNumber, CancellationToken cancellationToken = default);
}

public interface IInterviewDigestEntryRepository : IRepository<InterviewDigestEntry>
{
    Task<IReadOnlyList<InterviewDigestEntry>> GetByCompanyAsync(string company, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InterviewDigestEntry>> GetPendingForDigestAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InterviewDigestEntry>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InterviewDigestEntry>> GetUndigestedEntriesAsync(int limit, CancellationToken cancellationToken = default);
}

// Phase 2 Career OS Repositories
public interface ISkillGraphRepository : IRepository<SkillGraph>
{
    Task<IReadOnlyList<SkillGraph>> GetUserSkillsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SkillGraph?> GetSkillByNameAsync(Guid userId, string skillName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillGraph>> GetSkillsByCategoryAsync(Guid userId, string category, CancellationToken cancellationToken = default);
}

public interface IDreamCompanyMatchRepository : IRepository<DreamCompanyMatch>
{
    Task<DreamCompanyMatch?> GetByCompanyAndUserAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DreamCompanyMatch>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IOpportunityMatchRepository : IRepository<OpportunityMatch>
{
    Task<IReadOnlyList<OpportunityMatch>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpportunityMatch>> GetByCompanyAndUserAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpportunityMatch>> GetTop10ByMatchPercentageAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IReferralTargetRepository : IRepository<ReferralTarget>
{
    Task<IReadOnlyList<ReferralTarget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferralTarget>> GetByCompanyAsync(Guid userId, Guid dreamCompanyId, CancellationToken cancellationToken = default);
    Task<ReferralTarget?> GetByLinkedInUrlAsync(string linkedInUrl, CancellationToken cancellationToken = default);
}

public interface ISkillGapRecommendationRepository : IRepository<SkillGapRecommendation>
{
    Task<IReadOnlyList<SkillGapRecommendation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillGapRecommendation>> GetByPriorityAsync(Guid userId, int priority, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillGapRecommendation>> GetForDreamCompanyAsync(Guid userId, Guid dreamCompanyId, CancellationToken cancellationToken = default);
}

public interface ICareerProgressRepository : IRepository<CareerProgress>
{
    Task<IReadOnlyList<CareerProgress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CareerProgress>> GetByCompanyAsync(Guid userId, Guid dreamCompanyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CareerProgress>> GetRecentMilestonesAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
}
