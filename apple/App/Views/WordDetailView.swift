import SwiftUI
import VocabKit

/// Full entry: pronunciation, definition, senses, examples, synonyms/antonyms.
struct WordDetailView: View {
    let entry: WordEntry

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 16) {
                HStack(alignment: .firstTextBaseline) {
                    Text(entry.text).font(.largeTitle).bold()
                    if !entry.phonetic.isEmpty {
                        Text(entry.phonetic).foregroundStyle(.secondary)
                    }
                    Spacer()
                    Button {
                        Pronunciation.pronounce(entry)
                    } label: {
                        Image(systemName: "speaker.wave.2.fill")
                    }
                    .buttonStyle(.bordered)
                }

                if !entry.learnerDefinition.isEmpty {
                    Text(entry.learnerDefinition).font(.body)
                }

                if !entry.senses.isEmpty {
                    section("Senses") {
                        ForEach(Array(entry.senses.enumerated()), id: \.offset) { _, s in
                            Text("• [\(s.partOfSpeech)] \(s.meaning)")
                        }
                    }
                }

                if !entry.examples.isEmpty {
                    section("Examples") {
                        ForEach(entry.examples, id: \.self) { ex in
                            Text("“\(ex)”").italic().foregroundStyle(.secondary)
                        }
                    }
                }

                if !entry.synonyms.isEmpty { chips("Synonyms", entry.synonyms) }
                if !entry.antonyms.isEmpty { chips("Antonyms", entry.antonyms) }

                Text("Looked up \(entry.lookupCount)×")
                    .font(.caption).foregroundStyle(.secondary)
            }
            .padding()
        }
        .navigationTitle(entry.text)
    }

    @ViewBuilder private func section<Content: View>(_ title: String, @ViewBuilder _ content: () -> Content) -> some View {
        VStack(alignment: .leading, spacing: 6) {
            Text(title).font(.headline)
            content()
        }
    }

    @ViewBuilder private func chips(_ title: String, _ items: [String]) -> some View {
        section(title) {
            Text(items.joined(separator: ", ")).foregroundStyle(.secondary)
        }
    }
}
