// wink-lemmatizer ships no type declarations; declare the minimal surface we use.
declare module "wink-lemmatizer" {
  /** Lemmatize a noun (e.g. "cats" -> "cat"). */
  export function noun(word: string): string;
  /** Lemmatize a verb (e.g. "running" -> "run"). */
  export function verb(word: string): string;
  /** Lemmatize an adjective (e.g. "better" -> "good"). */
  export function adjective(word: string): string;
}
