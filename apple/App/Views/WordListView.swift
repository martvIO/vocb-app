import SwiftUI
import VocabKit

/// The saved words, most-looked-up first (the headline sort). Tapping a row
/// opens the detail page.
struct WordListView: View {
    @EnvironmentObject private var model: AppModel

    var body: some View {
        List(model.words) { word in
            NavigationLink(value: word) {
                HStack {
                    VStack(alignment: .leading, spacing: 2) {
                        Text(word.text).font(.headline)
                        Text(word.learnerDefinition)
                            .font(.subheadline)
                            .foregroundStyle(.secondary)
                            .lineLimit(2)
                    }
                    Spacer()
                    Text("\(word.lookupCount)×")
                        .font(.caption).bold()
                        .padding(.horizontal, 8).padding(.vertical, 4)
                        .background(.tint, in: Capsule())
                        .foregroundStyle(.white)
                }
            }
        }
        .navigationTitle("Words")
        .navigationDestination(for: WordEntry.self) { WordDetailView(entry: $0) }
        .overlay {
            if let status = model.statusMessage, model.words.isEmpty {
                ContentUnavailableView(status, systemImage: "text.book.closed")
            }
        }
        .refreshable { await model.reload() }
    }
}
