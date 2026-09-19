# Classroom project

One application developed progressively during CAP teaching. Its specifications, decisions, tasks, and implementation live together here.

**Current state:** Structure and starter documents only. The application name, behavior, and technology stack are still to be defined; there is no runnable application yet.

## Start here

For copy-paste prompts at each stage, use the [project workflow and session handoff guide](docs/workflow.md).

1. Define the product in [docs/brief.md](docs/brief.md).
2. Describe the first complete user workflow in [specs/001-first-workflow/spec.md](specs/001-first-workflow/spec.md).
3. Fill in its [plan](specs/001-first-workflow/plan.md) as needed and track implementation in its [tasks](specs/001-first-workflow/tasks.md).
4. Use the [roadmap](docs/roadmap.md) to find subsequent features.

Agents must read [AGENTS.md](AGENTS.md) and the [repository instructions](../AGENTS.md) before working here.

## Layout

```text
project/
├── README.md
├── AGENTS.md
├── docs/
│   ├── brief.md
│   ├── roadmap.md
│   ├── architecture.md
│   ├── workflow.md
│   ├── adrs/
│   └── prompt-logs/
├── specs/
│   └── 001-first-workflow/
│       ├── spec.md
│       ├── plan.md
│       └── tasks.md
├── src/
├── tests/
└── infrastructure/
```

`src/`, `tests/`, `infrastructure/`, `docs/adrs/`, and `docs/prompt-logs/` contain only `.gitkeep` placeholders so Git retains the scaffold. Replace the placeholders as those directories gain real content. Runtime configuration and dependency manifests will live under `project/` when a stack is selected.

## Document conventions

- **Brief:** Product purpose, users, scope, and open questions.
- **Roadmap:** Ordered milestones linking to feature folders. Detailed task status lives in each feature's checklist.
- **Architecture:** The current implementation and its boundaries.
- **ADRs:** Significant decisions, with context, an alternative, and consequences. Use names such as `001-short-decision.md`.
- **Feature folders:** `spec.md` defines behavior, `plan.md` describes the approach, and `tasks.md` records work and evidence. Rename `001-first-workflow/` to describe the actual workflow once it is chosen, updating its links.
- **Prompt logs:** Selected teaching evidence, named by week or feature. The [shared evidence template](../infrastructure/template/docs/prompt-logs/TEMPLATE.md) is available for reuse.

## Setup, run, and checks

No runtime, dependencies, build, or application tests are configured. Add exact commands here when the first implementation introduces them. For now, review Markdown links and use `git diff --check` from the repository root for tracked changes.

## Classroom checkpoints

Lessons link to this application and its feature documents. Use Git tags such as `demo/week-02-start` and `demo/week-02-end` when saving actual classroom checkpoints; no checkpoint tags have been created by this scaffold.
