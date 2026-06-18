using System.Collections.ObjectModel;
using Vocb.Core;

namespace Vocb.App.ViewModels;

public sealed class WordListViewModel
{
    /// <summary>Saved words, most-looked-up first (the project's headline sort).</summary>
    public ObservableCollection<WordEntry> Words { get; } = new();

    public string? Status { get; private set; }

    public async Task LoadAsync()
    {
        var session = App.Session;
        if (!session.IsSignedIn || session.Firestore is null)
        {
            Status = "Sign in from Settings to load your words.";
            return;
        }

        var token = await session.GetIdTokenAsync();
        var words = await session.Firestore.ListWordsAsync(session.Uid!, token);

        Words.Clear();
        foreach (var w in words.OrderByDescending(w => w.LookupCount).ThenBy(w => w.Text))
            Words.Add(w);

        Status = Words.Count == 0 ? "No words yet — select text anywhere and press the hotkey." : null;
    }
}
