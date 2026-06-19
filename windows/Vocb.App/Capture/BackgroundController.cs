using Vocb.App.Overlay;
using Vocb.App.Services;

namespace Vocb.App.Capture;

/// <summary>
/// The always-on capture pipeline. On the global hotkey it reads the current
/// selection, looks it up (auto-saving + counting it via the backend), and shows
/// the meaning in a lightweight overlay. Mirrors the macOS menu-bar agent.
/// </summary>
public sealed class BackgroundController : IDisposable
{
    private readonly SessionService _session;
    private GlobalHotkey? _hotkey;
    private bool _busy;

    public BackgroundController(SessionService session)
    {
        _session = session;
    }

    public void Start()
    {
        _hotkey = new GlobalHotkey();
        _hotkey.Pressed += OnHotkey;
    }

    private async void OnHotkey()
    {
        if (_busy) return; // ignore re-entrancy while a lookup is in flight
        _busy = true;
        try
        {
            var text = await SelectionCapture.CaptureSelectionAsync();
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!_session.IsSignedIn)
            {
                OverlayWindow.ShowMessage("Sign in to Vocb to look up words.");
                return;
            }

            OverlayWindow.ShowMessage($"Looking up “{text}”…");
            var result = await _session.LookupAsync(text);
            OverlayWindow.ShowEntry(result.Entry);
        }
        catch (Exception ex)
        {
            OverlayWindow.ShowMessage($"Lookup failed: {ex.Message}");
        }
        finally
        {
            _busy = false;
        }
    }

    public void Dispose()
    {
        if (_hotkey is not null)
        {
            _hotkey.Pressed -= OnHotkey;
            _hotkey.Dispose();
        }
    }
}
