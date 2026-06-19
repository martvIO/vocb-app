import UIKit
import SwiftUI
import UniformTypeIdentifiers
import FirebaseCore
import VocabKit

/// iOS/iPadOS Share Extension: receives text the user selected in any app (via the
/// system Share sheet), looks it up, and shows the result in a popup card (with a
/// speak button + X). Closing the card finishes the share request.
///
/// Setup notes for the Xcode target:
///  - Add an **App Group** shared between the app and this extension, and enable the
///    **Keychain Sharing** access group so the signed-in Firebase user is available
///    here (Firebase Auth persists its token in the keychain).
///  - Bundle the same `GoogleService-Info.plist`.
final class ShareViewController: UIViewController {
    override func viewDidLoad() {
        super.viewDidLoad()
        if FirebaseApp.app() == nil { FirebaseApp.configure() }

        view.backgroundColor = UIColor.black.withAlphaComponent(0.25)

        extractSelectedText { [weak self] text in
            guard let self else { return }
            guard let text, !text.isEmpty else { self.finish(); return }
            Task {
                do {
                    let result = try await FunctionsLookupService().lookup(text)
                    await MainActor.run { self.showCard(for: result.entry) }
                } catch {
                    await MainActor.run { self.showError(error) }
                }
            }
        }
    }

    private func showCard(for entry: WordEntry) {
        let card = WordPopupCard(entry: entry) { [weak self] in self?.finish() }
        let host = UIHostingController(rootView: card)
        host.view.backgroundColor = .clear
        addChild(host)
        host.view.translatesAutoresizingMaskIntoConstraints = false
        view.addSubview(host.view)
        NSLayoutConstraint.activate([
            host.view.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            host.view.centerYAnchor.constraint(equalTo: view.centerYAnchor),
            host.view.leadingAnchor.constraint(greaterThanOrEqualTo: view.leadingAnchor, constant: 16),
            host.view.trailingAnchor.constraint(lessThanOrEqualTo: view.trailingAnchor, constant: -16),
        ])
        host.didMove(toParent: self)
    }

    private func showError(_ error: Error) {
        let alert = UIAlertController(title: "Lookup failed",
                                      message: error.localizedDescription, preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "OK", style: .default) { [weak self] _ in self?.finish() })
        present(alert, animated: true)
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
