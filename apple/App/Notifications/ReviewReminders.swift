import UserNotifications

/// Daily spaced-repetition review reminders via local notifications.
enum ReviewReminders {
    private static let identifier = "daily-review"

    static func requestAuthorization() async {
        _ = try? await UNUserNotificationCenter.current()
            .requestAuthorization(options: [.alert, .sound, .badge])
    }

    static func scheduleDaily(hour: Int, minute: Int) {
        let center = UNUserNotificationCenter.current()
        center.removePendingNotificationRequests(withIdentifiers: [identifier])

        let content = UNMutableNotificationContent()
        content.title = "Time to review"
        content.body = "Your vocabulary is waiting — a few cards keeps it fresh."
        content.sound = .default

        var components = DateComponents()
        components.hour = hour
        components.minute = minute
        let trigger = UNCalendarNotificationTrigger(dateMatching: components, repeats: true)

        center.add(UNNotificationRequest(identifier: identifier, content: content, trigger: trigger))
    }
}
