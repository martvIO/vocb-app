import Foundation

/// Result of calling the backend `lookupWord` function.
public struct LookupResult: Sendable {
    public let entry: WordEntry
    /// true if a new entry was created, false if an existing one was bumped.
    public let created: Bool

    public init(entry: WordEntry, created: Bool) {
        self.entry = entry
        self.created = created
    }
}

/// Calls the backend to look up / auto-save a selected word or phrase.
/// The app provides the concrete implementation (Firebase Functions on Apple
/// platforms; REST on Windows). VocabKit stays Firebase-free.
public protocol LookupServicing: Sendable {
    /// Send raw selected text; the backend normalizes and returns the entry.
    func lookup(_ text: String) async throws -> LookupResult
}

/// Reads/writes the user's vocabulary, SRS state, and decks. Implemented by the
/// app over Firestore; kept as a protocol here so VocabKit and its tests don't
/// depend on Firebase.
public protocol VocabRepository: Sendable {
    /// All saved words, most-looked-up first.
    func words() async throws -> [WordEntry]
    /// SRS state for a given lemma, or nil if the card was never reviewed.
    func srsState(for lemma: String) async throws -> SRSState?
    /// Persist updated SRS state for a lemma.
    func saveSRSState(_ state: SRSState, for lemma: String) async throws
    /// Update the decks/tags a word belongs to.
    func updateOrganization(lemma: String, deckIds: [String], tags: [String]) async throws
    func decks() async throws -> [Deck]
}

/// Builds an ordered study queue from saved words and their SRS state.
public enum StudyQueue {
    /// A card to review: the word plus its current scheduling state.
    public struct Card: Sendable {
        public let entry: WordEntry
        public let state: SRSState
        public init(entry: WordEntry, state: SRSState) {
            self.entry = entry
            self.state = state
        }
    }

    /// Order cards for a review session.
    ///
    /// Due cards come first (oldest due date first). Among cards with no SRS
    /// state yet (never reviewed), the most-looked-up words are prioritized —
    /// the words the user encounters most get tested first.
    public static func build(
        words: [WordEntry],
        states: [String: SRSState],
        now: Date = Date(),
        limit: Int? = nil
    ) -> [Card] {
        var due: [Card] = []
        var fresh: [WordEntry] = []

        for word in words {
            if let state = states[word.lemma] {
                if SRSEngine.isDue(state, now: now) {
                    due.append(Card(entry: word, state: state))
                }
            } else {
                fresh.append(word)
            }
        }

        due.sort { $0.state.dueDate < $1.state.dueDate }
        fresh.sort { $0.lookupCount > $1.lookupCount }

        var queue = due + fresh.map { Card(entry: $0, state: .new(now: now)) }
        if let limit, queue.count > limit {
            queue = Array(queue.prefix(limit))
        }
        return queue
    }
}
