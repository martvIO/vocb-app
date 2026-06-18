using Windows.ApplicationModel.DataTransfer;
using static Vocb.App.Capture.NativeMethods;

namespace Vocb.App.Capture;

/// <summary>
/// Reads the user's current text selection in whatever app has focus.
///
/// Strategy: synthesize Ctrl+C, read the clipboard, then restore the prior
/// clipboard contents. This works across virtually every Windows app. (A more
/// surgical alternative is UI Automation's TextPattern, but it isn't supported
/// by every control; the clipboard approach is the robust fallback.)
/// </summary>
public static class SelectionCapture
{
    public static async Task<string?> CaptureSelectionAsync()
    {
        // 1. Snapshot the existing clipboard text so we can restore it.
        string? previous = await TryGetClipboardTextAsync();

        // 2. Synthesize Ctrl+C in the foreground app.
        SendCopy();

        // 3. Give the target app a moment to populate the clipboard, then read.
        string? captured = null;
        for (int attempt = 0; attempt < 5 && string.IsNullOrEmpty(captured); attempt++)
        {
            await Task.Delay(40);
            captured = await TryGetClipboardTextAsync();
            if (captured == previous) captured = null; // unchanged => nothing was selected
        }

        // 4. Restore the previous clipboard contents.
        if (previous is not null)
            TrySetClipboardText(previous);

        return string.IsNullOrWhiteSpace(captured) ? null : captured.Trim();
    }

    private static void SendCopy()
    {
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        keybd_event(VK_C, 0, 0, UIntPtr.Zero);
        keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private static async Task<string?> TryGetClipboardTextAsync()
    {
        try
        {
            var content = Clipboard.GetContent();
            if (content.Contains(StandardDataFormats.Text))
                return await content.GetTextAsync();
        }
        catch
        {
            // Clipboard can be momentarily locked by another process; ignore.
        }
        return null;
    }

    private static void TrySetClipboardText(string text)
    {
        try
        {
            var dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
        }
        catch { /* ignore */ }
    }
}
