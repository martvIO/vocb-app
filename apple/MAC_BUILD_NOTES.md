# Apple changes — build & verify on a Mac

These changes were authored on Windows and **cannot be compiled there** (Swift/SwiftUI
+ Firebase need Xcode on a Mac). Everything below is the part of the
"login/logout + popup speak/close" work that lands on the iPad/macOS side.

## What changed

**Shared (VocabKit — picked up automatically by every target that links the package):**
- `VocabKit/Sources/VocabKit/Speaker.swift` *(new)* — one TTS path (clip → else system
  voice) used by the app, the macOS agent, and the popup card.
- `VocabKit/Sources/VocabKit/WordPopupCard.swift` *(new)* — the shared "translation"
  popup card: word, pronunciation, definition, **speak** button, **X** button.

**iPad/macOS app:**
- `App/Audio/Pronunciation.swift` — now a thin wrapper over `VocabKit.Speaker`
  (existing `Pronunciation.pronounce(_:)` call sites unchanged).
- `App/Views/SignInView.swift` — added **Forgot password?** (`Auth.auth().sendPasswordReset`)
  with a green confirmation line. (Persisted login, the auth gate, and Sign out were
  already present — no change needed there.)
- `App/Views/ReaderView.swift` — tapping a word now presents `WordPopupCard` as a sheet
  (iOS: `presentationDetents`) instead of pushing the full detail screen. The full
  `WordDetailView` is still reachable from the **Words** tab.

**macOS menu-bar agent:**
- `macOSAgent/OverlayController.swift` — the floating overlay gained a **speak** button
  and an **X** button, and now inherits `NSObject` (required for button target/action).
  The auto-dismiss still fires (~10s) but **pauses while the pointer is over the panel**
  (via an `NSTrackingArea`) so it never disappears mid-read.

**iOS Share/Action extensions:**
- `ActionExtension/ActionViewController.swift` and `ShareExtension/ShareViewController.swift`
  — instead of a silent lookup, they now present `WordPopupCard` (speak + X) over a dimmed
  background; closing the card finishes the extension request. Errors show an alert.

## No new Xcode target membership needed

`Speaker.swift` and `WordPopupCard.swift` live in **VocabKit**, which the app, both
extensions, and the agent already add as a local Swift package — so they're shared with
zero `.pbxproj` surgery. The only pre-existing assumption is that `FunctionsLookupService`
is a member of the extension targets (already the case before this change).

## Build

```sh
# 1. Pure core (fast, no Xcode needed):
cd apple/VocabKit
swift build
swift test          # existing SRS/StudyQueue tests should stay green

# 2. The app + agent + extensions: open the Xcode project (assemble per apple/README.md
#    if not done yet) and build each scheme. Make sure GoogleService-Info.plist is added
#    (it is not in the repo).
```

## Manual verification checklist

- **Forgot password:** on the sign-in screen, enter an email, tap *Forgot password?* →
  green "reset email sent" line; check the inbox.
- **Login/logout (already working):** sign in, force-quit, relaunch → still signed in;
  Settings → *Sign out* returns to the sign-in screen.
- **macOS overlay:** select text anywhere, press ⌃⇧L → overlay shows **speaker** (hear the
  word) + **X** (closes immediately); hovering the panel keeps it open; it auto-closes ~10s
  after you move the pointer away.
- **iPad in-app Reader:** paste text, tap a word → popup **card** with speak + X (swipe or X
  to dismiss).
- **iPad selection action:** select text in any app → *Look up in Vocb* (Action) or the
  Share sheet → popup card with speak + X.

## Things to eyeball on the Mac (couldn't be compiled/run here)

1. **macOS overlay layout** — the header pins the speak/X buttons to the right via a fixed
   328pt width (panel content 360 − 16pt insets each side). If a long word crowds the
   buttons, lower the title label's `preferredMaxLayoutWidth` in `OverlayController`.
2. **Non-activating panel clicks** — the overlay is a `.nonactivatingPanel`; confirm the
   speak/X buttons receive clicks without first activating the agent (they should).
3. **iOS sheet detents** — `WordPopupCard` in the Reader uses `[.height(280), .medium]`;
   adjust if the card clips on a given device.
4. **iOS popup has no auto-timer by design** — a modal that vanishes on its own is
   non-idiomatic on iOS, so the card stays until X/swipe. Say the word if you'd like a
   timer there too and it's a small add.
