import Foundation

/// Per-card spaced-repetition state, stored at users/{uid}/srs/{lemma}.
/// Implements the classic SM-2 variables (repetitions, ease factor, interval).
public struct SRSState: Codable, Hashable, Sendable {
    /// Number of consecutive successful recalls.
    public var repetitions: Int
    /// SM-2 ease factor (>= 1.3). Higher = easier = longer intervals.
    public var easeFactor: Double
    /// Current interval in whole days.
    public var intervalDays: Int
    /// When this card is next due (epoch milliseconds).
    public var dueDate: Int64
    /// When it was last reviewed (epoch milliseconds), or nil if never.
    public var lastReviewed: Int64?

    public init(
        repetitions: Int = 0,
        easeFactor: Double = 2.5,
        intervalDays: Int = 0,
        dueDate: Int64,
        lastReviewed: Int64? = nil
    ) {
        self.repetitions = repetitions
        self.easeFactor = easeFactor
        self.intervalDays = intervalDays
        self.dueDate = dueDate
        self.lastReviewed = lastReviewed
    }

    /// A brand-new card, due immediately at `now`.
    public static func new(now: Date = Date()) -> SRSState {
        SRSState(dueDate: SRSEngine.millis(now))
    }

    public var dueDateValue: Date { Date(timeIntervalSince1970: Double(dueDate) / 1000) }
}

/// How well the user recalled a card. Maps to an SM-2 quality grade (0–5).
/// "again" is a lapse (< 3) and resets the card; the rest advance it.
public enum ReviewGrade: Int, CaseIterable, Sendable {
    case again = 1
    case hard = 3
    case good = 4
    case easy = 5
}

/// Pure SM-2 scheduler. No I/O — `schedule` takes the old state + a grade and
/// returns the new state, so it's trivially unit-testable and deterministic.
public enum SRSEngine {
    static let minimumEase = 1.3
    static let secondsPerDay: TimeInterval = 86_400

    static func millis(_ date: Date) -> Int64 {
        Int64((date.timeIntervalSince1970 * 1000).rounded())
    }

    /// Apply a review grade to a card, computing its next interval and due date.
    /// - Parameters:
    ///   - state: the card's current SRS state.
    ///   - grade: how well the user recalled it.
    ///   - now: review time (injectable for tests).
    public static func schedule(
        _ state: SRSState,
        grade: ReviewGrade,
        now: Date = Date()
    ) -> SRSState {
        let q = Double(grade.rawValue)
        var repetitions = state.repetitions
        var interval = state.intervalDays

        if grade.rawValue < 3 {
            // Lapse: reset the streak and review again tomorrow.
            repetitions = 0
            interval = 1
        } else {
            switch repetitions {
            case 0: interval = 1
            case 1: interval = 6
            default: interval = Int((Double(interval) * state.easeFactor).rounded())
            }
            repetitions += 1
        }
        interval = max(1, interval)

        // SM-2 ease-factor update (applied on every review), clamped at 1.3.
        var ease = state.easeFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
        ease = max(minimumEase, ease)

        let due = now.addingTimeInterval(Double(interval) * secondsPerDay)

        return SRSState(
            repetitions: repetitions,
            easeFactor: ease,
            intervalDays: interval,
            dueDate: millis(due),
            lastReviewed: millis(now)
        )
    }

    /// True if the card is due for review at `now`.
    public static func isDue(_ state: SRSState, now: Date = Date()) -> Bool {
        state.dueDate <= millis(now)
    }
}
