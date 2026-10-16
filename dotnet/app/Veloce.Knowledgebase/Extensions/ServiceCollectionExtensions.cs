using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Veloce.Knowledgebase.Extensions;

/// <summary>
/// Extension methods for configuring all application services on the <see cref="IServiceCollection"/>.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Knowledgebase domain services (persistence, application logic, API layer).
    /// This is the composition root that wires up all module-level service registrations.
    /// </summary>
    internal static IServiceCollection AddKnowledgebase(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddKnbNpgSql();
        services.AddKnbEfCore();
        services.AddKnbAppServices();
        services.AddKnbApi();

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication using the secret key from configuration.
    /// </summary>
    internal static IServiceCollection AddKnbAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"]
            ?? "SuperSecretDefaultKeyForDevelopmentOnly123456!";
        var key = Encoding.UTF8.GetBytes(jwtSecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configures Swagger / OpenAPI documentation with Bearer token support.
    /// </summary>
    internal static IServiceCollection AddKnbSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Veloce Knowledgebase API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization Header. Beispiel: 'Bearer {token}'",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
            });
        });

        return services;
    }

    /// <summary>
    /// Configures rate limiting policies including the AuthRateLimitPolicy used by AuthController.
    /// </summary>
    internal static IServiceCollection AddKnbRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("AuthRateLimitPolicy", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 10;
                opt.QueueLimit = 0;
            });
        });

        return services;
    }
}
