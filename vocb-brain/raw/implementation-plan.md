<!-- Source capture. Canonical: .claude/plans/the-goal-of-this-hashed-map.md — ingested 2026-06-17. Immutable; do not edit. -->

# vocb-app — Implementation Plan (source capture)

## Goal
A personal vocabulary-building app spanning **iPad, macOS, and Windows**. Core loop:
select a word anywhere → instant overlay with meaning → auto-saved, counted, and
enriched (definition, examples, synonyms/antonyms) via a dictionary API + Claude.
Study saved words with spaced repetition, flashcards, and quizzes; most-looked-up
words rise to the top and get prioritized for testing. Synced via Firebase.

## Decisions
- Platforms: iPad + macOS + Windows (all native). First milestone: iPad + macOS (shared SwiftUI). Windows second.
- Apple stack: one Xcode project, shared SwiftUI + a shared `VocabKit` Swift package, iOS + macOS targets.
- Windows stack: WinUI 3 / Windows App SDK (C# .NET), talking to Firebase via REST.
- Capture: Share/Action extension everywhere; richer desktop (tray + global hotkey + instant overlay on selection in any app) on macOS + Windows. iPad uses Share Sheet (iOS forbids true background capture).
- Content: Hybrid — Free Dictionary API (definitions/audio) + Claude (Haiku tier, swappable) for examples, related vocab, synonyms/antonyms.
- Language: English only.
- Select behavior: auto-save everything selected; overlay always shows. Cache by lemma — first lookup generates, repeats just increment the count.
- Count: times the user looked it up (increment per selection); drives sort + test priority.
- Study: SRS (SM-2) + flip cards + multiple-choice + type-the-word + fill-in-blank.
- Data/sync: Firebase (Auth + Firestore + Cloud Functions).
- Scope: personal use — single Claude API key, lightweight auth.
- Build env: Windows PC + cloud Mac for Apple builds.
- Extras in scope: pronunciation audio, daily review reminders, synonyms/antonyms, decks/tags.

## Architecture
Native clients (iPad/macOS SwiftUI, Windows WinUI 3, iOS Share/Action extensions) all talk to Firebase
(Auth + Firestore sync + Cloud Functions). The `lookupWord` Cloud Function proxies the Free Dictionary
API + Anthropic Claude. The Claude API key lives only in Cloud Functions, never in a client.

## Repository structure (monorepo)
- `backend/` — Firebase: functions/ (TS), firestore.rules, firestore.indexes.json
- `apple/` — Xcode workspace: App (iOS+macOS), VocabKit shared package, Share/Action extensions, macOS menu-bar agent
- `windows/` — WinUI 3 solution (.NET)
- `docs/` — setup, architecture notes

## Data model (Firestore), per user under users/{uid}/
- words/{lemma} — text, lemma, lookupCount, firstSeen, lastSeen, definitions/senses, examples, synonyms, antonyms, phonetic, audioUrl, deckIds, tags, generatedBy, schemaVersion. Doc id = normalized lemma so "running"/"run" map to one entry and just increment lookupCount.
- srs/{lemma} — SM-2 state: ease, interval, repetitions, dueDate, lastReviewed.
- decks/{deckId} — name, color, createdAt.

## Build phases
- Phase 0: prereqs (Firebase project on Blaze, Anthropic key, Apple Developer Program, cloud Mac, Node + Firebase CLI).
- Phase 1: backend — Firestore schema, Auth, `lookupWord` (dictionary + Claude), security rules.
- Phase 2: shared `VocabKit` (models, Firebase client, SRS engine, lookup client).
- Phase 3: iPad + macOS SwiftUI app.
- Phase 4: capture (iOS Share/Action extension; macOS menu-bar agent + Accessibility + hotkey + overlay).
- Phase 5: Windows app (WinUI 3) via Firebase REST + tray + hotkey + UI Automation selection + overlay.
- Phase 6: polish (sync conflicts, stats, prompt quality, cost guard).

## Constraints / open items
- iOS forbids true system-wide background capture → iPad relies on Share/Action extension; full overlays are macOS + Windows only.
- Apple Developer Program + cloud Mac required before the first milestone.
- Lemmatization edge cases: single-word lemmas; multi-word selections stored as phrase entries keyed by normalized text.
- Auto-save + Claude on every new word is mitigated by lemma caching; add a monthly spend guard in Phase 6.
