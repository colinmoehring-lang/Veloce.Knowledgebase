using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Net;
using Veloce.Knowledgebase.Services.User;

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
        var jwtSecretKey = configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(jwtSecretKey) || Encoding.UTF8.GetByteCount(jwtSecretKey) < 32)
        {
            throw new InvalidOperationException("Jwt:SecretKey must be configured with at least 32 UTF-8 bytes.");
        }
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
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "Veloce.Knowledgebase",
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"] ?? "Veloce.Knowledgebase.Clients",
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userId = context.Principal?.GetUserId();
                    var stamp = context.Principal?.FindFirst("security_stamp")?.Value;
                    if (userId is null || !Guid.TryParse(stamp, out var securityStamp))
                    {
                        context.Fail("Token is missing its user identity or security stamp.");
                        return;
                    }

                    var repository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                    var user = await repository.FindByIdAsync(userId.Value, context.HttpContext.RequestAborted);
                    if (user is null || user.Status != UserAccountStatus.Active || user.SecurityStamp != securityStamp)
                    {
                        context.Fail("Token is no longer valid.");
                    }
                }
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
    /// Configures distributed authentication limits and explicitly trusted proxies.
    /// </summary>
    internal static IServiceCollection AddKnbRateLimiting(this IServiceCollection services)
    {
        services.AddScoped<AuthRateLimitFilter>();
        services.AddScoped<AuthRateLimitStore>();
        services.Configure<MvcOptions>(options => options.Filters.AddService<AuthRateLimitFilter>());
        services.AddOptions<ForwardedHeadersOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var value in (configuration["Proxy:KnownProxies"] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!IPAddress.TryParse(value, out var address))
                {
                    throw new InvalidOperationException($"Invalid trusted proxy address: {value}");
                }
                options.KnownProxies.Add(address);
            }
        });

        return services;
    }
}
