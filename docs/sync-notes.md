# Sync, conflicts, and the spend guard (Phase 6 notes)

## Cross-device sync
All clients read/write per-user Firestore documents under `users/{uid}/`
(`words`, `srs`, `decks`, `usage`). Firestore handles real-time sync and offline
persistence. Because the backend normalizes to a canonical lemma, the same word
looked up on any device converges to one `words/{lemma}` document and one
`lookupCount`.

## Conflict handling
- **Word entries** are effectively write-once + counter: only the `lookupWord`
  function creates/updates them, and it uses `FieldValue.increment` for the count
  and a transaction for first-create. Concurrent lookups from two devices both
  resolve safely (one creates, the rest increment). No client-side conflict.
- **SRS state** (`srs/{lemma}`) is last-writer-wins per card. In practice a card
  is reviewed on one device at a time, so clobbering is rare. If stronger
  guarantees are wanted later, store reviews as an append-only log and fold them,
  or guard writes with `lastReviewed` (reject an update whose `lastReviewed` is
  older than the stored one).
- **Decks/tags** are small per-user lists; last-writer-wins is acceptable.

## Spend guard (implemented)
Each new-lemma generation (a Claude call) increments
`users/{uid}/usage/{yyyy-mm}.generations`. Before generating, `lookupWord` reads
the current month's count and, if it's at/over the cap, returns a
`resource-exhausted` error instead of calling Claude. Cache hits are never
capped. Configure the cap with the `VOCAB_MONTHLY_CAP` env var (default 1000).
See `backend/functions/src/usage.ts` (unit-tested in `test/usage.test.ts`).

## Future polish (not yet built)
- A stats dashboard (most-looked-up, retention, reviews/day) reading the same data.
- Offline write queue / explicit reconciliation if a device is offline for long.
