using Veloce.Knowledgebase.Services.Ride;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Microsoft.Extensions.DependencyInjection;

public static class KnbEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddKnbEfCoreRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRideRepository, RideRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();

        return services;
    }
}
