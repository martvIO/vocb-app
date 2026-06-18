---
tags: [architecture, decision]
sources: [implementation-plan.md, backend-setup-runbook.md, apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# Build Roadmap

Phased plan and current progress (status as of 2026-06-17).

## Phases
- **Phase 0 — Prereqs:** Firebase project on Blaze, Anthropic key, Apple Developer Program, cloud Mac, Node + Firebase CLI. *(Node 24 installed on the dev PC; cloud Mac + Firebase project still pending.)*
- **Phase 1 — Backend:** Firestore schema/rules, the [[lookupWord Function]] (dictionary + Claude). ✅ **Done & verified** — compiles; **Vitest unit suite of 19 tests passing** across `lemma`, `dictionary` (mocked fetch), `claude` (mocked SDK), and `entry` (the extracted pure `buildEntry` merge logic). Not yet emulator-tested end-to-end (needs a live project + key).
- **Phase 2 — [[VocabKit]]:** models, [[SM-2 Spaced Repetition]] engine, service protocols, tests. ✅ **Authored**, ⏳ unverified (needs `swift test` on the Mac).
- **Phase 3 — iPad + macOS app:** SwiftUI app — `VocbApp`, `AppModel`, `FirebaseRepository`, `FunctionsLookupService`, and views (word list by lookupCount, detail w/ pronunciation, SM-2 study, in-app reader, sign-in, settings/reminders). ✅ **Authored**, ⏳ unverified (needs Xcode on the Mac).
- **Phase 4 — Capture:** iOS Share + Action extensions; macOS menu-bar agent (`GlobalHotkey` Carbon, `AccessibilitySelection` AX API, `OverlayController` NSPanel) — see [[Text Capture Strategy]]. ✅ **Authored**, ⏳ unverified.
- **Phase 5 — Windows ([[WinUI 3]]):** `Vocb.Core` (✅ **11 xUnit tests pass**), `Vocb.Firebase` REST client (✅ **compiles clean**), `Vocb.App` WinUI 3 (tray/hotkey/clipboard capture/overlay/screens — ✅ authored, needs the Windows App SDK to build).
- **Phase 6 — Polish:** ✅ **monthly spend guard** implemented in the [[lookupWord Function]] (`usage.ts`, cap via `VOCAB_MONTHLY_CAP`, unit-tested) + sync-conflict notes (`docs/sync-notes.md`). Remaining: offline queue, stats dashboard, prompt-quality tuning (see [[Cloud Sync Architecture]]).

## Verification status (2026-06-17)
- **Verified on this Windows PC:** backend (23 Vitest tests + `tsc`), Windows `Vocb.Core` (11 xUnit tests), `Vocb.Firebase` (builds).
- **Authored, unverified (need a Mac / Windows App SDK / live Firebase):** all Apple code, the WinUI `Vocb.App`, and the end-to-end `lookupWord` emulator path.

## Immediate next steps
1. Backend: create the Firebase project, set the `ANTHROPIC_API_KEY` secret, run the emulator acceptance test, deploy.
2. Apple: on the cloud Mac, assemble the Xcode project (App + extensions + agent + VocabKit + Firebase), run `swift test`, build & run.
3. Windows: install the Windows App SDK and build `Vocb.App`; configure the project in Settings and test the ⌃⇧L capture flow.
