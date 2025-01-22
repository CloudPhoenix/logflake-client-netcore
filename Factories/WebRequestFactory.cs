using System.Net;
using System.Net.Mime;
using Microsoft.Extensions.Options;
using NLogFlake.Models.Options;

namespace NLogFlake.Factories;

internal class WebRequestFactory : IWebRequestFactory
{
    private const string UriTemplate = "api/ingestion/{0}";
    private const int PostTimeoutMilliseconds = 3000;
    private const string UserAgent = "logflake-client-netcore/1.8.3";

    private readonly Uri _requestUri;

    public WebRequestFactory(IOptions<LogFlakeOptions> options)
    {
        _requestUri = new(
            baseUri: new(options.Value.Endpoint),
            relativeUri: string.Format(UriTemplate, options.Value.AppId));
    }

    public HttpWebRequest Create(string queueName)
    {
        HttpWebRequest httpWebRequest = WebRequest.CreateHttp($"{_requestUri}/{queueName}");

        httpWebRequest.Accept = MediaTypeNames.Application.Json;
        httpWebRequest.ContentType = MediaTypeNames.Application.Octet;
        httpWebRequest.Method = WebRequestMethods.Http.Post;
        httpWebRequest.Timeout = PostTimeoutMilliseconds;
        httpWebRequest.UserAgent = UserAgent;

        return httpWebRequest;
    }
}
