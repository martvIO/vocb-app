import { describe, it, expect } from "vitest";
import { buildEntry } from "../src/entry";
import type { Normalized } from "../src/lemma";
import type { DictionaryResult } from "../src/dictionary";
import type { ClaudeGeneration } from "../src/types";
import { SCHEMA_VERSION } from "../src/types";

const norm: Normalized = { key: "run", display: "running", kind: "word" };
const now = 1_700_000_000_000;

const fullGen: ClaudeGeneration = {
  learnerDefinition: "to move fast on foot",
  senses: [{ partOfSpeech: "verb", meaning: "move quickly" }],
  examples: ["She runs daily."],
  synonyms: ["sprint", "jog"],
  antonyms: ["walk"],
};

const dict: DictionaryResult = {
  phonetic: "/rʌn/",
  audioUrl: "https://example.com/run.mp3",
  senses: [{ partOfSpeech: "noun", meaning: "an act of running" }],
  synonyms: ["dash"],
  antonyms: ["stroll"],
};

describe("buildEntry", () => {
  it("sets base fields and a fresh lookupCount", () => {
    const e = buildEntry(norm, dict, fullGen, now, "claude-haiku-4-5");
    expect(e.lemma).toBe("run");
    expect(e.text).toBe("running");
    expect(e.kind).toBe("word");
    expect(e.lookupCount).toBe(1);
    expect(e.firstSeen).toBe(now);
    expect(e.lastSeen).toBe(now);
    expect(e.generatedBy).toBe("claude-haiku-4-5");
    expect(e.schemaVersion).toBe(SCHEMA_VERSION);
    expect(e.deckIds).toEqual([]);
    expect(e.tags).toEqual([]);
  });

  it("prefers Claude's lists/senses over the dictionary's", () => {
    const e = buildEntry(norm, dict, fullGen, now, "m");
    expect(e.synonyms).toEqual(["sprint", "jog"]);
    expect(e.antonyms).toEqual(["walk"]);
    expect(e.senses).toEqual(fullGen.senses);
    expect(e.phonetic).toBe("/rʌn/"); // phonetic/audio always from dictionary
    expect(e.audioUrl).toBe("https://example.com/run.mp3");
  });

  it("falls back to dictionary lists when Claude returns empty", () => {
    const emptyGen: ClaudeGeneration = {
      learnerDefinition: "def",
      senses: [],
      examples: [],
      synonyms: [],
      antonyms: [],
    };
    const e = buildEntry(norm, dict, emptyGen, now, "m");
    expect(e.synonyms).toEqual(["dash"]);
    expect(e.antonyms).toEqual(["stroll"]);
    expect(e.senses).toEqual(dict.senses);
  });

  it("handles a null dictionary result", () => {
    const e = buildEntry(norm, null, fullGen, now, "m");
    expect(e.phonetic).toBe("");
    expect(e.audioUrl).toBe("");
    expect(e.synonyms).toEqual(["sprint", "jog"]);
  });
});
