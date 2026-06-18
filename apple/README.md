# Apple side (iPad + macOS)

Native Swift/SwiftUI. The plan is one Xcode project with shared SwiftUI views and
two app targets (iOS + macOS), plus Share/Action extensions and a macOS menu-bar
agent. All of that builds on a **Mac with Xcode** (or a cloud Mac) — it cannot be
compiled on Windows.

## What's here now: `VocabKit`

[VocabKit](VocabKit) is the pure-Swift core the apps and extensions share. It has
**no Firebase dependency**, so it compiles fast and is fully unit-tested:

- [Models.swift](VocabKit/Sources/VocabKit/Models.swift) — `WordEntry`, `WordSense`,
  `Deck`. Field-for-field match with the backend `WordEntry`
  ([backend/functions/src/types.ts](../backend/functions/src/types.ts)) so Firestore
  documents decode directly.
- [SRSEngine.swift](VocabKit/Sources/VocabKit/SRSEngine.swift) — the SM-2 spaced-
  repetition scheduler (`SRSState`, `ReviewGrade`, `SRSEngine.schedule`). Pure and
  deterministic.
- [Services.swift](VocabKit/Sources/VocabKit/Services.swift) — `LookupServicing` and
  `VocabRepository` protocols (the apps implement these over Firebase), plus
  `StudyQueue` which orders a review session (due cards first, then most-looked-up).
- Tests in [Tests/VocabKitTests](VocabKit/Tests/VocabKitTests).

### Build & test (on a Mac)

```sh
cd apple/VocabKit
swift build
swift test
```

## App, extensions, and agent (authored — assemble in Xcode)

All Swift source is written; it needs an Xcode project on a Mac to compile.
**Status: authored, not yet compiled** — verify by building on the cloud Mac.

- **[App/](App)** — the shared iPad + macOS SwiftUI app:
  - `VocbApp.swift` (`@main`), `AppModel.swift` (auth + data + lookup state).
  - `Services/FirebaseRepository.swift` implements VocabKit's `VocabRepository` (Firestore); `Services/FunctionsLookupService.swift` implements `LookupServicing` (the `lookupWord` callable).
  - `Views/` — `RootView` (auth gate + tabs), `SignInView`, `WordListView` (sorted by `lookupCount`), `WordDetailView` (pronunciation), `StudyView` (SM-2 flip cards), `ReaderView` (tap-a-word), `SettingsView`.
  - `Audio/Pronunciation.swift`, `Notifications/ReviewReminders.swift`.
- **[ShareExtension/](ShareExtension)** + **[ActionExtension/](ActionExtension)** — iOS capture: receive selected text from the Share sheet / selection callout → `lookupWord`.
- **[macOSAgent/](macOSAgent)** — background menu-bar agent: `GlobalHotkey` (Carbon, ⌃⇧L), `AccessibilitySelection` (AX API), `OverlayController` (floating `NSPanel`), `AgentAppDelegate` (`NSStatusItem`).

### Assembling in Xcode

1. New Xcode project with **iOS + macOS** app targets; add **VocabKit** as a local Swift package and the `App/` sources.
2. Add Firebase via SPM: `FirebaseAuth`, `FirebaseFirestore`, `FirebaseFunctions`; drop in `GoogleService-Info.plist`.
3. Add targets for the **Share Extension**, **Action Extension**, and the **macOS agent** (set `LSUIElement=YES` on the agent).
4. Capabilities: an **App Group** + **Keychain Sharing** (so extensions share the signed-in user), **Accessibility** usage for the macOS agent, and notifications for reminders.
5. Build/run; on macOS grant Accessibility permission when prompted.

See the full plan at `.claude/plans/the-goal-of-this-hashed-map.md`.
