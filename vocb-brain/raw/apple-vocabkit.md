<!-- Source capture. Canonical: apple/README.md + apple/VocabKit/Sources/VocabKit/*.swift — ingested 2026-06-17. Immutable; do not edit. -->

# Apple side & VocabKit (source capture)

Native Swift/SwiftUI. Plan: one Xcode project with shared SwiftUI views + two app targets (iOS + macOS),
Share/Action extensions, and a macOS menu-bar agent. Builds on a Mac with Xcode (or cloud Mac) — not on Windows.
VocabKit code was authored but NOT yet compiled (no Swift toolchain on Windows); treat as unverified until `swift test` on a Mac.

## VocabKit (pure-Swift core, no Firebase dependency)
- **Package.swift** — library target + test target; platforms iOS 16 / macOS 13.
- **Models.swift** — `WordSense`, `WordEntry` (field-for-field match with the backend WordEntry; timestamps as
  Int64 epoch ms with Date accessors; Codable/Identifiable by lemma), `Deck`.
- **SRSEngine.swift** — SM-2 scheduler. `SRSState {repetitions, easeFactor, intervalDays, dueDate, lastReviewed}`;
  `ReviewGrade {again=1, hard=3, good=4, easy=5}`; `SRSEngine.schedule(state, grade, now) -> SRSState` (pure);
  `isDue`. Grade <3 resets reps + interval to 1; else interval 1→6→round(interval*EF); EF updated every review, clamped ≥1.3.
- **Services.swift** — `LookupResult {entry, created}`; protocols `LookupServicing` (calls backend lookupWord)
  and `VocabRepository` (Firestore reads/writes) implemented by the apps; `StudyQueue.build(words, states, now, limit)`
  orders a review session: due cards first (oldest due first), then never-reviewed words by lookupCount desc.
- **Tests** — SRSEngineTests (interval progression 1/6/15, ease raise/lower, lapse reset, EF clamp, isDue) and
  StudyQueueTests (due-before-fresh, exclude not-due, due sorted by date, limit truncation).

## Next steps (need the Mac)
Create Xcode project; add VocabKit + Firebase via SPM; implement LookupServicing (call lookupWord) and
VocabRepository (Firestore); build SwiftUI screens (word list sorted by lookupCount, detail w/ pronunciation,
study modes via SRSEngine, decks/tags, in-app reader, reminders); add Share/Action extensions + macOS menu-bar agent.
