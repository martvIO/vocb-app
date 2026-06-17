---
tags: [data-model, backend, architecture]
sources: [backend-codebase.md, implementation-plan.md, apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# Word Entry Data Model

The shared schema for a saved vocabulary item, stored at `users/{uid}/words/{lemma}` in [[Firebase]] Firestore and
mirrored field-for-field by [[VocabKit]]'s Swift `WordEntry` (so the same document decodes on every client).

## Fields
- `lemma` — normalized key, also the document id (see [[Lemma-Keyed Caching]]).
- `text` — original surface form first looked up (e.g. "running").
- `kind` — `"word"` or `"phrase"`.
- `lookupCount` — times looked up; drives sorting and test priority (see [[Study and Testing System]]).
- `firstSeen` / `lastSeen` — epoch milliseconds.
- `phonetic`, `audioUrl` — from the [[Free Dictionary API]] (pronunciation).
- `learnerDefinition`, `senses[]` (partOfSpeech + meaning), `examples[]`, `synonyms[]`, `antonyms[]` — from [[Hybrid Content Generation]].
- `deckIds[]`, `tags[]` — organization.
- `generatedBy`, `schemaVersion` — provenance + migration.

## Related documents
- `srs/{lemma}` — [[SM-2 Spaced Repetition]] state.
- `decks/{deckId}` — `{name, color, createdAt}`.

`WordSense {partOfSpeech, meaning}` is shared. `schemaVersion` (currently 1) lets clients migrate older entries.
