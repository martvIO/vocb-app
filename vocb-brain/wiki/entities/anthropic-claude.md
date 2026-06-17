---
tags: [claude-api, backend]
sources: [implementation-plan.md, backend-codebase.md, backend-setup-runbook.md]
created: 2026-06-17
updated: 2026-06-17
---

# Anthropic Claude

The LLM used to generate the creative half of each vocabulary entry: a learner-friendly definition, example
sentences, and synonyms/antonyms. Part of [[Hybrid Content Generation]] (dictionary + AI).

- SDK: `@anthropic-ai/sdk` (TypeScript), called from `claude.ts` in the backend.
- Default model: **`claude-haiku-4-5`** (fast/cheap, ~$1/$5 per million input/output tokens), swappable via the `VOCAB_MODEL` env var to e.g. `claude-sonnet-4-6` or `claude-opus-4-8`.
- Output is constrained with **structured JSON output** (`output_config.format` + a JSON schema, `additionalProperties:false`) so the function gets a parseable `{learnerDefinition, senses, examples, synonyms, antonyms}` object.
- The API key is a [[Firebase]] Functions secret (`ANTHROPIC_API_KEY`); calls happen only server-side in the [[lookupWord Function]].
- Cost is bounded by [[Lemma-Keyed Caching]] — Claude is called once per new lemma, never on repeat lookups.
