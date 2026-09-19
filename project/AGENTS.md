# Classroom project instructions

These instructions apply to `project/` and its descendants, alongside the [repository instructions](../AGENTS.md).

## Before working

1. Read [README.md](README.md) for the current setup and project layout.
2. Read [docs/brief.md](docs/brief.md) for product scope and [docs/roadmap.md](docs/roadmap.md) to locate the relevant feature.
3. Read that feature's `spec.md` and `tasks.md`, plus its `plan.md` when it contains an implementation approach. Consult architecture and ADRs when the task touches those decisions.

## Specifications and implementation

- Develop one continuing application here. Keep weekly teaching agendas in `../curriculum/` and link to the relevant feature or checkpoint.
- Keep each feature's requirements, implementation plan, and tasks together in `specs/NNN-short-name/`. Use stable, increasing numbers and descriptive names.
- Define observable acceptance criteria before implementing behavior. Include a relevant invalid-input or error case.
- This scaffold leaves the product details and stack undecided. Treat `TBD` entries as open questions, not accepted requirements. Resolve material product choices from the user's instructions before implementing dependent behavior.
- Work in small, explainable changes. Use the simplest structure that supports the current feature; add dependencies and application layers when the work requires them.
- Update the relevant spec when requested behavior changes. Keep `docs/architecture.md` accurate and record significant technical decisions, alternatives, and consequences in `docs/adrs/`.

## Spec-driven workflow

- For a new feature or material behavior change, follow: clarify the outcome → write observable acceptance criteria → plan → break down tasks → implement → verify against the spec → review → commit.
- Give acceptance criteria stable IDs such as `AC-1`. Link each implementation task to the criteria it serves and describe how to verify it.
- Keep the process proportional: small fixes can update an existing spec or task instead of creating a new document set. Changes to prose alone do not need an application feature spec.
- Done means the requested criteria are satisfied, relevant checks pass, the pre-commit review has no supported blockers, and affected docs and task status reflect the actual result. Mark partial work explicitly.
- When a workflow plugin is used, keep `specs/NNN-short-name/` as the canonical feature record. Point the workflow at the existing spec, plan, and tasks; adapt its output paths where supported instead of maintaining duplicate plans. If adopting a different specification system, migrate the records and update these instructions together.

## Progress and evidence

- Keep task checkboxes current. Mark implementation tasks complete only when their acceptance checks pass; link to code, tests, or recorded evidence.
- At a handoff, leave the next step and unresolved issues in the relevant `tasks.md` so a fresh session can continue.
- Record useful teaching evidence in `docs/prompt-logs/`, using the [shared template](../infrastructure/template/docs/prompt-logs/TEMPLATE.md) when helpful. Capture relevant prompts, corrections, and actual check results without secrets or private data.
- Update README setup, run, and check commands when introducing tooling. No application commands exist yet; do not claim to have run them.
