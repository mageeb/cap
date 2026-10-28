# Session 07 — Planning, Verification, and Backend Practice

*Organized account of the October 23 recording. The [full automated transcript](transcript.txt) retains incidental conversation, repetition and recognition errors without editorial cleanup. These notes separate the recorded demonstration, proposals, and the independently checked repository practice.*

## 1. From scope and design to an execution plan

The session recapped personas, jobs to be done, a bounded MVP, and architecture. The product remains a translation review application: an external service supplies a machine draft, and the application helps a person compare source and target passages and correct the translation.

The next step was to derive an implementation plan, not ask for the entire application at once. A hierarchy of work items makes progress visible, gives agents bounded work, and provides points where humans can correct or redirect execution.

The recap described a 30-page limit. Earlier discussions had left some input limits unresolved. Treat this as the recap's working example rather than silently changing every product record or assuming the concrete input policy is implemented.

## 2. Use the repository as shared context

The instructor grounded the assistant in the Week 5 and Week 6 transcripts and notes. Recorded requirements and design debates can be recovered into concise specifications and then work items. The team still needs to review the resulting text and resolve ambiguous or conflicting statements.

Short, comprehensive bullets were preferred to reduce review burden. The broader lesson was to manage the software development lifecycle, including scope, design, verification and delivery, rather than concentrate only on code generation.

## 3. Generate and review a work-item hierarchy

The live prompt asked for a hierarchy representing the MVP implementation plan from Weeks 5–6. An Excel workbook was used for the demonstration; workplace issue trackers were discussed as alternatives. The hierarchy included epics, stories and tasks.

The transcript does not establish a required number of work items, a mandatory tracker, or that every generated item was approved. Useful properties are clear scope, understandable parent/child relationships, dependencies and verifiable completion criteria.

The group emphasized reviewing both the plan and the design before execution. Review can involve a human or a separate agent. Generating a plausible plan is not independent evidence that it is complete or correct.

A participant described self-contained units that state functionality, tests, and a definition of done. Such units can be delivered through several small, reviewable pull requests. After reviewing the hierarchy, ask the assistant to sequence dependencies and identify what can safely proceed in parallel.

The instructor also recommended using waiting time productively while a bounded agent operation runs. Results still need review before dependent work relies on them. Agent counts and claimed productivity multipliers were motivational remarks, not measured course outcomes or requirements.

## 4. Reason from first principles

The group asked what must be true to move from work items to a correct and reliable live service. This exposes missing requirements, setup, tests, security boundaries, and domain checks.

Engineers who have not experienced many conventional development cycles need a way to challenge generated output rather than accept familiar-looking terminology. The relevant skill is explaining why each step and verification check is necessary.

Historical and industry anecdotes illustrated this point; these notes do not treat them as verified accounts of other organizations. Software factories were discussed as an ambitious automation direction, not as a complete system configured or accepted in class.

## 5. Build a representative domain verification dataset

The central validation discussion went beyond checking a function or clicking a button. The application manipulates domain data, so the team needs representative inputs and reviewed expectations for what should happen to them.

For translation review, this includes aligned source and target passages, human corrections, and expected behavior when those corrections are saved and read. Such examples complement unit and system checks by grounding them in the actual business workflow.

The set should be varied enough to expose meaningful cases and small enough to run repeatedly. A thousand examples was illustrative, not a prescribed threshold. More rows do not automatically mean better coverage.

The group discussed real, synthetic and anonymized examples. Course preparation should use synthetic or appropriately authorized data; simply replacing names does not necessarily remove personal information. Expected outcomes need review too, or a generated check may merely validate the assistant's own assumptions.

The instructor described this preparation as less glamorous than coding but important for making later iteration reliable. A small representative search-index example was a proposed illustration, not an executed classroom benchmark.

## 6. Evaluate trade-offs across cases

A participant described comparing model configurations against a common dataset. A result can be faster on one case while less accurate across representative cases. Shared examples make those trade-offs visible.

The same principle applies to application changes. Define expected outcomes, compare actual behavior, and investigate failures. Passing a finite suite supports the tested claims; it does not prove correctness for every input, real adapter, or production environment.

