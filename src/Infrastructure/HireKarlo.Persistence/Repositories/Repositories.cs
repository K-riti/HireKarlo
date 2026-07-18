using HireKarlo.Application.Interfaces.Repositories;
using HireKarlo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireKarlo.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HireKarloDbContext _context;

    public UserRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByAzureAdB2CIdAsync(string azureId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdB2CId == azureId, cancellationToken);
    }

    public async Task<User?> GetWithResumesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Resumes)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetWithApplicationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Applications)
                .ThenInclude(a => a.JobListing)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetNewsletterSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.SubscribedToNewsletter && !string.IsNullOrEmpty(u.Email))
            .ToListAsync(cancellationToken);
    }
}

public class ResumeRepository : IResumeRepository
{
    private readonly HireKarloDbContext _context;

    public ResumeRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<Resume?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Resume>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resumes.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resume?> GetMasterResumeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .FirstOrDefaultAsync(r => r.UserId == userId && r.IsMaster, cancellationToken);
    }

    public async Task<IReadOnlyList<Resume>> GetTailoredVersionsAsync(Guid parentResumeId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Where(r => r.ParentResumeId == parentResumeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resume> AddAsync(Resume entity, CancellationToken cancellationToken = default)
    {
        await _context.Resumes.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Resume entity, CancellationToken cancellationToken = default)
    {
        _context.Resumes.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Resume entity, CancellationToken cancellationToken = default)
    {
        _context.Resumes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class JobListingRepository : IJobListingRepository
{
    private readonly HireKarloDbContext _context;

    public JobListingRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<JobListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobListings.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<JobListing>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobListings.ToListAsync(cancellationToken);
    }

    public async Task<JobListing?> GetByExternalIdAsync(string externalId, string source, CancellationToken cancellationToken = default)
    {
        // Parse source string to enum
        if (Enum.TryParse<Domain.Enums.JobSource>(source, true, out var jobSource))
        {
            return await _context.JobListings
                .FirstOrDefaultAsync(j => j.ExternalId == externalId && j.Source == jobSource, cancellationToken);
        }
        return null;
    }

    public async Task<IReadOnlyList<JobListing>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobListings
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobListing>> SearchAsync(string? query, string? location, bool? remote, CancellationToken cancellationToken = default)
    {
        var queryable = _context.JobListings
            .Where(j => j.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            queryable = queryable.Where(j =>
                j.Title.ToLower().Contains(query) ||
                j.Company.ToLower().Contains(query) ||
                (j.Description != null && j.Description.ToLower().Contains(query)));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            location = location.ToLower();
            queryable = queryable.Where(j => j.Location != null && j.Location.ToLower().Contains(location));
        }

        if (remote.HasValue)
        {
            queryable = queryable.Where(j => j.IsRemote == remote.Value);
        }

        return await queryable
            .OrderByDescending(j => j.PostedDate)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobListing>> GetByCompanyAsync(string company, CancellationToken cancellationToken = default)
    {
        return await _context.JobListings
            .Where(j => j.Company.ToLower().Contains(company.ToLower()))
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobListing>> GetByCompanyAsync(string company, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.JobListings
            .Where(j => j.Company.ToLower().Contains(company.ToLower()))
            .OrderByDescending(j => j.PostedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobListing> AddAsync(JobListing entity, CancellationToken cancellationToken = default)
    {
        await _context.JobListings.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(JobListing entity, CancellationToken cancellationToken = default)
    {
        _context.JobListings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(JobListing entity, CancellationToken cancellationToken = default)
    {
        _context.JobListings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobListing>> GetRecentJobsAsync(int days, int limit, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.JobListings
            .Where(j => j.IsActive && j.PostedDate >= cutoffDate)
            .OrderByDescending(j => j.PostedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

public class MatchRepository : IMatchRepository
{
    private readonly HireKarloDbContext _context;

    public MatchRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.JobListing)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Matches.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.JobListing)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.OverallScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetHighScoreMatchesAsync(Guid userId, double minScore = 90, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.JobListing)
            .Where(m => m.UserId == userId && m.OverallScore >= minScore)
            .OrderByDescending(m => m.OverallScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.JobListing)
            .Include(m => m.User)
            .Where(m => !m.NotificationSent && m.OverallScore >= 90)
            .ToListAsync(cancellationToken);
    }

    public async Task<Match> AddAsync(Match entity, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Match entity, CancellationToken cancellationToken = default)
    {
        _context.Matches.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Match entity, CancellationToken cancellationToken = default)
    {
        _context.Matches.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetTopMatchesForUserAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.JobListing)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.OverallScore)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetApplicationCountForWeekAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        return await _context.Applications
            .CountAsync(a => a.UserId == userId && a.AppliedDate >= weekAgo, cancellationToken);
    }
}

public class ApplicationRepository : IApplicationRepository
{
    private readonly HireKarloDbContext _context;

    public ApplicationRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobListing)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Applications.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Resume)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByStageAsync(Guid userId, Domain.Enums.ApplicationStage stage, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobListing)
            .Where(a => a.UserId == userId && a.Stage == stage)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Application?> GetWithDetailsAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Resume)
            .Include(a => a.Match)
            .Include(a => a.ReferralContact)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);
    }

    public async Task<Domain.Entities.Application> AddAsync(Domain.Entities.Application entity, CancellationToken cancellationToken = default)
    {
        await _context.Applications.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Domain.Entities.Application entity, CancellationToken cancellationToken = default)
    {
        _context.Applications.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Domain.Entities.Application entity, CancellationToken cancellationToken = default)
    {
        _context.Applications.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class DreamCompanyRepository : IDreamCompanyRepository
{
    private readonly HireKarloDbContext _context;

    public DreamCompanyRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<DreamCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DreamCompanies.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<DreamCompany>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DreamCompanies.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DreamCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.DreamCompanies
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DreamCompany>> GetTrackingEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DreamCompanies
            .Include(d => d.User)
            .Where(d => d.IsTrackingJobs)
            .ToListAsync(cancellationToken);
    }

    public async Task<DreamCompany> AddAsync(DreamCompany entity, CancellationToken cancellationToken = default)
    {
        await _context.DreamCompanies.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(DreamCompany entity, CancellationToken cancellationToken = default)
    {
        _context.DreamCompanies.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DreamCompany entity, CancellationToken cancellationToken = default)
    {
        _context.DreamCompanies.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class ContactRepository : IContactRepository
{
    private readonly HireKarloDbContext _context;

    public ContactRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contacts.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contact>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Include(c => c.DreamCompany)
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contact>> GetByCompanyAsync(Guid userId, Guid dreamCompanyId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Where(c => c.UserId == userId && c.DreamCompanyId == dreamCompanyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contact> AddAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class RoadmapItemRepository : IRoadmapItemRepository
{
    private readonly HireKarloDbContext _context;

    public RoadmapItemRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<RoadmapItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RoadmapItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<RoadmapItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RoadmapItems.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoadmapItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RoadmapItems
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.WeekNumber)
            .ThenBy(r => r.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoadmapItem>> GetByWeekAsync(Guid userId, int weekNumber, CancellationToken cancellationToken = default)
    {
        return await _context.RoadmapItems
            .Where(r => r.UserId == userId && r.WeekNumber == weekNumber)
            .OrderBy(r => r.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoadmapItem> AddAsync(RoadmapItem entity, CancellationToken cancellationToken = default)
    {
        await _context.RoadmapItems.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(RoadmapItem entity, CancellationToken cancellationToken = default)
    {
        _context.RoadmapItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(RoadmapItem entity, CancellationToken cancellationToken = default)
    {
        _context.RoadmapItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class InterviewDigestEntryRepository : IInterviewDigestEntryRepository
{
    private readonly HireKarloDbContext _context;

    public InterviewDigestEntryRepository(HireKarloDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewDigestEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InterviewDigestEntries.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<InterviewDigestEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InterviewDigestEntries.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InterviewDigestEntry>> GetByCompanyAsync(string company, CancellationToken cancellationToken = default)
    {
        return await _context.InterviewDigestEntries
            .Where(e => e.Company != null && e.Company.ToLower().Contains(company.ToLower()))
            .OrderByDescending(e => e.PublishedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InterviewDigestEntry>> GetPendingForDigestAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InterviewDigestEntries
            .Where(e => !e.IncludedInDigest)
            .OrderByDescending(e => e.FetchedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InterviewDigestEntry>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.InterviewDigestEntries
            .Where(e => e.FetchedDate >= cutoff)
            .OrderByDescending(e => e.FetchedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<InterviewDigestEntry> AddAsync(InterviewDigestEntry entity, CancellationToken cancellationToken = default)
    {
        await _context.InterviewDigestEntries.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(InterviewDigestEntry entity, CancellationToken cancellationToken = default)
    {
        _context.InterviewDigestEntries.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(InterviewDigestEntry entity, CancellationToken cancellationToken = default)
    {
        _context.InterviewDigestEntries.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InterviewDigestEntry>> GetUndigestedEntriesAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await _context.InterviewDigestEntries
            .Where(e => !e.IncludedInDigest)
            .OrderByDescending(e => e.FetchedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
