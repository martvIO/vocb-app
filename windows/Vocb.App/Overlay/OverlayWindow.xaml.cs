using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Vocb.App.Services;
using Vocb.Core;

namespace Vocb.App.Overlay;

/// <summary>
/// A small borderless, always-on-top overlay shown near the cursor with the
/// looked-up word's meaning. Has a speak button (pronounces the word) and a close
/// button, and auto-closes after a short delay — the countdown pauses while the
/// pointer is over the popup so it never vanishes mid-read. A single instance is
/// reused across lookups. All Show* calls must be made on the UI thread.
/// </summary>
public sealed partial class OverlayWindow : Window
{
    private const int AutoCloseSeconds = 10;

    private static OverlayWindow? _instance;

    private readonly DispatcherQueueTimer _autoClose;
    private string _currentWord = "";
    private string _currentAudioUrl = "";

    private OverlayWindow()
    {
        InitializeComponent();
        var presenter = OverlappedPresenter.CreateForToolWindow();
        presenter.IsAlwaysOnTop = true;
        presenter.SetBorderAndTitleBar(false, false);
        presenter.IsResizable = false;
        AppWindow.SetPresenter(presenter);
        AppWindow.Resize(new SizeInt32(400, 240));

        _autoClose = DispatcherQueue.CreateTimer();
        _autoClose.IsRepeating = false;
        _autoClose.Interval = TimeSpan.FromSeconds(AutoCloseSeconds);
        _autoClose.Tick += (_, _) => AppWindow.Hide();

        // Pause the countdown while the pointer is over the popup, restart on exit.
        RootBorder.PointerEntered += (_, _) => _autoClose.Stop();
        RootBorder.PointerExited += (_, _) => RestartAutoClose();
    }

    private static OverlayWindow Instance => _instance ??= new OverlayWindow();

    public static void ShowEntry(WordEntry entry)
    {
        var w = Instance;
        w._currentWord = entry.Text;
        w._currentAudioUrl = entry.AudioUrl;
        w.WordText.Text = entry.Text;
        w.PhoneticText.Text = entry.Phonetic;
        w.DefinitionText.Text = string.IsNullOrEmpty(entry.LearnerDefinition)
            ? (entry.Senses.FirstOrDefault()?.Meaning ?? "")
            : entry.LearnerDefinition;
        w.ExampleText.Text = entry.Examples.FirstOrDefault() ?? "";
        w.CountText.Text = $"Looked up {entry.LookupCount}×";
        w.SpeakButton.Visibility = Visibility.Visible;
        w.Present();
    }

    public static void ShowMessage(string message)
    {
        var w = Instance;
        w._currentWord = "";
        w._currentAudioUrl = "";
        w.WordText.Text = "";
        w.PhoneticText.Text = "";
        w.DefinitionText.Text = message;
        w.ExampleText.Text = "";
        w.CountText.Text = "";
        // No word to pronounce on a status message.
        w.SpeakButton.Visibility = Visibility.Collapsed;
        w.Present();
    }

    private void Present()
    {
        if (GetCursorPos(out var pt))
            AppWindow.Move(new PointInt32(pt.X + 12, pt.Y + 12));
        AppWindow.Show();
        Activate();
        RestartAutoClose();
    }

    private void RestartAutoClose()
    {
        _autoClose.Stop();
        _autoClose.Start();
    }

    private void Speak_Click(object sender, RoutedEventArgs e)
    {
        RestartAutoClose(); // interacting keeps the popup alive
        Speech.Pronounce(_currentWord, _currentAudioUrl);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _autoClose.Stop();
        AppWindow.Hide();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);
}
