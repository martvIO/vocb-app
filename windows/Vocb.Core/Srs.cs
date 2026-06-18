using System.Text.Json.Serialization;

namespace Vocb.Core;

/// <summary>Per-card SM-2 state, stored at users/{uid}/srs/{lemma}. Mirrors VocabKit's SRSState.</summary>
public sealed class SrsState
{
    [JsonPropertyName("repetitions")] public int Repetitions { get; set; }
    [JsonPropertyName("easeFactor")] public double EaseFactor { get; set; } = 2.5;
    [JsonPropertyName("intervalDays")] public int IntervalDays { get; set; }
    [JsonPropertyName("dueDate")] public long DueDate { get; set; }
    [JsonPropertyName("lastReviewed")] public long? LastReviewed { get; set; }

    /// <summary>A brand-new card, due immediately at <paramref name="nowMs"/>.</summary>
    public static SrsState New(long nowMs) => new() { DueDate = nowMs };
}

/// <summary>Recall quality mapped to an SM-2 grade (0–5). Below 3 is a lapse.</summary>
public enum ReviewGrade
{
    Again = 1,
    Hard = 3,
    Good = 4,
    Easy = 5
}

/// <summary>Pure SM-2 scheduler — identical algorithm to the Apple VocabKit SRSEngine.</summary>
public static class SrsEngine
{
    public const double MinimumEase = 1.3;
    public const long MillisPerDay = 86_400_000L;

    public static long Millis(DateTimeOffset when) => when.ToUnixTimeMilliseconds();

    public static SrsState Schedule(SrsState state, ReviewGrade grade, long nowMs)
    {
        double q = (int)grade;
        int repetitions = state.Repetitions;
        int interval = state.IntervalDays;

        if ((int)grade < 3)
        {
            // Lapse: reset the streak, review again tomorrow.
            repetitions = 0;
            interval = 1;
        }
        else
        {
            interval = repetitions switch
            {
                0 => 1,
                1 => 6,
                _ => (int)Math.Round(interval * state.EaseFactor, MidpointRounding.AwayFromZero)
            };
            repetitions += 1;
        }
        interval = Math.Max(1, interval);

        // SM-2 ease-factor update (every review), clamped at 1.3.
        double ease = state.EaseFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        ease = Math.Max(MinimumEase, ease);

        return new SrsState
        {
            Repetitions = repetitions,
            EaseFactor = ease,
            IntervalDays = interval,
            DueDate = nowMs + interval * MillisPerDay,
            LastReviewed = nowMs
        };
    }

    public static SrsState Schedule(SrsState state, ReviewGrade grade, DateTimeOffset now)
        => Schedule(state, grade, Millis(now));

    public static bool IsDue(SrsState state, long nowMs) => state.DueDate <= nowMs;
}
