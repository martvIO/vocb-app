---
tags: [architecture, backend, firebase, decision]
sources: [implementation-plan.md, backend-codebase.md, backend-setup-runbook.md, apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# Architecture Overview

A cross-cutting picture of vocb-app, tying the pieces together.

## Shape
Three native clients share one cloud backend:
- **Clients:** iPad + macOS (SwiftUI on a shared [[VocabKit]] core) and Windows ([[WinUI 3]]), plus iOS Share/Action extensions and a macOS menu-bar agent.
- **Backend:** [[Firebase]] — Auth + Firestore (sync) + Cloud Functions hosting the [[lookupWord Function]].
- **External:** the function proxies the [[Free Dictionary API]] and [[Anthropic Claude]]; the Claude key stays server-side.

## The core loop
1. User selects text (see [[Text Capture Strategy]]) → client sends **raw text** to the [[lookupWord Function]].
2. Server normalizes to a lemma ([[Lemma-Keyed Caching]]); on a miss it runs [[Hybrid Content Generation]] and stores a [[Word Entry Data Model|WordEntry]] with `lookupCount` 1; on a hit it just increments the count.
3. The entry shows in an overlay and syncs to all devices ([[Cloud Sync Architecture]]).
4. Saved words feed the [[Study and Testing System]] (driven by [[SM-2 Spaced Repetition]]), prioritized by `lookupCount`.

## Key design choices
- **Server-side normalization** gives one canonical key (consistent counts + sync) and one Claude call per new lemma (cost control).
- **Schema parity** between the backend `WordEntry` and VocabKit's Swift models means Firestore docs decode unchanged on every client.
- **Platform-specific capture** is the riskiest surface: iOS is Share-Sheet-only; macOS/Windows get tray + hotkey + overlay.

## Current status (2026-06-17)
All phases now have code. **Verified on the dev PC:** the backend (23 Vitest tests + spend guard) and the Windows
`Vocb.Core` (11 xUnit tests) + `Vocb.Firebase` (compiles). **Authored but unverified** (need a Mac / Windows App SDK /
live Firebase): the full Apple app + extensions + macOS agent, the WinUI `Vocb.App`, and the end-to-end `lookupWord`
emulator path. See [[Build Roadmap]] for the per-phase breakdown.
