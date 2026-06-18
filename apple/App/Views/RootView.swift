import SwiftUI

/// Gates the app on auth: signed-out shows sign-in, signed-in shows the tabs.
struct RootView: View {
    @EnvironmentObject private var model: AppModel

    var body: some View {
        if model.isSignedIn {
            TabView {
                NavigationStack { WordListView() }
                    .tabItem { Label("Words", systemImage: "list.bullet") }

                NavigationStack { StudyView() }
                    .tabItem { Label("Study", systemImage: "rectangle.on.rectangle") }

                NavigationStack { ReaderView() }
                    .tabItem { Label("Reader", systemImage: "doc.text") }

                NavigationStack { SettingsView() }
                    .tabItem { Label("Settings", systemImage: "gear") }
            }
        } else {
            SignInView()
        }
    }
}

/// Minimal settings: review-reminder time + sign out.
struct SettingsView: View {
    @EnvironmentObject private var model: AppModel
    @State private var reminderTime = Calendar.current.date(
        bySettingHour: 19, minute: 0, second: 0, of: Date()) ?? Date()

    var body: some View {
        Form {
            Section("Daily review reminder") {
                DatePicker("Time", selection: $reminderTime, displayedComponents: .hourAndMinute)
                Button("Schedule reminder") {
                    let c = Calendar.current.dateComponents([.hour, .minute], from: reminderTime)
                    ReviewReminders.scheduleDaily(hour: c.hour ?? 19, minute: c.minute ?? 0)
                }
            }
            Section {
                Button("Sign out", role: .destructive) { model.signOut() }
            }
        }
        .navigationTitle("Settings")
    }
}
