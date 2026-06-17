# Log

Chronological record of all operations.

## [2026-06-17] setup | Vault initialized
Created vault "vocb-brain" for the vocb-app project knowledge base (cross-platform vocabulary app — architecture, Firebase + Claude backend, Apple/Windows clients, data model, decisions).
Agent configs: CLAUDE.md.

## [2026-06-17] ingest | Initial project seed
Ingested 4 sources (implementation plan, backend setup runbook, backend codebase, Apple/VocabKit) into raw/.
Created 4 source pages, 7 entity pages (Firebase, Anthropic Claude, Free Dictionary API, lookupWord Function, VocabKit, WinUI 3, wink-lemmatizer), 7 concept pages (Word Entry Data Model, Lemma-Keyed Caching, SM-2 Spaced Repetition, Study and Testing System, Hybrid Content Generation, Text Capture Strategy, Cloud Sync Architecture), and 2 synthesis pages (Architecture Overview, Build Roadmap). Index updated.

## [2026-06-17] update | Backend test suite
Added a Vitest unit suite to the backend (19 tests across lemma, dictionary, claude, entry); extracted pure `buildEntry()` into `entry.ts`; hardened the Claude parser to drop null array items. Updated [[Build Roadmap]] and [[Backend Codebase]].

