using Microsoft.Extensions.Configuration;
using Veloce.Knowledgebase;

namespace Microsoft.Extensions.DependencyInjection;

public static class KnbServiceNpgSqlEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddKnbNpgSql(this IServiceCollection services)
    {
        services.AddDbContext<VeloceDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            options.UseKnbNpgsql(configuration);
        });

        services.AddScoped<IVeloceDbContext>(sp => sp.GetRequiredService<VeloceDbContext>());

        return services;
    }
}
