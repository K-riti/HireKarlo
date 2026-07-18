using HireKarlo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireKarlo.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.AzureAdB2CId).HasMaxLength(100);
        builder.Property(u => u.LinkedInProfileUrl).HasMaxLength(500);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Location).HasMaxLength(200);
        builder.Property(u => u.TargetRole).HasMaxLength(200);
        builder.Property(u => u.TargetLocations).HasMaxLength(1000);
        builder.Property(u => u.Preferences).HasMaxLength(4000);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.AzureAdB2CId).IsUnique();
    }
}

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FileName).IsRequired().HasMaxLength(255);
        builder.Property(r => r.BlobUrl).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.EmbeddingId).HasMaxLength(100);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Resumes)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.TailoredForJob)
            .WithMany(j => j.TailoredResumes)
            .HasForeignKey(r => r.TailoredForJobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.ParentResume)
            .WithMany(r => r.TailoredVersions)
            .HasForeignKey(r => r.ParentResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.UserId, r.IsMaster });
    }
}

public class JobListingConfiguration : IEntityTypeConfiguration<JobListing>
{
    public void Configure(EntityTypeBuilder<JobListing> builder)
    {
        builder.ToTable("JobListings");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.ExternalId).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(300);
        builder.Property(j => j.Company).IsRequired().HasMaxLength(200);
        builder.Property(j => j.CompanyLogoUrl).HasMaxLength(1000);
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.SalaryRange).HasMaxLength(100);
        builder.Property(j => j.Currency).HasMaxLength(10);
        builder.Property(j => j.JobType).HasMaxLength(50);
        builder.Property(j => j.ExperienceLevel).HasMaxLength(50);
        builder.Property(j => j.ApplyUrl).HasMaxLength(1000);
        builder.Property(j => j.EmbeddingId).HasMaxLength(100);

        builder.HasIndex(j => new { j.ExternalId, j.Source }).IsUnique();
        builder.HasIndex(j => j.Company);
        builder.HasIndex(j => j.PostedDate);
        builder.HasIndex(j => j.IsActive);
    }
}

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Matches)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.JobListing)
            .WithMany(j => j.Matches)
            .HasForeignKey(m => m.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Resume)
            .WithMany()
            .HasForeignKey(m => m.ResumeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.UserId, m.JobListingId }).IsUnique();
        builder.HasIndex(m => new { m.UserId, m.OverallScore });
        builder.HasIndex(m => m.Status);
    }
}

public class ApplicationConfiguration : IEntityTypeConfiguration<Domain.Entities.Application>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Application> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.JobListing)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Resume)
            .WithMany()
            .HasForeignKey(a => a.ResumeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Match)
            .WithMany()
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ReferralContact)
            .WithMany(c => c.ReferredApplications)
            .HasForeignKey(a => a.ReferralContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.UserId, a.JobListingId }).IsUnique();
        builder.HasIndex(a => new { a.UserId, a.Stage });
    }
}

public class DreamCompanyConfiguration : IEntityTypeConfiguration<DreamCompany>
{
    public void Configure(EntityTypeBuilder<DreamCompany> builder)
    {
        builder.ToTable("DreamCompanies");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.LogoUrl).HasMaxLength(1000);
        builder.Property(d => d.Website).HasMaxLength(500);
        builder.Property(d => d.CareersPageUrl).HasMaxLength(500);
        builder.Property(d => d.GreenhouseBoardToken).HasMaxLength(100);
        builder.Property(d => d.LeverCompanyId).HasMaxLength(100);
        builder.Property(d => d.TargetRoles).HasMaxLength(1000);

        builder.HasOne(d => d.User)
            .WithMany(u => u.DreamCompanies)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.UserId, d.Name }).IsUnique();
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.LinkedInUrl).HasMaxLength(500);
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.Company).HasMaxLength(200);
        builder.Property(c => c.Relationship).HasMaxLength(500);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.DreamCompany)
            .WithMany(d => d.Contacts)
            .HasForeignKey(c => c.DreamCompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.UserId);
    }
}

public class RoadmapItemConfiguration : IEntityTypeConfiguration<RoadmapItem>
{
    public void Configure(EntityTypeBuilder<RoadmapItem> builder)
    {
        builder.ToTable("RoadmapItems");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Category).HasMaxLength(100);
        builder.Property(r => r.ResourceLinks).HasMaxLength(4000);
        builder.Property(r => r.SkillTags).HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RoadmapItems)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ParentItem)
            .WithMany(r => r.SubItems)
            .HasForeignKey(r => r.ParentItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.UserId, r.WeekNumber });
        builder.HasIndex(r => new { r.UserId, r.Status });
    }
}

public class InterviewDigestEntryConfiguration : IEntityTypeConfiguration<InterviewDigestEntry>
{
    public void Configure(EntityTypeBuilder<InterviewDigestEntry> builder)
    {
        builder.ToTable("InterviewDigestEntries");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Company).IsRequired().HasMaxLength(200);
        builder.Property(i => i.SourceUrl).IsRequired().HasMaxLength(1000);
        builder.Property(i => i.SourcePlatform).IsRequired().HasMaxLength(50);
        builder.Property(i => i.OriginalTitle).HasMaxLength(500);
        builder.Property(i => i.Role).HasMaxLength(200);
        builder.Property(i => i.InterviewType).HasMaxLength(100);
        builder.Property(i => i.Difficulty).HasMaxLength(50);
        builder.Property(i => i.Topics).HasMaxLength(2000);
        builder.Property(i => i.KeyTakeaways).HasMaxLength(4000);

        builder.HasIndex(i => i.Company);
        builder.HasIndex(i => i.PublishedDate);
        builder.HasIndex(i => i.IncludedInDigest);
    }
}
