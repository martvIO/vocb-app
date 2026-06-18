using Microsoft.UI.Xaml;
using Vocb.App.Capture;
using Vocb.App.Services;

namespace Vocb.App;

public partial class App : Application
{
    /// <summary>Process-wide session (auth + Firebase clients). Configured in Settings.</summary>
    public static SessionService Session { get; } = new();

    private Window? _window;
    private BackgroundController? _background;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();

        // Start the always-on capture pipeline: global hotkey -> read selection ->
        // lookupWord -> overlay. Runs for the life of the process (tray app).
        _background = new BackgroundController(Session);
        _background.Start();
    }
}
