# Demo 1B — A task file and one fresh repair

Demo 1 lets the agent choose what to improve. Here, Bash reads five agreed
tasks from a file and checks each requested output before moving on.

**Goal:** create the paint page, style it, add drawing, then color buttons,
brush size and Clear.
Each attempt is fresh. Saved files carry context; the script chooses task order
and when to retry.

## Who plans and advances the queue?

The checked-in task files are the agreed plan. Bash owns task order, the
completion gate and the one permitted repair; each model call handles its
current task. Routine execution bookkeeping is already in code. This demo
has no conversational Planner and runs one task at a time.

A fixed queue is useful when order is known. Dependencies and independent work
lead to a graph; a user-facing planning conversation can help agree that graph.
See [Planner and Controller](reference.md#planner-and-controller).

## Real scenarios where you might use this

This paint exercise is small enough for one session; it illustrates the mechanism.

- **Documentation migration:** Convert an agreed list of module guides to a new
  format in order, checking each result before moving to the next guide.
- **API client migration:** Update agreed API calls, then their callers and
  examples in a fixed order, with a suitable check for each task.

Use one session for a short checklist. Larger queues need appropriate checks;
the demo's nonempty-file check does not prove behavior.

Complete the [shared prerequisites](README.md#shared-prerequisites) first.

## Set up

Start in your CAP repository root:

```bash
mkdir -p .demo-runs/01b/app
cp -R curriculum/weeks/04/scripts/task-queue/. .demo-runs/01b/
cd .demo-runs/01b
npm init -y
npm install playwright
npx playwright install chromium
```

Read `queue.sh`, `capture.mjs`, `queue.txt`, and `tasks/01.txt` through `05.txt`.

`while read` selects a task and its expected file. `for attempt in 1 2` allows
one attempt and one fresh repair. `test -s` checks that the file is nonempty.
A successful call and file check advance the queue; two failures stop it.
These checks demonstrate the completion gate; they do not prove the app works.

## Run and watch

```bash
bash queue.sh
```

Each attempt saves its prompt, log, file-check result and screenshot under
`artifacts/01-1/`, then later task/attempt folders. Output also appears here.
Look for `DONE task 01` through `DONE task 05`. Failure feedback goes to the next call.

Workers edit/check local files. Only the harness runs the server and headless
Chromium. `capture.mjs` drags the same stroke before every screenshot. From an
empty app, task 01 shows an unstyled empty canvas, task 02 a styled empty canvas,
task 03 a sample stroke if drawing works, task 04 color buttons, and task 05
brush size and Clear. The file check still does not prove behavior. Optionally
open <http://127.0.0.1:4173> and refresh after each task.
Run one demo at a time.

## Optional: open the history and app

```bash
open artifacts/history.html
cat ledger.md
python3 -m http.server 4173 --bind 127.0.0.1 --directory app
```

The gallery shows actual screenshots in task/attempt order, including failures.
Open <http://127.0.0.1:4173> to try drawing; `Ctrl+C` stops this server.

The 1,000-line limit is a prompt instruction. A script comment suggests stronger
checks and deadlines. This revision has not been run with model calls.

Rerunning uses the current app and replaces numbered artifacts. For an empty
start, delete only `.demo-runs/01b` and repeat setup from the CAP root.
