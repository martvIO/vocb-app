import Foundation

/// One sense of a word: part of speech + a learner-friendly meaning.
/// Mirrors `WordSense` in the backend (`backend/functions/src/types.ts`).
public struct WordSense: Codable, Hashable, Sendable {
    public var partOfSpeech: String
    public var meaning: String

    public init(partOfSpeech: String, meaning: String) {
        self.partOfSpeech = partOfSpeech
        self.meaning = meaning
    }
}

/// A stored vocabulary entry. The document id in Firestore is `lemma`.
/// Field names and types match the backend `WordEntry` exactly so the same
/// documents decode on iPad, macOS, and (via REST) Windows.
public struct WordEntry: Codable, Identifiable, Hashable, Sendable {
    /// Normalized key, also the Firestore document id (e.g. "run").
    public var lemma: String
    /// Original surface form first looked up (e.g. "running").
    public var text: String
    public var kind: Kind

    /// How many times the user looked this up — drives sorting + test priority.
    public var lookupCount: Int
    /// Epoch milliseconds (matches the backend's `Date.now()`).
    public var firstSeen: Int64
    public var lastSeen: Int64

    public var phonetic: String
    public var audioUrl: String

    public var learnerDefinition: String
    public var senses: [WordSense]
    public var examples: [String]
    public var synonyms: [String]
    public var antonyms: [String]

    public var deckIds: [String]
    public var tags: [String]

    public var generatedBy: String
    public var schemaVersion: Int

    public enum Kind: String, Codable, Sendable {
        case word
        case phrase
    }

    /// `Identifiable` uses the lemma (the document id).
    public var id: String { lemma }

    public var firstSeenDate: Date { Date(timeIntervalSince1970: Double(firstSeen) / 1000) }
    public var lastSeenDate: Date { Date(timeIntervalSince1970: Double(lastSeen) / 1000) }

    public init(
        lemma: String,
        text: String,
        kind: Kind,
        lookupCount: Int,
        firstSeen: Int64,
        lastSeen: Int64,
        phonetic: String = "",
        audioUrl: String = "",
        learnerDefinition: String,
        senses: [WordSense] = [],
        examples: [String] = [],
        synonyms: [String] = [],
        antonyms: [String] = [],
        deckIds: [String] = [],
        tags: [String] = [],
        generatedBy: String,
        schemaVersion: Int
    ) {
        self.lemma = lemma
        self.text = text
        self.kind = kind
        self.lookupCount = lookupCount
        self.firstSeen = firstSeen
        self.lastSeen = lastSeen
        self.phonetic = phonetic
        self.audioUrl = audioUrl
        self.learnerDefinition = learnerDefinition
        self.senses = senses
        self.examples = examples
        self.synonyms = synonyms
        self.antonyms = antonyms
        self.deckIds = deckIds
        self.tags = tags
        self.generatedBy = generatedBy
        self.schemaVersion = schemaVersion
    }
}

/// A user-defined deck for organizing words: users/{uid}/decks/{deckId}.
public struct Deck: Codable, Identifiable, Hashable, Sendable {
    public var id: String
    public var name: String
    /// Hex color string, e.g. "#E8A33D".
    public var color: String
    public var createdAt: Int64

    public init(id: String, name: String, color: String, createdAt: Int64) {
        self.id = id
        self.name = name
        self.color = color
        self.createdAt = createdAt
    }
}
