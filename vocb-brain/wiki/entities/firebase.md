---
tags: [firebase, backend, data-model]
sources: [implementation-plan.md, backend-setup-runbook.md, backend-codebase.md]
created: 2026-06-17
updated: 2026-06-17
---

# Firebase

Google's backend platform. vocb-app uses three Firebase products:

- **Auth** — sign-in (Sign in with Apple + Email/Password). Per-user identity (`uid`) scopes all data.
- **Firestore** — per-user document storage and cross-device sync. Layout under `users/{uid}/`: `words/{lemma}`, `srs/{lemma}`, `decks/{deckId}` (see [[Word Entry Data Model]]). Security rules restrict access to the owner; word docs are written by the [[lookupWord Function]] via the Admin SDK and read directly by clients.
- **Cloud Functions** — hosts the [[lookupWord Function]] (Node 20, TypeScript), which proxies the [[Free Dictionary API]] and [[Anthropic Claude]] so the Claude key never reaches a client.

Requires the **Blaze** plan (outbound network calls). The Apple apps use the official Firebase SDK; the [[WinUI 3]]
Windows app has no official SDK and talks to Firebase via REST (Firestore REST + Identity Toolkit token). Central to
the [[Cloud Sync Architecture]].
