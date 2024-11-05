using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLogFlake.Constants;
using NLogFlake.Models.Options;
using NLogFlake.Services;

namespace NLogFlake;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLogFlake(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<LogFlakeOptions>(configuration.GetSection(LogFlakeOptions.SectionName))
           .AddOptionsWithValidateOnStart<LogFlakeOptions, LogFlakeOptionsValidator>();

        services.TryAddSingleton<IVersionService, VersionService>();

        services.AddHttpClient(HttpClientConstants.ClientName, ConfigureClient);

        services.AddSingleton<ILogFlake, LogFlake>();
        services.AddSingleton<ILogFlakeService, LogFlakeService>();

        return services;
    }

    private static void ConfigureClient(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(HttpClientConstants.PostTimeoutSeconds);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "logflake-client-netcore/1.8.3");
    }
}
