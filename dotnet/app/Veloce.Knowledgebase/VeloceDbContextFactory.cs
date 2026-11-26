using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veloce.Knowledgebase;

public class VeloceDbContextFactory : IDesignTimeDbContextFactory<VeloceDbContext>
{
    public VeloceDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var connectionString = configuration.GetConnectionString("veloce-knowledgebasedb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A PostgreSQL connection string must be configured for EF tools.");
        var options = new DbContextOptionsBuilder<VeloceDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly("Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL"))
            .Options;
        return new VeloceDbContext(options);
    }
}
