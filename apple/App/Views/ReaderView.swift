import SwiftUI
import VocabKit

/// In-app reader: paste or type text, then tap any word to look it up. This is
/// the cross-platform capture path (the only one available on iPad, where a
/// system-wide background listener isn't permitted).
struct ReaderView: View {
    @EnvironmentObject private var model: AppModel
    @State private var text = ""
    @State private var result: WordEntry?
    @State private var error: String?

    private var tokens: [String] {
        let raw = text.split { !$0.isLetter && $0 != "'" && $0 != "-" }.map(String.init)
        var seen = Set<String>()
        return raw.filter { $0.count > 1 && seen.insert($0.lowercased()).inserted }
    }

    private let columns = [GridItem(.adaptive(minimum: 90), spacing: 8)]

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            TextEditor(text: $text)
                .frame(minHeight: 140)
                .overlay(RoundedRectangle(cornerRadius: 8).stroke(.quaternary))

            Text("Tap a word to look it up — it's saved and counted.")
                .font(.caption).foregroundStyle(.secondary)

            if let error { Text(error).foregroundStyle(.red).font(.footnote) }

            ScrollView {
                LazyVGrid(columns: columns, alignment: .leading, spacing: 8) {
                    ForEach(tokens, id: \.self) { token in
                        Button(token) { lookUp(token) }
                            .buttonStyle(.bordered)
                    }
                }
            }
        }
        .padding()
        .navigationTitle("Reader")
        .sheet(item: $result) { entry in
            let card = WordPopupCard(entry: entry) { result = nil }
            #if os(iOS)
            // A small, draggable popup card rather than a full-screen push.
            card.presentationDetents([.height(280), .medium])
            #else
            card.frame(minWidth: 360, minHeight: 220).padding()
            #endif
        }
    }

    private func lookUp(_ word: String) {
        error = nil
        Task {
            do { result = try await model.lookup(word) }
            catch { self.error = error.localizedDescription }
        }
    }
}
