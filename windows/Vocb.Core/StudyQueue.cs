namespace Vocb.Core;

/// <summary>A card to review: the word plus its current scheduling state.</summary>
public sealed record StudyCard(WordEntry Entry, SrsState State);

/// <summary>
/// Builds an ordered study queue. Mirrors VocabKit's StudyQueue: due cards first
/// (oldest due first), then never-reviewed words by lookupCount descending.
/// </summary>
public static class StudyQueue
{
    public static IReadOnlyList<StudyCard> Build(
        IEnumerable<WordEntry> words,
        IReadOnlyDictionary<string, SrsState> states,
        long nowMs,
        int? limit = null)
    {
        var due = new List<StudyCard>();
        var fresh = new List<WordEntry>();

        foreach (var word in words)
        {
            if (states.TryGetValue(word.Lemma, out var state))
            {
                if (SrsEngine.IsDue(state, nowMs))
                    due.Add(new StudyCard(word, state));
            }
            else
            {
                fresh.Add(word);
            }
        }

        due.Sort((a, b) => a.State.DueDate.CompareTo(b.State.DueDate));
        fresh.Sort((a, b) => b.LookupCount.CompareTo(a.LookupCount));

        var queue = new List<StudyCard>(due);
        foreach (var w in fresh)
            queue.Add(new StudyCard(w, SrsState.New(nowMs)));

        if (limit is int max && queue.Count > max)
            queue = queue.GetRange(0, max);

        return queue;
    }
}
