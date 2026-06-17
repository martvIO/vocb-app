## Most Important Rule: Ask, Don't Assume

**Always ask me questions before acting — even if there are a lot of them. Never make assumptions.**

If anything about a task is ambiguous, underspecified, or could be interpreted in more than one way, stop and ask me first. It is always better to ask several clarifying questions up front than to guess and build the wrong thing. Do not fill in gaps with your own assumptions about:

- What I want a feature to do or how it should behave
- Which library, framework, or pattern to use
- File names, locations, or project structure
- Edge cases and how they should be handled
- Naming, styling, or formatting preferences

When in doubt, ask. I would rather answer ten questions than have you proceed on a guess.

## Code Quality

Write clean, readable code:

- Use clear, descriptive names for variables, functions, and files.
- Keep functions small and focused on a single responsibility.
- Add comments that explain *why* something is done, not just *what* it does. Comment any non-obvious logic, important decisions, and tricky edge cases.
- Prefer clarity over cleverness — code is read far more often than it is written.
- Follow the conventions and style already present in the codebase.

## Build for Future Expansion

Write code so that it can be easily extended later, even if it isn't being extended right now:

- Favor modular, loosely-coupled components with clear boundaries.
- Avoid hardcoding values that may change — use constants, config, or parameters instead.
- Separate concerns (for example, keep business logic apart from I/O, and UI apart from data).
- Design simple, well-documented interfaces between the parts of the system.
- Don't over-engineer. Keep it simple, but leave clean seams where future features could plug in.

The goal: if we later decide to expand a feature, it should be straightforward to do without rewriting existing code.

## Testing in a Real Environment

- Write and run the code in a real environment — don't just assume it works.
- Actually execute the code, run the tests, and verify the output before considering a task done.
- If something fails, fix it and re-run to confirm it passes.
- Include tests for any new functionality you add.

## Browser Testing with Playwright

- Use **Playwright** for browser automation and end-to-end testing.
- If Playwright is not installed, install and set it up before proceeding:
  - Install the package (for example `npm install -D @playwright/test`, or `pip install playwright` for Python — match the project's language).
  - Install the required browsers (for example `npx playwright install`, or `playwright install` for Python).
  - Add any needed configuration (such as a `playwright.config` file) following the project's conventions.
- Before relying on Playwright, run a test to confirm the setup actually works.

## Git Workflow

Commit to GitHub at the end of every step and every stage:

- After completing each meaningful step or stage of work, commit the changes.
- Write clear, descriptive commit messages that explain what changed and why.
- Push commits to the GitHub remote so progress is saved and visible.
- Keep commits focused — one logical change per commit where possible.

This keeps the work versioned and recoverable at every point.