# Week 8: Deployment and Operating Boundaries

## Teaching Purpose
Deploy a bounded demonstration that can be verified, observed, recovered, and shut down without confusing a public URL with production readiness.

## Entry Check
Bring a working container image, PostgreSQL migration evidence, and a reviewed access policy with verified enforcement. Instructor rehearses one cloud path; students arrange cloud access and a personal budget before class. A cloud account is additional to the day-one LLM requirement.

## Exit Evidence
A deployed synthetic-data demonstration, release evidence, least-privilege identity decisions, and a runbook for verification, recovery, and cleanup.

## Session Plan (180 minutes)
Core instruction (75m), an instructor demonstration (45m), and student practice/review (60m). The assignment is in [homework.md](homework.md).

## Core Instruction
### 1. Choose one deployment path (25m)
Default to a single app container on Cloud Run with managed PostgreSQL; an equivalent provider is allowed when the student maps the same responsibilities. A same-origin UI avoids an unnecessary second-host integration problem. Review durable storage and connection limits.

### 2. Identity and configuration (25m)
Separate the deployer, runtime service identity, and application users. Attach an appropriate runtime identity and grant the needed resource access. Use the provider secret mechanism for database configuration; a service-account key JSON is not an IAM policy.

### 3. Release and operational checks (25m)
Review the access mode before public exposure. Establish a budget alert, low scaling limits appropriate to the demo, startup/liveness behavior, release smoke checks, and rollback/forward-repair steps. Budget alerts notify; they do not cap spending. Record cleanup steps and retain needed evidence.

## Instructor Demonstration Plan (45m)
Use a pre-rehearsed instructor project with synthetic data. Inspect the image/version, runtime identity, secret reference, and managed database connection before deploying. Run a browser success check, forbidden-write check, restart/persistence check, and log lookup. Show the release runbook and cleanup path; use a labeled recording if cloud access is unavailable.

## Student Practice and Review (60m)
Review release readiness in pairs (15m), execute or rehearse a bounded release step (30m), and conduct a go/no-go review against the access and persistence evidence (15m).

## Misconception to Address
Demonstrate a missing runtime permission in the instructor sandbox. Identify which identity needs which resource grant rather than adding broad administrator access. Keep this separate from app-user authorization.

## Connection to the Next Stage
Week 9 rehearses incident response in a disposable environment using the same release and observability habits.

## Further Reading
[Cloud Run service identity](https://docs.cloud.google.com/run/docs/securing/service-identity) · [Cloud Run secrets](https://docs.cloud.google.com/run/docs/configuring/services/secrets) · [Budget alerts](https://docs.cloud.google.com/billing/docs/how-to/budgets)
