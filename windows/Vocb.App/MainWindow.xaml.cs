using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vocb.App.Views;

namespace Vocb.App;

/// <summary>
/// Gates the app on auth: while we try to restore a saved session it shows a spinner,
/// then shows either the login screen (signed out) or the app shell (signed in). It
/// reacts to <see cref="Services.SessionService.SignedInChanged"/> so signing in/out
/// swaps the view automatically.
/// </summary>
public sealed partial class MainWindow : Window
{
    // True while we programmatically set the nav selection, so the resulting
    // SelectionChanged doesn't trigger a second navigation.
    private bool _suppressNav;

    public MainWindow()
    {
        InitializeComponent();
        App.Session.SignedInChanged += OnSignedInChanged;
        _ = InitializeAsync();
    }

    private async System.Threading.Tasks.Task InitializeAsync()
    {
        await App.Session.TryRestoreAsync();
        ApplyAuthState();
    }

    private void OnSignedInChanged(object? sender, System.EventArgs e)
    {
        // The event can arrive off the UI thread (e.g. a background token refresh),
        // so marshal the view swap onto the dispatcher.
        DispatcherQueue.TryEnqueue(ApplyAuthState);
    }

    private void ApplyAuthState()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;

        if (App.Session.IsSignedIn)
        {
            LoginFrame.Visibility = Visibility.Collapsed;
            LoginFrame.Content = null;

            Nav.Visibility = Visibility.Visible;
            _suppressNav = true;
            Nav.SelectedItem = WordsItem;
            _suppressNav = false;
            ContentFrame.Navigate(typeof(WordListPage));
        }
        else
        {
            Nav.Visibility = Visibility.Collapsed;
            ContentFrame.Content = null;

            LoginFrame.Visibility = Visibility.Visible;
            LoginFrame.Navigate(typeof(LoginPage));
        }
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressNav || !App.Session.IsSignedIn) return;
        if (args.SelectedItem is not NavigationViewItem item) return;
        switch (item.Tag as string)
        {
            case "words": ContentFrame.Navigate(typeof(WordListPage)); break;
            case "study": ContentFrame.Navigate(typeof(StudyPage)); break;
            case "settings": ContentFrame.Navigate(typeof(SettingsPage)); break;
        }
    }
}
