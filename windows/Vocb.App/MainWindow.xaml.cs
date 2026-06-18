using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vocb.App.Views;

namespace Vocb.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ContentFrame.Navigate(typeof(WordListPage));
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        switch (item.Tag as string)
        {
            case "words": ContentFrame.Navigate(typeof(WordListPage)); break;
            case "study": ContentFrame.Navigate(typeof(StudyPage)); break;
            case "settings": ContentFrame.Navigate(typeof(SettingsPage)); break;
        }
    }
}
