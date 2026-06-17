---
tags: [data-model, backend, decision]
sources: [backend-codebase.md, backend-setup-runbook.md, implementation-plan.md]
created: 2026-06-17
updated: 2026-06-17
---

# Lemma-Keyed Caching

The mechanism that makes "auto-save everything" cheap and keeps one entry per word across devices.

## How it works
- Clients send **raw selected text**; the backend (`lemma.ts`) is the single source of truth for normalization.
- A single word is cleaned (punctuation/whitespace) and reduced to a base form via [[wink-lemmatizer]] (noun→verb→adjective chain). A multi-word selection becomes a `phrase` keyed lowercase with spaces→underscores.
- The resulting key is the Firestore document id under `users/{uid}/words/`.

## Why it matters
- **Cost control:** the [[lookupWord Function]] calls [[Anthropic Claude]] only on the *first* lookup of a lemma. Repeat lookups of "running", "ran", or "runs" all resolve to `run` and just `increment` `lookupCount` — no AI call.
- **Consistent counts + sync:** because normalization is server-side, iPad, macOS, and Windows produce the same key, so the [[Cloud Sync Architecture]] merges them into one entry and one count.

## Verified behavior
running/ran/runs → `run`; "Cats!" → `cat`; better → `good`; studies → `study`; "a quick test" → phrase. Tradeoff: the
chain over-reduces some nouns (e.g. "meeting" → "meet"), acceptable for a personal key. Feeds `lookupCount` in the
[[Word Entry Data Model]].
