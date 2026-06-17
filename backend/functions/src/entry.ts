import { SCHEMA_VERSION, type WordEntry, type ClaudeGeneration } from "./types";
import type { Normalized } from "./lemma";
import type { DictionaryResult } from "./dictionary";

/**
 * Assemble a stored WordEntry from the normalized selection, the dictionary
 * result, and the Claude generation. Pure (no Firestore / network) so the
 * merge logic is unit-testable in isolation.
 *
 * Merge policy: prefer Claude's generated lists/definition; fall back to the
 * dictionary's when Claude returned nothing.
 */
export function buildEntry(
  norm: Normalized,
  dict: DictionaryResult | null,
  gen: ClaudeGeneration,
  now: number,
  model: string
): WordEntry {
  return {
    lemma: norm.key,
    text: norm.display,
    kind: norm.kind,
    lookupCount: 1,
    firstSeen: now,
    lastSeen: now,
    phonetic: dict?.phonetic ?? "",
    audioUrl: dict?.audioUrl ?? "",
    learnerDefinition: gen.learnerDefinition,
    senses: gen.senses.length > 0 ? gen.senses : dict?.senses ?? [],
    examples: gen.examples,
    synonyms: gen.synonyms.length > 0 ? gen.synonyms : dict?.synonyms ?? [],
    antonyms: gen.antonyms.length > 0 ? gen.antonyms : dict?.antonyms ?? [],
    deckIds: [],
    tags: [],
    generatedBy: model,
    schemaVersion: SCHEMA_VERSION,
  };
}
