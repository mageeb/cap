# Week 3: From AI Chat to Agentic Development

## Learning Goals
Move from in-the-loop conversational coding to structured terminal-agent workflows. Students should be able to explain the value and risks of CLI agents; distinguish hooks, skills, project instructions, and subagents; create and use goal/spec/plan/task artifacts; persist project context across sessions; use Git worktrees for parallel work; and complete a change through review, simplification, verification, and a pull request.

## Recommended 90-minute Run of Show

| Segment | Time | Presentation slides / emphasis |
|---|---:|---|
| Bridge from Week 2 | 4m | Prior agent practices; today's terminal workflows, feature artifacts, and worktrees |
| CLI coding agents | 12m | Tool loop, GitHub CLI access, resume, model/effort, Plan mode, permissions/YOLO, harness, MCP |
| Hooks | 5m | definition, event → action, common uses, lint-after-edit example |
| Agents & subagents | 6m | isolation, delegation, parallelism, when not to delegate |
| Spec-driven development + durable context | 14m | goal → spec → explore → plan → tasks; context files; session recovery |
| Skills + worktrees | 8m | reusable procedures; worktree mental model and commands |
| Live Excalidraw demo | 25m | choose **one**: Focus Mode or Presentation Mode |
| Bonus: coding benchmarks | 6m | SWE-bench, Terminal-Bench, score caveats |
| Bonus: Boris Cherny | 7m | origin, terminal design, workflow, resources |
| Recap / questions | 3m | seven takeaways |

**If time is tight:** skip the benchmark comparison table discussion and/or shorten the Boris material. Do not shorten the live demo verification and review steps; those are central to the course message.

## Teaching Through-Line

Week 2 practiced prompts, context, project instructions, subagents, reusable review skills, and verification during a live application build.

Week 3 builds on those practices with terminal workflows, hooks, explicit feature artifacts, saved project state, and Git worktrees:

> Goal → spec → explore → plan → tasks → implement → verify → review → PR

The module should never imply that “agentic” means unsupervised. The point is to make the work more structured, persistent, testable, and delegable.

