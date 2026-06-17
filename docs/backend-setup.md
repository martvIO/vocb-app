# Backend setup — Firebase + `lookupWord`

The backend is a Firebase project: **Auth** (sign-in), **Firestore** (per-user
word storage + sync), and one **Cloud Function** (`lookupWord`) that proxies the
Free Dictionary API + Claude so the API key is never exposed to clients.

See [the data model and architecture](../../.claude/plans/the-goal-of-this-hashed-map.md)
for the bigger picture. This doc is the runbook to get the backend running.

## 0. Prerequisites (one-time)

This machine does **not** currently have Node installed — install these first:

1. **Node.js 20 LTS** — https://nodejs.org (the Functions runtime is Node 20).
   On Windows you can also use `winget install OpenJS.NodeJS.LTS`.
2. **Firebase CLI** — `npm install -g firebase-tools`, then `firebase login`.
3. **A Firebase project** on the **Blaze (pay-as-you-go)** plan — required
   because the function makes outbound calls to the dictionary + Anthropic.
   Create it at https://console.firebase.google.com.
4. In the Firebase console, enable: **Authentication** (Sign in with Apple +
   Email/Password), **Firestore Database** (production mode), **Functions**.
5. An **Anthropic API key** — https://console.anthropic.com.

## 1. Point the repo at your project

Edit [backend/.firebaserc](../backend/.firebaserc) and replace
`YOUR_FIREBASE_PROJECT_ID` with your project id, or run `firebase use --add`.

## 2. Install dependencies

```sh
cd backend/functions
npm install
```

## 3. Set the Claude API key as a secret

```sh
# Run from the backend/ directory
firebase functions:secrets:set ANTHROPIC_API_KEY
# paste your key when prompted
```

The key is stored in Google Secret Manager and bound to the function via
`defineSecret` in [src/lookupWord.ts](../backend/functions/src/lookupWord.ts) —
it is never in the code or the repo.

(Optional) choose a different model by setting `VOCAB_MODEL` — see
[functions/.env.example](../backend/functions/.env.example).

## 4. Run locally with the emulator

```sh
cd backend
firebase emulators:start --only functions,firestore,auth
# or: cd functions && npm run serve
```

The Emulator UI (default http://localhost:4000) lets you create a test auth
user and inspect Firestore writes. To exercise the function from the Functions
shell:

```sh
cd backend/functions
npm run shell
# then, in the shell:
lookupWord({ text: "running" }, { auth: { uid: "test-user" } })
```

**What to verify:**
- First call for `"running"` → response `created: true`, a new
  `users/test-user/words/run` doc with `lookupCount: 1`, populated
  `learnerDefinition`, `examples`, `synonyms`.
- Second call for `"running"` (or `"runs"`, `"ran"`) → `created: false`,
  `lookupCount: 2`, and **no** Anthropic call (lemma cache hit).

## 5. Deploy

```sh
cd backend
firebase deploy --only firestore:rules,firestore:indexes,functions
```

## 6. Calling it from a client

`lookupWord` is an HTTPS **callable** function. Clients send raw selected text;
the backend does all normalization.

- **Apple (Swift, Firebase SDK):**
  ```swift
  let functions = Functions.functions()
  let result = try await functions.httpsCallable("lookupWord").call(["text": selectedText])
  ```
- **Windows (.NET, no official Firebase SDK):** call the callable endpoint over
  REST — `POST https://<region>-<project>.cloudfunctions.net/lookupWord` with
  body `{"data": {"text": "..."}}` and an `Authorization: Bearer <Firebase ID token>`
  header (obtained via the Identity Toolkit REST API at sign-in).

The response is `{ "entry": WordEntry, "created": boolean }` — see
[src/types.ts](../backend/functions/src/types.ts) for the `WordEntry` shape, which
the native client models should mirror.

## Cost notes

- Generation cost is incurred **once per new lemma** (dictionary is free; Claude
  Haiku is ~$1 / $5 per million input/output tokens). Repeat lookups are a single
  Firestore write.
- `maxInstances` is capped at 5 ([src/index.ts](../backend/functions/src/index.ts))
  and input length is bounded in the function to keep a personal project cheap.
