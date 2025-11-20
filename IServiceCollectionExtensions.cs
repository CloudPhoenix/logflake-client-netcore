using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLogFlake.Constants;
using NLogFlake.Extensions;
using NLogFlake.Models.Options;
using NLogFlake.Services;

namespace NLogFlake;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds LogFlake logging services and configuration to the specified service collection.
    /// </summary>
    /// <remarks>This method registers all required LogFlake services, including configuration, HTTP client,
    /// and core logging interfaces. It should be called during application startup to enable LogFlake logging
    /// features.
    /// Note: this method may be removed in following version, please move to <see cref="IHostApplicationBuilderExtensions.AddLogFlake(Microsoft.Extensions.Hosting.IHostApplicationBuilder)"/></remarks>
    /// <param name="services">The service collection to which the LogFlake services will be added. Cannot be null.</param>
    /// <param name="configuration">The application configuration containing the LogFlake settings. Cannot be null.</param>
    /// <returns>The same service collection instance, for chaining further service registrations.</returns>
    public static IServiceCollection AddLogFlake(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<LogFlakeOptions>(configuration.GetSection(LogFlakeOptions.SectionName))
           .AddOptionsWithValidateOnStart<LogFlakeOptions, LogFlakeOptionsValidator>();

        services.TryAddSingleton<IVersionService, VersionService>();

        services.AddHttpClient(HttpClientConstants.ClientName, (_, client) => client.ConfigureClient());

        services.AddSingleton<ILogFlake, LogFlake>();
        services.AddSingleton<ILogFlakeService, LogFlakeService>();

        return services;
    }
}
