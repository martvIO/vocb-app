using Vocb.Core;
using Xunit;

namespace Vocb.Core.Tests;

public class SrsEngineTests
{
    // Fixed reference instant (matches the Swift tests' 1_700_000_000 seconds).
    private const long Now = 1_700_000_000_000L;

    private static double DaysUntilDue(SrsState s, long fromMs)
        => (s.DueDate - fromMs) / (double)SrsEngine.MillisPerDay;

    [Fact]
    public void FirstGoodReview_SchedulesOneDay()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Good, Now);
        Assert.Equal(1, s.Repetitions);
        Assert.Equal(1, s.IntervalDays);
        Assert.Equal(2.5, s.EaseFactor, 4); // "good" leaves EF unchanged
        Assert.Equal(1, DaysUntilDue(s, Now), 3);
    }

    [Fact]
    public void SecondGoodReview_SchedulesSixDays()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Good, Now);
        s = SrsEngine.Schedule(s, ReviewGrade.Good, Now);
        Assert.Equal(2, s.Repetitions);
        Assert.Equal(6, s.IntervalDays);
    }

    [Fact]
    public void ThirdGoodReview_UsesEaseFactor()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Good, Now);
        s = SrsEngine.Schedule(s, ReviewGrade.Good, Now);
        s = SrsEngine.Schedule(s, ReviewGrade.Good, Now);
        Assert.Equal(3, s.Repetitions);
        Assert.Equal(15, s.IntervalDays); // round(6 * 2.5)
    }

    [Fact]
    public void Easy_RaisesEaseFactor()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Easy, Now);
        Assert.Equal(2.6, s.EaseFactor, 4);
    }

    [Fact]
    public void Hard_LowersEaseFactor()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Hard, Now);
        Assert.Equal(2.36, s.EaseFactor, 4);
    }

    [Fact]
    public void Again_ResetsRepetitionsAndInterval()
    {
        var s = SrsEngine.Schedule(SrsState.New(Now), ReviewGrade.Good, Now);
        s = SrsEngine.Schedule(s, ReviewGrade.Good, Now); // interval 6, reps 2
        s = SrsEngine.Schedule(s, ReviewGrade.Again, Now);
        Assert.Equal(0, s.Repetitions);
        Assert.Equal(1, s.IntervalDays);
    }

    [Fact]
    public void EaseFactor_ClampedAtMinimum()
    {
        var s = SrsState.New(Now);
        for (int i = 0; i < 10; i++)
            s = SrsEngine.Schedule(s, ReviewGrade.Again, Now);
        Assert.Equal(SrsEngine.MinimumEase, s.EaseFactor, 4);
    }

    [Fact]
    public void IsDue_ComparesDueDateToNow()
    {
        var past = new SrsState { DueDate = Now - 3_600_000 };
        var future = new SrsState { DueDate = Now + 3_600_000 };
        Assert.True(SrsEngine.IsDue(past, Now));
        Assert.False(SrsEngine.IsDue(future, Now));
    }
}
