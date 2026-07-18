using HireKarlo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireKarlo.Persistence;

public class HireKarloDbContext : DbContext
{
    public HireKarloDbContext(DbContextOptions<HireKarloDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobListing> JobListings => Set<JobListing>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Domain.Entities.Application> Applications => Set<Domain.Entities.Application>();
    public DbSet<DreamCompany> DreamCompanies => Set<DreamCompany>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<RoadmapItem> RoadmapItems => Set<RoadmapItem>();
    public DbSet<InterviewDigestEntry> InterviewDigestEntries => Set<InterviewDigestEntry>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningModule> LearningModules => Set<LearningModule>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<VectorDocument> VectorDocuments => Set<VectorDocument>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HireKarloDbContext).Assembly);

        // Disable cascade delete globally to avoid cycles
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
