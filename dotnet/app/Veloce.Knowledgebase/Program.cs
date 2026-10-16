using Veloce.Knowledgebase;
using Veloce.Knowledgebase.Extensions;

/// <summary>
/// Application entry point for the Veloce Knowledgebase API.
/// </summary>
public static partial class Program
{
    /// <summary>
    /// Starts the application host.
    /// </summary>
    private static async Task Main(string[] args)
    {
        var app = BuildApplication(args);
        await app.RunAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Builds and fully configures the <see cref="WebApplication"/> without starting it.
    /// Useful for integration tests that need a configured host.
    /// </summary>
    public static WebApplication BuildApplication(string[] args)
    {
        var builder = CreateWebApplicationBuilder(args);
        var app = builder.Build();

        KnowledgebaseStartup.ApplyMigrations(app);
        KnowledgebaseStartup.ConfigurePipeline(app);

        return app;
    }

    /// <summary>
    /// Creates and configures the <see cref="WebApplicationBuilder"/> with all required services.
    /// </summary>
    private static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddKnowledgebase();
        builder.Services.AddKnbAuthentication(builder.Configuration);
        builder.Services.AddKnbRateLimiting();
        builder.Services.AddKnbSwagger();

        return builder;
    }
}
