import Anthropic from "@anthropic-ai/sdk";
import { logger } from "firebase-functions/v2";
import type { ClaudeGeneration, WordSense } from "./types";
import type { DictionaryResult } from "./dictionary";

/**
 * Claude client. Generates learner-friendly definitions, example sentences,
 * and synonyms/antonyms, returned as a single structured JSON object via
 * `output_config.format` (JSON schema). Model is swappable via the VOCAB_MODEL
 * env var; defaults to the fast/cheap Haiku tier.
 */

export const DEFAULT_MODEL = "claude-haiku-4-5";

// JSON schema constraining Claude's output. Note structured-output limits:
// no min/max constraints, and every object needs additionalProperties: false.
const GENERATION_SCHEMA = {
  type: "object",
  properties: {
    learnerDefinition: {
      type: "string",
      description: "A clear, concise definition written for an English learner.",
    },
    senses: {
      type: "array",
      description: "Distinct senses of the word.",
      items: {
        type: "object",
        properties: {
          partOfSpeech: { type: "string" },
          meaning: { type: "string" },
        },
        required: ["partOfSpeech", "meaning"],
        additionalProperties: false,
      },
    },
    examples: {
      type: "array",
      description: "Natural example sentences that use the word.",
      items: { type: "string" },
    },
    synonyms: { type: "array", items: { type: "string" } },
    antonyms: { type: "array", items: { type: "string" } },
  },
  required: ["learnerDefinition", "senses", "examples", "synonyms", "antonyms"],
  additionalProperties: false,
} as const;

function buildPrompt(
  text: string,
  kind: "word" | "phrase",
  dict: DictionaryResult | null
): string {
  const dictContext = dict
    ? `Dictionary data (use as reference, correct it if wrong):\n` +
      `- senses: ${JSON.stringify(dict.senses)}\n` +
      `- synonyms: ${JSON.stringify(dict.synonyms)}\n` +
      `- antonyms: ${JSON.stringify(dict.antonyms)}\n`
    : `No dictionary data was found for this ${kind}.\n`;

  return (
    `You are building a vocabulary flashcard for an English learner.\n` +
    `The ${kind} is: "${text}"\n\n` +
    dictContext +
    `\nProduce:\n` +
    `- learnerDefinition: one or two sentences a learner can understand.\n` +
    `- senses: the main senses (part of speech + short meaning); 1-4 entries.\n` +
    `- examples: 3 natural example sentences that clearly show how to use it.\n` +
    `- synonyms / antonyms: up to 6 each (empty arrays if none apply).\n` +
    `Keep it accurate and concise. Use the dictionary data where it is correct.`
  );
}

export async function generate(
  apiKey: string,
  text: string,
  kind: "word" | "phrase",
  dict: DictionaryResult | null
): Promise<ClaudeGeneration> {
  const client = new Anthropic({ apiKey });
  const model = process.env.VOCAB_MODEL || DEFAULT_MODEL;

  // output_config is cast loosely so this compiles across SDK minor versions.
  const message = await client.messages.create({
    model,
    max_tokens: 1500,
    messages: [{ role: "user", content: buildPrompt(text, kind, dict) }],
    output_config: { format: { type: "json_schema", schema: GENERATION_SCHEMA } },
  } as Anthropic.MessageCreateParamsNonStreaming);

  const block = message.content.find((b) => b.type === "text");
  if (!block || block.type !== "text") {
    throw new Error("claude-no-text-block");
  }

  let parsed: Partial<ClaudeGeneration>;
  try {
    parsed = JSON.parse(block.text) as Partial<ClaudeGeneration>;
  } catch (err) {
    logger.error("claude returned non-JSON", { model, text, err: String(err) });
    throw new Error("claude-invalid-json");
  }

  const senses: WordSense[] = Array.isArray(parsed.senses)
    ? parsed.senses.filter((s) => s && s.meaning).map((s) => ({
        partOfSpeech: String(s.partOfSpeech ?? ""),
        meaning: String(s.meaning ?? ""),
      }))
    : [];

  const strArr = (v: unknown): string[] =>
    Array.isArray(v)
      ? v.filter((x) => x != null).map(String).filter(Boolean)
      : [];

  return {
    learnerDefinition: String(parsed.learnerDefinition ?? "").trim(),
    senses,
    examples: strArr(parsed.examples),
    synonyms: strArr(parsed.synonyms),
    antonyms: strArr(parsed.antonyms),
  };
}
