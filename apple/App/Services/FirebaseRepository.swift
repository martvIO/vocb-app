import Foundation
import FirebaseAuth
import FirebaseFirestore
import VocabKit

/// Concrete `VocabRepository` backed by Cloud Firestore, scoped to the signed-in
/// user's `users/{uid}/…` subtree. Word documents are created by the backend
/// `lookupWord` function; this reads them and writes SRS state + organization.
public struct FirebaseRepository: VocabRepository {
    private let db: Firestore
    private let uid: String

    public init(uid: String, db: Firestore = Firestore.firestore()) {
        self.uid = uid
        self.db = db
    }

    private var wordsRef: CollectionReference { db.collection("users").document(uid).collection("words") }
    private var srsRef: CollectionReference { db.collection("users").document(uid).collection("srs") }
    private var decksRef: CollectionReference { db.collection("users").document(uid).collection("decks") }

    public func words() async throws -> [WordEntry] {
        let snapshot = try await wordsRef
            .order(by: "lookupCount", descending: true)
            .getDocuments()
        return snapshot.documents.compactMap { try? $0.data(as: WordEntry.self) }
    }

    public func srsState(for lemma: String) async throws -> SRSState? {
        let doc = try await srsRef.document(lemma).getDocument()
        guard doc.exists else { return nil }
        return try doc.data(as: SRSState.self)
    }

    public func saveSRSState(_ state: SRSState, for lemma: String) async throws {
        try srsRef.document(lemma).setData(from: state)
    }

    public func updateOrganization(lemma: String, deckIds: [String], tags: [String]) async throws {
        try await wordsRef.document(lemma).updateData(["deckIds": deckIds, "tags": tags])
    }

    public func decks() async throws -> [Deck] {
        let snapshot = try await decksRef.order(by: "createdAt").getDocuments()
        return snapshot.documents.compactMap { try? $0.data(as: Deck.self) }
    }
}
