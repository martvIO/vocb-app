import AppKit
import FirebaseCore
import VocabKit

/// The macOS menu-bar agent: runs in the background (no Dock icon — set
/// `LSUIElement` = YES in the target's Info.plist), registers the global hotkey,
/// and on trigger reads the current selection, looks it up, and shows the overlay.
@main
final class AgentAppDelegate: NSObject, NSApplicationDelegate {
    static func main() {
        let app = NSApplication.shared
        let delegate = AgentAppDelegate()
        app.delegate = delegate
        app.setActivationPolicy(.accessory) // menu-bar agent, no Dock icon
        app.run()
    }

    private var statusItem: NSStatusItem!
    private let hotkey = GlobalHotkey()
    private let overlay = OverlayController()
    private let lookup = FunctionsLookupService()
    private var busy = false

    func applicationDidFinishLaunching(_ notification: Notification) {
        FirebaseApp.configure()
        setupStatusItem()

        if !AccessibilitySelection.isTrusted {
            AccessibilitySelection.promptForTrust()
        }
        hotkey.register { [weak self] in self?.onHotkey() }
    }

    private func setupStatusItem() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem.button?.title = "📖"

        let menu = NSMenu()
        menu.addItem(NSMenuItem(title: "Look up selection (⌃⇧L)",
                                action: #selector(onHotkey), keyEquivalent: ""))
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Grant Accessibility…",
                                action: #selector(grantAccessibility), keyEquivalent: ""))
        menu.addItem(NSMenuItem(title: "Quit Vocb Agent",
                                action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q"))
        statusItem.menu = menu
    }

    @objc private func grantAccessibility() {
        AccessibilitySelection.promptForTrust()
    }

    @objc private func onHotkey() {
        guard !busy else { return }
        guard let text = AccessibilitySelection.currentSelection(), !text.isEmpty else {
            overlay.show(message: "No text selected.")
            return
        }
        busy = true
        overlay.show(message: "Looking up “\(text)”…")
        Task {
            defer { busy = false }
            do {
                let result = try await lookup.lookup(text)
                await MainActor.run { self.overlay.show(entry: result.entry) }
            } catch {
                await MainActor.run { self.overlay.show(message: "Lookup failed: \(error.localizedDescription)") }
            }
        }
    }
}
