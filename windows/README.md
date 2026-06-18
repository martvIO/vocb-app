# Windows side (WinUI 3 / .NET)

Native Windows client, talking to Firebase over REST (there's no official Firebase
SDK for Windows/.NET). Solution: [Vocb.sln](Vocb.sln).

## Projects

| Project | TFM | What | Build status |
|---|---|---|---|
| [Vocb.Core](Vocb.Core) | net8.0 | Models (`WordEntry`/`Deck`), SM-2 `SrsEngine`, `StudyQueue`, DTOs — pure, no deps | ✅ builds + **11 xUnit tests pass** |
| [Vocb.Firebase](Vocb.Firebase) | net8.0 | REST clients: `FirebaseAuthClient`, `FirestoreClient` (+ typed-value mapping), `LookupClient` | ✅ builds clean |
| [Vocb.Core.Tests](Vocb.Core.Tests) | net8.0 | xUnit tests for the SM-2 engine + study queue | ✅ green |
| [Vocb.App](Vocb.App) | net8.0-windows | WinUI 3 app: tray/background, global hotkey, selection capture, overlay, screens | ⏳ authored — needs the Windows App SDK to build |

## Build & test

Requires the .NET 8 SDK. The Core/Firebase/Tests projects build with the plain SDK:

```sh
cd windows
dotnet test Vocb.Core.Tests/Vocb.Core.Tests.csproj   # SM-2 + study queue
dotnet build Vocb.Firebase/Vocb.Firebase.csproj      # REST client
```

`Vocb.App` (WinUI 3) additionally needs the **Windows App SDK** + Windows 10 SDK
(`winget install Microsoft.WindowsAppRuntime` and the workload). Its NuGet
packages restore on first build. It cannot be built on a machine without those.

## How the app works

- **Background capture:** `Vocb.App/Capture/` — a global hotkey (default **Ctrl+Shift+L**, via a message-only window + `RegisterHotKey`) reads the current selection (synthesize Ctrl+C → read clipboard → restore it; UI Automation `TextPattern` is the surgical alternative) and runs it through `lookupWord`. The result shows in a borderless always-on-top overlay near the cursor (`Vocb.App/Overlay/`).
- **Screens:** Words (sorted by lookup count), Study (SM-2 flip cards), Settings (configure the Firebase project + sign in).
- **Config:** open **Settings**, enter the Firebase Web API key + project id, and sign in with email/password. Tokens auto-refresh (`SessionService`).

## Why REST (not an SDK)

Firebase has no Windows/.NET SDK, so `Vocb.Firebase` calls Identity Toolkit
(auth), Firestore REST (read words/srs/decks, write srs), and the `lookupWord`
callable directly. `FirestoreMapping` converts our models to/from Firestore's
typed-value JSON wire format.
