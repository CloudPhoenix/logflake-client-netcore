using System.Runtime.Serialization;

namespace NLogFlake;

[Serializable]
public class LogFlakeException : ApplicationException
{
    public LogFlakeException() { }

    public LogFlakeException(string message) : base(message) { }

    public LogFlakeException(string message, Exception innerException) : base(message, innerException) { }
}
