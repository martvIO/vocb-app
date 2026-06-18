using Vocb.Core;
using Xunit;

namespace Vocb.Core.Tests;

public class StudyQueueTests
{
    private const long Now = 1_700_000_000_000L;

    private static WordEntry Word(string lemma, int lookupCount) => new()
    {
        Lemma = lemma,
        Text = lemma,
        Kind = WordKind.Word,
        LookupCount = lookupCount,
        LearnerDefinition = $"def of {lemma}",
        GeneratedBy = "test",
        SchemaVersion = 1
    };

    [Fact]
    public void DueCardsComeBeforeFresh_AndExcludeNotDue()
    {
        var words = new[]
        {
            Word("alpha", 5),    // fresh
            Word("bravo", 10),   // fresh, higher count
            Word("charlie", 1),  // due yesterday
            Word("delta", 99),   // not due (tomorrow)
        };
        var states = new Dictionary<string, SrsState>
        {
            ["charlie"] = new SrsState { DueDate = Now - SrsEngine.MillisPerDay },
            ["delta"] = new SrsState { DueDate = Now + SrsEngine.MillisPerDay },
        };

        var order = StudyQueue.Build(words, states, Now).Select(c => c.Entry.Lemma).ToArray();
        Assert.Equal(new[] { "charlie", "bravo", "alpha" }, order);
    }

    [Fact]
    public void DueCards_SortedByDueDate()
    {
        var words = new[] { Word("a", 1), Word("b", 1) };
        var states = new Dictionary<string, SrsState>
        {
            ["a"] = new SrsState { DueDate = Now - 3_600_000 },
            ["b"] = new SrsState { DueDate = Now - 7_200_000 },
        };
        var order = StudyQueue.Build(words, states, Now).Select(c => c.Entry.Lemma).ToArray();
        Assert.Equal(new[] { "b", "a" }, order);
    }

    [Fact]
    public void Limit_TruncatesQueue()
    {
        var words = Enumerable.Range(0, 10).Select(i => Word($"w{i}", i)).ToArray();
        var queue = StudyQueue.Build(words, new Dictionary<string, SrsState>(), Now, limit: 3);
        Assert.Equal(3, queue.Count);
        Assert.Equal(new[] { "w9", "w8", "w7" }, queue.Select(c => c.Entry.Lemma).ToArray());
    }
}
