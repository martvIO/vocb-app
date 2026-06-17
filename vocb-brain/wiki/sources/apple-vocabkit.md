---
tags: [apple-swiftui, spaced-repetition, data-model]
sources: [apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# Apple Side & VocabKit

Source summary of the Apple plan and the VocabKit Swift package.

## Source metadata
- Canonical: `apple/README.md`, `apple/VocabKit/Sources/VocabKit/*.swift`, tests under `apple/VocabKit/Tests/`.
- Status: authored, **not yet compiled** (no Swift toolchain on Windows) — unverified until `swift test` on a Mac.

## Key claims
- [[VocabKit]] is the pure-Swift, Firebase-free core shared by the iPad and macOS apps (and extensions).
- `Models.swift` mirrors the backend [[Word Entry Data Model]] field-for-field so Firestore docs decode directly.
- `SRSEngine.swift` implements [[SM-2 Spaced Repetition]] (pure, deterministic, unit-tested).
- `Services.swift` defines `LookupServicing` + `VocabRepository` protocols (apps implement over [[Firebase]]) and `StudyQueue` ordering (see [[Study and Testing System]]).
- The Apple app is the **first milestone**; remaining UI/extensions/menu-bar agent need the cloud Mac.

## Summary
VocabKit isolates models, the SRS algorithm, and service protocols from any cloud dependency so it compiles fast and
is fully testable. The apps will add Firebase via SPM and implement the protocols. Remaining work: SwiftUI screens,
Share/Action extensions, and the macOS menu-bar agent for [[Text Capture Strategy]].
