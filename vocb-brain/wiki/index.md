# Index

Master catalog of all wiki pages. Updated on every ingest.

## Sources

- [[Implementation Plan]] — locked-in decisions, architecture, data model, and build phases for vocb-app.
- [[Backend Setup Runbook]] — prereqs, secret, emulator test, deploy, and client integration for the backend.
- [[Backend Codebase]] — the TypeScript Cloud Functions modules and Firestore config.
- [[Apple Side & VocabKit]] — the Apple plan and the pure-Swift VocabKit core package.

## Entities

- [[Firebase]] — Auth + Firestore + Cloud Functions; the backend platform and sync layer.
- [[Anthropic Claude]] — LLM (default Haiku) generating definitions, examples, synonyms/antonyms.
- [[Free Dictionary API]] — free, key-less dictionary for phonetics, audio, and base senses.
- [[lookupWord Function]] — the single HTTPS callable: normalize → cache → (miss) generate → store.
- [[VocabKit]] — shared, Firebase-free Swift core (models, SRS engine, service protocols).
- [[WinUI 3]] — the native Windows client framework (talks to Firebase via REST).
- [[wink-lemmatizer]] — JS lemmatizer used to derive the canonical word key.

## Concepts

- [[Word Entry Data Model]] — the shared WordEntry schema stored in Firestore and mirrored in Swift.
- [[Lemma-Keyed Caching]] — server-side normalization → one entry per word, one Claude call per new lemma.
- [[SM-2 Spaced Repetition]] — the review scheduling algorithm in VocabKit's SRSEngine.
- [[Study and Testing System]] — SRS + flashcards + quizzes; queue prioritized by lookupCount.
- [[Hybrid Content Generation]] — dictionary (structured) + Claude (creative) merged into each entry.
- [[Text Capture Strategy]] — per-OS capture: iOS Share/Action; macOS + Windows tray + hotkey + overlay.
- [[Cloud Sync Architecture]] — per-user Firestore data flow and cross-device convergence.

## Synthesis

- [[Architecture Overview]] — how all the pieces fit and the core select→save→study loop.
- [[Build Roadmap]] — phased plan with current status (backend done; clients pending).
