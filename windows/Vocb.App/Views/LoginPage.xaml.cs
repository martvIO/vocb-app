using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vocb.Firebase;

namespace Vocb.App.Views;

/// <summary>
/// Email/password sign-in shown when no one is signed in. The Firebase project is
/// baked into the app, so the only things a user enters are email + password. On
/// success, <see cref="Services.SessionService"/> raises SignedInChanged and the
/// MainWindow swaps this page out for the app shell.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void SignIn_Click(object sender, RoutedEventArgs e)
        => await RunAuthAsync(() => App.Session.SignInAsync(EmailBox.Text.Trim(), PasswordBox.Password), "Signing in…");

    private async void Create_Click(object sender, RoutedEventArgs e)
        => await RunAuthAsync(() => App.Session.SignUpAsync(EmailBox.Text.Trim(), PasswordBox.Password), "Creating your account…");

    private async void Forgot_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailBox.Text.Trim();
        if (email.Length == 0)
        {
            ShowError("Enter your email first, then tap Forgot password.");
            return;
        }

        SetBusy(true, "Sending reset email…");
        try
        {
            await App.Session.SendPasswordResetAsync(email);
            ShowInfo($"Password-reset email sent to {email}. Check your inbox.");
        }
        catch (System.Exception ex)
        {
            ShowError(FriendlyMessage(ex));
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async System.Threading.Tasks.Task RunAuthAsync(System.Func<System.Threading.Tasks.Task> action, string busyMessage)
    {
        if (EmailBox.Text.Trim().Length == 0 || PasswordBox.Password.Length == 0)
        {
            ShowError("Enter your email and password.");
            return;
        }

        SetBusy(true, busyMessage);
        try
        {
            // On success the session raises SignedInChanged and MainWindow replaces
            // this page, so there's no need to reset the busy state here.
            await action();
        }
        catch (System.Exception ex)
        {
            ShowError(FriendlyMessage(ex));
            SetBusy(false, null);
        }
    }

    private static string FriendlyMessage(System.Exception ex)
        => ex is FirebaseAuthException fa
            ? fa.Message
            : "Something went wrong. Check your connection and try again.";

    private void SetBusy(bool busy, string? message)
    {
        SignInButton.IsEnabled = !busy;
        CreateButton.IsEnabled = !busy;
        ForgotButton.IsEnabled = !busy;
        if (busy && message is not null) ShowInfo(message);
    }

    private void ShowError(string message)
    {
        StatusBar.Severity = InfoBarSeverity.Error;
        StatusBar.Message = message;
        StatusBar.IsOpen = true;
    }

    private void ShowInfo(string message)
    {
        StatusBar.Severity = InfoBarSeverity.Informational;
        StatusBar.Message = message;
        StatusBar.IsOpen = true;
    }
}
