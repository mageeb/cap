# Week 8 Homework: Verify Backend CRUD and Connect a Basic Frontend

## Goal and starting point

Continue the translation-review MVP in your own repository. Implement and verify the basic create, read, update and delete operations, then connect a small frontend page to the development backend. Reuse the reviewed resource/API design from Weeks 6–7 rather than asking an agent to invent an unrelated application.

The recording's closing assignment is backend CRUD work, familiarization with Azure, and trying a basic frontend connected to the backend deployed in the development environment. The instructor offers account access; the recording does not establish that every student already has an account or that all endpoint/access details were distributed. The artifact paths, budget and assessment below adapt that direction to the course.

Read the [organized session notes](session-notes.md), [full automated transcript](transcript.txt) and [reference](reference.md). Distinguish the instructor's skeleton deployment demonstration from verified business behavior: the closing discussion explicitly says the backend logic still needs to be exercised.

## Required work

### 1. Make a bounded CRUD journey work

Identify the book/document or paragraph resources your reviewed design uses. Implement the necessary create, retrieve, update and delete behavior for a small, explicit journey. Specify identifiers, payloads, expected responses, validation and important error/access cases. Explain how operations preserve paragraph relationships, reading order and reviewer edits where relevant.

Trace one journey using synthetic or authorized examples: create a record, read the expected values, update a field/correction, read the change, and delete the intended resource. Check the resulting state rather than relying on a successful HTTP status alone. Include meaningful failures such as a missing identifier, invalid input, wrong-resource access or a dependent operation that must not corrupt existing work.

Do not treat a compiling skeleton, a healthy pod, a generic health endpoint, or passing tests against fakes as proof that the actual data flow works. Label your HTTP, storage and provider boundaries as real, simulated, or unimplemented. Report missing functionality as partial.

### 2. Connect a small frontend

Build a simple page that calls the development backend and displays real response data. Use a deliberately narrow interaction such as listing documents/paragraphs or displaying aligned source and translated text, with one useful mutation if your chosen journey supports it.

Show the configured backend address, the request/response path, a loading/empty state, and a useful failure message. Keep secrets out of browser code and source control. A polished redesign of the Week 5 wireframes is not required; the page is a verification surface for the implemented backend.

Use the instructor-provided Azure development environment only when you have approved access and the necessary endpoint details. If access is unavailable, use your own permitted development environment or verify the same frontend/backend flow locally, record the specific blockage, and label the Azure connection criterion incomplete. A local rehearsal is useful evidence but is not a completed cloud connection.

### 3. Verify tests, integration and telemetry as distinct claims

Retain and extend meaningful unit/component checks from Week 7. Add evidence that the implemented CRUD path exercises the intended service and data boundary. Report the actual commands, cases and results; explain what your fakes, test database, API client, and frontend each establish.

The live discussion asked a test agent for approximately 80% coverage and debated what matters, what costs time, and why trivial checks can inflate the metric. Report coverage if your setup measures it, including tool/configuration and gaps. There is no new universal 80% grading floor in this source-derived assignment; useful behavior and failure checks matter more than the number alone.

If telemetry is available, correlate a request with the relevant request/exception/trace evidence and explain what it proves. Seeing telemetry arrive proves collection for that activity, not correctness of every business operation. Do not invent a successful trace or claim an unavailable service was exercised.

### 4. Keep parallel work and cloud scope understandable

If you use separate agents/worktrees for tests and deployment work, give each a bounded objective, owned paths and checks. Record the base/version being tested or deployed and the integration point. Parallel completion is not automatic release readiness. Multiple agents and worktrees are useful options, not a required count or a reason to bypass review.

Explain the Azure tenant/directory, subscription, resource group and development environment you actually use or inspect. Do not provision a Kubernetes cluster or reproduce the instructor's infrastructure solely to satisfy this homework. The lesson's deployment path is a demonstration; any real resource changes require your authorized account, understood scope and a cleanup plan.

## Suggested budget: 5–10 hours

| Activity | Suggested time |
|---|---|
| Reconcile existing API/state and choose the bounded journey | 30–60 minutes |
| Implement and verify CRUD behavior | 2–3 hours |
| Connect the basic frontend and investigate failures | 1–2 hours |
| Tests, evidence, review and truthful handoff | 1.5–4 hours |

Keep the total within the course budget by limiting UI scope and reusing existing architecture. Record what remains incomplete instead of adding unrelated infrastructure or speculative features.

## Submission

Share a repository/PR link before the next session containing:

- Backend source, reviewed route/response notes and a small representative CRUD fixture/journey.
- Unit/component/integration evidence appropriate to the boundaries implemented, with actual commands/results.
- A basic frontend page and a short walkthrough of successful data flow and a meaningful failure.
- Development configuration/run instructions, deployed endpoint/version if used, and cloud-access or integration gaps.
- A brief explanation of test/coverage/telemetry evidence and its limits.
- `docs/prompt-logs/week-08.md` using the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md): selected prompts, human decisions, review findings and corrections. Disclose reused code and material.

Use a met/partial/unmet table linked to evidence. Do not publish credentials, private source text, or live customer data. Make a cloud-access blockage visible without presenting local results as hosted behavior.

## Assessment

The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) supplies four dimensions worth 25% each:

| Dimension | Evidence this week |
|---|---|
| Context management | The reviewed MVP and existing API/resource design guide the CRUD journey, frontend and scope choices |
| Validation and correction | Meaningful expected/actual checks, supported diagnoses, repairs and accurately interpreted coverage/telemetry |
| Structural oversight | Frontend, HTTP contract, service responsibilities and data boundary agree; parallel work has a controlled integration point |
| System integrity | Implemented CRUD preserves intended state/access rules; errors and missing integrations are handled and disclosed |

A deployed URL, coverage percentage, or screenshot alone is insufficient. Missing evidence keeps the affected assessment provisional. Judge demonstrated behavior and engineering understanding, not cloud spend, number of agents, or visual polish.

## Bring to the next session

Bring the working journey or coherent partial result, observed failures, and questions about the business behavior. The closing discussion proposes verifying actual features and connecting the frontend next; this homework does not establish production readiness or revise the remaining course schedule.
