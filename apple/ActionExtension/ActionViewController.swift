import UIKit
import SwiftUI
import UniformTypeIdentifiers
import FirebaseCore
import VocabKit

/// iOS/iPadOS Action Extension: appears in the selection callout / Share sheet as a
/// "Look up in Vocb" action. Looks the selection up, then shows the result in a popup
/// card (with a speak button + X) right in place. Closing the card finishes the action.
///
/// Requires the same App Group + Keychain Sharing + GoogleService-Info setup as the
/// Share Extension so the signed-in user is available.
final class ActionViewController: UIViewController {
    override func viewDidLoad() {
        super.viewDidLoad()
        if FirebaseApp.app() == nil { FirebaseApp.configure() }

        // Dim the host so the card reads as a focused popup.
        view.backgroundColor = UIColor.black.withAlphaComponent(0.25)

        loadText { [weak self] text in
            guard let self else { return }
            guard let text, !text.isEmpty else { self.done(); return }
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
        let card = WordPopupCard(entry: entry) { [weak self] in self?.done() }
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
        alert.addAction(UIAlertAction(title: "OK", style: .default) { [weak self] _ in self?.done() })
        present(alert, animated: true)
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
