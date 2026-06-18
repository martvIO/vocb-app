import SwiftUI
import FirebaseCore

/// Shared entry point for the iPad and macOS apps. The same SwiftUI scene runs
/// on both; platform differences are handled inline with `#if os(...)`.
@main
struct VocbApp: App {
    @StateObject private var model = AppModel()

    init() {
        FirebaseApp.configure()
    }

    var body: some Scene {
        WindowGroup {
            RootView()
                .environmentObject(model)
                .task { await ReviewReminders.requestAuthorization() }
        }
    }
}
