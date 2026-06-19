using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Vocb.App.Capture;
using Vocb.App.Services;

namespace Vocb.App;

public partial class App : Application
{
    /// <summary>Process-wide session (auth + Firebase clients), pre-configured with the baked-in project.</summary>
    public static SessionService Session { get; } = new();

    /// <summary>Local, non-secret preferences (reminder time, first-run flag).</summary>
    public static LocalSettingsStore SettingsStore { get; } = new();

    /// <summary>Daily review reminder (created once the UI thread exists).</summary>
    public static ReviewReminder? Reminder { get; private set; }

    private Window? _window;
    private BackgroundController? _background;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Bake in the Firebase project so users only ever enter email + password.
        Session.Configure(FirebaseDefaults.Config);

        // Enable Windows toast notifications for the (unpackaged) app.
        try { AppNotificationManager.Default.Register(); } catch { /* best effort */ }

        _window = new MainWindow();
        _window.Activate();

        // Daily review reminder (fires while the app is running).
        Reminder = new ReviewReminder(_window.DispatcherQueue);
        var settings = SettingsStore.Load();
        if (settings.ReminderEnabled)
            Reminder.Schedule(settings.ReminderHour, settings.ReminderMinute);

        // Start the always-on capture pipeline: global hotkey -> read selection ->
        // lookupWord -> overlay. Runs for the life of the process (tray app).
        _background = new BackgroundController(Session);
        _background.Start();
    }
}
