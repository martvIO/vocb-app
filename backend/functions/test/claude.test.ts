import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";

// Mock the Anthropic SDK before importing the module under test.
const { mockCreate } = vi.hoisted(() => ({ mockCreate: vi.fn() }));
vi.mock("@anthropic-ai/sdk", () => ({
  default: vi.fn(() => ({ messages: { create: mockCreate } })),
}));

import { generate } from "../src/claude";

function textResponse(obj: unknown) {
  return { content: [{ type: "text", text: JSON.stringify(obj) }] };
}

beforeEach(() => {
  mockCreate.mockReset();
  delete process.env.VOCAB_MODEL;
});
afterEach(() => {
  delete process.env.VOCAB_MODEL;
});

describe("generate", () => {
  it("parses a well-formed structured response", async () => {
    mockCreate.mockResolvedValue(
      textResponse({
        learnerDefinition: "to move quickly on foot",
        senses: [{ partOfSpeech: "verb", meaning: "move quickly" }],
        examples: ["She runs daily."],
        synonyms: ["sprint", "jog"],
        antonyms: ["walk"],
      })
    );

    const gen = await generate("key", "run", "word", null);
    expect(gen.learnerDefinition).toBe("to move quickly on foot");
    expect(gen.senses).toEqual([{ partOfSpeech: "verb", meaning: "move quickly" }]);
    expect(gen.examples).toEqual(["She runs daily."]);
    expect(gen.synonyms).toEqual(["sprint", "jog"]);
    expect(gen.antonyms).toEqual(["walk"]);
  });

  it("defaults missing/invalid fields and drops malformed senses", async () => {
    mockCreate.mockResolvedValue(
      textResponse({
        learnerDefinition: "  trimmed  ",
        senses: [{ partOfSpeech: "verb", meaning: "ok" }, { partOfSpeech: "noun" }],
        synonyms: ["a", 5, null, "b"],
        // examples + antonyms omitted entirely
      })
    );

    const gen = await generate("key", "run", "word", null);
    expect(gen.learnerDefinition).toBe("trimmed");
    expect(gen.senses).toEqual([{ partOfSpeech: "verb", meaning: "ok" }]); // sense w/o meaning dropped
    expect(gen.synonyms).toEqual(["a", "5", "b"]); // coerced to strings, falsy dropped
    expect(gen.examples).toEqual([]);
    expect(gen.antonyms).toEqual([]);
  });

  it("uses the default model and honors VOCAB_MODEL override", async () => {
    mockCreate.mockResolvedValue(
      textResponse({ learnerDefinition: "d", senses: [], examples: [], synonyms: [], antonyms: [] })
    );

    await generate("key", "run", "word", null);
    expect(mockCreate).toHaveBeenLastCalledWith(
      expect.objectContaining({ model: "claude-haiku-4-5" })
    );

    process.env.VOCAB_MODEL = "claude-sonnet-4-6";
    await generate("key", "run", "word", null);
    expect(mockCreate).toHaveBeenLastCalledWith(
      expect.objectContaining({ model: "claude-sonnet-4-6" })
    );
  });

  it("throws when there is no text block", async () => {
    mockCreate.mockResolvedValue({ content: [{ type: "tool_use" }] });
    await expect(generate("key", "run", "word", null)).rejects.toThrow("claude-no-text-block");
  });

  it("throws on non-JSON output", async () => {
    mockCreate.mockResolvedValue({ content: [{ type: "text", text: "not json" }] });
    await expect(generate("key", "run", "word", null)).rejects.toThrow("claude-invalid-json");
  });
});
