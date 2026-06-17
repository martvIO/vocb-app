/**
 * Shared data model for the vocab app. These shapes mirror the Firestore
 * documents under users/{uid}/ and the lookupWord request/response contract.
 * Keep this file in sync with the native client models (Apple VocabKit, Windows).
 */

/** A single sense of a word: part of speech + a learner-friendly meaning. */
export interface WordSense {
  partOfSpeech: string;
  meaning: string;
}

/**
 * A stored vocabulary entry: users/{uid}/words/{lemma}.
 * `lemma` is the document id (normalized — see lemma.ts).
 */
export interface WordEntry {
  /** Normalized key, also the document id (e.g. "run"). */
  lemma: string;
  /** The original surface form that was first looked up (e.g. "running"). */
  text: string;
  /** "word" for single tokens, "phrase" for multi-word selections. */
  kind: "word" | "phrase";

  /** How many times the user has looked this up. Drives sorting + test priority. */
  lookupCount: number;
  /** ISO-ish epoch millis. */
  firstSeen: number;
  lastSeen: number;

  /** Phonetic spelling, e.g. "/rʌn/" (from the dictionary API; may be empty). */
  phonetic: string;
  /** Pronunciation audio URL (from the dictionary API; may be empty). */
  audioUrl: string;

  /** Cleaned-up, learner-friendly definition (from Claude). */
  learnerDefinition: string;
  /** Dictionary senses (part of speech + meaning). */
  senses: WordSense[];
  /** Example sentences using the word (from Claude). */
  examples: string[];
  synonyms: string[];
  antonyms: string[];

  /** Organization. */
  deckIds: string[];
  tags: string[];

  /** Which model/source produced the generated fields. */
  generatedBy: string;
  /** Schema version, so clients can migrate old entries. */
  schemaVersion: number;
}

/** Request payload for the lookupWord callable function. */
export interface LookupRequest {
  /** Raw selected text. The backend does ALL normalization/lemmatization. */
  text: string;
}

/** Response payload for the lookupWord callable function. */
export interface LookupResponse {
  entry: WordEntry;
  /** true if this lookup created a new entry, false if it hit the cache. */
  created: boolean;
}

/** The structured object we ask Claude to produce. */
export interface ClaudeGeneration {
  learnerDefinition: string;
  senses: WordSense[];
  examples: string[];
  synonyms: string[];
  antonyms: string[];
}

export const SCHEMA_VERSION = 1;
