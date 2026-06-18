using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vocb.Firebase;

namespace Vocb.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void SignIn_Click(object sender, RoutedEventArgs e)
    {
        var apiKey = ApiKeyBox.Text.Trim();
        var projectId = ProjectIdBox.Text.Trim();
        var region = string.IsNullOrWhiteSpace(RegionBox.Text) ? "us-central1" : RegionBox.Text.Trim();
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (apiKey.Length == 0 || projectId.Length == 0 || email.Length == 0 || password.Length == 0)
        {
            StatusText.Text = "Fill in all fields.";
            return;
        }

        SignInButton.IsEnabled = false;
        StatusText.Text = "Signing in…";
        try
        {
            App.Session.Configure(new FirebaseConfig(apiKey, projectId, region));
            await App.Session.SignInAsync(email, password);
            StatusText.Text = $"Signed in as {App.Session.Uid}. You can now look up and study words.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Sign-in failed: {ex.Message}";
        }
        finally
        {
            SignInButton.IsEnabled = true;
        }
    }
}
