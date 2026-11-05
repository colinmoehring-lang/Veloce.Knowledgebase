using Microsoft.EntityFrameworkCore;

namespace Veloce.Knowledgebase;

/// <summary>
/// Handles application-level startup configuration: database migrations and the HTTP middleware pipeline.
/// </summary>
public static class KnowledgebaseStartup
{
    /// <summary>
    /// Runs database migrations on application startup.
    /// </summary>
    public static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VeloceDbContext>();
        dbContext.Database.Migrate();
    }

    /// <summary>
    /// Configures the HTTP request pipeline (middleware, routing, auth, endpoints).
    /// </summary>
    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Veloce API v1");
        });

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
