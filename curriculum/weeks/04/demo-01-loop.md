# Demo 1 — Repeat a prompt in fresh agent calls

Earlier lessons used an interactive agent to guide a change. Here, Bash repeats
one broad goal, and the agent chooses each next improvement from saved files.

**Goal:** use three fresh calls to build and improve a browser paint app from
an empty folder. The app and ledger carry context between calls.

## Who chooses the work and advances the loop?

You supply the intent. Each agent call chooses one improvement and leaves code
and a ledger entry for the next call. Bash owns the repeat count, fresh calls
and stopping on a failed command; the model does not have to remember to
launch the next pass. This demo has neither a conversational Planner nor
parallel workers.

The next step is an agreed task list with a completion gate, as in Demo 1B.
For the larger pattern of user conversation, planning and execution ownership,
see [Planner and Controller](reference.md#planner-and-controller).

## Real scenarios where you might use this

This paint exercise is small enough for one session; it illustrates the mechanism.

- **Dashboard prototype:** Explore small changes to navigation, filters and
  empty states when the next useful improvement is still being discovered.
- **Legacy script cleanup:** Make one readability or error-message improvement
  per pass, using the current code and ledger as the next starting point.

Use one session for a few clear edits. Repeated passes still need review;
more calls do not guarantee better work.

Complete the [shared prerequisites](README.md#shared-prerequisites) first.

## Set up

Start in your CAP repository root:

```bash
mkdir -p .demo-runs/01/app
cp -R curriculum/weeks/04/scripts/loop/. .demo-runs/01/
cd .demo-runs/01
npm init -y
npm install playwright
npx playwright install chromium
```

Read `loop.sh` and `prompt.txt` to follow the flow.

Workers edit/check local files. Only the harness runs the server and captures
headless Chromium screenshots. The prompt asks for one improvement, at most
1,000 changed lines, and a progress entry in `ledger.md`.

## Run and watch

```bash
bash loop.sh
```

Agent output appears in this terminal and is saved in `artifacts/001.log`, then
`002.log`, and so on. Optionally open <http://127.0.0.1:4173>; refresh after
each saved pass. The script starts and stops the app server itself.

For 100 passes, edit `ITERATIONS=100` in `loop.sh`. Each call consumes model usage.

## Optional: open the history and app

```bash
open artifacts/history.html
cat ledger.md
python3 -m http.server 4173 --bind 127.0.0.1 --directory app
```

The gallery numbers actual screenshots in pass order. Open
<http://127.0.0.1:4173> to try the final app; `Ctrl+C` stops this server.
Compare the images with the ledger: did each chosen improvement help?

The loop stops on a failed command. Screenshots do not verify drawing; the
line limit is a prompt instruction. A script comment suggests hardening.
This revision has not been run with model calls.

Rerunning uses the current app and replaces numbered artifacts. For an empty
start, delete only `.demo-runs/01` and repeat setup from the CAP root.
