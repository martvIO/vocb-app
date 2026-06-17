---
tags: [spaced-repetition, apple-swiftui]
sources: [apple-vocabkit.md, implementation-plan.md]
created: 2026-06-17
updated: 2026-06-17
---

# SM-2 Spaced Repetition

The scheduling algorithm behind the review system, implemented as a pure function in [[VocabKit]]'s `SRSEngine.swift`
and stored per word at `users/{uid}/srs/{lemma}`.

## State (`SRSState`)
`repetitions`, `easeFactor` (≥ 1.3, default 2.5), `intervalDays`, `dueDate`, `lastReviewed`.

## Scheduling (`schedule(state, grade, now)`)
- `ReviewGrade`: `again=1`, `hard=3`, `good=4`, `easy=5` (SM-2 quality 0–5).
- Grade < 3 (a lapse) → reset `repetitions` to 0 and `interval` to 1 day.
- Otherwise interval progresses `1 → 6 → round(interval × easeFactor)` and `repetitions` increments.
- Ease factor is updated on every review (`good` leaves it ~unchanged, `easy` raises, `hard` lowers) and clamped at 1.3.
- `dueDate = now + intervalDays`.

## Verified by tests
First good → interval 1; second → 6; third → 15; `easy` raises EF to 2.6, `hard` lowers to 2.36; repeated `again`
clamps EF at 1.3; `isDue` compares `dueDate` to now. Consumed by the [[Study and Testing System]] via `StudyQueue`.
