import ApplicationServices

/// Reads the current text selection from whatever app has focus, using the
/// macOS Accessibility API. Requires the user to grant Accessibility permission
/// (System Settings → Privacy & Security → Accessibility).
enum AccessibilitySelection {
    /// The selected text in the focused UI element, or nil if none/unavailable.
    static func currentSelection() -> String? {
        let systemWide = AXUIElementCreateSystemWide()

        var focused: AnyObject?
        guard AXUIElementCopyAttributeValue(systemWide, kAXFocusedUIElementAttribute as CFString, &focused) == .success,
              let element = focused
        else { return nil }

        var value: AnyObject?
        guard AXUIElementCopyAttributeValue(element as! AXUIElement, kAXSelectedTextAttribute as CFString, &value) == .success
        else { return nil }

        return (value as? String)?.trimmingCharacters(in: .whitespacesAndNewlines)
    }

    static var isTrusted: Bool { AXIsProcessTrusted() }

    /// Show the system prompt asking the user to grant Accessibility access.
    static func promptForTrust() {
        let key = kAXTrustedCheckOptionPrompt.takeUnretainedValue() as String
        _ = AXIsProcessTrustedWithOptions([key: true] as CFDictionary)
    }
}
