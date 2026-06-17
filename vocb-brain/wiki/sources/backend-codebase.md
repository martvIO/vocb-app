---
tags: [backend, claude-api, data-model]
sources: [backend-codebase.md]
created: 2026-06-17
updated: 2026-06-17
---

# Backend Codebase

Source summary of the TypeScript Cloud Functions backend.

## Source metadata
- Canonical: `backend/functions/src/*.ts`, `backend/firestore.rules`, `backend/firestore.indexes.json`, `backend/functions/test/*.test.ts`.
- Status: compiles (`tsc` passes); **19 Vitest unit tests passing** (lemma, dictionary, claude, entry).
- `entry.ts` holds the pure `buildEntry()` merge helper (extracted from `lookupWord.ts` for testability).

## Key claims
- `types.ts` defines the canonical [[Word Entry Data Model]] (`WordEntry`, `WordSense`, `Deck`, DTOs, `SCHEMA_VERSION`).
- `lemma.ts` is the single source of truth for normalization (see [[Lemma-Keyed Caching]]); uses [[wink-lemmatizer]].
- `dictionary.ts` wraps the [[Free Dictionary API]]; `claude.ts` calls [[Anthropic Claude]] with structured JSON output (see [[Hybrid Content Generation]]).
- `lookupWord.ts` is the [[lookupWord Function]]: fast cache path (increment, no AI) vs slow path (dictionary + Claude + race-safe transaction).
- `firestore.rules` enforce per-user isolation; word docs are written by the function (Admin SDK) and read by clients.

## Summary
Six modules implement the lookup pipeline: normalize → cache check → (on miss) dictionary fetch + Claude generation →
store. Input is bounded (≤200 chars), `maxInstances` is 5, and the entry schema mirrors [[VocabKit]]'s Swift models
field-for-field so the same Firestore documents decode on every client.
