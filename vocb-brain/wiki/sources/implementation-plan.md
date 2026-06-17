---
tags: [architecture, decision, data-model]
sources: [implementation-plan.md]
created: 2026-06-17
updated: 2026-06-17
---

# Implementation Plan

Source summary of the vocb-app implementation plan.

## Source metadata
- Canonical: `.claude/plans/the-goal-of-this-hashed-map.md`
- Type: planning document with locked-in decisions, architecture, data model, and build phases.

## Key claims
- vocb-app is a personal, English-only vocabulary app for **iPad + macOS + Windows**, native on each.
- Core loop: select word → overlay + auto-save → enriched entry → study (SRS + flashcards + quizzes); most-looked-up words are prioritized for testing. See [[Word Entry Data Model]], [[Lemma-Keyed Caching]], [[Study and Testing System]].
- Content is hybrid: [[Free Dictionary API]] + [[Anthropic Claude]] (Haiku tier, swappable). See [[Hybrid Content Generation]].
- Sync/auth/proxy via [[Firebase]]. The Claude key lives only in Cloud Functions.
- Capture differs by OS — see [[Text Capture Strategy]] (iOS can't do background capture; desktop gets tray + hotkey + overlay).
- First milestone: iPad + macOS (shared [[VocabKit]]); Windows ([[WinUI 3]]) second.

## Summary
The plan fixes every major decision (platforms, stacks, content source, study system, storage, scope) and lays out
six build phases (Phase 0 prereqs → backend → VocabKit → Apple app → capture → Windows → polish). It documents the
[[Cloud Sync Architecture]] and the per-user Firestore data model, and flags constraints: iOS background-capture
limits, the Apple Developer Program + cloud-Mac requirement, and lemma-cache cost control. See the cross-cutting
[[Architecture Overview]] and [[Build Roadmap]].
