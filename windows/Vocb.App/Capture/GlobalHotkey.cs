using static Vocb.App.Capture.NativeMethods;

namespace Vocb.App.Capture;

/// <summary>
/// Registers a system-wide hotkey (default Ctrl+Shift+L) using a hidden
/// message-only window to receive WM_HOTKEY. Raises <see cref="Pressed"/> on the
/// thread that created it (the UI thread).
/// </summary>
public sealed class GlobalHotkey : IDisposable
{
    private const int HotkeyId = 0x4C56; // arbitrary

    // Keep the delegate alive for the window's lifetime (prevents GC of the callback).
    private readonly WndProcDelegate _wndProc;
    private readonly IntPtr _hwnd;
    private bool _registered;

    public event Action? Pressed;

    public GlobalHotkey(uint modifiers = MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, uint vk = 0x4C /* 'L' */)
    {
        _wndProc = WndProc;
        var className = "VocbHotkeyWindow_" + Guid.NewGuid().ToString("N");
        var wc = new WNDCLASS
        {
            lpfnWndProc = _wndProc,
            hInstance = GetModuleHandleW(null),
            lpszClassName = className,
        };
        RegisterClassW(ref wc);

        _hwnd = CreateWindowExW(0, className, null, 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, wc.hInstance, IntPtr.Zero);
        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create hotkey message window.");

        _registered = RegisterHotKey(_hwnd, HotkeyId, modifiers, vk);
        if (!_registered)
            throw new InvalidOperationException("Failed to register the global hotkey (already in use?).");
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            Pressed?.Invoke();
            return IntPtr.Zero;
        }
        return DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_hwnd, HotkeyId);
            _registered = false;
        }
        if (_hwnd != IntPtr.Zero)
            DestroyWindow(_hwnd);
    }
}
