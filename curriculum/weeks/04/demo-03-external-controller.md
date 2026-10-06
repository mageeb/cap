# Demo 3: chat to plan; a program runs the graph

[Demo 2](demo-02-supervisor.md) lets a supervisor agent dispatch workers.
Here you plan with an interactive agent, then Python dispatches fresh calls.
Both demos add saved paint preferences; their outputs stay in separate folders.

## Where execution ownership moves

Use the planner conversation to agree intent, the PRD, ownership and completion
criteria. Here its output is constrained to the four-node UI, Storage,
Integration and Review example; it is not unrestricted task decomposition.
After you exit that chat, Python owns ready-node selection, parallel launch
and dependency completion. The model workers handle their assigned tasks
rather than deciding which task should run next.

A fuller system can keep the planning conversation available for steering and
escalations while a Controller advances approved work. This demo only shows
the scheduling boundary: it has no live route back to the planner, user
notifications, durable completion state or automatic retry policy. Its
completed-node set exists only in the Python process. The saved `graph.json`
survives either process exiting, but restarting the controller schedules the
nodes again; it does not recover completed work. This demo uses local threads
and files, not workers on multiple machines. A fuller implementation persists
completion, claims and results outside both processes so replacements can
continue under defined recovery rules. See
[Planner and Controller](reference.md#planner-and-controller).

The starting paint app draws black strokes. Add pencil/eraser and black/teal
selectors, remembering both choices after reload. Python runs UI and storage
together, waits for both, then runs integration and finally read-only review.

## Real scenarios where you might use this

This paint exercise is small enough for one session; it illustrates the mechanism.

- **Release preparation:** App changes and release notes can run independently;
  packaging waits for both, then review checks the combined result.
- **Client/server upgrade:** Plan both sides of an API change, then integration
  and review. A program schedules the agreed graph after the planner exits.

Use one session for a small graph. This controller schedules ready nodes;
it does not provide durable crash recovery or automatic retries.

## Copy the demo

Complete [shared prerequisites](README.md#shared-prerequisites). Start in the
CAP repository root:

```bash
mkdir -p .demo-runs/03
cp -R curriculum/weeks/04/scripts/external-controller/. .demo-runs/03/
cd .demo-runs/03
```

## Plan, then execute

```bash
codex "$(cat planner-prompt.txt)"
```

Discuss the [planner prompt](scripts/external-controller/planner-prompt.txt)
with the agent. It saves `prd.md`, `graph.json` and the task prompts in `prompts/`.
Exit the planner when those are ready, then run:

```bash
python3 controller.py
```

Watch `START ui` and `START storage` together; `START integrate` follows both
`DONE` messages, then `START review` comes last. Read
[controller.py](scripts/external-controller/controller.py): ready-node selection,
`ThreadPoolExecutor` fan-out and `as_completed` join are the orchestration.
Workers share this folder but own different files; review edits nothing.

Calls must succeed, produce their declared files and finish with `DONE` or
`TLDR: DONE`. Failed calls, `BLOCKED` replies, empty replies or a stalled graph
stop the run; read `logs/NAME.txt` and `.log`. Reports are not browser acceptance.

After fixing a failure, rerun `python3 controller.py`. It starts the graph
again from UI/storage, retains existing app files and replaces the task logs.

## Try the result

Run from `.demo-runs/03`, where setup left your terminal; `pwd` should show that folder.

```bash
pwd
cat logs/review.txt
python3 -m http.server 4303 --bind 127.0.0.1 --directory app
```

Open <http://127.0.0.1:4303>. The starter shows `Pencil · black` and draws black.
The completed app should pass these checks:

- **Tool** offers Pencil/Eraser; **Color** offers Black/Teal.
- Choose Pencil + Teal, draw, reload: both selections remain and drawing is teal.
- Choose Eraser, reload: Eraser + Teal remain. Switch to Pencil, draw a fresh
  stroke, then switch to Eraser and erase it.

`DONE` and file existence show orchestration completion; these browser checks
validate the app. Strokes may clear on reload. **Ctrl+C** stops the server.

For Claude Code, change `AGENT = "codex"` to `"claude"` in `controller.py` and use
`claude "$(cat planner-prompt.txt)"` for planning. Run the same Python command.
Its fresh `claude -p` calls use file tools for workers and read tools for review;
[CLI flags](https://code.claude.com/docs/en/cli-reference) follow the official docs.
The Claude flow has not been rehearsed.
