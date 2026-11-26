using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Veloce.Knowledgebase;

namespace Microsoft.Extensions.DependencyInjection;

public static class KnbNpgSqlDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseKnbNpgsql(
        this DbContextOptionsBuilder optionsBuilder,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(KnbPostgreSqlConsts.ConnectionStringName)
            ?? configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A PostgreSQL connection string must be configured.");
        }

        return optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptionsAction: sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL");
            });
    }
}
