import { describe, it, expect, vi, afterEach } from "vitest";
import { lookupDictionary } from "../src/dictionary";

function mockFetch(impl: () => unknown) {
  vi.stubGlobal("fetch", vi.fn(impl as () => Promise<Response>));
}

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("lookupDictionary", () => {
  it("parses phonetic, audio, senses, synonyms, and antonyms", async () => {
    mockFetch(async () => ({
      ok: true,
      status: 200,
      json: async () => [
        {
          phonetic: "/rʌn/",
          phonetics: [{ text: "/rʌn/", audio: "//ssl.example.com/run.mp3" }],
          meanings: [
            {
              partOfSpeech: "verb",
              synonyms: ["sprint"],
              antonyms: ["walk"],
              definitions: [
                { definition: "to move quickly", synonyms: ["dash"], antonyms: [] },
              ],
            },
          ],
        },
      ],
    }));

    const result = await lookupDictionary("run");
    expect(result).not.toBeNull();
    expect(result!.phonetic).toBe("/rʌn/");
    // leading "//" is normalized to an https URL
    expect(result!.audioUrl).toBe("https://ssl.example.com/run.mp3");
    expect(result!.senses).toEqual([
      { partOfSpeech: "verb", meaning: "to move quickly" },
    ]);
    expect(result!.synonyms).toEqual(expect.arrayContaining(["sprint", "dash"]));
    expect(result!.antonyms).toContain("walk");
  });

  it("returns null on a 404 (word not found)", async () => {
    mockFetch(async () => ({ ok: false, status: 404, json: async () => ({}) }));
    expect(await lookupDictionary("asdfqwer")).toBeNull();
  });

  it("returns null when fetch throws", async () => {
    mockFetch(async () => {
      throw new Error("network down");
    });
    expect(await lookupDictionary("run")).toBeNull();
  });

  it("returns null for an empty result array", async () => {
    mockFetch(async () => ({ ok: true, status: 200, json: async () => [] }));
    expect(await lookupDictionary("run")).toBeNull();
  });
});
