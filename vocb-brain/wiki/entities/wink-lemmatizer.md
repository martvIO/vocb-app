---
tags: [backend, data-model]
sources: [backend-codebase.md]
created: 2026-06-17
updated: 2026-06-17
---

# wink-lemmatizer

A small, pure-JavaScript English lemmatizer used by the backend's `lemma.ts` to reduce single words to their base
form — the heart of [[Lemma-Keyed Caching]].

- Exposes POS-specific functions: `noun()`, `verb()`, `adjective()`. The backend chains **noun → verb → adjective**, since each returns its input unchanged when no rule applies.
- Verified results: running/ran/runs → "run", "Cats!" → "cat" (after punctuation stripping), better → "good", studies → "study".
- Biased toward over-reducing (e.g. the noun "meeting" → "meet") — an acceptable tradeoff for a personal vocab key.
- Ships no TypeScript types; the project declares a local `.d.ts` for `noun`/`verb`/`adjective`.

The Apple/[[VocabKit]] side normalizes locally too, but the backend is the single source of truth for the key, so
clients send raw text and let the [[lookupWord Function]] decide the lemma.
