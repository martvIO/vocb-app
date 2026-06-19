using Vocb.Core;
using Xunit;

namespace Vocb.Core.Tests;

public class ReminderScheduleTests
{
    // Use a fixed offset so the tests are deterministic regardless of the host TZ.
    private static readonly System.TimeSpan Offset = System.TimeSpan.FromHours(2);

    private static System.DateTimeOffset At(int hour, int minute)
        => new(2026, 6, 19, hour, minute, 0, Offset);

    [Fact]
    public void NextOccurrence_LaterToday_ReturnsToday()
    {
        var now = At(8, 0);
        var next = ReminderSchedule.NextOccurrence(now, 19, 0);
        Assert.Equal(At(19, 0), next);
    }

    [Fact]
    public void NextOccurrence_AlreadyPassed_ReturnsTomorrow()
    {
        var now = At(20, 0);
        var next = ReminderSchedule.NextOccurrence(now, 19, 0);
        Assert.Equal(At(19, 0).AddDays(1), next);
    }

    [Fact]
    public void NextOccurrence_ExactlyNow_ReturnsTomorrow()
    {
        var now = At(19, 0);
        var next = ReminderSchedule.NextOccurrence(now, 19, 0);
        Assert.Equal(At(19, 0).AddDays(1), next);
    }

    [Fact]
    public void TimeUntilNext_IsNeverNegative()
    {
        var now = At(23, 30);
        var delay = ReminderSchedule.TimeUntilNext(now, 1, 0);
        Assert.True(delay > System.TimeSpan.Zero);
        Assert.Equal(System.TimeSpan.FromMinutes(90), delay);
    }
}
