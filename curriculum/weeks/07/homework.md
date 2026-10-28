# Week 7 Homework: Implement the Reviewer Backend

## Goal and starting point

Implement the smallest useful translation-review backend in your own repository. Start from your reviewed Week 5 MVP and Week 6 architecture/schema/API proposal, then use a clear work plan to guide the agents and your review.

The recording's closing assignment is backend implementation, repository setup, and meaningful unit and component tests. A minimal frontend is optional. The submission structure and assessment below adapt that direction to the course; they are not a transcript of every classroom instruction.

Read the [organized session notes](session-notes.md), [full unedited automated transcript](transcript.txt), and [reference prompts](reference.md). The [classroom backend](../../../project/src/Translation/Backend/README.md) is a reference for the same case study; inspect its actual boundaries before reuse. Do not treat follow-up repository work as proof that every behavior was implemented during the recording.

## Required work

### 1. Review the plan and its scope

Use your agents to derive or refine a hierarchy of work from the reviewed product and architecture context. Review its dependencies, completion checks, and sequence before execution. A spreadsheet, Markdown table, or tracker export is sufficient. The classroom demonstration used Excel; a paid tracker, a fixed task count, and concurrent agents are not required.

Choose a first bounded unit and explain how it contributes to the whole reviewer journey. Keep unresolved input, provider, storage, or completion policies visible rather than allowing the assistant to choose them silently. Split work so its purpose and evidence can be reviewed; do not equate the number of PRs or agents with progress.

### 2. Implement the minimal backend behavior

Provide the backend operations your reviewed MVP needs to manage a document/book, retrieve its ordered source/target paragraphs, and save review corrections and review state. Include the translation-draft request/orchestration behavior your agreed design requires; the external translation model remains an integration, not a model you must train.

The class demonstrated high-level C#/.NET interfaces before implementation. Use the agreed technology for your own proposal, explaining a deliberate change rather than treating the classroom choice as an unexplained universal rule. Review responsibilities before filling in details.

Your implementation should honor the book/paragraph relationship, intended reading order, input/error behavior, and preservation of reviewer work. Label the boundaries precisely: service methods, HTTP routes, storage adapter, provider adapter, and any fakes or stubs. A compiling skeleton is not an implemented backend. If a planned API, durable store, or provider is missing, report the corresponding criterion as partial rather than claiming completion.

Stay within the smallest reviewer MVP. Do not add publishing, full formatting, offline synchronization, a universal ebook parser, elaborate role workflows, or distributed infrastructure just because they might be useful later.

### 3. Verify with unit and component tests

Add meaningful isolated tests for the service logic and component checks across the implemented responsibilities. Use a small representative set of source/translation/review fixtures with explicit expected results. Include normal behavior and relevant adverse cases, such as a wrong-book paragraph, invalid input, failed translation request, or an edit that must remain intact after an interrupted operation.

Choose cases that can expose real failures in your implementation. Record the actual commands and results. Fakes are useful for controlled dependency behavior, but tests using them do not establish that a real database, HTTP layer, or translation provider works.

The instructor offered reviewed translation examples for domain validation; the recording does not establish that this dataset has been distributed. Start with synthetic or authorized material if needed, explain its coverage and limitations, and record what qualified/domain review remains necessary. There is no fixed fixture count or coverage percentage for this assignment. Passing your selected cases is evidence for those cases, not proof of complete correctness.

### 4. Set up the repository for continued work

Document prerequisites, configuration, build/run/test commands, and what is incomplete. Provide concise local agent instructions and a discoverable record of consequential design decisions. In this course use the exact `AGENTS.md` filename; link a rationale record such as `MEMORY.md` or a feature decision document rather than copying extensive history into every folder. A reusable skill/procedure is useful where it captures a real recurring task, but empty skill files are not a deliverable.

Keep changes coherent and reviewable. Record your response to substantive review feedback, including rejected suggestions and the reason. Preserve acceptance intent when repairing a failing check. Disclose reused code and what you implemented or verified yourself.

## Optional extensions

A tiny frontend that displays parallel text is useful for validation, but is not required. An API client or the executable unit/component checks can demonstrate the backend this week. If the HTTP boundary is not implemented, tests can still show completed service behavior, with the route requirement reported honestly as partial.

A reproducible local “onebox” entry point is also useful once the relevant components exist. Clearly identify which components it starts and which are simulated. A health endpoint or hello-world page verifies startup, not the full review workflow. A polished UI, cloud deployment, CI pipeline, and a complete automated software factory are not required.

## Work budget: 5–10 hours

| Activity | Suggested time |
|---|---|
| Review scope, dependencies, and completion checks | 1–2 hours |
| Implement bounded backend behavior | 2–4 hours |
| Add representative tests and repair supported defects | 1–2 hours |
| Repository guidance, review, and submission | 1–2 hours |

Prioritize core behavior and evidence over speculative features. If the complete agreed backend exceeds the budget, submit a coherent partial result with explicit remaining work; do not redefine the missing behavior as complete.

## Submission

Submit your own repository/PR link before the next session, containing:

- Reviewed work hierarchy or execution plan, with the MVP boundary and open decisions.
- Backend code and a concise API/service contract, including implemented and missing boundaries.
- Unit and component tests, representative fixtures, actual commands/results, and known validation gaps.
- Reproducible setup/run/check instructions and concise agent/decision guidance.
- A short walkthrough of one successful operation and one meaningful failure path, through tests, an API client, or an optional minimal frontend.
- `docs/prompt-logs/week-07.md` using the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md), with selected agent exchanges, human decisions, review feedback, and verification evidence.

Report each criterion as met, partial, or unmet and link the evidence. Never include credentials or unauthorized source material. A claimed result without a relevant check remains unverified.

## Assessment

The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) applies, with four equally weighted dimensions:

| Dimension | Weight | Evidence this week |
|---|---|---|
| Context management | 25% | Reviewed source drives scope, work hierarchy, interfaces, and repository guidance; assumptions stay visible |
| Validation and correction | 25% | Useful domain fixtures, unit/component checks, actual results, and supported repairs or justified review responses |
| Structural oversight | 25% | Responsibilities and contracts remain coherent; dependencies and bounded changes are understood and defended |
| System integrity | 25% | Core backend behavior works within stated boundaries; errors and reviewer data are handled consistently; missing integrations are disclosed |

Score the evidence, not code volume, agent count, or model settings. Missing evidence keeps the affected assessment provisional. Optional UI polish cannot substitute for required backend behavior and tests.

## Bring to the next session

Bring your implementation, check results, unresolved questions, and a concise account of what you trusted, checked, corrected, reused, and left incomplete. Future infrastructure work starts from a verified baseline; this homework does not imply a deployed service or production readiness.
