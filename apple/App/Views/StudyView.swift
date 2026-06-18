import SwiftUI
import VocabKit

/// A flip-card review session driven by the SM-2 engine and the study queue.
struct StudyView: View {
    @EnvironmentObject private var model: AppModel

    @State private var queue: [StudyQueue.Card] = []
    @State private var index = 0
    @State private var revealed = false
    @State private var loading = true

    private var current: WordEntry? { index < queue.count ? queue[index].entry : nil }

    var body: some View {
        VStack(spacing: 24) {
            if loading {
                ProgressView()
            } else if let word = current {
                Spacer()
                Text(word.text).font(.system(size: 40, weight: .semibold))

                if revealed {
                    Text(word.learnerDefinition.isEmpty
                         ? (word.senses.first?.meaning ?? "")
                         : word.learnerDefinition)
                        .multilineTextAlignment(.center)
                    if let ex = word.examples.first {
                        Text("“\(ex)”").italic().foregroundStyle(.secondary)
                            .multilineTextAlignment(.center)
                    }
                }
                Spacer()

                if revealed {
                    HStack {
                        gradeButton("Again", .again, .red)
                        gradeButton("Hard", .hard, .orange)
                        gradeButton("Good", .good, .blue)
                        gradeButton("Easy", .easy, .green)
                    }
                } else {
                    Button("Show answer") { revealed = true }
                        .buttonStyle(.borderedProminent)
                }
            } else {
                ContentUnavailableView("All caught up!", systemImage: "checkmark.circle")
            }
        }
        .padding()
        .navigationTitle("Study")
        .task { await load() }
    }

    private func gradeButton(_ title: String, _ grade: ReviewGrade, _ color: Color) -> some View {
        Button(title) { grade(grade) }
            .buttonStyle(.bordered)
            .tint(color)
    }

    private func load() async {
        loading = true
        guard let repo = model.repository else { loading = false; return }
        var states: [String: SRSState] = [:]
        for word in model.words {
            if let state = try? await repo.srsState(for: word.lemma) {
                states[word.lemma] = state
            }
        }
        queue = StudyQueue.build(words: model.words, states: states, limit: 30)
        index = 0
        revealed = false
        loading = false
    }

    private func grade(_ grade: ReviewGrade) {
        guard index < queue.count, let repo = model.repository else { return }
        let card = queue[index]
        let next = SRSEngine.schedule(card.state, grade: grade)
        Task { try? await repo.saveSRSState(next, for: card.entry.lemma) }
        index += 1
        revealed = false
    }
}
