# Week 7: Containers, PostgreSQL, and Delivery

## Teaching Purpose
Reproduce the application outside the development machine and make database changes without losing the evidence or data the system depends on.

## Entry Check
Bring the Week 6 architecture/schema/API proposal. Containerization additionally requires a runnable application, passing local checks, and an initial migration; Weeks 5–6 do not claim to produce that code. Use an instructor-supplied baseline or a separately implemented approved project, and record the remaining gap if your own baseline is not ready. The 5–10 hour assignment budget covers containerization and migration work, not an unplanned full application build. Install Docker before class; record setup blockers explicitly. Verify an access policy and enforcement tests before any public deployment.

## Exit Evidence
A containerized app connected to PostgreSQL, with explicit migration steps, data-preservation evidence, and CI image build checks.

## Session Plan (180 minutes)
Core instruction (75m), an instructor demonstration (45m), and student practice/review (60m). The assignment is in [homework.md](homework.md).

## Core Instruction
### 1. Build and runtime boundaries (25m)
Inspect a multi-stage Dockerfile in the chosen classroom project: Node builds static React assets; Python serves the API and assets on the same origin. Dependencies used at build time differ from runtime dependencies. Keep secrets out of layers and inspect the runtime user.

### 2. Database transition and migrations (25m)
Run migrations on PostgreSQL, seed records, and verify constraints and query behavior. SQLite results alone do not establish PostgreSQL behavior. Add a nullable field, upgrade against existing data, and compare IDs and relationships afterward.

### 3. Delivery and recovery (25m)
Separate image build, migration, and app startup. Define health checks, rollout verification, and what happens if migration or startup fails. Do not have every replica race to run migrations. A rollback may require forward repair rather than a destructive downgrade.

## Instructor Demonstration Plan (45m)
Prepare and walk through a Compose file for the chosen classroom project, build the image, start PostgreSQL, run the initial migration and seed as one-off commands, then start the app. Create an item and restart only the app to demonstrate persistence. Apply an additive description-column migration in a disposable database and compare saved records before and after.

## Student Practice and Review (60m)
Trace configuration and networking (15m), run the Compose sequence in their project, pairing with a classmate if Docker is unavailable (30m), then review migration/recovery plans (15m).

## Misconception to Address
Change the database hostname to an invalid service name in a disposable environment. Read the failure and fix the configuration. Contrast this with schema mismatch: identical symptoms at the UI may require different repairs.

## Connection to the Next Stage
Week 8 deploys the tested image with managed persistence and an explicit release/access decision.

## Further Reading
[Docker multi-stage builds](https://docs.docker.com/build/building/multi-stage/) · [Compose startup order](https://docs.docker.com/compose/how-tos/startup-order/) · [Alembic tutorial](https://alembic.sqlalchemy.org/en/latest/tutorial.html)
