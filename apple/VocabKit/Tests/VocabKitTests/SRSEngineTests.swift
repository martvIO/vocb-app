import XCTest
@testable import VocabKit

final class SRSEngineTests: XCTestCase {
    let now = Date(timeIntervalSince1970: 1_700_000_000)

    private func daysBetween(_ state: SRSState, from: Date) -> Double {
        (state.dueDateValue.timeIntervalSince1970 - from.timeIntervalSince1970)
            / SRSEngine.secondsPerDay
    }

    func testFirstGoodReviewSchedulesOneDay() {
        let s = SRSEngine.schedule(.new(now: now), grade: .good, now: now)
        XCTAssertEqual(s.repetitions, 1)
        XCTAssertEqual(s.intervalDays, 1)
        XCTAssertEqual(s.easeFactor, 2.5, accuracy: 0.0001) // "good" leaves EF unchanged
        XCTAssertEqual(daysBetween(s, from: now), 1, accuracy: 0.01)
    }

    func testSecondGoodReviewSchedulesSixDays() {
        var s = SRSEngine.schedule(.new(now: now), grade: .good, now: now)
        s = SRSEngine.schedule(s, grade: .good, now: now)
        XCTAssertEqual(s.repetitions, 2)
        XCTAssertEqual(s.intervalDays, 6)
    }

    func testThirdGoodReviewUsesEaseFactor() {
        var s = SRSEngine.schedule(.new(now: now), grade: .good, now: now)
        s = SRSEngine.schedule(s, grade: .good, now: now)
        s = SRSEngine.schedule(s, grade: .good, now: now)
        XCTAssertEqual(s.repetitions, 3)
        XCTAssertEqual(s.intervalDays, 15) // round(6 * 2.5)
    }

    func testEasyRaisesEaseFactor() {
        let s = SRSEngine.schedule(.new(now: now), grade: .easy, now: now)
        XCTAssertEqual(s.easeFactor, 2.6, accuracy: 0.0001)
    }

    func testHardLowersEaseFactor() {
        let s = SRSEngine.schedule(.new(now: now), grade: .hard, now: now)
        XCTAssertEqual(s.easeFactor, 2.36, accuracy: 0.0001)
    }

    func testAgainResetsRepetitionsAndInterval() {
        var s = SRSEngine.schedule(.new(now: now), grade: .good, now: now)
        s = SRSEngine.schedule(s, grade: .good, now: now) // interval 6, reps 2
        s = SRSEngine.schedule(s, grade: .again, now: now)
        XCTAssertEqual(s.repetitions, 0)
        XCTAssertEqual(s.intervalDays, 1)
    }

    func testEaseFactorClampedAtMinimum() {
        var s = SRSState.new(now: now)
        for _ in 0..<10 {
            s = SRSEngine.schedule(s, grade: .again, now: now)
        }
        XCTAssertGreaterThanOrEqual(s.easeFactor, SRSEngine.minimumEase)
        XCTAssertEqual(s.easeFactor, SRSEngine.minimumEase, accuracy: 0.0001)
    }

    func testIsDue() {
        let past = SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(-3600)))
        let future = SRSState(dueDate: SRSEngine.millis(now.addingTimeInterval(3600)))
        XCTAssertTrue(SRSEngine.isDue(past, now: now))
        XCTAssertFalse(SRSEngine.isDue(future, now: now))
    }
}