The session discussed test-driven development, product/output-oriented verification, and specification-driven development. The actionable connection was that a task should state both intended behavior and how to verify it. The presenter's product-driven label is classroom usage, not a universal acronym taxonomy. Incidental vendor announcements are not requirements for the exercise.

## 7. Organize the application separately from curriculum

The instructor selected `project/src/Translation/` for the application, with `Backend/` and `Frontend/` folders. Weekly teaching materials stay under `curriculum/weeks/`. The session focused on the backend, while leaving the frontend location available for later work.

C#/.NET was the chosen service direction for this classroom practice. It does not establish a final database, identity system, translation provider or frontend framework. The canonical code remains in the project folder rather than being copied into the weekly curriculum.

## 8. Review high-level interfaces before implementation

The live request asked the assistant for a proposed high-level interface skeleton and paused for human review. The instructor wanted to check whether the first abstraction layer reflected the earlier product and design discussion.

The demonstrated boundaries were:

| Interface | Responsibility in the proposal |
| --- | --- |
| `IBookService` | Book/source-document use cases and ordered paragraph access. |
| `IReviewService` | Review editing and related use cases. |
| `IDraftTranslationService` | Coordination of initial machine-draft translation. |
| `IReviewStore` | The persistence/access boundary used by application services. |
| `ITranslationProvider` | The external translation boundary. |

The instructor confirmed the boundaries after a brief review, while acknowledging the class did not perform the full review it would deserve in real development. Later the assistant explicitly reported implementation had not started; the instructor then authorized implementation. Interface generation, approval, implementation and verification are separate states.

## 9. Reproducible local development: the onebox idea

A participant proposed starting a local backend with a simple health endpoint and a frontend that can call it. The group called a locally reproducible development setup a onebox: a developer can clone the repository and start the needed components together.

The instructor suggested asking the assistant for setup scripts, then deferred doing this during the live demonstration and asked students to try it. A proposed onebox is not evidence that a frontend, database or HTTP host was running in class.

## 10. Keep changes small and review comments traceable

The group observed that the growing change was already large. Smaller coherent commits and pull requests help humans and agents understand what changed, why it changed, and which evidence belongs to it. File count by itself is not a correctness measure, but unrelated concerns in one change make investigation harder.

A participant described reviewing through pull-request comments and asking the assistant to respond: fix a valid issue, or explain why a comment does not apply. The intended workflow is traceable reasoning and resolution, not blindly accepting every automated finding or silently marking comments resolved.

A structured plan, reviewable units, explicit verification and persistent rationale all support maintaining control as the application grows.

## 11. Persistent instructions, rationale, and procedures

The group added local guidance to make future agent sessions less dependent on the original conversation. Three distinct kinds of context were discussed:

- **Instructions:** what an agent should do and which boundaries it must preserve. The repository uses the exact filename `AGENTS.md`.
- **Rationale/history:** why a decision was made, its status, evidence and limits. The classroom practice uses `MEMORY.md` files alongside feature records.
- **Reusable procedures:** how to carry out a task or verification workflow. A course skill can use a `SKILL.md` entry point and supporting files.

The recording sometimes uses imprecise singular/lowercase names. These notes use the actual repository conventions. Different tools discover and load context differently; a rationale filename is not a universal automatic-memory standard.

The instructor initially chose guidance in each application directory, then acknowledged that granularity should depend on scale and usefulness. This does not require adding two files to every small folder in every student repository. A few useful scoped instructions can be better than duplicated or contradictory guidance.

Memory and design records should remain concise. Link evidence, keep accepted decisions distinguishable from proposals, and avoid filling context with repeated prose. A participant described feature-specific design documents that retain decisions as individual work items and PRs evolve. This fits the project's feature-oriented specifications and task records.

The practical purpose is to preserve useful team knowledge while keeping future sessions able to find the relevant context. Generated documentation still needs review; it can be verbose, stale or wrong.

## 12. Publication and context management

The instructor requested a remote branch and pull request and showed a PR link in class. This demonstrates a review path, not proof that a deployed application was available. The lesson's value was the route from recorded requirements to a reviewable change.

The discussion also noted that large logs, infrastructure troubleshooting and long conversations can consume the useful context window. Separate focused sessions and durable feature records were suggested ways to retain control. Compaction should not be treated as a guarantee that every detail survives.

