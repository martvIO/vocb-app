---
tags: [backend, firebase, claude-api]
sources: [backend-codebase.md, backend-setup-runbook.md]
created: 2026-06-17
updated: 2026-06-17
---

# lookupWord Function

The single backend Cloud Function — an HTTPS **callable** (`onCall`, Firebase Functions v2) and the only server
endpoint clients call. Source: `backend/functions/src/lookupWord.ts`.

## Behavior
1. Require auth (`request.auth.uid`); reject empty/oversized input (≤200 chars, ≤12 phrase words).
2. Normalize raw text → canonical key (see [[Lemma-Keyed Caching]]).
3. **Fast path:** if `users/{uid}/words/{key}` exists → `FieldValue.increment(1)` on `lookupCount` + update `lastSeen`, return `{entry, created:false}`. No AI call.
4. **Slow path:** fetch [[Free Dictionary API]] (by lemma for words) + call [[Anthropic Claude]], assemble a [[Word Entry Data Model|WordEntry]], commit in a race-safe transaction, return `{entry, created:true}`.

## Notes
- Runs with the `ANTHROPIC_API_KEY` secret bound; `maxInstances` 5; region `us-central1`.
- Clients send **raw selected text** — all normalization is server-side, keeping one canonical key across iPad/macOS/Windows.
- The contract (`LookupRequest`/`LookupResponse`) and the entry shape match [[VocabKit]]'s Swift models.
