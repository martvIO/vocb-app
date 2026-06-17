import { logger } from "firebase-functions/v2";
import type { WordSense } from "./types";

/**
 * Free Dictionary API client (https://dictionaryapi.dev) — no key required.
 * Provides phonetics, pronunciation audio, and base definitions/synonyms,
 * which we then enrich with Claude. Returns null when the word isn't found
 * (404) or the request fails, so generation can proceed from Claude alone.
 */

const ENDPOINT = "https://api.dictionaryapi.dev/api/v2/entries/en";

export interface DictionaryResult {
  phonetic: string;
  audioUrl: string;
  senses: WordSense[];
  synonyms: string[];
  antonyms: string[];
}

// Minimal shape of the parts of the API response we consume.
interface ApiPhonetic {
  text?: string;
  audio?: string;
}
interface ApiDefinition {
  definition?: string;
  synonyms?: string[];
  antonyms?: string[];
}
interface ApiMeaning {
  partOfSpeech?: string;
  definitions?: ApiDefinition[];
  synonyms?: string[];
  antonyms?: string[];
}
interface ApiEntry {
  phonetic?: string;
  phonetics?: ApiPhonetic[];
  meanings?: ApiMeaning[];
}

export async function lookupDictionary(
  word: string
): Promise<DictionaryResult | null> {
  let entries: ApiEntry[];
  try {
    const res = await fetch(`${ENDPOINT}/${encodeURIComponent(word)}`, {
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      // 404 = not in dictionary (common for phrases / rare words) — not an error.
      if (res.status !== 404) {
        logger.warn("dictionary api non-ok", { word, status: res.status });
      }
      return null;
    }
    entries = (await res.json()) as ApiEntry[];
  } catch (err) {
    logger.warn("dictionary api fetch failed", { word, err: String(err) });
    return null;
  }

  if (!Array.isArray(entries) || entries.length === 0) return null;

  const phonetic =
    entries.find((e) => e.phonetic)?.phonetic ??
    entries.flatMap((e) => e.phonetics ?? []).find((p) => p.text)?.text ??
    "";

  const audioUrl =
    entries
      .flatMap((e) => e.phonetics ?? [])
      .find((p) => p.audio && p.audio.length > 0)?.audio ?? "";

  const senses: WordSense[] = [];
  const synonyms = new Set<string>();
  const antonyms = new Set<string>();

  for (const entry of entries) {
    for (const meaning of entry.meanings ?? []) {
      const pos = meaning.partOfSpeech ?? "";
      (meaning.synonyms ?? []).forEach((s) => synonyms.add(s));
      (meaning.antonyms ?? []).forEach((a) => antonyms.add(a));
      for (const def of meaning.definitions ?? []) {
        if (def.definition) senses.push({ partOfSpeech: pos, meaning: def.definition });
        (def.synonyms ?? []).forEach((s) => synonyms.add(s));
        (def.antonyms ?? []).forEach((a) => antonyms.add(a));
      }
    }
  }

  return {
    phonetic,
    audioUrl: audioUrl.startsWith("//") ? `https:${audioUrl}` : audioUrl,
    senses: senses.slice(0, 8),
    synonyms: [...synonyms].slice(0, 12),
    antonyms: [...antonyms].slice(0, 12),
  };
}
