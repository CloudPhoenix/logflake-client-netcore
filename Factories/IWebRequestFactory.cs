using System.Net;

namespace NLogFlake.Factories;

public interface IWebRequestFactory
{
    HttpWebRequest Create(string queueName);
}
