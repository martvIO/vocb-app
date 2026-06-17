---
tags: [backend, claude-api, decision]
sources: [implementation-plan.md, backend-codebase.md]
created: 2026-06-17
updated: 2026-06-17
---

# Hybrid Content Generation

How each vocabulary entry's content is produced: a **hybrid** of a structured dictionary and an LLM.

- **[[Free Dictionary API]]** supplies structured data — phonetic spelling, pronunciation `audioUrl`, base senses, and some synonyms/antonyms. Free, key-less; returns `null` when a word/phrase isn't found.
- **[[Anthropic Claude]]** (default `claude-haiku-4-5`) supplies the learner-friendly definition, natural example sentences, and curated synonyms/antonyms, returned as a structured JSON object.
- The [[lookupWord Function]] merges them into a [[Word Entry Data Model|WordEntry]], preferring Claude's lists/definition and falling back to the dictionary's when Claude returns nothing.

This runs **once per new lemma** thanks to [[Lemma-Keyed Caching]]; repeat lookups reuse the stored entry. The model
is swappable (`VOCAB_MODEL`) to trade cost for quality (Haiku → Sonnet → Opus).
