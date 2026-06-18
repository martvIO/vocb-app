/**
 * Monthly generation/spend guard. Each new-lemma generation (a Claude call)
 * increments a per-user monthly counter at users/{uid}/usage/{yyyy-mm}. When the
 * cap is hit, lookupWord stops generating new entries until next month (cache
 * hits still work). Cap is configurable via the VOCAB_MONTHLY_CAP env var.
 */

export const DEFAULT_MONTHLY_CAP = 1000;

/** UTC year-month key, e.g. "2026-06". Used as the usage document id. */
export function monthKey(date: Date = new Date()): string {
  const year = date.getUTCFullYear();
  const month = String(date.getUTCMonth() + 1).padStart(2, "0");
  return `${year}-${month}`;
}

/** The configured monthly generation cap (falls back to the default). */
export function generationCap(): number {
  const v = Number(process.env.VOCAB_MONTHLY_CAP);
  return Number.isFinite(v) && v > 0 ? Math.floor(v) : DEFAULT_MONTHLY_CAP;
}

/** True if the user has reached/exceeded the cap for the month. */
export function isOverCap(used: number, cap: number = generationCap()): boolean {
  return used >= cap;
}