<!-- Slide 3 revision: replaced the interaction/delegation split; framing pending user review in [DEC-003](../../../DECISIONS.md#dec-003-module-03-transition-slide). -->

## Instructor Demonstration Plan (25m)

Choose one live follow-along path:

- [Demo A — Selection Focus Mode](demo-focus-mode.md)
- [Demo B — Frame Presentation Mode](demo-presentation-mode.md)

Have students complete [pre-class setup](pre-class-setup.md) first: fork and `git clone` Excalidraw, install dependencies, and verify the app. In class, introduce either demo; students confirm their fork remote, create a feature branch, and follow the matching runbook's exact prompts in order:

1. Create a feature branch, open the already-running app, and enter built-in Plan mode.
2. Paste the runbook's repository-exploration prompt; compare 2–3 approaches with cited evidence.
3. Choose an approach, leave Plan mode, and paste the goal/spec prompts in sequence.
4. Paste the runbook prompt to explore code and create `PLAN.md` and `TASKS.md`.
5. Implement one task at a time, observe the running app, and run focused checks.
6. Review the diff, fix supported findings, simplify, and re-verify.
7. Save project context and PR notes; inspect/stage changes, commit, push, and open a PR against each student's own fork.

## Live debugging

When an AI step fails, keep the class at the same step, read the output, reproduce the problem, inspect the relevant code, and agree on a small fix. The demo runbooks include a short debugging prompt.

## Key Instructor Messages

### CLI
A CLI agent is valuable because it shares the engineering environment: files, shell, Git, tests, build tools. The useful change is not the text UI itself; it is the **tool-and-feedback loop**.

After the Claude/Codex comparison, show the session picker and model/effort controls. Resume restores conversation history; inspect current Git state before continuing. Distinguish model capability from reasoning effort, and show available choices rather than prescribing a fixed model ranking. Keep this within the CLI segment; the later context section covers fresh-session handoff.

<!-- CLI controls addition: placement and teaching choices pending review in [DEC-004](../../../DECISIONS.md#dec-004-module-03-cli-session-and-model-controls). -->

Introduce Plan mode after model/effort selection: inspect → propose → review → implement. Show Claude's Plan indicator and Codex `/plan`. Chat can produce plans, and some graphical coding-agent interfaces also have dedicated Plan modes. Compare repository access and execution controls; avoid claiming planning is exclusive to a CLI. Keep the control walkthrough brief and use the later spec/plan section for depth.

<!-- Planning-mode addition: framing pending review in [DEC-005](../../../DECISIONS.md#dec-005-module-03-planning-mode-and-chat-comparison). -->

After permissions, explain YOLO as bypassing execution safeguards to reduce approval interruptions during long or batch runs. Distinguish permission prompts from sandbox boundaries, explain the tool-specific flags, and point out that worktrees provide no security isolation. Discuss the examples without enabling bypass on the classroom machine.

<!-- YOLO addition: framing pending review in [DEC-006](../../../DECISIONS.md#dec-006-module-03-yolo-mode). -->

Close the CLI section with the harness diagram: trace a prompt through the CLI to the runtime, a model tool request back to the runtime, and a tool result into the next model call. Explain that the harness manages context, tools, permissions, and sessions; the model supplies reasoning. Use this overview to connect the upcoming hooks, skills, and subagents sections.

Introduce MCP as the standard interface a CLI host uses to connect external services. Trace the path: CLI host/client → MCP server → service. Show the OpenAI docs server commands on the slide; explain that MCP connects tools/context while skills provide reusable instructions. Mention GitHub, Figma, and monitoring as other examples, and ask students to inspect server access before using it.

For the demo's initial comparison, show Claude's built-in `Read` and `Bash` search tools (`Glob`/`Grep` where available), or Codex's shell-based file reading/search. Plan mode controls the workflow; tools gather evidence; skills supply reusable instructions. Choose the approach, leave Plan mode, then explicitly request goal/spec documents only. That transition permits document edits without authorizing feature implementation. See the [reference workflow](reference.md#compare-approaches-with-built-in-capabilities).

<!-- Harness depiction: [DEC-007](../../../DECISIONS.md#dec-007-module-03-coding-harness-diagram), pending user review. Built-in replacement workflow: [DEC-008](../../../DECISIONS.md#dec-008-remove-the-custom-approach-comparison-skill), redirected by the user. -->

### Hooks
Keep the main lesson to two slides: what a hook is, how an event triggers a configured action, typical uses, and a lint-after-edit example. The harness runs the configured action when its matching event occurs. Configuration details and operational guidance are in the Appendix at the end of the deck, after both bonus sections and the closing slides; use them for questions or later reference rather than the core walkthrough.

<!-- Hooks simplification and appendix placement: [DEC-009](../../../DECISIONS.md#dec-009-hooks-basics-and-appendix), pending user review. -->

### Subagents
The main benefit is **context isolation and task focus**, not merely “more AI.” Parallel agents are useful only when work can be decomposed cleanly.

### Spec-driven development
Separate product decisions from implementation decisions. A spec says what behavior must exist; codebase exploration reveals existing architecture; a plan maps requirements onto that architecture; tasks make progress observable.

### Durable context
The repository should contain enough state for a fresh session—or another AI—to continue. Store decisions, current status, evidence, and next steps; do not store a giant transcript.

### Worktrees
Branches isolate history. Worktrees isolate both branch **and working directory**, which is particularly useful for simultaneous coding agents.

### Skills and plugins
Define a skill as one repeatable procedure centered on `SKILL.md`; define a plugin as an installable marketplace bundle whose contents depend on the CLI. Show the OpenAI Developers plugin paths for Claude Code and Codex, and call out that their bundled integrations differ.

### Review and simplification
AI-generated code is not “done” when it compiles. Review it against the spec, validate observable behavior, then simplify unnecessary complexity while preserving acceptance criteria.

### Boris Cherny workflow examples
At the end of Boris’s section, distinguish Claude Code’s bundled `/simplify`, `/batch`, and `/loop` skills and built-in `/goal` command from his personal `/commit-push-pr`, `/go`, and `/babysit` workflows. Explain the `/loop 5m /babysit` example as a timer invoking a personal routine. Course-provided `/goal` and `/simplify` skills may overlap by name, so direct students to inspect `/skills` for the implementation available in their CLI.

## Student Practice / Homework
See [homework.md](homework.md). Students may use Claude Code or Codex CLI and should work only in their own repositories/forks.

## Pre-class Requirements
Share [pre-class-setup.md](pre-class-setup.md) ahead of class. Students should arrive with their own Excalidraw fork cloned locally, Excalidraw running, GitHub CLI authenticated, and one AI coding CLI working.
