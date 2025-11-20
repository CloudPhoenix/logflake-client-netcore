using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NLogFlake.Constants;
using NLogFlake.Extensions;
using NLogFlake.Models.Options;
using NLogFlake.Services;

namespace NLogFlake;

public static class IHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds LogFlake logging services and configuration to the specified application builder.
    /// </summary>
    /// <remarks>This method registers LogFlake and its dependencies with the application's dependency
    /// injection container. It configures LogFlake options from the application's configuration and adds required
    /// services for logging and versioning. Call this method during application startup to enable LogFlake logging
    /// features.</remarks>
    /// <param name="builder">The application builder to which LogFlake services and configuration will be added. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IHostApplicationBuilder"/> for chaining further configuration.</returns>
    public static IHostApplicationBuilder AddLogFlake(this IHostApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;

        _ = services.Configure<LogFlakeOptions>(builder.Configuration.GetSection(LogFlakeOptions.SectionName))
           .AddOptionsWithValidateOnStart<LogFlakeOptions, LogFlakeOptionsValidator>();

        services.TryAddSingleton<IVersionService, VersionService>();

        services.AddHttpClient(HttpClientConstants.ClientName, (_, client) => client.ConfigureClient());

        services.AddSingleton<ILogFlake, LogFlake>();
        services.AddSingleton<ILogFlakeService, LogFlakeService>();

        return builder;
    }
}
