import UIKit
import UniformTypeIdentifiers
import FirebaseCore
import VocabKit

/// iOS/iPadOS Share Extension: receives text the user selected in any app
/// (via the system Share sheet), looks it up, and auto-saves it.
///
/// Setup notes for the Xcode target:
///  - Add an **App Group** shared between the app and this extension, and enable
///    the **Keychain Sharing** access group so the signed-in Firebase user is
///    available here (Firebase Auth persists its token in the keychain).
///  - Bundle the same `GoogleService-Info.plist`.
final class ShareViewController: UIViewController {
    override func viewDidLoad() {
        super.viewDidLoad()
        if FirebaseApp.app() == nil { FirebaseApp.configure() }

        extractSelectedText { [weak self] text in
            guard let self else { return }
            guard let text, !text.isEmpty else { self.finish(); return }
            Task {
                _ = try? await FunctionsLookupService().lookup(text)
                self.finish()
            }
        }
    }

    private func extractSelectedText(_ completion: @escaping (String?) -> Void) {
        guard
            let item = extensionContext?.inputItems.first as? NSExtensionItem,
            let provider = item.attachments?.first
        else { completion(nil); return }

        let plainText = UTType.plainText.identifier
        guard provider.hasItemConformingToTypeIdentifier(plainText) else { completion(nil); return }

        provider.loadItem(forTypeIdentifier: plainText, options: nil) { data, _ in
            let text = (data as? String) ?? (data as? NSAttributedString)?.string
            completion(text?.trimmingCharacters(in: .whitespacesAndNewlines))
        }
    }

    private func finish() {
        extensionContext?.completeRequest(returningItems: nil)
    }
}
