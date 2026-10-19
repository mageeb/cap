# Week 5 Reference: Product Discovery with AI

Use these prompts and templates to move from a session discussion to a reviewable product definition. They support the [lesson](talk-outline.md) and [homework](homework.md); they do not replace stakeholder answers or your judgment.

## Ground the Conversation
Use the [organized session notes](session-notes.md) for the case discussion; this reference supplies reusable methods.

Give AI the relevant discussion, the problem, users, constraints, and known uncertainties before requesting a PRD. Keep source claims separate from suggestions. A plausible completion of a missing detail is still an assumption.

For this case, the long-term vision has reviewer, administrator, project manager, publisher, and auditor/adjudicator personas. The MVP working direction is one reviewer, one small document, and paragraph-level translation review in a web application. The auditor's final name, authority boundaries, exact input limit, completion rules, history behavior, metrics, and stack are not settled.

## Requirements Interview Prompt

```text
Act as a product-discovery interviewer. Use the supplied Session 05 material.
First summarize the problem, five personas, one-reviewer MVP, and known unknowns.
Label each statement: source working direction, proposed decision, or open question.
Interview me about the highest-impact gaps before drafting the PRD.
Ask one focused question at a time and explain which workflow it affects.
Cover input format and size, translation trigger and failure recovery, paragraph
alignment, editing, save/resume, completion, and minimum draft/history behavior.
Do not select a stack, invent stakeholder answers, or turn desktop demonstrations
into mandatory MVP features. Keep unanswered items visible.
```

If no stakeholder is available, record a provisional design choice and the question it answers. Do not describe your own answer as confirmation.

## Compact PRD Template

```markdown
# Product requirements: Translation Review
## Problem and product objective
Who struggles, with what task, and why does machine output need qualified review?
## Vision and personas
Five roles, their jobs, and which are deferred from the one-reviewer MVP.
## MVP experiment
One small document, proposed boundary, usefulness/quality hypothesis, evidence plan.
## Core journey
Input → translate → compare paragraphs → edit → save/resume → complete.
## Requirements and acceptance
| ID | User need | Observable criterion | UX state/link | Status/source |
|---|---|---|---|---|
## Scope
Required for this experiment; deferred capabilities; reasons for the boundary.
## Risks and failure recovery
Translation quality, unavailable services, unsaved edits, interruption, misalignment.
## Proposed decisions and unresolved questions
Link the decision log; identify who or what evidence can resolve each question.
## Measurement proposal
Method, sample, candidate target, limitations, and next action after the experiment.
```

Write criteria a reviewer can inspect. For example: “When a translation request fails, the design identifies the failed step and offers recovery while preserving the source.” This is a proposed acceptance criterion, not a claim that the session specified a retry mechanism.

## Decision and Question Log

| ID | Topic | Source position | Proposal or question | Reason/alternative | Evidence or owner needed | Status |
|---|---|---|---|---|---|---|
| Q-01 | Document size | One small document; limit unresolved | Choose a bounded initial input | Keep first experiment reviewable | Stakeholder/sample review | Open |
| Q-02 | Completion | Paragraph vs document debated | Compare both interaction models | Progress clarity vs complexity | Reviewer walkthrough | Open |
| Q-03 | Draft/history | Exact behavior unresolved | Define what is retained and visible | Recovery and comparison needs | Reviewer/stakeholder answer | Open |

Add translation trigger, accepted format, save strategy, and metrics. Statuses such as open, proposed, and confirmed describe evidence; only explicit stakeholder confirmation establishes agreement.

## UX Prompt and State Template

```text
Using the supplied PRD, propose a web review flow for one reviewer and one document.
Show aligned Arabic source and English target paragraphs with in-place target edits.
Explore an interface suited to this workflow rather than copying the desktop layout.
Describe empty, loading/translating, editable, unsaved, saving, saved, translation-error,
save-error, and resumed states. Show the action, visible feedback, preserved data,
and recovery path for each transition. Flag undecided behavior explicitly.
Compare two options for one consequential interaction and explain their tradeoffs.
Output low-fidelity wireframes and trace each screen/state to requirement IDs.
Do not claim these designs have passed application tests.
```

| State | User action or event | Visible feedback | Data retained | Next/recovery action | Requirement |
|---|---|---|---|---|---|
| Translation failed | Service returns a failure | Explain failed stage without implying review is complete | Source and any existing edits | Proposed recovery choice | REQ-… |
| Save failed | Edited paragraph cannot be persisted | Distinguish unsaved changes from saved work | Specify where pending edits remain | Proposed retry/leave behavior | REQ-… |

These are design prompts. Browser reload durability, retry idempotency, and service behavior need later implementation and testing; a wireframe cannot prove them.

## Challenge the Draft

```text
Review this PRD and UX as a skeptical reviewer.
Find unsupported source claims, feature creep, contradictory completion rules,
missing failure states, and places where edits could be lost or source/target
paragraphs confused. Trace a normal review, translation failure, and interrupted save.
Separate supported blockers from optional improvements. Cite requirement/state IDs.
Suggest the smallest correction and state what remains a stakeholder question.
Do not invent findings or rewrite the product objective to make review easier.
```

Use the output as leads. Verify findings yourself, record accepted/rejected feedback with reasons, and repeat the affected walkthrough after revision.

## Compare Technologies after Product Scope

The session discussed React, ASP.NET Core/C#, Blazor, TypeScript, and Node.js without choosing a final stack. Existing C# logic may be reusable; that is an investigation, not proof that reuse or a particular web framework is best.

```text
Compare two or three viable architecture/stack options against this PRD.
Consider paragraph editing and Arabic/English layout, translation integration,
persistence/recovery, team familiarity, hosting/budget, maintainability, and possible
reuse of existing C# logic. Separate observed repository facts from assumptions.
For each option, explain benefits, costs, unknowns, and the smallest useful spike.
Challenge bias toward the existing implementation and toward a preferred framework.
Provide a provisional recommendation with conditions that would change it.
Do not present the session as having selected a stack or require implementation now.
```

Keep this a bounded comparison for the architecture discussion. Do not spend the homework budget implementing several stacks.

## Measure without Inventing Results
The session's review-speed claims were experience reports, not verified benchmarks. Its 30-page/30-minute example was illustrative, not an adopted acceptance target. Choose and label your own hypothesis, sample, and measurement method; avoid turning either example into a course requirement.

Possible questions include whether the reviewer can resume confidently, whether alignment reduces confusion, where review time is spent, and which domain errors remain. A short peer walkthrough can expose usability questions; it cannot establish translation quality, a speedup, or production readiness.
