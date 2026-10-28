# Week 7 Reference: From Design to Governed Execution

Use these prompts with the [session notes](session-notes.md), [full unedited automated transcript](transcript.txt), and [homework](homework.md). They are reusable teaching aids, not a claim that every proposed field, workflow, or tool was adopted during the recording.

## 1. Recover the agreed product before planning

The recording treats the repository's Week 5 product discussion and Week 6 architecture discussion as source context. The product assists human review of machine-translated text; it does not require building a new translation model.

```text
Read the Week 5 and Week 6 material relevant to the translation-review MVP.
Restate the target user, core journey, scope boundary, architecture directions,
and unresolved decisions in concise bullets. Cite the source topic or timestamp.
Separate explicit agreements, working directions, examples, and assumptions.
Identify anything that must be clarified before implementation planning.
Do not treat every demonstrated feature as an MVP requirement.
```

The session's recap mentions a 30-page bound. Record the limit with its source and distinguish it from earlier discussions that explored several sizes. If your reviewed specification changes a boundary, explain that change rather than silently combining incompatible versions.

## 2. Turn the MVP into a hierarchy of work

The instructor asked an agent to derive a hierarchy of implementation work items from the preceding two sessions and save it to an Excel sheet. A tracker such as Azure DevOps or Jira was discussed as another destination. The objective is a reviewable plan, not buying a tracker or generating an arbitrary number of tasks.

```text
Using the reviewed source summary and MVP boundary, propose a hierarchy of
work items that leads to a complete reviewer journey. Give each item a stable
ID, parent, purpose, inputs, output, dependencies, and completion evidence.
Keep unresolved product decisions visible before dependent implementation.
Include the work needed to verify the behavior, not only to generate code.
Label estimates and assumptions as provisional. Separate deferred capabilities.
Prepare a table suitable for a workbook or issue tracker; do not publish or
assign external work without authorization.
```

| Proposed planning field | Question it answers |
|---|---|
| ID and parent | Where does this item fit in the product outcome? |
| User behavior or purpose | Why is the work needed? |
| Inputs and constraints | Which source, contract, and decisions guide it? |
| Output and owned scope | What should change, and where? |
| Dependencies | What must be available or agreed first? |
| Completion check | What evidence distinguishes success from a plausible claim? |
| Status, evidence, and open issue | What happened, and what still blocks acceptance? |

These fields are a review template; the recording does not mandate a particular issue schema or task count. An item saying only “build the backend” does not expose enough responsibility to govern or verify it.

## 3. Challenge the plan from first principles

The session asks students to understand the building blocks well enough to challenge generated work. Familiarity with a tool's output is not a substitute for understanding the intended outcome.

```text
Review this proposed plan from first principles. For each required reviewer
behavior, find the data, component responsibility, contract, and verification
work that support it. Look for missing dependencies, duplicated responsibility,
scope expansion, and items whose completion cannot be observed.
Separate supported blockers from optional improvements. Cite item IDs.
Suggest the smallest correction; do not pad the plan with administrative tasks.
```

Use the assistant's critique as leads. Check the source and decide whether each finding is valid. Record accepted and rejected feedback with reasons. Do not invent a defect or repair to make the review appear productive.

## 4. Prepare verification before accelerating execution

The discussion uses a representative search-index environment as an example of investing in setup before trusting broad changes. The transferable point is to create evidence that can expose relevant failures. The example's scale and company anecdotes are not requirements or validated benchmarks for this application.

```text
For the proposed reviewer workflow, identify the smallest representative
fixtures and checks that could expose a meaningful failure. Include ordinary
and adverse cases. State what the evidence would prove and what it would not.
Explain what setup must exist before generated changes can be accepted.
Do not claim a test or environment ran unless there are actual results.
```

Verification should follow behavior and risk. A green command, generated screenshot, or agent statement establishes only what it actually checked.

## 5. Keep supervision explicit

The human can delegate implementation, but still needs to understand progress, review evidence, resolve ambiguity, and redirect effort.

| Checkpoint | Human review question |
|---|---|
| Before execution | Is the goal clear, and are required decisions settled? |
| After a bounded change | Does the result satisfy the intended behavior and scope? |
| After a failure | What evidence distinguishes likely causes? |
| Before accepting work | Which checks ran, and what remains uncertain? |
| Before expanding scope | Is this still serving the reviewed MVP? |

