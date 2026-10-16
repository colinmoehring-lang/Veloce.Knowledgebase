using Veloce.Knowledgebase.Services.Auth;
using Veloce.Knowledgebase.Services.Email;
using Veloce.Knowledgebase.Services.Ride;
using Veloce.Knowledgebase.Services.User;
using Veloce.Knowledgebase.Services.Vehicle;

namespace Microsoft.Extensions.DependencyInjection;

public static class KnbApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddKnbAppServices(this IServiceCollection services)
    {
        services.AddKnbServices();
        services.AddKnbHelpers();
        return services;
    }

    public static IServiceCollection AddKnbServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<IRideAppService, RideAppService>();
        services.AddScoped<IRidePointAppService, RidePointAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<IVehicleAppService, VehicleAppService>();

        return services;
    }

    public static IServiceCollection AddKnbHelpers(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();
        services.AddScoped<IVerificationCodeGenerator, VerificationCodeGenerator>();

        return services;
    }
}