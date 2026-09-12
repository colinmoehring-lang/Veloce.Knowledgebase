using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL;

public class VeloceDbContextFactory : IDesignTimeDbContextFactory<VeloceDbContext>
{
    public VeloceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VeloceDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=veloce_kb;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL"));

        return new VeloceDbContext(optionsBuilder.Options);
    }
}