Multiple agents, tabs, or automation stages are optional means of executing the plan. Their number is not evidence of quality. A simpler arrangement is sufficient when it provides clear ownership, progress, verification, and human decision points.

The “software factory” discussion describes a broader aspiration to automate stages of the development lifecycle. It does not establish that an end-to-end factory was implemented, verified, or required for this assignment.
## 6. Ground validation in representative domain examples

A major emphasis of the session is a small, diverse dataset that exercises the actual business behavior. For this product, that means aligned source/translated passages and review edits with meaningful expected outcomes. Unit tests remain useful; they do not replace checking the domain workflow.

```text
Propose a small representative validation dataset for this reviewer MVP.
List scenario, initial source/target state, reviewer action, expected resulting
state, and the requirement being checked. Explain where expected outcomes
come from and which require qualified human review. Include edge/failure cases.
Use synthetic or authorized material; do not expose private customer data.
Keep the suite quick enough for repeated checks and list important gaps.
Do not define expected results solely by copying the implementation's output.
```

| Proposed fixture field | Purpose |
|---|---|
| Scenario and requirement ID | Link the case to a user need |
| Source and initial translated passage | Establish the input and alignment |
| Prior review state | Make the starting conditions explicit |
| Reviewer operation | Describe what the user does |
| Expected output or invariant | Define what should remain true or change |
| Provenance and review status | Explain why the expected result is trustworthy |
| Coverage gap | Prevent a sample from being mistaken for all possible behavior |

A thousand entries was an illustrative scale in the discussion, not a course threshold. Prefer a justified set of useful cases over volume. Passing it establishes evidence for those cases and checks, not proof that every behavior is correct.

The recording's anonymized-data example illustrates the need to protect customer information. Replacing names alone is not a general assurance of anonymization. Synthetic or explicitly authorized examples are sufficient for this exercise.

## 7. Write completion criteria and verification together

```text
For each selected work item, write the intended behavior, scope, relevant
fixtures, acceptance check, and definition of done. Distinguish a proposed
check from an executed result. Include the security and setup work needed
for the chosen scope. Identify anything that requires human/domain judgment.
Explain how independent review could challenge the result without weakening
the requirement merely to make the generated code pass.
```

The presenter contrasts test-first development with product-output and specification-focused verification. Use those distinctions as prompts to ask what will be verified and against what evidence. Labels such as TDD, product-driven development, and specification-driven development do not themselves establish that a check is useful. The session's labels and vendor-announcement references are not treated here as a verified history or universal industry taxonomy.

## 8. Sequence work from dependencies

```text
Analyze the reviewed work-item hierarchy and its dependencies.
Recommend a first bounded implementation unit and a high-level sequence.
Explain which items can run independently, which share contracts or files,
and which must wait for a decision or another result. Identify an integration
owner and a check for the combined result. Do not assume parallel execution
is beneficial merely because multiple agents are available.
```

Use a simple table if it makes dependencies clear. Review the proposed sequence before execution. A work item's useful size depends on the behavior, coupling, review burden, and checks; the goal is a coherent change that people can understand and verify, not a universal number of lines or generated tasks.

Do not paste credentials into example prompts or commit them with the plan. A tracker integration must use the approved account/access mechanism and permissions for that environment. This reference requires no live tracker connection.
## 9. Review responsibilities before implementation details

The classroom demonstration requests a proposed C#/.NET backend skeleton and places application code under `project/src/Translation/`, with Backend and Frontend folders. Weekly folders remain curriculum. The high-level boundaries shown include book, review, and draft-translation services, with narrow persistence and translation-provider responsibilities.

```text
Using the reviewed product, architecture, and work-item context, propose the
smallest high-level interfaces for the backend. Explain each responsibility,
inputs, outputs, dependencies, and unresolved policy. Trace one review/save
operation across them. Separate user-facing orchestration from storage and
external translation integration. Stop for boundary review before implementing.
```

Ask whether the abstractions reflect the reviewer workflow rather than a tool's favorite architecture. A skeleton that compiles does not establish that application behavior exists. After explicit boundary approval, select a bounded implementation unit and require actual behavior checks before accepting it.

## 10. Make local setup reproducible

