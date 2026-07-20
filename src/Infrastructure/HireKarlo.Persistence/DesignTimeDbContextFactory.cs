using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HireKarlo.Persistence;

/// <summary>
/// Design-time factory for creating DbContext instances during EF Core migrations.
/// This factory ensures migrations are generated for PostgreSQL.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HireKarloDbContext>
{
    public HireKarloDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HireKarloDbContext>();

        // Use PostgreSQL for migrations - this ensures migrations are generated
        // with PostgreSQL-compatible SQL (uuid instead of uniqueidentifier, etc.)
        // The actual connection string doesn't matter for migration generation,
        // only the provider type matters.
        optionsBuilder.UseNpgsql("Host=localhost;Database=hirekarlo_design;Username=postgres;Password=postgres");

        return new HireKarloDbContext(optionsBuilder.Options);
    }
}
