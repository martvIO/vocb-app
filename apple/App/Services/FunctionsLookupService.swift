import Foundation
import FirebaseFunctions
import VocabKit

/// Concrete `LookupServicing` backed by the `lookupWord` Cloud Function.
/// Sends raw selected text; the backend normalizes and returns the entry.
public struct FunctionsLookupService: LookupServicing {
    private let functions: Functions

    public init(functions: Functions = Functions.functions()) {
        self.functions = functions
    }

    public func lookup(_ text: String) async throws -> LookupResult {
        let response = try await functions.httpsCallable("lookupWord").call(["text": text])
        // The callable returns a plain JSON object; round-trip it through Codable.
        let data = try JSONSerialization.data(withJSONObject: response.data)
        let decoded = try JSONDecoder().decode(Wire.self, from: data)
        return LookupResult(entry: decoded.entry, created: decoded.created)
    }

    private struct Wire: Decodable {
        let entry: WordEntry
        let created: Bool
    }
}
