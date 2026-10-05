# Week 4 Reference: Agent Contracts and Harness Designs

## In the Loop and On the Loop
In-the-loop work uses human decisions between small steps. On-the-loop work executes within agreed boundaries between checkpoints. Select the mode based on uncertainty and the quality of checks; use human review where the harness cannot establish correctness.

## Agent Contract Template
```text
Role and purpose:
Task and acceptance criteria:
Inputs and source of truth:
Relevant context and skills:
Allowed tools and file/workspace ownership:
Output artifact and recipient:
Checks required before handoff:
Retry/time/usage budget:
Stop/escalation conditions:
```

Example: a persistence worker owns storage code and returns a documented interface, changed files, tests/results, and limitations to the integrator. A UI worker consumes that interface. The reviewer verifies acceptance independently and reports gaps rather than silently rewriting either worker's contract.

## Planner and controller

A **planner** turns the user's goal into a product brief, acceptance criteria,
and tasks with real dependencies. A **controller** owns execution of the accepted
work: which task is ready, who owns it, whether it is running or blocked, and
what must happen before integration and review. These are responsibilities;
they can live in one agent session, in helper code, or in a native runtime.

### One conversation can still be enough

In one interactive session, a supervisor can discuss requirements with the user,
delegate bounded tasks, collect results, integrate, and ask for decisions. Native
subagents already do their work in separate agent threads and return summaries,
so delegation does not put every worker's intermediate output into the main
conversation. The main supervisor can remain the user's conversational entry
point. See the [official subagent documentation](https://learn.chatgpt.com/docs/agent-configuration/subagents).

This is a reasonable choice when the workflow is bounded, the responsibilities
are clear, and the supervisor can track the handoffs. Separate worker contexts
help manage context; they do not by themselves provide a durable task graph,
restart recovery, or a policy for integrating code.

### Let a program advance the agreed work

For a larger workflow, the user can keep talking to a planning or supervisory
agent about brainstorming, the PRD, priorities, and escalations while a program
advances the approved task graph. The graph makes prerequisites and ownership
explicit. Control state records the current task status and assigned owner;
where that state lives and whether it survives a restart depend on the implementation.

The model handles decisions that require judgment: decomposing an unclear goal,
interpreting a failure, proposing a changed contract, or evaluating a review.
Programmed rules can handle routine work: find ready tasks, launch within a
limit, collect results, run defined checks, and release dependent tasks. A
routine completion report need not trigger another main-model decision when a
specified rule is sufficient. Model calls still perform the implementation and
any review that has been assigned to an agent.

A native runtime may already supply parts of this controller. Gas City's
[architecture documentation](https://github.com/gastownhall/gascity/blob/main/docs/getting-started/how-gas-city-works.md)
describes an orchestrator, durable work in a bead store, and an event stream.
That is an existing implementation of orchestration responsibilities; students
do not need to buy another product or build a separate controller to use this
architecture. Inspect the chosen workflow's integration and notification behavior.
An event stream alone does not establish that a particular main chat receives a decision request.

### Messages travel in both directions

| Direction | What crosses the boundary | What the recipient does |
|---|---|---|
| User conversation → controller | Accepted plan, task contracts, priorities, or an approved revision | Apply the supported change to the graph and execution policy |
| Controller → user conversation | Progress summary, blocker evidence, or a decision request | Explain the situation, discuss choices, and obtain needed direction |
| Worker → controller | Result, changed artifact, check evidence, or failure | Update ownership/status and follow the defined completion or escalation rule |

A useful implementation can keep routine task reports in its logs and send a
summary or actionable blocker to the user conversation. The conversation can
send an approved revision back, such as changing a priority or replacing a task.
Applying live changes requires a defined policy for running tasks and their
outputs; editing a plan file alone does not guarantee the controller adopts it.
Persistence, retries, notification routing, live steering, and recovery are
capabilities only when the selected implementation supplies them.

### Example: services and mobile clients

Suppose a product needs an API contract, service X, service Y, an iOS client,
and an Android client. First agree the API contract. If X and Y are independent
implementations of that contract, they can run in parallel. The clients can also
begin against the approved contract and mocks when those inputs are sufficient;
if a client needs a working service, record that actual dependency instead.
Integration waits for the required service and client outputs, and independent
review follows integration. The graph follows the work's dependencies rather
than assigning one arbitrary sequence to every project.

The user might say, "Prioritize iOS so we can demo it on Friday." The planner
clarifies the changed acceptance scope and proposes a revision. Once accepted,
the controller can prioritize ready iOS tasks within its supported scheduling
policy; it still respects their prerequisites. If service X exposes a field that
conflicts with the approved API contract, the controller can report the mismatch
and ask whether to correct X or revise the contract and affected clients.
That decision belongs in the user conversation. Ordinary successful handoffs
can continue under the agreed rules without sending every raw report to the main LLM.

### A graph can outlive every process

Keep the task graph and execution records **outside the planner and controller
processes**. The planner can be replaced without losing the accepted work. A
replacement controller can read the same graph and continue when completed
tasks, active claims, attempts and results were durably recorded and it has a
policy for recovering interrupted work. Saving the plan alone does not prevent
duplicate execution. Workers can also be replaced or reassigned independently.

For example, a planner can write **GitHub Issues** with acceptance criteria and
blocking relationships. GitHub hosts those issues after either agent exits;
GitHub supports [issue dependencies](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/creating-issue-dependencies)
and an [API for blocked-by/blocking relationships](https://docs.github.com/en/rest/issues/issue-dependencies).
A controller reads ready issues and records execution/result state. After the
API contract is accepted, Linux workers build services X and Y, a Mac worker
builds iOS, and another worker builds Android; their commits and checks feed
integration and review. Finishing a prerequisite releases only the tasks whose
actual inputs are ready.

This can spread work across machines and let you change executors without
loading their detailed working context into the main chat. It requires shared
state, reliable task claiming and a way to send results back. Issue assignment
or a comment alone is not an atomic claim or a scheduler. A replacement
controller needs those recovery rules; distributed execution does not follow
merely from drawing separate boxes for planner, controller and workers.
GitHub is an architectural example here, not either demo's task backend.

Gas City's native Beads graph also persists outside planner/controller process
lifetimes. Our prepared Demo 4 uses local Dolt and tmux; multi-machine execution
and complete crash/restart recovery have not been demonstrated here. Durability
of the graph does not establish that every workflow recovers correctly.

### What Demo 3 actually demonstrates

[Demo 3](demo-03-external-controller.md) is a small example of the handoff:
the user chats with a planner, which writes the PRD and a graph constrained to
UI, storage, integration, and review. The planner session then exits. Python
loads the graph and schedules ready tasks in fresh agent calls, using process,
file, and final-report gates before releasing dependent tasks.

Its saved `graph.json` survives the planner or Python process exiting. The
completed-node set exists only in Python memory, so a rerun schedules the graph
again rather than recovering completed work. It has no live notification channel
back to the planner, live plan steering, durable completion state or automatic
retries. Its local threads/files are not a multi-machine runtime. It demonstrates
execution ownership and dependency scheduling; the broader architecture above
explains what an implementation could add.

## Choosing a pattern

Use the simplest pattern that makes ownership, handoffs, and acceptance clear.
The small drawing apps in Demos 1–3 can be built in one session. Demo 4 uses a
larger two-release product to expose native graph execution and integration;
using every pattern is not a project requirement.

| Pattern | Execution owner | Useful when | Benefit and trade-off |
|---|---|---|---|
| One sustained session | Main agent, with user checkpoints | A bounded feature or investigation | Little setup; the session must track context, decisions, and completion |
| Fresh-context loop (Demo 1) | Script repeats a goal | Successive passes improve one bounded result | Simple repetition with fresh context; no explicit dependency graph |
| Fixed queue (Demo 1B) | Script advances ordered tasks | The stages and order are known | Clear per-task progress; adapting the sequence needs a changed queue |
| Native supervisor (Demo 2) | Main agent delegates and joins | A few separate responsibilities need judgment at handoff | Separate worker contexts and an interactive supervisor; coordination still needs clear contracts |
| Planner + external controller (Demo 3) | Program advances the accepted graph | Ready tasks and joins should follow explicit rules | Inspectable scheduling and ownership; state, recovery, and message routing require implementation |
| Native orchestration runtime (Demo 4) | Configured runtime and agent workflows | Work needs reusable workflows and coordination outside one session | Reuses platform machinery; setup and the selected workflow's limits still matter |

### What the expanded Demo 4 requires

[Demo 4](demo-04-gas-city.md) turns the paint starter into Studio Board. The
accepted release brief supplies product scope, not a prewritten implementation
graph. Native requirements, planning, review and decomposition generate the
actual tasks and dependencies. Each of two releases targets roughly 60–80
meaningful implementation tasks; the pinned drain has a hard limit of 100
members. Task counts are estimates, not a reason to invent administrative work.

Its stock separate drain gates dependencies and gives items their own worktrees.
It does not automatically combine their commits into one app or make a dependent
worktree inherit prerequisite code. Workers must import verified prerequisites,
and one integration owner must assemble and check the release. Release 2 starts
from integrated Release 1 code. Independent Node and headless browser checks
must establish the actual product behavior; artifact validation and successful
isolated reports do not establish it.

Setup supplies the stock scripts/schemas, a demo-local PyYAML environment and
test-only Playwright/Chromium. Native limits cap the city at six active sessions
and the implementation role at four. Bounded repair/review loops can still
fail; no elapsed time or full-run acceptance is guaranteed. The expanded run
has not been rehearsed end to end.

## Three Designs
| Design | Who directs execution? | Artifact flow | What to inspect |
|---|---|---|---|
| Script-driven | Python stages with explicit dependencies | Task JSON → worker result → check report → integration | Exit codes, validated inputs/outputs, timeouts, and bounded retries |
| Instruction-driven | Agent/orchestrator following a workflow playbook | Plan → role task → evidence → handoff → review | Whether instructions were followed and evidence actually exists |
| Hybrid | Supervisor agent using reviewed helper code | Goal → generated coordinator → worker results → supervisor decision | Generated code, ownership boundaries, changed plans, and final acceptance |

A harness is the surrounding process, checks, state, and controls that let agents do useful work repeatedly. Scripts alone need a model/tool adapter to invoke a real agent. An instruction document alone does not enforce permission or correctness boundaries.

## Reusable Workflow Playbook
1. Inspect the repository and agree acceptance criteria.
2. Decompose work by dependencies, not by arbitrary agent count.
3. Assign explicit ownership and a shared interface contract.
4. Dispatch ready tasks; record state and limits.
5. Require evidence at handoff. A worker's success claim is not a passed check.
6. Integrate in one designated workspace; handle conflicts explicitly.
7. Independently run acceptance checks and review changes to tests.
8. Stop or revise the plan on unexplained failures; report remaining limits.

## Handoff Template
```text
Task ID and owner:
Input contract/version:
Changed files or commit:
Output and interface:
Checks actually run/results:
Known issues and blocked dependencies:
Requested next action and recipient:
```

## Coordination Checklist
Do agents have enough context to act independently? Can they avoid writing the same files? Who owns the source of truth and integration? What happens when a worker fails or times out? Can the supervisor resume from recorded state? How will you know the final product meets the original specification?
