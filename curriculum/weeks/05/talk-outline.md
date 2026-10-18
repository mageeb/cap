# Week 5: Product Discovery — From a Demo to a PRD

## Learning goals

Explain the business problem and the human review boundary. Identify personas and jobs, reduce scope using an MVP goal, define observable success, and turn the workflow into a PRD and annotated UX. Use AI to discover gaps and compare alternatives while retaining ownership of product decisions.

## Teaching through-line

Weeks 1–4 developed the ability to work with and coordinate agents. Week 5 asks what those agents should build. A familiar game's rules may be widely known; this business workflow depends on domain knowledge, publishing goals, and stakeholder choices that a model cannot infer reliably from the phrase “translation app.”

Use the [session write-up](session-notes.md) as source context, the [reference](reference.md) for working prompts, and the [homework](homework.md) for required evidence. The [slides](presentation.md) cover the 75-minute discussion; the full workshop below totals 180 minutes.

## Core discussion — 75 minutes

| Time | Topic and instructor move | Evidence to capture |
| --- | --- | --- |
| 0–12 | Show the prototype or walk through aligned text, editing, scripture lookup, terminology reuse, and source narration. Ask what the reviewer still decides. | Problem statement and distinction between machine draft and reviewed output. |
| 12–23 | Identify reviewer, administrator, project manager, publisher, and adjudicator. Explain that personas are application users, not automatically book authors or readers. | Persona-to-job table. |
| 23–35 | Set a goal: validate review of one small document. Reduce five roles to one reviewer with sufficient authority. Separate reviewed text from published ebook. | MVP objective, included/excluded scope, open limits. |
| 35–50 | Break “review and edit” into upload, translation request, paragraph comparison, editing, saving/resuming, failure handling, and completion. | Happy path, failure path, and testable acceptance examples. |
| 50–63 | Favor web delivery for accessibility; compare technology categories and reuse versus fresh design. Challenge an AI recommendation without choosing a class stack. | Decision factors and unresolved architecture questions. |
| 63–75 | Ask AI to interview the owner, draft a PRD from reviewed evidence, then sketch the UX. Explain individual homework and the architecture handoff. | PRD outline, annotated screens, questions for next session. |

### Demonstration emphasis

The product assists qualified humans; fluent wording alone does not establish theological accuracy. Use the recorded “old man” versus “old self” example and exact scripture-version wording to make the difference concrete. Do not reconstruct unclear names or book titles from the transcript.

Show source and target text together. Distinguish the source language text from the original machine-generated target and the edited target; the debate about simplifying change-history UI did not mean deleting the source-language view.

Describe the reported 4,000 versus 200 words/hour as a personal experience claim. It motivates workflow improvement but is not an acceptance threshold. The repeated 40,000 figure in the transcript is inconsistent and should not be repeated as fact.

### MVP judgment

Paragraphs were the working review unit. One small document and one reviewer were the working scope. The proposed page limits varied; require students to choose and label their own limit. Persistence and failure handling belong in the core experience. Rich diffs, exact completion granularity, and publication formatting need separate scope decisions.

Ask: “What does the smallest useful experiment prove?” Then test each feature against that goal. Removing an independent publisher role does not eliminate the future publishing vision. Deferring a separate administrator interface does not make public write access safe or establish an authentication design.

## Guided discovery demonstration — 45 minutes

1. **Frame the problem (5m).** Supply the reviewed source summary and ask the assistant to restate the objective, users, assumptions, and unknowns. Stop it from coding.
2. **Interview the owner (10m).** Use the reference's discovery prompt. Answer a small set of scope, input, progress, quality, and failure questions. Mark answers as instructor choices for this demonstration, not historical class decisions.
3. **Draft and challenge the PRD (10m).** Generate a short PRD. Look for added roles, invented limits, a mandatory stack, or publication sneaking into the MVP. Correct supported problems.
4. **Sketch the workflow (10m).** Produce upload/status, paragraph review, and completion sketches. Walk through a translation failure and leaving/returning after an edit. Annotate behavior; do not present static sketches as working persistence.
5. **Review the handoff (10m).** Trace one need through a requirement, screen, and acceptance check. List architecture questions and evidence still needed. Save the corrected artifacts and review record.

Use ordinary prose and sketches; no CLI-specific commands or special model features are necessary. Prompts are suggestions, not guaranteed outputs. Label prepared material and actual observed behavior accurately.

## Student workshop — 60 minutes

- **15m:** Draft the problem, persona table, MVP goal, and exclusions individually.
- **20m:** Use AI to interview for gaps; draft the PRD and measurable checks.
- **15m:** Sketch happy, failure, save/resume, and completion paths.
- **10m:** Peer review. Identify one contradiction or missing decision, revise it, and record remaining uncertainty.

## Misconceptions to address

- A machine draft is not automatically a publishable translation.
- “Reviewer” is a product user; it is not an agent role from the orchestration lesson.
- An MVP is an experiment with a goal, not the first arbitrary handful of features.
- A PRD defines product needs and behavior; it is not a task queue or deployment plan.
- A model's confident recommendation does not settle architecture or stakeholder intent.
- A mockup can demonstrate an intended flow; it cannot prove runtime behavior.

## Exit check and next stage

Ask each student to explain one included feature, one deliberate exclusion, one failure path, and one open decision. See the homework for artifact paths and grading.

The next discussion is architecture. Week 6 starts from the PRD/UX and asks students to compare database choices, defend a schema, and define book/paragraph API routes. Service internals and detailed work items follow later. Neither Week 5 nor Week 6 claims a working implementation, migration, or CI run; later implementation exercises require an explicit runnable baseline.
