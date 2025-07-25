using System.Diagnostics;

namespace NLogFlake;

internal class PerformanceCounter : IPerformanceCounter
{
    private readonly ILogFlake _logFlake;
    private readonly Stopwatch _internalStopwatch;

    private string _label;
    private bool _alreadySent;

    internal PerformanceCounter(ILogFlake logFlake, string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentNullException(nameof(label));
        }

        _logFlake = logFlake ?? throw new ArgumentNullException(nameof(logFlake));
        _label = label;
        _internalStopwatch = Stopwatch.StartNew();
    }

    ~PerformanceCounter()
    {
        if (!_alreadySent) Stop();
    }

    public void Start() => _internalStopwatch.Start();

    public void Restart() => _internalStopwatch.Restart();

    public long Stop() => Stop(true);

    public long Pause() => Stop(false);

    public void SetLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentNullException(nameof(label));
        }

        _label = label;
    }

    private long Stop(bool shouldSend)
    {
        _internalStopwatch.Stop();

        if (!shouldSend)
        {
            return _internalStopwatch.ElapsedMilliseconds;
        }

        _alreadySent = true;
        _logFlake.SendPerformance(_label, _internalStopwatch.ElapsedMilliseconds);

        return _internalStopwatch.ElapsedMilliseconds;
    }
}
