# Week 03 — From AI Chat to Agentic Development

This package contains the CAP Module/Week 03 curriculum: a 90-minute lesson, a live Excalidraw follow-along, and workflow practice.

## Files

- [`presentation.md`](presentation.md) — main Marp deck; designed for ~90 minutes including one live demo.
- [`talk-outline.md`](talk-outline.md) — instructor timing and teaching guidance.
- [`pre-class-setup.md`](pre-class-setup.md) — student tool installation and environment checks.
- [`reference.md`](reference.md) — commands, concepts, implementation notes, and sources.
- [`demo-focus-mode.md`](demo-focus-mode.md) — Excalidraw Selection Focus Mode live follow-along.
- [`demo-presentation-mode.md`](demo-presentation-mode.md) — Excalidraw Frame Presentation Mode live follow-along.
- [`homework.md`](homework.md) — suggested follow-up assignment.
- [`templates/`](templates/) — spec/plan/task/context templates for the demo and students.

## Demo Choice

Teach **one** demo live:

1. **Selection Focus Mode** — selected elements become the focus of the viewport and unrelated elements are visually de-emphasized. This is intentionally distinct from Excalidraw’s existing Zen Mode and View Mode.
2. **Frame Presentation Mode** — navigate existing Excalidraw frames as a lightweight presentation with previous/next, slide count, and exit behavior.

Both use the same spec-driven workflow, so the lecture does not need to change when you switch demos.

## Marp

The deck follows the visual style used in CAP Week 2 (`curriculum/weeks/02/presentation.md`): 16:9, warm background, navy headings, teal accents, and speaker notes in HTML comments.

Typical render command if Marp CLI is installed:

```bash
marp presentation.md --html
# or
marp presentation.md --pdf
```

With npm:

```bash
npx @marp-team/marp-cli presentation.md --html
```

## Repository Boundary

Keep the teaching materials in this CAP repository. Implement the live feature and submit its PR in your own Excalidraw fork; follow the homework instructions for the separate assignment.
