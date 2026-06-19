namespace Vocb.Core;

/// <summary>
/// Pure helpers for computing when a daily reminder should next fire. Kept free of
/// any UI/timer dependency so it can be unit-tested and reused across platforms.
/// </summary>
public static class ReminderSchedule
{
    /// <summary>
    /// The next local date-time at <paramref name="hour"/>:<paramref name="minute"/>
    /// strictly after <paramref name="now"/>. If today's time has already passed (or
    /// is exactly now), returns tomorrow's occurrence.
    /// </summary>
    public static DateTimeOffset NextOccurrence(DateTimeOffset now, int hour, int minute)
    {
        var todayTarget = new DateTimeOffset(
            now.Year, now.Month, now.Day, hour, minute, 0, now.Offset);
        return todayTarget > now ? todayTarget : todayTarget.AddDays(1);
    }

    /// <summary>Time from <paramref name="now"/> until the next occurrence (never negative).</summary>
    public static TimeSpan TimeUntilNext(DateTimeOffset now, int hour, int minute)
        => NextOccurrence(now, hour, minute) - now;
}
