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
- **Phase 3 — iPad + macOS app:** SwiftUI screens (word list by lookupCount, detail w/ pronunciation, study modes, decks/tags, in-app reader, reminders). ⏳ Not started — **first milestone**.
- **Phase 4 — Capture:** iOS Share/Action extension; macOS menu-bar agent + Accessibility + hotkey + overlay (see [[Text Capture Strategy]]). ⏳ Not started.
- **Phase 5 — Windows ([[WinUI 3]]):** feature parity via Firebase REST + tray + hotkey + UI Automation + overlay. ⏳ Not started.
- **Phase 6 — Polish:** sync conflicts / offline queue (see [[Cloud Sync Architecture]]), stats dashboard, prompt-quality tuning, monthly spend guard.

## Immediate next steps
1. Backend: create the Firebase project, set the `ANTHROPIC_API_KEY` secret, run the emulator acceptance test, deploy.
2. Apple: on the cloud Mac, create the Xcode project, add VocabKit + Firebase, run `swift test`, then build Phase 3 UI.
3. Windows: scaffold the WinUI 3 solution (can be partially built on the dev PC with the .NET SDK).
