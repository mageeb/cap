# Repository instructions

## Must-follow directions in priority order

1. **Optimize for human time.** Use relevant workflows and delegate independent work to subagents proactively when doing so reduces elapsed time or human effort. Integrate and verify their results.
2. **Protect quality.** For every completed change, run relevant checks, review the result, and perform an adversarial pass that challenges assumptions and looks for failure cases. Add or update meaningful tests for executable behavior; check accuracy, consistency, and links for prose. Be opinionated: make clear recommendations and distinguish supported blockers from optional improvements.
3. **Keep it simple.** Choose the simplest solution that satisfies the requirements. Treat unnecessary complexity as a reason to revise the design.
4. **Optimize cost after human time and quality.** Err on the side of premium models when reasoning or correctness matters. Use cheaper models for bounded tasks they can handle reliably, accounting for retries and review effort.

### Task sizing

- Size each planned task for **2–5 minutes**, including its relevant verification. Split anything estimated to exceed five minutes before starting; do not pad work that naturally takes less than two minutes.
- Give each task one concrete outcome and a clear completion check. Organize larger features as groups of these small tasks.
- If a task grows beyond its estimate, split the remaining work into smaller tasks. Do not skip validation to meet a time estimate.

### Communication

- Be concise and direct. Err on the side of brevity; omit fluff, repetition, and unnecessary background.
- Address the user as a technically fluent college graduate. Use precise technical terms when useful, with plain language and no unnecessary jargon.
- End **every user-facing message**, including progress updates, with a short **TLDR:** summary.

## Scope

These instructions apply throughout this repository. Read them at the start of each task. Follow any additional `AGENTS.md` instructions within the area you work on.

## Start with the right context

- Before working under `project/`, read [project/AGENTS.md](project/AGENTS.md), including when the session starts at the repository root.
- For curriculum work, read the relevant week's README and requested material. Use the [course overview](CAP%202.0%20AI-Native.md) for program requirements and [delivery plan](conductor/plan.md) for instructor preparation.
- After a context reset or compaction, reread the applicable instruction files and the current task's spec or checklist before continuing.
- Read the files relevant to the task; follow links when needed instead of loading the whole curriculum or every specification.

## Repository map

- `curriculum/weeks/`: Weekly lessons, presentations, homework, reference procedures, and isolated teaching examples.
- `project/`: The continuing classroom application's product documents, feature specs, tasks, code, tests, and deployment configuration.
- `infrastructure/template/`: Reusable student starter material; application-specific infrastructure belongs in `project/infrastructure/`.
- `conductor/plan.md`: Course preparation and delivery decisions; application tasks belong with their feature specs.
- `tools/`: Teaching and assessment support.

## Working conventions

- Keep curriculum organized by week and the application organized by feature. Link lessons to project files and Git checkpoints instead of copying the application into weekly folders.
- Preserve the user's existing changes and keep edits focused on the requested work.
- Keep project instructions concise and current. Put product requirements, plans, and task progress in their dedicated documents and link to them.
- Distinguish proposed work, prepared examples, and verified results. Do not invent completed checks, demonstrations, or product decisions.
- When editing curriculum, keep the lesson, assignment, and assessment aligned. Repair affected relative links after moves or renames.
- For Mermaid diagrams, follow [.github/instructions/mermaid.instructions.md](.github/instructions/mermaid.instructions.md).

## Branches and commits

- Before editing, inspect `git status` and the current branch. For each new feature or independent change, create a descriptive `feature/<short-name>` branch from the intended base. Continue on that branch for follow-up work on the same task.
- Never edit or commit directly on `main` or `master`. If a task starts there, create the feature branch first, preserving existing work. Do not reset, discard, or include unrelated changes to make the branch clean.
- Make each commit one coherent change. Aim for **100 changed lines or fewer**, counting additions plus deletions in handwritten code and documentation. Keep required tests with the behavior they verify.
- Split larger work into sensible commits when possible. If an inseparable change exceeds the target, explain why in the commit body. Identify generated files, lockfiles, and mechanical moves separately; do not distort code or leave a broken intermediate state to hit the target.
- For a task that produces multiple commits, format every subject as `<number>/N [<task-tag>] [<project-tag>] <title>`. Use the literal uppercase `N`, not the total commit count. Start at `1/N` and increment for each commit in that task, including across sessions.
- Keep both tags consistent within a series: the first identifies the task, and the second identifies the application or project. For the classroom application, use `[project-tbd]` until a project tag is agreed, then use the agreed tag for new series; do not use `[cap]` as its project tag.
- Example: `1/N [project-scaffold] [project-tbd] Add product brief`. If a task grows beyond one commit, update its earlier unpushed commit subject to follow the series format.
- Stage only intended files or hunks. Inspect `git diff --cached` and `git diff --cached --numstat` before committing; the staged contents are the unit being reviewed.

## Before every commit

1. Run relevant validation against the changes intended for the commit. Checks that depend on additional unstaged work do not establish that the commit is valid.
2. Review the exact staged diff against the request and acceptance criteria. For executable changes, use [review-with-refuter](curriculum/weeks/02/skills/review-with-refuter/SKILL.md), or another code-review skill explicitly selected for the task. If the skill cannot run, perform and report a manual correctness and regression review.
3. For documentation-only changes, perform a sanity review of accuracy, scope, internal consistency, local links, and unintended edits. Run `git diff --cached --check` for all commits.
4. Resolve supported blockers and missing required checks before committing. Optional feedback does not require a fix. Recheck the staged diff after any revision.
5. Include a concise `Review:` and `Checks:` summary in the commit body, using actual results. State a size exception there when needed; do not claim an independent review unless one occurred.

## Validation

- For documentation and scaffold changes, check the diff, whitespace, and affected local links. No application tests are needed for empty directories or prose alone.
- For executable changes, inspect the relevant project's configuration and README, then run the checks appropriate to the changed behavior.
- Report what changed, what was checked, and anything still unresolved. Record actual commands and results when they are part of teaching evidence.
