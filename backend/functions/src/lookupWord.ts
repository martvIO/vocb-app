import { onCall, HttpsError } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";
import { logger } from "firebase-functions/v2";
import { getFirestore, FieldValue } from "firebase-admin/firestore";

import { normalize } from "./lemma";
import { lookupDictionary } from "./dictionary";
import { generate, DEFAULT_MODEL } from "./claude";
import { buildEntry } from "./entry";
import { monthKey, isOverCap } from "./usage";
import { LookupRequest, LookupResponse, WordEntry } from "./types";

const ANTHROPIC_API_KEY = defineSecret("ANTHROPIC_API_KEY");

const MAX_INPUT_LEN = 200;
const MAX_PHRASE_WORDS = 12;

/**
 * Callable: look up a selected word/phrase, auto-save it, and return the entry.
 *
 * Caching by lemma is the core cost control: the first lookup of a lemma fetches
 * the dictionary + calls Claude and stores the entry with lookupCount 1; every
 * later lookup of the same lemma just increments lookupCount (no AI call).
 */
export const lookupWord = onCall(
  { secrets: [ANTHROPIC_API_KEY], cors: true },
  async (request): Promise<LookupResponse> => {
    const uid = request.auth?.uid;
    if (!uid) {
      throw new HttpsError("unauthenticated", "Sign in to look up words.");
    }

    const raw = (request.data as LookupRequest | undefined)?.text;
    if (typeof raw !== "string" || raw.trim().length === 0) {
      throw new HttpsError("invalid-argument", "Provide non-empty `text`.");
    }
    if (raw.length > MAX_INPUT_LEN) {
      throw new HttpsError("invalid-argument", "Selection is too long.");
    }

    let norm;
    try {
      norm = normalize(raw);
    } catch {
      throw new HttpsError("invalid-argument", "Selection has no usable text.");
    }
    if (norm.kind === "phrase" && norm.display.split(/\s+/).length > MAX_PHRASE_WORDS) {
      throw new HttpsError("invalid-argument", "Phrase is too long.");
    }

    const db = getFirestore();
    const ref = db.doc(`users/${uid}/words/${norm.key}`);
    const now = Date.now();

    // Fast path: entry already exists → just bump the count, no AI call.
    const existing = await ref.get();
    if (existing.exists) {
      await ref.update({ lookupCount: FieldValue.increment(1), lastSeen: now });
      const updated = await ref.get();
      return { entry: updated.data() as WordEntry, created: false };
    }

    // Spend guard: stop generating new entries once the monthly cap is hit.
    // (Cache hits above are unaffected — only new-lemma generation is capped.)
    const usageRef = db.doc(`users/${uid}/usage/${monthKey()}`);
    const used = ((await usageRef.get()).data()?.generations as number | undefined) ?? 0;
    if (isOverCap(used)) {
      throw new HttpsError(
        "resource-exhausted",
        "Monthly lookup limit reached. Raise VOCAB_MONTHLY_CAP or try next month."
      );
    }

    // Slow path: generate the entry (dictionary + Claude) outside any txn.
    // For single words, look the dictionary up by the base form (the lemma).
    const dictionaryTerm = norm.kind === "word" ? norm.key : norm.display;
    const dict = await lookupDictionary(dictionaryTerm);

    const gen = await generate(
      ANTHROPIC_API_KEY.value(),
      norm.display,
      norm.kind,
      dict
    );

    const entry: WordEntry = buildEntry(
      norm,
      dict,
      gen,
      now,
      process.env.VOCAB_MODEL || DEFAULT_MODEL
    );

    // Commit with race safety: a concurrent lookup may have created it meanwhile.
    const created = await db.runTransaction(async (tx) => {
      const snap = await tx.get(ref);
      if (snap.exists) {
        tx.update(ref, { lookupCount: FieldValue.increment(1), lastSeen: now });
        return false;
      }
      tx.set(ref, entry);
      return true;
    });

    if (created) {
      logger.info("created word entry", { uid, key: norm.key, kind: norm.kind });
      // Count the generation against this month's cap.
      await usageRef.set(
        { generations: FieldValue.increment(1), updatedAt: now },
        { merge: true }
      );
    }

    const finalSnap = await ref.get();
    return { entry: finalSnap.data() as WordEntry, created };
  }
);
