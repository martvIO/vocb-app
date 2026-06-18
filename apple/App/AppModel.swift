import Foundation
import FirebaseAuth
import VocabKit

/// App-wide state: auth, the Firestore repository, the lookup service, and the
/// loaded word list. Drives every screen.
@MainActor
final class AppModel: ObservableObject {
    @Published var uid: String?
    @Published var words: [WordEntry] = []
    @Published var statusMessage: String?

    private(set) var repository: VocabRepository?
    private let lookupService: LookupServicing

    private var authHandle: AuthStateDidChangeListenerHandle?

    init(lookupService: LookupServicing = FunctionsLookupService()) {
        self.lookupService = lookupService
        observeAuth()
    }

    var isSignedIn: Bool { uid != nil }

    private func observeAuth() {
        authHandle = Auth.auth().addStateDidChangeListener { [weak self] _, user in
            Task { @MainActor in
                guard let self else { return }
                self.uid = user?.uid
                if let uid = user?.uid {
                    self.repository = FirebaseRepository(uid: uid)
                    await self.reload()
                } else {
                    self.repository = nil
                    self.words = []
                }
            }
        }
    }

    func reload() async {
        guard let repository else { return }
        do {
            words = try await repository.words()
            statusMessage = words.isEmpty ? "No words yet — look one up to get started." : nil
        } catch {
            statusMessage = "Couldn't load words: \(error.localizedDescription)"
        }
    }

    /// Look up (and auto-save) a selected word/phrase, then refresh the list.
    @discardableResult
    func lookup(_ text: String) async throws -> WordEntry {
        let result = try await lookupService.lookup(text)
        await reload()
        return result.entry
    }

    func signOut() {
        try? Auth.auth().signOut()
    }
}
