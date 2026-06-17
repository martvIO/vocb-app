import { describe, it, expect } from "vitest";
import { normalize } from "../src/lemma";

describe("normalize", () => {
  it("lemmatizes verb inflections to one key", () => {
    expect(normalize("running").key).toBe("run");
    expect(normalize("ran").key).toBe("run");
    expect(normalize("runs").key).toBe("run");
  });

  it("strips surrounding punctuation/whitespace and lowercases the key", () => {
    const n = normalize("  Cats! ");
    expect(n.key).toBe("cat");
    expect(n.display).toBe("Cats"); // display keeps original casing, trimmed
    expect(n.kind).toBe("word");
  });

  it("reduces plurals and comparatives", () => {
    expect(normalize("studies").key).toBe("study");
    expect(normalize("better").key).toBe("good");
  });

  it("treats multi-word selections as phrases", () => {
    const n = normalize("a quick test");
    expect(n.kind).toBe("phrase");
    expect(n.key).toBe("a_quick_test");
    expect(n.display).toBe("a quick test");
  });

  it("collapses internal whitespace in phrases", () => {
    expect(normalize("  hello   world  ").key).toBe("hello_world");
  });

  it("throws on empty or punctuation-only input", () => {
    expect(() => normalize("   ")).toThrow();
    expect(() => normalize("!!!")).toThrow();
  });
});
