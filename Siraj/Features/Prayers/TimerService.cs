using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Siraj.Features.Prayers;

internal sealed class TimerService(TimeProvider timeProvider)
{
    public event Action? Tick;

    private PeriodicTimer? timer;

    public void Start()
    {
        if (timer is not null)
        {
            return;
        }

        timer = new PeriodicTimer(GetRemaining(), timeProvider);
        _ = RunAsync(timer);
    }

    private async Task RunAsync(PeriodicTimer periodicTimer)
    {
        while (await periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                Tick?.Invoke();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Timer tick failed");
            }

            // Re-align with the clock as the timer may drift, the system may sleep, or the clock may change.
            periodicTimer.Period = GetRemaining();
        }
    }

    private TimeSpan GetRemaining()
    {
        var now = timeProvider.GetLocalNow();
        var remaining = now.TruncateToMinute().AddMinutes(1) - now;

        // Periodic timer requires at least a millisecond.
        return remaining < TimeSpan.FromMilliseconds(1) ? TimeSpan.FromMilliseconds(1) : remaining;
    }
}
