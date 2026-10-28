# Week 7: Planning, Verification, and Backend Practice

Turn the Week 5 product scope and Week 6 architecture discussion into reviewed work items, a verification strategy, and a minimal backend. The October 23 recording demonstrates the path from repository context to a C# service proposal, implementation work, review and durable guidance.

- [Talk outline and classroom agenda](talk-outline.md)
- [Presentation and speaker notes](presentation.md)
- [Organized session write-up](session-notes.md)
- [Full unedited automated transcript](transcript.txt)
- [Planning, verification and repository-context reference](reference.md)
- [Homework and assessment](homework.md)

## Classroom practice

Application source belongs in the continuing project, not copied into this folder:

- [Backend overview, build and regression commands](../../../project/src/Translation/Backend/README.md)
- [Approved boundary specification](../../../project/specs/002-backend-skeleton/spec.md)
- [Implementation status and verification evidence](../../../project/specs/003-backend-implementation/tasks.md)
- [Backend source and capability layout](../../../project/src/Translation/Backend/)
- [Proposed MVP work-item workbook](../../../outputs/mvp-plan/mvp-implementation-plan.xlsx)

The checkpoint includes application services and 17 deterministic service checks. It has no HTTP host or concrete input-reader, database or translation-provider adapters. Fake-based checks do not establish durable persistence or an end-to-end application. The homework asks students to continue toward a minimal backend in their own repositories and report partial results honestly.

## What you will practice

Derive a work-item hierarchy from the reviewed scope/design, define completion checks, sequence dependencies, prepare representative domain examples, review high-level interfaces, and implement small verified units. Preserve concise instructions, rationale and procedures so future sessions can continue the work.

The source's explicit homework is backend implementation with unit and component tests. A minimal frontend is optional; API-client requests or runnable tests can support verification. Rich UI, cloud deployment and a full software factory are outside the required exercise.

## Delivery and source limits

The slides support an 80-minute lesson based on the roughly 78-minute recording. The outline adds a 45-minute guided exercise and 55-minute student workshop for the three-hour course format. These are prepared teaching allocations, not evidence of a rehearsed full build.

The transcript preserves raw automated recognition, including incidental conversation and recognition errors. The notes remove those distractions and separate recording claims from fresh repository checks. Example dataset sizes, agent counts, productivity multipliers and coverage preferences are not universal course thresholds.

Bring the previous product/design artifacts, your existing agent setup, and a repository you own. Follow the documented .NET prerequisites if using the classroom C# practice; no tracker subscription, cloud account or paid model endpoint is required merely to work through the lesson with deterministic fixtures.

Preparation scope and verification are recorded in [DEC-034](../../../DECISIONS.md#dec-034-source-grounded-week-7-materials). Later weeks and their sequencing are outside this update.
