using NLogFlake.Constants;

namespace NLogFlake.Extensions;

internal static class HttpClientExtensions
{
    internal static void ConfigureClient(this HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(HttpClientConstants.PostTimeoutSeconds);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "logflake-client-netcore/1.8.3");
    }
}
