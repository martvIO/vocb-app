import AppKit
import VocabKit

/// Shows a small borderless, non-activating overlay near the mouse with the looked-up
/// meaning. Has a speak button (pronounces the word) and a close (X) button, and
/// auto-dismisses after a short delay — the countdown pauses while the pointer is over
/// the panel so it never vanishes mid-read.
@MainActor
final class OverlayController: NSObject {
    private var panel: NSPanel?
    private var dismissWorkItem: DispatchWorkItem?
    private var currentEntry: WordEntry?
    private let autoDismissSeconds: TimeInterval = 10

    func show(entry: WordEntry) {
        currentEntry = entry
        let definition = entry.learnerDefinition.isEmpty
            ? (entry.senses.first?.meaning ?? "")
            : entry.learnerDefinition
        present(title: entry.text, subtitle: definition,
                footnote: "Looked up \(entry.lookupCount)×", canSpeak: true)
    }

    func show(message: String) {
        currentEntry = nil
        present(title: message, subtitle: "", footnote: "", canSpeak: false)
    }

    private func present(title: String, subtitle: String, footnote: String, canSpeak: Bool) {
        let panel = panel ?? makePanel()
        self.panel = panel

        // Header row: title, then (optionally) speak + close pushed to the right.
        let header = NSStackView()
        header.orientation = .horizontal
        header.spacing = 8
        header.alignment = .firstBaseline
        header.addArrangedSubview(label(title, font: .systemFont(ofSize: 18, weight: .semibold)))

        let spacer = NSView()
        spacer.setContentHuggingPriority(.defaultLow, for: .horizontal)
        spacer.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
        header.addArrangedSubview(spacer)

        if canSpeak {
            header.addArrangedSubview(
                iconButton(symbol: "speaker.wave.2.fill", help: "Pronounce", action: #selector(speakTapped)))
        }
        header.addArrangedSubview(
            iconButton(symbol: "xmark", help: "Close", action: #selector(closeTapped)))

        let stack = NSStackView()
        stack.orientation = .vertical
        stack.alignment = .leading
        stack.edgeInsets = NSEdgeInsets(top: 14, left: 16, bottom: 14, right: 16)
        stack.spacing = 6
        stack.addArrangedSubview(header)
        if !subtitle.isEmpty { stack.addArrangedSubview(label(subtitle, font: .systemFont(ofSize: 13))) }
        if !footnote.isEmpty {
            stack.addArrangedSubview(label(footnote, font: .systemFont(ofSize: 11), color: .secondaryLabelColor))
        }

        // Make the header span the content width so the spacer pushes the buttons to
        // the right edge. (Panel content is 360 wide minus the stack's 16pt insets.)
        header.translatesAutoresizingMaskIntoConstraints = false
        header.widthAnchor.constraint(equalToConstant: 328).isActive = true

        // Host the content in a hover-tracking view so we can pause the auto-dismiss
        // while the pointer is over the popup.
        let content = HoverTrackingView()
        content.onEnter = { [weak self] in self?.cancelDismiss() }
        content.onExit = { [weak self] in
            guard let self else { return }
            self.scheduleDismiss(after: self.autoDismissSeconds)
        }
        stack.translatesAutoresizingMaskIntoConstraints = false
        content.addSubview(stack)
        NSLayoutConstraint.activate([
            stack.topAnchor.constraint(equalTo: content.topAnchor),
            stack.leadingAnchor.constraint(equalTo: content.leadingAnchor),
            stack.trailingAnchor.constraint(equalTo: content.trailingAnchor),
            stack.bottomAnchor.constraint(equalTo: content.bottomAnchor),
        ])
        panel.contentView = content

        // Position near the mouse (NSEvent origin is bottom-left).
        let mouse = NSEvent.mouseLocation
        panel.setFrameTopLeftPoint(NSPoint(x: mouse.x + 12, y: mouse.y - 12))
        panel.setContentSize(NSSize(width: 360, height: 0))
        panel.layoutIfNeeded()
        panel.orderFrontRegardless()

        scheduleDismiss(after: autoDismissSeconds)
    }

    @objc private func speakTapped() {
        cancelDismiss()
        if let entry = currentEntry { Speaker.pronounce(entry) }
        scheduleDismiss(after: autoDismissSeconds)
    }

    @objc private func closeTapped() {
        cancelDismiss()
        panel?.orderOut(nil)
    }

    private func makePanel() -> NSPanel {
        let panel = NSPanel(contentRect: .zero,
                            styleMask: [.borderless, .nonactivatingPanel],
                            backing: .buffered, defer: true)
        panel.isFloatingPanel = true
        panel.level = .floating
        panel.hasShadow = true
        panel.backgroundColor = .windowBackgroundColor
        panel.isMovableByWindowBackground = true
        return panel
    }

    private func iconButton(symbol: String, help: String, action: Selector) -> NSButton {
        let button: NSButton
        if let image = NSImage(systemSymbolName: symbol, accessibilityDescription: help) {
            button = NSButton(image: image, target: self, action: action)
        } else {
            button = NSButton(title: help, target: self, action: action)
        }
        button.isBordered = false
        button.bezelStyle = .regularSquare
        button.setButtonType(.momentaryChange)
        button.toolTip = help
        button.setContentHuggingPriority(.required, for: .horizontal)
        return button
    }

    private func label(_ text: String, font: NSFont, color: NSColor = .labelColor) -> NSTextField {
        let field = NSTextField(wrappingLabelWithString: text)
        field.font = font
        field.textColor = color
        field.isEditable = false
        field.isBordered = false
        field.drawsBackground = false
        field.preferredMaxLayoutWidth = 280
        return field
    }

    private func cancelDismiss() {
        dismissWorkItem?.cancel()
    }

    private func scheduleDismiss(after seconds: TimeInterval) {
        dismissWorkItem?.cancel()
        let work = DispatchWorkItem { [weak self] in self?.panel?.orderOut(nil) }
        dismissWorkItem = work
        DispatchQueue.main.asyncAfter(deadline: .now() + seconds, execute: work)
    }
}

/// An NSView that reports when the pointer enters/exits its bounds, so the overlay can
/// pause its auto-dismiss countdown while the user is reading or interacting.
private final class HoverTrackingView: NSView {
    var onEnter: (() -> Void)?
    var onExit: (() -> Void)?
    private var tracking: NSTrackingArea?

    override func updateTrackingAreas() {
        super.updateTrackingAreas()
        if let tracking { removeTrackingArea(tracking) }
        let area = NSTrackingArea(
            rect: bounds,
            options: [.mouseEnteredAndExited, .activeAlways, .inVisibleRect],
            owner: self, userInfo: nil)
        addTrackingArea(area)
        tracking = area
    }

    override func mouseEntered(with event: NSEvent) { onEnter?() }
    override func mouseExited(with event: NSEvent) { onExit?() }
}
