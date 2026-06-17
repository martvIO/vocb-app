---
tags: [backend, claude-api]
sources: [implementation-plan.md, backend-codebase.md]
created: 2026-06-17
updated: 2026-06-17
---

# Free Dictionary API

A free, key-less dictionary service (dictionaryapi.dev) providing the structured half of [[Hybrid Content Generation]].

- Wrapped by `dictionary.ts` (`lookupDictionary(word)`); endpoint `https://api.dictionaryapi.dev/api/v2/entries/en/{word}`.
- Returns `{phonetic, audioUrl, senses, synonyms, antonyms}`. Provides pronunciation audio URLs (used for the app's pronunciation feature).
- Returns `null` on 404 (word not found — common for phrases/rare words) or error, so [[Anthropic Claude]] can still generate an entry on its own.
- For single words, the [[lookupWord Function]] queries it by the **lemma** (base form), so "running" looks up "run".

Its output is merged with Claude's: the entry prefers Claude's lists/definition and falls back to the dictionary's.
