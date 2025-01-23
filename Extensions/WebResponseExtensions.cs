using System.Net;

namespace NLogFlake.Extensions;

public static class WebResponseExtensions
{
    public static bool IsSuccessStatusCode(this WebResponse webResponse)
    {
        if (webResponse is HttpWebResponse httpWebResponse)
        {
            int statusCode = (int)httpWebResponse.StatusCode;

            return statusCode >= 200 && statusCode <= 299;
        }

        return false;
    }
}
