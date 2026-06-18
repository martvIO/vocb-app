using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Vocb.Core;

namespace Vocb.App.Overlay;

/// <summary>
/// A small borderless, always-on-top overlay shown near the cursor with the
/// looked-up word's meaning. A single instance is reused across lookups.
/// All Show* calls must be made on the UI thread.
/// </summary>
public sealed partial class OverlayWindow : Window
{
    private static OverlayWindow? _instance;

    private OverlayWindow()
    {
        InitializeComponent();
        var presenter = OverlappedPresenter.CreateForToolWindow();
        presenter.IsAlwaysOnTop = true;
        presenter.SetBorderAndTitleBar(false, false);
        presenter.IsResizable = false;
        AppWindow.SetPresenter(presenter);
        AppWindow.Resize(new SizeInt32(400, 240));
    }

    private static OverlayWindow Instance => _instance ??= new OverlayWindow();

    public static void ShowEntry(WordEntry entry)
    {
        var w = Instance;
        w.WordText.Text = entry.Text;
        w.PhoneticText.Text = entry.Phonetic;
        w.DefinitionText.Text = string.IsNullOrEmpty(entry.LearnerDefinition)
            ? (entry.Senses.FirstOrDefault()?.Meaning ?? "")
            : entry.LearnerDefinition;
        w.ExampleText.Text = entry.Examples.FirstOrDefault() ?? "";
        w.CountText.Text = $"Looked up {entry.LookupCount}×";
        w.Present();
    }

    public static void ShowMessage(string message)
    {
        var w = Instance;
        w.WordText.Text = "";
        w.PhoneticText.Text = "";
        w.DefinitionText.Text = message;
        w.ExampleText.Text = "";
        w.CountText.Text = "";
        w.Present();
    }

    private void Present()
    {
        if (GetCursorPos(out var pt))
            AppWindow.Move(new PointInt32(pt.X + 12, pt.Y + 12));
        AppWindow.Show();
        Activate();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);
}
