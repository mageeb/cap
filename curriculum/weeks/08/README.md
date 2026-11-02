# Week 8: Parallel Testing, Azure Deployment, and Observability

Follow Session 08 from the backend skeleton into parallel test/deployment work, container packaging, authenticated Azure operations, health checks and telemetry. Keep operational progress separate from verification of the translation-review features.

- [Talk outline and classroom agenda](talk-outline.md)
- [Presentation and speaker notes](presentation.md)
- [Organized session write-up](session-notes.md)
- [Full unedited automated English transcript](transcript.txt)
- [Technical clarifications and reusable workflow prompts](reference.md)
- [Homework and assessment](homework.md)

## What the session does

The instructor separates test and deployment worktrees, requests meaningful test coverage, prepares a container/development deployment, resolves local setup and sign-in friction, hands artifacts between sessions, and inspects health requests and application telemetry. The closing discussion explicitly leaves the business logic for further validation.

The homework continues the actual product: implement bounded backend CRUD, familiarize yourself with the authorized Azure environment, and try a basic frontend connected to the development backend. A cloud-access blockage must be reported rather than replaced with an invented success.

## Preparation and delivery

The presentation supports a 110-minute lesson based on the roughly 107-minute recording. The outline adds a 35-minute guided verification exercise and 35-minute workshop within the three-hour course format. These are teaching allocations, not a promise that a full cluster can be provisioned or every feature implemented during class.

Bring Week 7's code, setup/check commands, and the reviewed API/resource design. Azure-specific practice requires explicit access to an approved development environment and its endpoint details. No student needs to buy a new subscription or provision a separate AKS cluster merely to read or prepare this lesson. Local work can proceed while access gaps remain visible.

## Source and artifact status

The transcript preserves the final English speech-recognition output without editorial cleanup, including recognition errors and side conversation. It is an automated reference, not a certified verbatim transcript. The notes and reference clarify technical shorthand without rewriting the raw file.

The [classroom backend baseline](../../../project/src/Translation/Backend/README.md) provides prior context. The recording also uses separate testing/deployment worktrees. This curriculum update does not merge their pending application changes or independently verify the live Azure environment. Do not assume that the baseline checkout contains every container/API/monitoring artifact described in the recording.

Kubernetes is the instructor's selected demonstration path, not a universal MVP requirement. Example coverage targets, account credits, prices and capacity figures are not general guarantees. See the reference for the image/container/orchestrator, identity, readiness, forwarding and telemetry distinctions.

Preparation scope and verification are recorded in [DEC-035](../../../DECISIONS.md#dec-035-source-grounded-week-8-materials). Other weeks and the wider course schedule remain outside this update.
