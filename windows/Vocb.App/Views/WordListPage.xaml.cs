using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vocb.App.Services;
using Vocb.App.ViewModels;
using Vocb.Core;

namespace Vocb.App.Views;

public sealed partial class WordListPage : Page
{
    private readonly WordListViewModel _vm = new();

    public WordListPage()
    {
        InitializeComponent();
        WordsList.ItemsSource = _vm.Words;
        ShowFirstRunHintIfNeeded();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            await _vm.LoadAsync();
        }
        catch (System.Exception ex)
        {
            _vm.Words.Clear();
            StatusText.Text = $"Couldn't load words: {ex.Message}";
            return;
        }
        StatusText.Text = _vm.Status ?? "";
    }

    private void Speak_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WordEntry word })
            Speech.Pronounce(word.Text, word.AudioUrl);
    }

    private void ShowFirstRunHintIfNeeded()
    {
        if (!App.SettingsStore.Load().HasSeenFirstRunHint)
            FirstRunHint.IsOpen = true;
    }

    private void FirstRunHint_Closed(InfoBar sender, object args)
    {
        var settings = App.SettingsStore.Load();
        settings.HasSeenFirstRunHint = true;
        App.SettingsStore.Save(settings);
    }
}
