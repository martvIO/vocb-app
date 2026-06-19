using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Vocb.Core;

namespace Vocb.App.Services;

/// <summary>
/// Fires a daily "time to review" Windows toast at a chosen local time, mirroring the
/// Apple client's ReviewReminders. Because the app is an always-on tray process, an
/// in-process timer is reliable without packaged identity — the trade-off is that the
/// reminder only fires while the app is running.
/// </summary>
public sealed class ReviewReminder
{
    private readonly DispatcherQueue _dispatcher;
    private DispatcherQueueTimer? _timer;
    private int _hour = 19;
    private int _minute;

    public ReviewReminder(DispatcherQueue dispatcher) => _dispatcher = dispatcher;

    /// <summary>Schedule (or reschedule) the daily reminder for the given local time.</summary>
    public void Schedule(int hour, int minute)
    {
        _hour = Math.Clamp(hour, 0, 23);
        _minute = Math.Clamp(minute, 0, 59);
        Stop();
        _timer = _dispatcher.CreateTimer();
        _timer.IsRepeating = false;
        _timer.Tick += OnTick;
        ArmNext();
    }

    public void Stop()
    {
        if (_timer is null) return;
        _timer.Tick -= OnTick;
        _timer.Stop();
        _timer = null;
    }

    private void ArmNext()
    {
        if (_timer is null) return;
        // Compute fresh each time so we stay correct across DST and day boundaries.
        _timer.Interval = ReminderSchedule.TimeUntilNext(DateTimeOffset.Now, _hour, _minute);
        _timer.Start();
    }

    private void OnTick(DispatcherQueueTimer sender, object args)
    {
        ShowToast();
        ArmNext(); // re-arm for tomorrow
    }

    private static void ShowToast()
    {
        try
        {
            var toast = new AppNotificationBuilder()
                .AddText("Time to review")
                .AddText("Your vocabulary is waiting — a few cards keeps it fresh.")
                .BuildNotification();
            AppNotificationManager.Default.Show(toast);
        }
        catch { /* notifications are best-effort */ }
    }
}