The session calls a local environment that starts the needed components together a “onebox.” It was discussed as a useful student follow-up; it was not demonstrated as a completed setup during that portion of the recording.

```text
Propose a local setup/run workflow for this repository. List prerequisites,
configuration, secrets supplied outside source, startup commands, and checks.
Explain which components are real and which are fakes or stubs. Describe a
clean shutdown and what data should survive. Prefer one documented entry
point if it simplifies reproducibility. Do not claim a clean-clone run passed
until it has actually been performed and recorded.
```

A health endpoint or hello-world page can check a startup boundary. It cannot prove translation correctness, paragraph alignment, persistence, or review behavior. Choose evidence at the appropriate level.

## 11. Keep changes and review conversations traceable

The discussion favors small, coherent commits and PRs because large mixed changes make review and later investigation harder. Size is a means of keeping purpose and evidence understandable, not a quality score by itself.

```text
For this bounded change, propose a coherent PR scope and completion check.
Explain why the included files belong together and what is intentionally deferred.
Review the exact diff and actual check results. For each review comment, record
whether it is accepted, rejected with evidence, or still unresolved. When a
correction is requested, implement it and rerun the relevant check before
claiming the comment is resolved. Preserve the original acceptance intent.
```

A model can assist review and answer comments, but it should not silently redefine the product or treat its own confidence as approval. Capture meaningful decisions and results in the repository so later work can recover the reasoning.
## 12. Preserve instructions, rationale, and procedures

The closing discussion distinguishes three kinds of reusable repository context:

| Kind | Purpose | Course convention |
|---|---|---|
| Instructions | Tell a future agent how to work in this area | `AGENTS.md`, following this repository's exact naming and scope rules |
| Rationale/history | Explain why a decision was made, its status, evidence, and limits | A concise linked `MEMORY.md` or feature decision record |
| Procedure | Explain how to perform and verify a repeatable task | A supported skill's `SKILL.md`, with any referenced scripts/resources |

These are distinct responsibilities, not a requirement to create empty files in every directory. The recording initially requests broad folder coverage, then discusses tailoring scope to the codebase. Place guidance where it carries useful local knowledge and avoid duplicating global instructions.

`MEMORY.md` is a project convention, not a universal guarantee of automatic loading by every assistant. Exact instruction discovery and skill packaging depend on the chosen tool. Link the necessary context explicitly and verify that a fresh session finds it.

```text
Review the implemented area and propose concise repository guidance.
Separate operating instructions, decision rationale, and reusable procedures.
Use this repository's exact instruction filenames and scope rules.
For rationale record choice, status, reason, evidence, and unresolved limits.
Link feature specifications rather than copying them into every folder.
Preserve existing user decisions and do not rewrite proposals as confirmations.
Have a fresh session explain where it found the instructions and relevant design.
```

A running decision record should stay short enough to be useful. Capture consequential changes and their reasons; archive or link detail rather than making every future task reread a large narrative. The transcript's context-compaction discussion is a reminder to preserve durable context, not a guarantee that a condensed conversation retains every important detail.

## 13. Verify the implemented backend without requiring a polished UI

The closing assignment requires backend implementation and meaningful unit/component tests in each student's own repository. A small frontend is optional; an API client or executable tests can exercise the service. Choose checks appropriate to the actual boundary implemented.

```text
Select the minimum backend behavior needed for the reviewed MVP.
Implement it in bounded units using the reviewed contracts and plan.
Add isolated unit tests and component checks using representative fixtures.
Show inputs, expected behavior, actual results, and unresolved limitations.
State which storage/provider/HTTP boundaries are real, fake, or unimplemented.
Do not present interface generation or a passing build as completed behavior.
Keep the optional frontend to basic validation rather than the full UX design.
```

The [continuing classroom backend](../../../project/src/Translation/Backend/README.md) is a reference for the same case study. Inspect its current scope and run instructions before reuse. A framework-independent service implementation is not automatically a public HTTP API, durable database, or real translation-provider integration. Later repository verification is separate evidence from what was visibly completed during the recording.

The instructor offered previously reviewed translation material for validation. Do not assume that dataset has already been distributed. Synthetic or authorized examples can support initial checks; record the domain-validation gap and replace or supplement fixtures when suitable reviewed material becomes available.
