using Asp.Versioning;
using Veloce.Knowledgebase;

namespace Microsoft.Extensions.DependencyInjection;

public static class KnbApiCollectionExtensions
{
    public static IServiceCollection AddKnbApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IKnbHttpApiAssemblyMarker).Assembly);

        services.AddKnbApiVersioning();
        services.AddKnbSwagger();

        return services;
    }

    private static IServiceCollection AddKnbApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    private static IServiceCollection AddKnbSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
