# Week 4 Homework: Use a Harness to Build Something Hard

## The Brief
**Build a paint app.** Or choose an equivalently demanding application: a slide editor, spreadsheet, diagram editor, or multitrack audio editor. You own the specification. The challenge is to use a harness to build a coherent product and demonstrate that you remained in control.

Use 5–10 hours. Choose a core that is ambitious but defensible in that budget. A collection of disconnected mock screens is not a complete application.

## Define Your Own Acceptance Contract
Before implementation, write a short product brief with a target user, one complete workflow, at least three interacting capabilities, saved/recoverable state, and one difficult edge case. Explain why those capabilities require coordination. Separate required scope from extensions.

For a paint app, candidates include tools, selection, layers, undo/redo, and save/load. For a slide editor, consider slide organization, object editing, presentation, and persistence. These are prompts for your specification, not a requirement to implement every feature.

## Harness Requirement
Choose a script-driven, instruction-driven, or hybrid harness, including an instructor-provided setup if useful. You may adapt an existing orchestrator; buying a specific product or writing one from scratch is not required. A single interactive supervisor with native delegation is valid when it meets the responsibilities and handoff requirements below.

- Define at least two distinct agent responsibilities and a coordination/integration responsibility. They may execute sequentially; concurrent agents are not required.
- Provide each responsibility with a task contract: inputs, outputs, context, tools, ownership, acceptance checks, and stop conditions.
- Demonstrate an actual handoff and a combined result. Merely naming personas or opening two chats is insufficient.
- Establish test/check commands, time/usage limits, retry limits, and human intervention points.
- Show on-the-loop execution: agreed work progresses between checkpoints, while you review evidence and intervene when needed.
- Review generated coordination code before executing it. Record changes to task scope, tests, and integration decisions.

## Suggested Budget
Specification and role design (1–2h); harness setup and a first working slice (1–2h); coordinated implementation (2–4h); independent validation and submission (1–2h).

## Submission
A GitHub repository and PR with:
- Application source and a README with reproducible setup, run, and check commands.
- Product brief and final acceptance table: met, partial, or unmet with evidence.
- Harness scripts/configuration/workflow instructions, role contracts, and dependency/ownership diagram or table.
- `docs/prompt-logs/week-04.md` using the [shared template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md), with selected session excerpts showing a handoff, checkpoint, and intervention or justified non-intervention.
- A short product demo and architecture walkthrough. Explain what you trusted, checked, corrected, reused, and left incomplete.
- Evidence of a meaningful failure-path or integration check. If the run had no observed defect, test a deliberately adverse condition and label it as such.

All instructor references are available publicly to students. Disclose reuse; explain your own architecture and verification. Submit before the next session.

## Assessment
| Criterion | Weight |
|---|---|
| Coherent working product and acceptance evidence | 30% |
| Useful agent contracts, handoffs, and harness operation | 25% |
| Independent checks and integration integrity | 25% |
| Engineering defense, supervision decisions, and honest limitations | 20% |

Agent count, impressive personas, elapsed run time, and code volume are not quality scores. A documented partial result earns credit for demonstrated criteria but is not full completion.

For the engineering defense, use the [pattern comparison](reference.md#choosing-a-pattern):
identify who owns execution and task status, how worker context reaches the supervisor,
and how a blocker or user decision reaches the right recipient. Explain how an approved
scope change would be applied and which recovery or notification capabilities your
setup actually implements. Defend why your chosen pattern is sufficient; a separate
controller or live notification system is not an additional assignment requirement.

## What Happens in Week 5?
You may continue this project if it fits the production curriculum or start a new application. Week 4 does not require a specific backend, database, or migration framework. Retain the workflow lessons and artifacts either way.
