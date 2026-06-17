import { initializeApp } from "firebase-admin/app";
import { setGlobalOptions } from "firebase-functions/v2";

// Initialize the Admin SDK once for all functions in this codebase.
initializeApp();

// Keep cold-start cost predictable for a personal app.
setGlobalOptions({ maxInstances: 5, region: "us-central1" });

export { lookupWord } from "./lookupWord";
