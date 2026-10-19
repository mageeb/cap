# Week 8 Homework: Deployment and Operating Boundaries

## Goal and Starting Point
Deploy the production project as a bounded demonstration. Use either authenticated/authorized writes with verified access tests or a read-only public mode. Do not expose unauthenticated write endpoints.

Bring a working container image, PostgreSQL migration evidence, and a reviewed access policy with verified enforcement. Instructor rehearses one cloud path; students arrange cloud access and a personal budget before class. A cloud account is additional to the day-one LLM requirement.

## Required Acceptance Evidence
- A reachable demonstration URL, or documented cloud-access blockage plus a local release rehearsal; the latter is partial, not a completed deployment.
- Managed persistent storage and migration results, with restart/persistence evidence.
- An access policy enforced in the deployed environment, including a forbidden-operation check.
- Documented deployer/runtime/user identity distinctions, narrow grants, and secrets supplied outside source/image layers.
- Budget alert and scaling-limit evidence plus cleanup steps; acknowledge that alerts do not stop charges.
- A release record with image/version, smoke checks, relevant logs, rollback/forward-repair plan, and known limitations.

## Work Budget: 5–10 Hours
- Planning and baseline review: 1–2h.
- Main implementation or exercise: 2–4h.
- Independent checks and repair: 1–2h.
- Demonstration, reflection, and submission: 1–2h.

Report each required criterion as met, partial, or unmet with evidence. Prioritize broken core behavior before optional extensions.

## Submission
Submit the URL, release/runbook document, sanitized configuration evidence, deployment checks, and `docs/prompt-logs/week-08.md`. Never include credentials or raw customer data.

Use the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md). Share the PR/repository link to your own repository before the next session. Disclose reused material and explain your contribution.

## Assessment
The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) applies: context management, validation and correction, structural oversight, and system integrity, each worth 25%. Assess these against the required evidence above; missing evidence keeps the affected score provisional.

## Next
Week 9 rehearses incident response in a disposable environment using the same release and observability habits.
