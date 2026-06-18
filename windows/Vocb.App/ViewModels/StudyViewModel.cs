using Vocb.Core;

namespace Vocb.App.ViewModels;

/// <summary>
/// Drives a study session: builds the queue from saved words + SRS state, then
/// grades each card with SM-2 and persists the new state.
/// </summary>
public sealed class StudyViewModel
{
    private List<StudyCard> _queue = new();
    private int _index;

    public WordEntry? Current => _index < _queue.Count ? _queue[_index].Entry : null;
    public bool HasCard => Current is not null;
    public string Status { get; private set; } = "";

    public async Task LoadAsync(int limit = 30)
    {
        var session = App.Session;
        if (!session.IsSignedIn || session.Firestore is null)
        {
            Status = "Sign in from Settings to study.";
            return;
        }

        var token = await session.GetIdTokenAsync();
        var words = await session.Firestore.ListWordsAsync(session.Uid!, token);
        var states = await session.Firestore.ListSrsAsync(session.Uid!, token);

        var nowMs = SrsEngine.Millis(DateTimeOffset.UtcNow);
        _queue = StudyQueue.Build(words, states, nowMs, limit).ToList();
        _index = 0;
        Status = _queue.Count == 0 ? "Nothing due — add or look up more words." : "";
    }

    public async Task GradeAsync(ReviewGrade grade)
    {
        if (_index >= _queue.Count) return;

        var card = _queue[_index];
        var nowMs = SrsEngine.Millis(DateTimeOffset.UtcNow);
        var next = SrsEngine.Schedule(card.State, grade, nowMs);

        var session = App.Session;
        if (session.IsSignedIn && session.Firestore is not null)
        {
            var token = await session.GetIdTokenAsync();
            await session.Firestore.SaveSrsAsync(session.Uid!, card.Entry.Lemma, next, token);
        }

        _index++;
    }
}