Infrastructure work was discussed as a future topic. No deployment, cloud topology or complete frontend was established by this recording.

## 13. The explicit homework

The instructor restated the assignment near the end so it could be extracted from the recording:

1. Implement the absolute minimum translation-review backend in your own repository.
2. Expose the necessary APIs and make the basic reviewer workflow testable.
3. Implement unit tests and component tests, with a useful repository setup.
4. A very small frontend to display parallel text and exercise the backend is optional; a suitable API client or runnable tests can also support this week's demonstration.
5. Use reviewed translation examples for realistic validation when available. The instructor offered to share such data; the recording does not establish that it was distributed.

The suggestion to work an hour or two each evening was encouragement, not a verified delivery estimate. The published homework adds the course's usual budget, submission structure and assessment evidence. Those are curriculum preparation, not additional historical classroom decisions.

Rich UX, cloud deployment and a fully automated software factory are not the required result. Missing implementation or integration must be reported honestly rather than hidden behind a generated plan or a successful fake-based test suite.

## 14. The repository practice checkpoint

The user identified the backend files as the practice from this session. The canonical implementation is under [project/src/Translation](../../../project/src/Translation/Backend/README.md); it is linked here rather than copied into the weekly folder.

The branch includes these existing practice commits:

| Commit | Practice artifact |
| --- | --- |
| `17c1ca9` | Backend service contracts and proposal. |
| `f42e56e` | Application service implementations and regression suite. |
| `a175350` | Local agent instructions and rationale. |
| `7a6a456` | Capability-level guidance and rationale documentation. |

As a fresh repository check during this material update, the backend built with zero warnings/errors and all 17 deterministic service regressions passed. They cover validation, access denial, repeated translation requests, complete-draft publication, failure/cancellation behavior, import ordering and related boundaries.

These checks use fakes. They do not establish HTTP/API behavior, concrete database durability, real translation-provider integration, deployment or an end-to-end user journey. Those gaps are explicit in the [implementation task record](../../../project/specs/003-backend-implementation/tasks.md). The classroom checkpoint is therefore a useful starting point, not a complete submission for every backend acceptance criterion.

The [planning workbook](../../../outputs/mvp-plan/mvp-implementation-plan.xlsx) is also part of the Week 7 practice package. Its proposed hierarchy is a reviewed planning aid, not evidence that all listed work has been implemented.

## 15. Consolidated status

| Topic | What the session establishes |
| --- | --- |
| Planning | Derive and review an MVP work-item hierarchy from source requirements/design. |
| Execution | Sequence dependencies and identify safe parallelism; retain human review points. |
| Verification | Use representative domain cases with reviewed expectations alongside unit/system checks. |
| Source organization | Application under `project/src/Translation`; curriculum remains in weekly folders. |
| Backend direction | C#/.NET high-level service/port boundaries reviewed, then implementation authorized. |
| Onebox | Reproducible local setup proposed and assigned for exploration; not demonstrated as complete. |
| Review | Small coherent changes, evidence-based comments, and explicit definition of done. |
| Durable context | Scoped instructions, concise rationale and reusable procedures, with reviewed source records. |
| Homework | Minimal backend implementation, unit/component tests and repo setup; simple frontend optional. |
| Remaining gaps | Concrete adapters, HTTP host and end-to-end behavior still need implementation/verification. |

## Recording reference points

| Approximate time | Topic |
| --- | --- |
| 00:00–00:05 | Recap of personas, MVP, scope and architecture. |
| 00:05–00:16 | Repository-grounded work-item hierarchy and supervision. |
| 00:16–00:24 | First-principles reasoning, review and definition of done. |
| 00:24–00:35 | Domain verification dataset, expected outcomes and testing approaches. |
| 00:35–00:42 | Hierarchy review, dependency sequence, parallel work and model-choice caveats. |
| 00:42–00:48 | Source layout, C# interface proposal and onebox idea. |
| 00:48–01:00 | Verification, boundary approval, implementation authorization and reviewable changes. |
| 01:00–01:05 | PR workflow, context and large-change limits. |
| 01:05–01:15 | Instructions, memory, procedures and feature-level rationale. |
| 01:15–01:18 | Backend/unit/component-test homework and optional frontend. |
