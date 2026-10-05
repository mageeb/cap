# Week 4: Orchestration — Building and Using Harnesses

## Learning Goals
Use a harness to build a difficult application on the loop. Explain agent responsibilities, execution ownership, connections, context boundaries, coordination, and final verification. Students need not build a production orchestrator from scratch.

## Delivery formats

The [presentation](presentation.md) is a 60-minute session with demos included.
The 75-minute instruction, 45-minute demonstration plan, and 60-minute workshop
below form the existing three-hour classroom format. Use the shorter deck's
sequence for the 60-minute delivery; reserve the expanded activities for the workshop.

## Core Instruction (75m)
- **Control modes (15m):** Compare Week 2's in-the-loop work with Week 3's on-the-loop supervision. With multiple agents, someone must still own goals, acceptance, and integration.
- **What makes a useful agent (20m):** Define purpose, inputs, outputs, tools, context, permissions, acceptance criteria, failure reporting, and stop conditions. A persona is a useful perspective, not an operational contract.
- **Harness designs and execution ownership (25m):** Compare one sustained session, a fresh-context loop, a fixed queue, native delegation, a planner plus external controller, and a native runtime using the [pattern table](reference.md#choosing-a-pattern). Relate these to script-driven, instruction-driven, and hybrid designs. Native subagents already have separate contexts; a main supervisor may remain interactive. Distinguish model judgment from routine programmed scheduling.
- **Connections and coordination (15m):** Use the [API/services/mobile example](reference.md#example-services-and-mobile-clients) to trace actual dependencies through integration and review. Show a plan or approved revision going to a controller and a summary, blocker, or decision request returning to the user conversation. Explain who owns status and running work. Persist the graph and execution state outside planner/controller processes: replacement agents can retain the accepted work, and controllers can recover when claims, results and retry rules are durable. Use GitHub Issues/dependencies as a concrete example of shared state for Linux service workers and a Mac iOS worker; task claiming/result transport still need implementation. Demo 3 persists graph.json, not completion; prepared Demo 4 is local and does not establish multi-machine or full crash recovery. Retries, live steering, and notification routing exist only when implemented.

## Instructor Demonstration Plan (45m)
### Preparation
Bring **one working, rehearsed orchestrator setup** using your existing agent access. Gas Town may be selected after rehearsal, but no named orchestrator is mandatory. Record the exact tool/version, account requirements, invocation commands, and known limits in the demo repository before class. Do not assume a subscription automatically covers every adapter or concurrent run.

Use a small shared task for the early patterns: add color selection and saved tool preferences to a minimal drawing app. Prepare a baseline app and checks, a working result, and an integration-mismatch checkpoint. Keep all references in GitHub and disclose prepared results. The runnable [demo guides](README.md#slides-and-student-experiments) record their concrete setup; a different classroom adapter still needs its own rehearsal. Demo 3's planner exits before Python runs its constrained graph, so it has no live steering or notifications.

Demo 4 expands the starter into Studio Board through two native release graphs,
with roughly 60–80 meaningful implementation tasks each and a hard drain limit
of 100. Its fixed workflow recipe generates the actual task graphs from accepted
scope. Show task ownership, prerequisite code imports and one explicit integrated
release, then independent product checks. The expanded run is unrehearsed: prepare
and label recorded evidence before class, and show a short slice rather than
promise the whole build finishes within the demonstration slot.

### 1. Frame the Roles (5m)
Show the running orchestrator and define a UI worker, persistence worker, and integrator/reviewer. Agree the preference schema first. Assign non-overlapping files. Explain why both workers must use the same contract.

### 2. Script-driven Harness (12m)
Ask AI to create a few simple Python scripts connected through files or JSON:

```text
Generate a small coordinator with plan.py, dispatch.py, verify.py, and report.py.
Use an explicit task JSON schema, dependency IDs, owned paths, and output artifacts.
Dispatch through the existing agent adapter I supply; do not invent its API.
Use subprocess argument arrays, checked exit codes, per-task timeouts, and bounded retries.
Stop if a worker output is missing, invalid, or contradicts the agreed interface.
Do not execute generated code until I have inspected it.
```

Inspect the generated code and connect it to the rehearsed adapter. Run a bounded task through the stages and show an artifact moving between them. Show the actual agent invocation and its result.

### 3. Instruction-driven Harness (10m)
Use the same app, role contracts, and checks with the [workflow playbook](reference.md#reusable-workflow-playbook). Ask the orchestrator to plan, assign, require evidence, and integrate according to that document. Show which decisions moved from Python into instructions. Verify actual execution; a narrative saying “the reviewer approved” is not evidence that review ran.

### 4. Hybrid Harness (12m)
Keep a supervisor agent responsible for the goal. Ask it to generate a small helper that dispatches ready tasks and collects reports, using the same known adapter. Review and run that helper. Return results to the supervisor for the next decision; do not let the helper silently redefine the task or acceptance tests.

Introduce a prepared schema mismatch, such as one worker returning `colour` where the agreed interface uses `color`. Show the integration check rejecting it and the supervisor routing a correction. Label this as a prepared failure case.

### 5. Compare and Debrief (6m)
Who owned execution and task status? What was mechanically enforced? What needed model judgment? Which reports reached the user conversation, and which stayed in the controller? Which evidence justified acceptance? Would one sustained session with native delegation have been sufficient? How would an iOS priority change or API mismatch reach the responsible decision-maker? Point students to the open-ended homework: use a harness, demonstrate coordination, and defend the integrated result.

## Student Workshop (60m)
Choose a hard application and write acceptance criteria (15m); define roles, task dependencies, and handoffs (15m); run a small harness-driven slice and inspect the result (20m); defend the control choices (10m).

## Homework
Use [homework.md](homework.md) for the open-ended build and assessment. Week 4 is a substantial project, but continuation into Week 5 is optional.
