import AppKit
import VocabKit

/// Shows a small borderless, non-activating overlay near the mouse with the
/// looked-up meaning, and auto-dismisses it.
@MainActor
final class OverlayController {
    private var panel: NSPanel?
    private var dismissWorkItem: DispatchWorkItem?

    func show(entry: WordEntry) {
        let definition = entry.learnerDefinition.isEmpty
            ? (entry.senses.first?.meaning ?? "")
            : entry.learnerDefinition
        present(title: entry.text, subtitle: definition, footnote: "Looked up \(entry.lookupCount)×")
    }

    func show(message: String) {
        present(title: message, subtitle: "", footnote: "")
    }

    private func present(title: String, subtitle: String, footnote: String) {
        let panel = panel ?? makePanel()
        self.panel = panel

        let stack = NSStackView()
        stack.orientation = .vertical
        stack.alignment = .leading
        stack.edgeInsets = NSEdgeInsets(top: 14, left: 16, bottom: 14, right: 16)
        stack.spacing = 6

        let titleLabel = label(title, font: .systemFont(ofSize: 18, weight: .semibold))
        stack.addArrangedSubview(titleLabel)
        if !subtitle.isEmpty { stack.addArrangedSubview(label(subtitle, font: .systemFont(ofSize: 13))) }
        if !footnote.isEmpty {
            stack.addArrangedSubview(label(footnote, font: .systemFont(ofSize: 11), color: .secondaryLabelColor))
        }
        panel.contentView = stack

        // Position near the mouse (NSEvent origin is bottom-left).
        let mouse = NSEvent.mouseLocation
        panel.setFrameTopLeftPoint(NSPoint(x: mouse.x + 12, y: mouse.y - 12))
        panel.setContentSize(NSSize(width: 360, height: 0))
        panel.layoutIfNeeded()
        panel.orderFrontRegardless()

        scheduleDismiss(after: 6)
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

    private func label(_ text: String, font: NSFont, color: NSColor = .labelColor) -> NSTextField {
        let field = NSTextField(wrappingLabelWithString: text)
        field.font = font
        field.textColor = color
        field.isEditable = false
        field.isBordered = false
        field.drawsBackground = false
        field.preferredMaxLayoutWidth = 330
        return field
    }

    private func scheduleDismiss(after seconds: TimeInterval) {
        dismissWorkItem?.cancel()
        let work = DispatchWorkItem { [weak self] in self?.panel?.orderOut(nil) }
        dismissWorkItem = work
        DispatchQueue.main.asyncAfter(deadline: .now() + seconds, execute: work)
    }
}
