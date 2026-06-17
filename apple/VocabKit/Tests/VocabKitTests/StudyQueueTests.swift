import XCTest
@testable import VocabKit

final class StudyQueueTests: XCTestCase {
    let now = Date(timeIntervalSince1970: 1_700_000_000)

    private func word(_ lemma: String, lookupCount: Int) -> WordEntry {
        WordEntry(
            lemma: lemma,
            text: lemma,
            kind: .word,
            lookupCount: lookupCount,
            firstSeen: 0,
            lastSeen: 0,
            learnerDefinition: "def of \(lemma)",
            generatedBy: "test",
            schemaVersion: SCHEMA_VERSION
        )
    }

    func testDueCardsComeBeforeFreshAndExcludeNotDue() {
        let words = [
            word("alpha", lookupCount: 5),   // fresh
            word("bravo", lookupCount: 10),  // fresh, higher count
            word("charlie", lookupCount: 1), // due yesterday
            word("delta", lookupCount: 99),  // not due (tomorrow)
        ]
        let states: [String: SRSState] = [
            "charlie": SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(-86_400))),
            "delta": SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(86_400))),
        ]

        let queue = StudyQueue.build(words: words, states: states, now: now)
        let order = queue.map(\.entry.lemma)

        // charlie is due; delta is excluded; fresh words ordered by lookupCount desc.
        XCTAssertEqual(order, ["charlie", "bravo", "alpha"])
    }

    func testDueCardsSortedByDueDate() {
        let words = [word("a", lookupCount: 1), word("b", lookupCount: 1)]
        let states: [String: SRSState] = [
            "a": SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(-3_600))),
            "b": SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(-7_200))),
        ]
        let queue = StudyQueue.build(words: words, states: states, now: now)
        XCTAssertEqual(queue.map(\.entry.lemma), ["b", "a"]) // older due first
    }

    func testLimitTruncatesQueue() {
        let words = (0..<10).map { word("w\($0)", lookupCount: $0) }
        let queue = StudyQueue.build(words: words, states: [:], now: now, limit: 3)
        XCTAssertEqual(queue.count, 3)
        // Highest lookupCount first among fresh cards.
        XCTAssertEqual(queue.map(\.entry.lemma), ["w9", "w8", "w7"])
    }
}
