<!-- Source capture. Canonical: docs/backend-setup.md — ingested 2026-06-17. Immutable; do not edit. -->

# Backend setup — Firebase + lookupWord (source capture)

The backend is a Firebase project: Auth (sign-in), Firestore (per-user word storage + sync),
and one Cloud Function (`lookupWord`) that proxies the Free Dictionary API + Claude so the API
key is never exposed to clients.

## Prerequisites (one-time)
1. Node.js 20 LTS (Functions runtime is Node 20). [Note: dev machine has Node 24 installed; fine for compiling.]
2. Firebase CLI — `npm install -g firebase-tools`, then `firebase login`.
3. A Firebase project on the **Blaze (pay-as-you-go)** plan (required for outbound calls).
4. Enable Authentication (Sign in with Apple + Email/Password), Firestore (production mode), Functions.
5. An Anthropic API key (https://console.anthropic.com).

## Steps
1. Point repo at project: edit `backend/.firebaserc` (replace `YOUR_FIREBASE_PROJECT_ID`) or `firebase use --add`.
2. Install deps: `cd backend/functions && npm install`.
3. Set the secret: `firebase functions:secrets:set ANTHROPIC_API_KEY` (stored in Google Secret Manager, bound via `defineSecret`).
   Optional: `VOCAB_MODEL` env var to change the model (default `claude-haiku-4-5`).
4. Run locally: `cd backend && firebase emulators:start --only functions,firestore,auth` (or `cd functions && npm run serve`).
   Test via Functions shell: `lookupWord({ text: "running" }, { auth: { uid: "test-user" } })`.
   - First call for "running" → created:true, new doc users/test-user/words/run, lookupCount 1.
   - Second call ("running"/"runs"/"ran") → created:false, lookupCount 2, NO Anthropic call (lemma cache hit).
5. Deploy: `cd backend && firebase deploy --only firestore:rules,firestore:indexes,functions`.

## Calling it from a client
`lookupWord` is an HTTPS callable. Clients send raw selected text; backend normalizes.
- Apple (Swift): `Functions.functions().httpsCallable("lookupWord").call(["text": selectedText])`.
- Windows (.NET, no official Firebase SDK): POST `https://<region>-<project>.cloudfunctions.net/lookupWord`
  with body `{"data": {"text": "..."}}` and `Authorization: Bearer <Firebase ID token>`.
Response: `{ "entry": WordEntry, "created": boolean }`.

## Cost notes
- Generation cost is incurred once per new lemma (dictionary free; Claude Haiku ~$1/$5 per MTok in/out). Repeat lookups are a single Firestore write.
- maxInstances capped at 5; input length bounded in the function.
