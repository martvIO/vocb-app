---
tags: [spaced-repetition, apple-swiftui, decision]
sources: [implementation-plan.md, apple-vocabkit.md]
created: 2026-06-17
updated: 2026-06-17
---

# Study and Testing System

How the user reviews saved words. The plan calls for **all** of: [[SM-2 Spaced Repetition]] scheduling plus multiple
test types — flip cards, multiple-choice, type-the-word, and fill-in-blank.

## Queue ordering (`StudyQueue.build`)
Implemented in [[VocabKit]]'s `Services.swift`:
1. **Due cards first**, oldest `dueDate` first (from each word's `SRSState`).
2. Then **never-reviewed words**, ordered by `lookupCount` descending — the words the user encounters most get tested first.
3. Optional `limit` truncates the session.

## Ties to the rest of the system
- Priority comes from `lookupCount` in the [[Word Entry Data Model]], which is incremented by [[Lemma-Keyed Caching]] on every lookup.
- Daily review reminders (local notifications on Apple, toasts on [[WinUI 3]]) nudge the user; this is an in-scope extra.
- Pronunciation (dictionary `audioUrl` or platform TTS) supports recall during review.
