using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vocb.App.ViewModels;

namespace Vocb.App.Views;

public sealed partial class WordListPage : Page
{
    private readonly WordListViewModel _vm = new();

    public WordListPage()
    {
        InitializeComponent();
        WordsList.ItemsSource = _vm.Words;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            await _vm.LoadAsync();
        }
        catch (Exception ex)
        {
            _vm.Words.Clear();
            StatusText.Text = $"Couldn't load words: {ex.Message}";
            return;
        }
        StatusText.Text = _vm.Status ?? "";
    }
}
