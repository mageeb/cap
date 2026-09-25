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
- Before escalating a question to the user, run [panel](skills/panel/SKILL.md): three blind generalists evaluate the whole question with private emphases on user value, implementation, and assumptions and risks. Round 1 uses the same neutral packet, differing only in the recipient's own emphasis, with no inherited conversation, peer disclosure, or coordinator preference. Share complete answers only in round 2 if needed, optionally with a labeled coordinator view; stop after two rounds and use the coordinator's own judgment. If it resolves an authorized choice, log the provisional decision and continue; panel agreement never counts as user approval.
- If the user's input, clarification, preference, or approval is still needed, use the available Ask User Question tool. Put the actual question, necessary context, and concise choices in that prompt; do not leave a request only in ordinary prose. Reuse an existing panel when reviewing its logged decision instead of rerunning it; still ask when the user explicitly requests that review.
- Use a blocking question tool when available. If only an asynchronous question tool is available, keep the question pending and pause dependent work until the user answers; continue only independent work. Silence, elapsed time, and preselected choices are not answers. If no question tool is available, explain the limitation and leave work requiring an answer paused.

### Autonomous decisions and user review

- Record every choice you make that is not already settled by the user's instructions or confirmed project records in [DECISIONS.md](DECISIONS.md). Record it when made, with a stable ID, task or scope, choice, reason, alternatives, affected files or behavior, and technical verification evidence. Mark it **Pending user review**, even if checks pass. Carrying out an already agreed choice is not a new decision.
- Read the pending decisions relevant to your task at session start and after compaction. Keep them visible at handoff by linking their IDs from the relevant task record; do not rely on chat history.
- Continue authorized work with provisional choices when user input is not required. Recording a decision does not replace asking for required input, and technical verification does not establish user confirmation.
- When the user asks to review autonomous decisions, present **one decision at a time** through Ask User Question, including what was chosen, why, its effect, and options to confirm, redirect, or defer. Wait for the answer before presenting the next decision.
- Record each response: **Confirmed** only after explicit confirmation, **Redirected** with the user's replacement direction and any follow-up work, or **Pending user review** if deferred. Preserve the original choice and record actual follow-up results; do not mark a requested correction complete before verifying it.

## Scope

These instructions apply throughout this repository. Read them at the start of each task. Follow any additional `AGENTS.md` instructions within the area you work on.

## Start with the right context

- Before working under `project/`, read [project/AGENTS.md](project/AGENTS.md), including when the session starts at the repository root.
- For curriculum work, read the relevant week's README and requested material. Use the [course overview](CAP%202.0%20AI-Native.md) for program requirements and [delivery plan](conductor/plan.md) for instructor preparation.
- After a context reset or compaction, reread the applicable instruction files and the current task's spec or checklist before continuing.
- Read the files relevant to the task; follow links when needed instead of loading the whole curriculum or every specification.

## Repository map

- `curriculum/weeks/`: Weekly lessons, presentations, homework, reference procedures, and isolated teaching examples.
- `skills/`: Maintained workflows used in this repo; classroom comparison examples stay under `curriculum/`.
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
- Keep each PR focused on one coherent change. A multi-commit PR contains one numbered series; different series belong in separate PRs, even when their branches share history.
- For a single-commit PR, use `[<task-tag>] [<project-tag>] <title>` without a numeric prefix. For a multi-commit PR, use `<number>/N [<task-tag>] [<project-tag>] <title>`: start at `1/N`, use the literal uppercase `N`, and increment consecutively across tasks and sessions without restarting inside the PR. If the PR grows beyond one commit, number the first commit too; if it becomes a single commit, remove its prefix. Fold defect corrections into their introducing commits as described below.
- Keep both tags consistent within a series: the first identifies the task, and the second identifies the application or project. For the classroom application, use `[project-tbd]` until a project tag is agreed, then use the agreed tag for new series; do not use `[cap]` as its project tag.
- Multi-commit example: `1/N [project-scaffold] [project-tbd] Add product brief`. Single-commit example: `[pr-series] [project-tbd] Define commit numbering rules`. A separate multi-commit panel PR starts its own `1/N [panel] [project-tbd] ...` series.
- For dependent work, stack PRs: branch from the prerequisite branch and target that branch as the PR base, so each PR shows only its own series. Record dependencies in PR descriptions and verify the actual base, commit list, and diff before and after publishing or retargeting. Retarget remaining PRs as prerequisites land, checking that earlier series have dropped out of their diffs. Creating a PR does not authorize merging it; rewriting unmerged history follows the rule below.
- Stage only intended files or hunks. Inspect `git diff --cached` and `git diff --cached --numstat` before committing; the staged contents are the unit being reviewed.

### Correct unmerged history at its source

- When a defect is discovered, identify the commit that introduced it. If that change has not merged into the default branch (`main` or `master` here), correct that commit, even if discovered later in a dependent PR. Amend it or fold a local fixup into it; do not publish a later patch commit that leaves the introducing commit defective.
- Review the whole series for avoidable add/change/delete churn. Move the proper implementation into its introducing commit and remove redundant later edits. Preserve warranted stages, such as an intentional scaffold followed by implementation, or genuinely new requirements; explain their purpose. Repeated edits alone do not establish a defect.
- Rebuild dependent commits and PR branches on the corrected history while preserving unrelated work and each PR's scope. Keep numbering consecutive. Verify the affected commits individually, including relevant checks and renewed review; a passing final tip does not establish that intermediate commits are valid. Update their `Review:` and `Checks:` evidence to match the rewritten snapshots.
- Before rewriting, verify current remote branches and PR merge status, preserve recovery refs, and check for concurrent work. Never rewrite `main`, `master`, or changes already merged there; fix merged defects through a new change. Use an isolated checkout when another session is using the shared worktree.
- This instruction authorizes rewriting the affected unmerged feature commits and updating their published PR branches when necessary for these corrections. Use explicit `--force-with-lease=<ref>:<expected-sha>` leases against freshly verified remote heads. If a lease fails or unfamiliar concurrent work appears, stop and reconcile it rather than overwriting it.
- Before pushing, resolve known defects and required verification gaps, and inspect the complete PR series and diff. After publishing a rewrite, verify remote heads, PR bases, commit lists, and diffs again. Keep recovery refs until the rewritten stack is verified.

## Before every commit

1. Run relevant validation against the changes intended for the commit. Checks that depend on additional unstaged work do not establish that the commit is valid.
2. Review the exact staged diff against the request and acceptance criteria. For executable changes, use [code-review](skills/code-review/SKILL.md): Superpowers reviewer → independent refuter → coordinator verdict. If automatic discovery is unavailable, read the linked file directly. Another skill may be explicitly selected for the task; if the required workflow cannot run, perform and report a manual correctness and regression review, naming the unavailable parts.
3. For documentation-only changes, perform a sanity review of accuracy, scope, internal consistency, local links, and unintended edits. Run `git diff --cached --check` for all commits.
4. Resolve supported blockers and missing required checks before committing. Optional feedback does not require a fix. Recheck the staged diff after any revision.
5. Include a concise `Review:` and `Checks:` summary in the commit body, using actual results. State a size exception there when needed; do not claim an independent review unless one occurred.

## Validation

- For documentation and scaffold changes, check the diff, whitespace, and affected local links. No application tests are needed for empty directories or prose alone.
- For executable changes, inspect the relevant project's configuration and README, then run the checks appropriate to the changed behavior.
- Report what changed, what was checked, and anything still unresolved. Record actual commands and results when they are part of teaching evidence.
