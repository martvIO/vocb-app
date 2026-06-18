using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vocb.App.ViewModels;
using Vocb.Core;

namespace Vocb.App.Views;

public sealed partial class StudyPage : Page
{
    private readonly StudyViewModel _vm = new();

    public StudyPage()
    {
        InitializeComponent();
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
            StatusText.Text = $"Couldn't load: {ex.Message}";
            return;
        }
        Render();
    }

    private void Render()
    {
        if (!_vm.HasCard)
        {
            PromptText.Text = "🎉";
            DefinitionText.Text = "";
            ExampleText.Text = "";
            StatusText.Text = _vm.Status.Length > 0 ? _vm.Status : "All caught up!";
            SetGradingVisible(false);
            RevealButton.Visibility = Visibility.Collapsed;
            return;
        }

        var word = _vm.Current!;
        PromptText.Text = word.Text;
        DefinitionText.Text = string.IsNullOrEmpty(word.LearnerDefinition)
            ? (word.Senses.FirstOrDefault()?.Meaning ?? "")
            : word.LearnerDefinition;
        ExampleText.Text = word.Examples.FirstOrDefault() ?? "";
        StatusText.Text = "";

        // Front of the card: prompt only.
        AnswerPanel.Visibility = Visibility.Collapsed;
        RevealButton.Visibility = Visibility.Visible;
        SetGradingVisible(false);
    }

    private void Reveal_Click(object sender, RoutedEventArgs e)
    {
        AnswerPanel.Visibility = Visibility.Visible;
        RevealButton.Visibility = Visibility.Collapsed;
        SetGradingVisible(true);
    }

    private async void Grade_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not string tag) return;
        var grade = (ReviewGrade)int.Parse(tag);
        try
        {
            await _vm.GradeAsync(grade);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Couldn't save: {ex.Message}";
            return;
        }
        Render();
    }

    private void SetGradingVisible(bool visible)
    {
        var v = visible ? Visibility.Visible : Visibility.Collapsed;
        AgainButton.Visibility = v;
        HardButton.Visibility = v;
        GoodButton.Visibility = v;
        EasyButton.Visibility = v;
    }
}
