import * as winkLemmatizer from "wink-lemmatizer";

/**
 * The backend is the single source of truth for normalization: clients send
 * raw selected text and we derive the canonical key here, so the same word
 * looked up on iPad, macOS, and Windows always maps to one entry.
 */
export interface Normalized {
  /** Firestore document id for the entry. */
  key: string;
  /** Cleaned display form to store as `text` when the entry is first created. */
  display: string;
  kind: "word" | "phrase";
}

/** Strip surrounding punctuation/whitespace and collapse internal whitespace. */
function clean(raw: string): string {
  return raw
    .normalize("NFC")
    .replace(/\s+/g, " ")
    .trim()
    // strip leading/trailing characters that aren't letters, digits, or internal marks
    .replace(/^[^\p{L}\p{N}]+|[^\p{L}\p{N}]+$/gu, "")
    .trim();
}

/**
 * Reduce a single word to its base form. wink-lemmatizer is POS-specific and
 * returns the input unchanged when no rule applies, so we chain noun → verb →
 * adjective to cover plurals, verb inflections, and comparatives. This is
 * best-effort: it's biased toward over-reducing (e.g. the noun "meeting"
 * becomes "meet"), which is an acceptable tradeoff for a personal vocab key.
 */
function lemmatizeWord(word: string): string {
  const lower = word.toLowerCase();
  let lemma = winkLemmatizer.noun(lower);
  lemma = winkLemmatizer.verb(lemma);
  lemma = winkLemmatizer.adjective(lemma);
  return lemma;
}

/** Firestore doc ids can't contain "/"; keep keys safe and predictable. */
function safeKey(s: string): string {
  return s.replace(/\//g, "-");
}

export function normalize(raw: string): Normalized {
  const display = clean(raw);
  if (!display) {
    throw new Error("empty-selection");
  }

  const isPhrase = /\s/.test(display);
  if (isPhrase) {
    // Phrases are keyed by their normalized lowercase form (spaces → "_").
    const key = safeKey(display.toLowerCase().replace(/\s+/g, "_"));
    return { key, display, kind: "phrase" };
  }

  const key = safeKey(lemmatizeWord(display));
  return { key, display, kind: "word" };
}
