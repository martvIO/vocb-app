# Apple side (iPad + macOS)

Native Swift/SwiftUI. The plan is one Xcode project with shared SwiftUI views and
two app targets (iOS + macOS), plus Share/Action extensions and a macOS menu-bar
agent. All of that builds on a **Mac with Xcode** (or a cloud Mac) — it cannot be
compiled on Windows.

## What's here now: `VocabKit`

[VocabKit](VocabKit) is the pure-Swift core the apps and extensions share. It has
**no Firebase dependency**, so it compiles fast and is fully unit-tested:

- [Models.swift](VocabKit/Sources/VocabKit/Models.swift) — `WordEntry`, `WordSense`,
  `Deck`. Field-for-field match with the backend `WordEntry`
  ([backend/functions/src/types.ts](../backend/functions/src/types.ts)) so Firestore
  documents decode directly.
- [SRSEngine.swift](VocabKit/Sources/VocabKit/SRSEngine.swift) — the SM-2 spaced-
  repetition scheduler (`SRSState`, `ReviewGrade`, `SRSEngine.schedule`). Pure and
  deterministic.
- [Services.swift](VocabKit/Sources/VocabKit/Services.swift) — `LookupServicing` and
  `VocabRepository` protocols (the apps implement these over Firebase), plus
  `StudyQueue` which orders a review session (due cards first, then most-looked-up).
- Tests in [Tests/VocabKitTests](VocabKit/Tests/VocabKitTests).

### Build & test (on a Mac)

```sh
cd apple/VocabKit
swift build
swift test
```

## Next steps (need the Mac)

1. Create the Xcode project; add VocabKit as a local Swift package.
2. Add Firebase via SPM (`FirebaseAuth`, `FirebaseFirestore`, `FirebaseFunctions`).
3. Implement the protocols:
   - `LookupServicing` → call the `lookupWord` callable (see
     [docs/backend-setup.md](../docs/backend-setup.md) §6).
   - `VocabRepository` → Firestore reads/writes under `users/{uid}/…`.
4. Build the SwiftUI screens (word list sorted by `lookupCount`, word detail with
   pronunciation, study modes driven by `SRSEngine`, decks/tags, in-app reader,
   daily review reminders).
5. Add the iOS Share/Action extensions and the macOS menu-bar agent
   (global hotkey + Accessibility selection + overlay).

See the full plan at `.claude/plans/the-goal-of-this-hashed-map.md`.
