#if canImport(SwiftUI)
import SwiftUI

/// A compact lookup popup: the word, its pronunciation, a short definition, a speak
/// button, and a close (X) button. Shared by the in-app Reader and the iOS
/// Action/Share extensions so the popup looks and behaves the same everywhere.
///
/// `onClose` is called when the X is tapped — the host decides what "close" means
/// (dismiss a sheet, or finish an extension request).
public struct WordPopupCard: View {
    private let entry: WordEntry
    private let onClose: () -> Void

    public init(entry: WordEntry, onClose: @escaping () -> Void) {
        self.entry = entry
        self.onClose = onClose
    }

    private var definition: String {
        entry.learnerDefinition.isEmpty
            ? (entry.senses.first?.meaning ?? "")
            : entry.learnerDefinition
    }

    public var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            HStack(alignment: .firstTextBaseline) {
                Text(entry.text).font(.title2).bold()
                if !entry.phonetic.isEmpty {
                    Text(entry.phonetic).foregroundStyle(.secondary).font(.subheadline)
                }
                Spacer()
                Button {
                    Speaker.pronounce(entry)
                } label: {
                    Image(systemName: "speaker.wave.2.fill")
                }
                .buttonStyle(.borderless)
                .accessibilityLabel("Pronounce")

                Button {
                    onClose()
                } label: {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundStyle(.secondary)
                }
                .buttonStyle(.borderless)
                .accessibilityLabel("Close")
            }

            if !definition.isEmpty {
                Text(definition).font(.body)
            }
            if let example = entry.examples.first, !example.isEmpty {
                Text("“\(example)”").italic().foregroundStyle(.secondary).font(.callout)
            }
            Text("Looked up \(entry.lookupCount)×")
                .font(.caption).foregroundStyle(.secondary)
        }
        .padding(16)
        .frame(maxWidth: 360, alignment: .leading)
        .background(.regularMaterial, in: RoundedRectangle(cornerRadius: 16))
        .shadow(radius: 12, y: 4)
    }
}
#endif
