---
tags: [architecture, apple-swiftui, windows-winui, decision]
sources: [implementation-plan.md]
created: 2026-06-17
updated: 2026-06-17
---

# Text Capture Strategy

How the app captures the word the user selects — the mechanism differs per OS because of platform limits.

- **iPadOS / iOS:** a **Share Extension + Action Extension**. iOS forbids a true system-wide background listener across other apps, so this is the supported path: select text → Share/Action → the extension calls the [[lookupWord Function]] and shows the entry. An in-app reader (paste/type text, tap words) is the fallback.
- **macOS:** a background **menu-bar agent** (`LSUIElement` / `NSStatusItem`) with a global hotkey. On trigger it reads the current selection via the **Accessibility API** (`AXUIElement` → `AXSelectedText`) and shows a borderless floating overlay (`NSPanel`) near the cursor, auto-saving in the background.
- **Windows ([[WinUI 3]]):** a system-tray app with a global hotkey that reads the selection via UI Automation `TextPattern` (Ctrl+C/clipboard fallback) and shows a topmost borderless overlay.

In all cases the selection is auto-saved and counted (see [[Word Entry Data Model]] / [[Lemma-Keyed Caching]]). The
overlay always appears with the meaning. This is the project's hardest, most platform-specific surface.
