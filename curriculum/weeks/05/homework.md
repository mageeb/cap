# Week 5 Homework: Define the Translation Review Product

## The Brief
Prepare an extended product requirements document (PRD), UX flows, and wireframes for the translation review web application discussed in Session 05. Each participant prepares their own proposal for the same core product; compare ideas with peers, but own the whole submission.

Use 5–10 hours. The deliverable is a defensible product definition ready for the next architecture discussion. Application code, a selected stack, deployment, and a working translation service are not required this week.

## Start from the Session
Read the [organized session notes](session-notes.md) as the shared source account.

The product helps a qualified reviewer correct machine-translated Arabic religious or theological text into English. Fluent output can still distort meaning, terminology, quotations, or references. Human judgment remains part of the workflow.

Describe the full vision through five personas: reviewer, administrator, project manager, publisher, and auditor/adjudicator. The fifth role's name and exact authority remained open. Then narrow the MVP to one user acting primarily as reviewer, reviewing one small book or article in a web application.

Separate three categories throughout your PRD:

- **Session working direction:** supported by the discussion; cite the relevant topic or approximate recording time.
- **Your proposed decision:** a choice you make to produce a coherent design; explain why and identify what would validate it.
- **Open question:** requires stakeholder input or further evidence; state its effect on the design.

The session did not settle input format, document-size limits, translation trigger, completion granularity, minimum change-history behavior, success thresholds, or technology stack. Do not present your choices as agreements from class.

## Required Product Definition
Your PRD must include:

- Problem, target user, domain risks, product objective, and the relationship between the larger vision and this MVP.
- The five personas and their use cases, with a clear explanation of why only the reviewer workflow is in the first release.
- One end-to-end journey: provide source material → request translation → compare aligned paragraphs → edit target text → save → leave and resume → indicate completion.
- Observable acceptance criteria for source input, translation, paragraph alignment, in-place editing, persistence/resumption, and translation-failure handling.
- Explicit in-scope and deferred capabilities. Separate the demonstrated desktop capabilities from requirements for the new web MVP.
- A decision/question log covering the unresolved choices above, including how the reviewer distinguishes unsaved, saved, and failed-save work.
- A proposed learning experiment: what usefulness or review quality means, how to measure it, and how the result would inform the next iteration. Mark targets as hypotheses; invent no measured results.

Scripture lookup and source narration/text-to-speech were useful desktop demonstrations and future candidates. They are not required MVP features. Multi-reviewer agreement, adjudication, publication formatting, multiple concurrent books, and a trusted corpus for future model improvement belong to the broader vision unless you justify a later experiment.

## Required UX Evidence
Create a user-flow diagram and annotated wireframes. Markdown tables, sketches, or an exported design are sufficient if a peer can follow them.

Show the source and target paragraph relationship, how editing works, and how the reviewer leaves and resumes safely. Cover empty, translating/loading, editable, saved, translation-failed, and save-failed states. Explain recovery without silently discarding corrections. For one unresolved interaction, compare two alternatives and justify your provisional choice.

Walk a peer through a small synthetic or authorized sample containing several paragraphs. Ask them to trace a successful review, a translation failure, and an interrupted save. Record confusion or contradictions and revise at least one supported issue, or explain why no revision was warranted. This is a document/wireframe walkthrough, not an application test.

## Suggested Budget
Source review and AI-assisted requirements interview (1–2h); PRD and scope decisions (2–3h); UX flows and wireframes (1–3h); peer challenge, revision, and submission (1–2h). Keep the total within 5–10 hours by limiting design fidelity and feature scope.

## Submission
Submit a repository/PR link before the next session containing:

- `docs/product/prd.md` with the required definition, acceptance table, and source/decision distinctions.
- `docs/product/ux.md` plus any linked wireframe assets, with state transitions and recovery annotations.
- `docs/product/decisions.md` with assumptions, proposed choices, alternatives, and unresolved questions.
- `docs/prompt-logs/week-05.md` using the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md): selected interview exchanges, an AI assumption you checked, a challenged recommendation, and resulting corrections.
- A short walkthrough or written review record showing what the peer traced and what changed. Disclose reused materials and explain your contribution.

Report each required artifact or criterion as met, partial, or unmet with links to evidence. Protect private source material; synthetic examples are sufficient. Use the [reference prompts and templates](reference.md) as starting points, not as completed answers.

## Assessment
The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) supplies four equally weighted dimensions, applied to this week's document evidence:

| Dimension | Weight | Evidence this week |
|---|---|---|
| Context management | 25% | Accurate problem/personas and traceable separation of session directions, proposals, and unknowns. |
| Validation and correction | 25% | Challenged AI assumptions, peer walkthrough findings, and justified revisions. |
| Structural oversight | 25% | Coherent MVP boundary; aligned PRD, acceptance criteria, UX flows, and state/recovery design. |
| System integrity | 25% | Honest limitations, preserved reviewer work in the design, domain-quality risks, and a defensible measurement proposal. |

Assess documentation demonstrably: links, consistency, walkthrough records, and reasoning. Do not claim runtime correctness, production readiness, or measured performance from wireframes. More pages, more features, and more prompts do not earn credit by themselves.

## Bring to the Next Session
Bring the PRD, UX, unresolved questions, and a concise explanation of the most consequential tradeoff. The next discussion uses these artifacts to compare architecture options before substantial implementation.
