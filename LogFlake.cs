using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using NLogFlake.Constants;
using NLogFlake.Factories;
using NLogFlake.Extensions;
using NLogFlake.Models;
using Snappier;

namespace NLogFlake;

internal class LogFlake : ILogFlake, IDisposable
{
    private string? _hostname = Environment.MachineName;

    private readonly ConcurrentQueue<PendingLog> _logsQueue = new();
    private readonly ManualResetEvent _processLogs = new(false);
    private readonly IWebRequestFactory _webRequestFactory;

    private Thread LogsProcessorThread { get; set; }
    private bool IsShuttingDown { get; set; }

    internal int FailedPostRetries { get; set; } = 3;

    internal void SetHostname() => SetHostname(null);

    internal string? GetHostname() => _hostname;

    internal void SetHostname(string? hostname) => _hostname = string.IsNullOrWhiteSpace(hostname) ? null : hostname;

    public LogFlake(IWebRequestFactory webRequestFactory)
    {
        LogsProcessorThread = new Thread(LogsProcessor);
        LogsProcessorThread.Start();

        _webRequestFactory = webRequestFactory ?? throw new ArgumentNullException(nameof(webRequestFactory));
    }

    public void Dispose() => Shutdown();

    ~LogFlake() => Shutdown();

    protected void Shutdown()
    {
        IsShuttingDown = true;
        _processLogs.Set();
        LogsProcessorThread.Join();
    }

    private void LogsProcessor()
    {
        SendLog(LogLevels.DEBUG, $"LogFlake started on {_hostname}");

        _processLogs.WaitOne();

        while (!_logsQueue.IsEmpty)
        {
            _ = _logsQueue.TryDequeue(out PendingLog? log);
            log.Retries++;
            bool success = PostAsync(log.QueueName!, log.JsonString!).GetAwaiter().GetResult();
            if (!success && log.Retries < FailedPostRetries)
            {
                _logsQueue.Enqueue(log);
            }

            _processLogs.Reset();

            if (_logsQueue.IsEmpty && !IsShuttingDown)
            {
                _processLogs.WaitOne();
            }
        }
    }

    private async Task<bool> PostAsync(string queueName, string jsonString)
    {
        if (queueName != QueuesConstants.LOGS && queueName != QueuesConstants.PERFORMANCES)
        {
            return false;
        }

        try
        {
            HttpWebRequest req = _webRequestFactory.Create(queueName);
            using Stream requestStream = await req.GetRequestStreamAsync();

            await requestStream.WriteAsync(Compress(jsonString));

            using WebResponse webResponse = await req.GetResponseAsync();
            return webResponse.IsSuccessStatusCode();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static byte[] Compress(string jsonString)
    {
        byte[] jsonStringBytes = Encoding.UTF8.GetBytes(jsonString);
        string base64String = Convert.ToBase64String(jsonStringBytes);

        return Snappy.CompressToArray(Encoding.UTF8.GetBytes(base64String));
    }

    public void SendLog(string content, Dictionary<string, object>? parameters = null) => SendLog(LogLevels.DEBUG, content, parameters);

    public void SendLog(LogLevels level, string content, Dictionary<string, object>? parameters = null) => SendLog(level, null, content, parameters);

    public void SendLog(LogLevels level, string? correlation, string? content, Dictionary<string, object>? parameters = null)
    {
        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.LOGS,
            JsonString = new LogObject
            {
                Level = level,
                Hostname = GetHostname(),
                Content = content!,
                Correlation = correlation,
                Parameters = parameters,
            }.ToString()
        });

        _processLogs.Set();
    }

    public void SendException(Exception e) => SendException(e, null);

    public void SendException(Exception e, string? correlation)
    {
        StringBuilder additionalTrace = new();
        if (e.Data.Count > 0)
        {
            additionalTrace.Append($"{Environment.NewLine}Data:");
            additionalTrace.Append($"{Environment.NewLine}{JsonSerializer.Serialize(e.Data, new JsonSerializerOptions { WriteIndented = true })}");
        }

        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.LOGS,
            JsonString = new LogObject
            {
                Level = LogLevels.EXCEPTION,
                Hostname = GetHostname(),
                Content = $"{e.Demystify()}{additionalTrace}",
                Correlation = correlation,
            }.ToString()
        });

        _processLogs.Set();
    }

    public void SendPerformance(string label, long duration)
    {
        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.PERFORMANCES,
            JsonString = new LogObject
            {
                Label = label,
                Duration = duration,
            }.ToString()
        });

        _processLogs.Set();
    }

    public IPerformanceCounter MeasurePerformance() => new PerformanceCounter(this);

    public IPerformanceCounter MeasurePerformance(string label) => new PerformanceCounter(this, label);
}
