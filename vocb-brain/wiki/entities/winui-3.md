---
tags: [windows-winui, architecture]
sources: [implementation-plan.md]
created: 2026-06-17
updated: 2026-06-17
---

# WinUI 3

Windows App SDK UI framework (C# / .NET) chosen for the native Windows client (Phase 5 of the [[Build Roadmap]]).

- Talks to [[Firebase]] via **REST** (Firestore REST + Identity Toolkit for auth tokens) — there is no official Firebase SDK for Windows/.NET.
- Provides the richer desktop behavior in the [[Text Capture Strategy]]: system-tray background app, global hotkey, UI Automation `TextPattern` selection capture (with a Ctrl+C/clipboard fallback), and a topmost borderless overlay near the cursor.
- Feature parity with the Apple app: word list sorted by lookupCount, word detail with pronunciation, the [[Study and Testing System]], decks/tags, reminders (Windows toasts).
- Built second, after the iPad + macOS milestone.
