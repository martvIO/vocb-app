---
tags: [architecture, firebase, data-model]
sources: [implementation-plan.md, backend-setup-runbook.md]
created: 2026-06-17
updated: 2026-06-17
---

# Cloud Sync Architecture

How data flows and syncs across the three clients via [[Firebase]].

- Every client (iPad/macOS SwiftUI, [[WinUI 3]] Windows, iOS Share/Action extensions) authenticates with Firebase Auth and reads/writes under `users/{uid}/`.
- The only server endpoint is the [[lookupWord Function]]; clients never hold the [[Anthropic Claude]] key.
- Writes are per-user Firestore documents (`words`, `srs`, `decks`) following the [[Word Entry Data Model]]; Firestore handles cross-device sync.
- Because [[Lemma-Keyed Caching]] normalizes server-side, the same word looked up on any device converges to one entry and one `lookupCount`.
- Apple clients use the Firebase SDK with offline persistence; Windows uses Firestore REST + an Identity Toolkit token.
- Security rules restrict each user to their own subtree; word docs are written by the function (Admin SDK) and read by clients.

Open item for Phase 6: sync conflict handling / offline write queue. See [[Build Roadmap]].
