<!-- Source capture. Canonical: backend/functions/src/*.ts + backend/firestore.* — ingested 2026-06-17. Immutable; do not edit. -->

# Backend codebase (source capture)

TypeScript Cloud Functions (Firebase, Node 20 runtime). Verified compiling (`tsc --noEmit` passes);
the lemma normalizer was runtime-tested.

## Modules (backend/functions/src/)
- **types.ts** — shared data model. `WordSense {partOfSpeech, meaning}`; `WordEntry` (see below); `Deck`;
  request/response DTOs `LookupRequest {text}` / `LookupResponse {entry, created}`; `ClaudeGeneration`; `SCHEMA_VERSION = 1`.
- **lemma.ts** — `normalize(raw) -> {key, display, kind}`. Cleans punctuation/whitespace; single words are
  lemmatized via wink-lemmatizer chained noun→verb→adjective; multi-word selections are phrases keyed
  lowercase with spaces→underscores. Backend is the single source of truth for normalization.
  Verified: running/ran/runs→"run", "Cats!"→"cat", better→"good", studies→"study", "a quick test"→phrase.
- **dictionary.ts** — `lookupDictionary(word)` calls Free Dictionary API (dictionaryapi.dev, no key).
  Returns {phonetic, audioUrl, senses, synonyms, antonyms} or null on 404/error (so Claude can still generate).
- **claude.ts** — `generate(apiKey, text, kind, dict)` calls Anthropic (`@anthropic-ai/sdk`). Model from
  `VOCAB_MODEL` env, default `claude-haiku-4-5`. Uses structured JSON output (`output_config.format` JSON schema)
  to return {learnerDefinition, senses, examples, synonyms, antonyms}. Schema uses additionalProperties:false.
- **lookupWord.ts** — the callable (`onCall`, v2, secret ANTHROPIC_API_KEY). Auth required. Normalizes text;
  if entry exists → `FieldValue.increment(1)` lookupCount + lastSeen, return created:false (no AI call);
  else fetch dictionary (by lemma for words) + Claude, build WordEntry, commit in a transaction (race-safe),
  return created:true. Input bounded (≤200 chars, ≤12 phrase words). maxInstances 5, region us-central1.
- **index.ts** — initializes Admin SDK, sets global options, exports lookupWord.

## WordEntry shape
lemma (doc id), text, kind ("word"|"phrase"), lookupCount, firstSeen, lastSeen (epoch ms),
phonetic, audioUrl, learnerDefinition, senses[], examples[], synonyms[], antonyms[],
deckIds[], tags[], generatedBy, schemaVersion.

## Firestore
- **firestore.rules** — users can read/write only their own `users/{uid}/**`. Word docs are written by the
  function via Admin SDK (bypasses rules); clients read them directly.
- **firestore.indexes.json** — composite indexes: words by deckIds array-contains + lookupCount desc; words by tags + lookupCount desc.

## Dependencies
@anthropic-ai/sdk, firebase-admin, firebase-functions, wink-lemmatizer (no bundled types — local .d.ts declares noun/verb/adjective).
