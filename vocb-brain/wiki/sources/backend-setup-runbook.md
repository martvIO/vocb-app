---
tags: [backend, firebase, setup]
sources: [backend-setup-runbook.md]
created: 2026-06-17
updated: 2026-06-17
---

# Backend Setup Runbook

Source summary of the backend setup/runbook doc.

## Source metadata
- Canonical: `docs/backend-setup.md`
- Type: operational runbook (prereqs, secret, emulator test, deploy, client integration).

## Key claims
- Backend = [[Firebase]]: Auth + Firestore + one Cloud Function, the [[lookupWord Function]].
- Requires the Blaze plan (outbound calls), Node 20 runtime, Firebase CLI, and an [[Anthropic Claude]] API key.
- The Claude key is a Functions **secret** (`ANTHROPIC_API_KEY`), never committed; model overridable via `VOCAB_MODEL`.
- Emulator acceptance test proves [[Lemma-Keyed Caching]]: first "running" lookup creates `words/run` (count 1, created:true); repeats just increment with no AI call.
- Clients call the callable with raw text — Apple via the Firebase SDK, [[WinUI 3]] via REST + a Firebase ID token.

## Summary
A step-by-step path from empty Firebase project to deployed function: set project id, install deps, set the secret,
run the emulator, verify the cache behavior, deploy rules + indexes + functions. Notes cost: generation happens once
per new lemma; repeat lookups are a single Firestore write. Part of the [[Cloud Sync Architecture]].
