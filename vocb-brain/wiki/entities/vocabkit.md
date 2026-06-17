---
tags: [apple-swiftui, spaced-repetition, data-model]
sources: [apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# VocabKit

The pure-Swift core package shared by the iPad and macOS apps (and their extensions). Deliberately **Firebase-free**
so it compiles fast and is fully unit-testable; the apps wire concrete Firebase implementations to its protocols.

## Contents
- **Models.swift** — `WordSense`, `WordEntry`, `Deck`. Field-for-field match with the backend [[Word Entry Data Model]] so Firestore docs decode directly (timestamps as Int64 epoch ms).
- **SRSEngine.swift** — the [[SM-2 Spaced Repetition]] scheduler (`SRSState`, `ReviewGrade`, `schedule`, `isDue`).
- **Services.swift** — `LookupServicing` (calls the [[lookupWord Function]]) and `VocabRepository` ([[Firebase]] reads/writes) protocols, plus `StudyQueue` ordering (see [[Study and Testing System]]).
- **Tests** — SRSEngine + StudyQueue unit tests.

Platforms iOS 16 / macOS 13. **Status:** authored but not yet compiled (no Swift toolchain on Windows) — verify with
`swift test` on the cloud Mac. Foundation for the Apple [[Build Roadmap]] milestone.
