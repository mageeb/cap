# Week 7 Homework: Containers, PostgreSQL, and Delivery

## Goal and Starting Point
Containerize the production project, verify it on PostgreSQL, and add one safe schema evolution. Extend CI to build the app image; document how the application and schema versions remain compatible.

Bring the Week 6 architecture/schema/API proposal. Containerization additionally requires a runnable application, passing local checks, and an initial migration; Weeks 5–6 do not claim to produce that code. Use an instructor-supplied baseline or a separately implemented approved project, and record the remaining gap if your own baseline is not ready. The 5–10 hour assignment budget covers containerization and migration work, not an unplanned full application build. Install Docker before class; record setup blockers explicitly. Verify an access policy and enforcement tests before any public deployment.

## Required Acceptance Evidence
- A repeatable multi-stage image build and a local Compose run with app and PostgreSQL separated.
- Database credentials provided through runtime configuration; local example credentials are clearly local-only.
- Initial migrations work on an empty PostgreSQL database, and the relevant API/integrity tests run against PostgreSQL.
- A follow-up migration preserves pre-existing IDs, values, and relationships; include before/after evidence.
- A restart demonstration retains data; explain the difference between app containers and the database volume.
- A CI image build and a release sequence with stop conditions and a non-destructive recovery plan.

## Work Budget: 5–10 Hours
- Planning and baseline review: 1–2h.
- Main implementation or exercise: 2–4h.
- Independent checks and repair: 1–2h.
- Demonstration, reflection, and submission: 1–2h.

Report each required criterion as met, partial, or unmet with evidence. Prioritize broken core behavior before optional extensions.

## Submission
Submit Docker/Compose configuration, the migration, PostgreSQL check results, restart evidence, a CI build URL, and `docs/prompt-logs/week-07.md`. Screenshots of a running container alone are insufficient.

Use the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md). Share the PR/repository link to your own repository before the next session. Disclose reused material and explain your contribution.

## Assessment
The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) applies: context management, validation and correction, structural oversight, and system integrity, each worth 25%. Assess these against the required evidence above; missing evidence keeps the affected score provisional.

## Next
Week 8 deploys the tested image with managed persistence and an explicit release/access decision.
