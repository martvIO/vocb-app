import { describe, it, expect, afterEach } from "vitest";
import { monthKey, generationCap, isOverCap, DEFAULT_MONTHLY_CAP } from "../src/usage";

afterEach(() => {
  delete process.env.VOCAB_MONTHLY_CAP;
});

describe("monthKey", () => {
  it("formats UTC year-month, zero-padded", () => {
    expect(monthKey(new Date("2026-06-17T10:00:00Z"))).toBe("2026-06");
    expect(monthKey(new Date("2026-01-01T00:00:00Z"))).toBe("2026-01");
    expect(monthKey(new Date("2026-12-31T23:59:59Z"))).toBe("2026-12");
  });
});

describe("generationCap", () => {
  it("defaults when unset or invalid", () => {
    expect(generationCap()).toBe(DEFAULT_MONTHLY_CAP);
    process.env.VOCAB_MONTHLY_CAP = "not-a-number";
    expect(generationCap()).toBe(DEFAULT_MONTHLY_CAP);
    process.env.VOCAB_MONTHLY_CAP = "0";
    expect(generationCap()).toBe(DEFAULT_MONTHLY_CAP);
  });

  it("honors a valid override (floored)", () => {
    process.env.VOCAB_MONTHLY_CAP = "50";
    expect(generationCap()).toBe(50);
    process.env.VOCAB_MONTHLY_CAP = "50.9";
    expect(generationCap()).toBe(50);
  });
});

describe("isOverCap", () => {
  it("is true at or above the cap", () => {
    expect(isOverCap(9, 10)).toBe(false);
    expect(isOverCap(10, 10)).toBe(true);
    expect(isOverCap(11, 10)).toBe(true);
  });
});
