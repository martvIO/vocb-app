import UIKit
import UniformTypeIdentifiers
import FirebaseCore
import VocabKit

/// iOS/iPadOS Action Extension: appears in the selection callout / Share sheet
/// as a "Look up in Vocb" action. Same mechanism as the Share Extension, but
/// returns control to the host app when done (actions don't compose content).
///
/// Requires the same App Group + Keychain Sharing + GoogleService-Info setup as
/// the Share Extension so the signed-in user is available.
final class ActionViewController: UIViewController {
    override func viewDidLoad() {
        super.viewDidLoad()
        if FirebaseApp.app() == nil { FirebaseApp.configure() }

        loadText { [weak self] text in
            guard let self else { return }
            guard let text, !text.isEmpty else { self.done(); return }
            Task {
                _ = try? await FunctionsLookupService().lookup(text)
                self.done()
            }
        }
    }

    private func loadText(_ completion: @escaping (String?) -> Void) {
        let plainText = UTType.plainText.identifier
        for item in extensionContext?.inputItems as? [NSExtensionItem] ?? [] {
            for provider in item.attachments ?? [] where provider.hasItemConformingToTypeIdentifier(plainText) {
                provider.loadItem(forTypeIdentifier: plainText, options: nil) { data, _ in
                    completion((data as? String)?.trimmingCharacters(in: .whitespacesAndNewlines))
                }
                return
            }
        }
        completion(nil)
    }

    private func done() {
        extensionContext?.completeRequest(returningItems: extensionContext?.inputItems)
    }
}
