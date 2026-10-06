# Demo 4: native Gas City orchestration

[Shared prerequisites](README.md#shared-prerequisites) · [Slides](presentation.md)

Demo 3 uses our small Python controller. Here, Gas City's native runtime stores
the work graph in Beads and dispatches ready stages automatically. Mayor is the
main conversation; planning, implementation and review run as separate sessions.

**Mayor running means the chat is ready. The app has not been built yet.**

Already ran the older paint-preferences demo? Follow [the older-run help](#help-an-earlier-demo-already-exists)
first. Do not rerun setup over an existing demo.

## The flow

1. **Terminal:** prepare the demo and open Mayor.
2. **Mayor chat:** ask for Release 1.
3. **Wait:** Gas City plans, creates tasks, runs workers, integrates and reviews.
4. **Inspect:** Mayor reports the finished Release 1 app; check its artwork and evidence.
5. **Mayor chat:** ask for Release 2, which builds on verified Release 1.
6. **Inspect:** check the final app, then finish the demo.

## What you are building and why parallel work helps

Build **Studio Board**, a local workspace for posters, illustrated notes and
short comics, from the tiny paint page. A short preferences fix still fits one
interactive session. This larger product has separate tool, layer, history,
storage, workspace and QA workstreams under shared interfaces. A native runtime
advances the agreed dependencies while Mayor stays available for intent,
clarification and escalations. Each release ends in one integrated app.

| Release | Visible product outcome |
| --- | --- |
| **1: drawing studio** | Create a layered poster with strokes, shapes and text; undo/redo; autosave/reload; import JSON and export JSON/PNG/SVG. |
| **2: creative workspace** | Edit and arrange selected objects; organize projects/pages in a gallery; reuse templates; zoom/pan; use keyboard controls and recover safely from invalid data. |

The [accepted brief](scripts/gas-city/fixture/context.md) gives product behavior
and acceptance examples. Native planning should naturally produce roughly
60–80 meaningful implementation tasks per release, about 120–160 total, without
padding. These are estimates. GC 1.4.2 caps a drain at 100 members; each release
must stay below that limit. Workflow/control beads do not count as product tasks.

Start before class and record actual task counts, elapsed time and evidence.
There is no guaranteed 30-minute runtime and no artificial delay. A teacher
rehearsal must establish the complete two-release path before classroom use.

## Step 1: prepare the demo and open Mayor

**Terminal — start at your CAP repository root.** This step is for a new demo
with no `.demo-runs/04` folder. Existing demos use the help section linked above.
Replace `~/code/cap` below with your CAP checkout root if it is elsewhere.
Use the [shared prerequisites](README.md#shared-prerequisites). If Gas City is not
installed, use its [official Homebrew install](https://github.com/gastownhall/gascity/blob/main/docs/getting-started/installation.md):

```bash
brew install gascity
```

Then run the [setup script](scripts/gas-city/setup.sh) and open Mayor:

```bash
cd ~/code/cap &&
  source curriculum/weeks/04/scripts/gas-city/setup.sh &&
  cd .demo-runs/04/city &&
  gc start &&
  gc session attach mayor
```

Setup copies the starter app, prepares native workflow scripts and test-only
browser tooling, and creates a separate local Git repository and native registry.
It limits implementation work to four sessions and total sessions to six.
App work stays in this demo and its isolated worktrees on local feature branches;
no remote writes, push or PR. No custom controller or manual worker launches are
needed. This is the **city terminal**: its `city` folder holds orchestration
configuration, not the app. Run `gc` here; do not run app npm commands here.
The terminal now shows the Mayor conversation.

## Step 2: ask Mayor to build Release 1

**Mayor chat — paste this text into the conversation, not your shell.**
The accepted brief describes the product; native planning generates its tasks.

```text
Read ../paint-app/context.md. Launch native build-basic for the paint rig's
accepted Studio Board Release 1. Use headless interaction, drain_policy=separate,
agent reviews, max_iterations=2, push=false and open_pr=false. Use
plans/studio-board/release-1. Generate the graph; honor ownership, prerequisite
commit imports, explicit integration and headless browser acceptance in the
brief. Stay available for conversation. Report one integrated app path/branch/
commit, actual task count, elapsed time, check results and screenshot paths.
```

Mayor should next show that the workflow has started or is planning. That
means the build is underway; continue to Step 3. **If Mayor cannot locate the
accepted Studio Board brief or still sees paint preferences, stop here and use
the older-run help. Do not continue to app checks or Release 2.**

## Step 3: wait for Release 1 to finish

**Mayor chat — stay in this conversation while Gas City works.** It creates
requirements and a plan, decomposes the work, starts ready workers, and runs
integration and review. Mayor stays available for conversation. A launch message,
finished planning stage or several completed tasks does not mean the app is done.

A **convoy** is a named group of related tasks and their dependencies. The
implementation convoy becomes available when this release's tasks are created.
For progress, paste this request into Mayor:

```text
Show me the implementation tasks for this release and their dependencies.
Print the exact terminal commands to view their graph and progress, with the
real convoy ID already filled in. Summarize finished, running and blocked tasks.
If the implementation tasks are not created yet, tell me what stage we are at.
```

If you want to run Mayor's returned terminal commands, detach with Ctrl+B, then D.
Run them in that same city terminal, then use `gc session attach mayor` to return
to the conversation. The graph shows dependencies/readiness within that group;
status shows completion counts and each task's status/assignee. Record real
implementation-task counts separately from workflow/control beads.

The graph establishes shared interfaces first, then independent owned modules,
then integration and review. Dependencies order execution; workers must import
prerequisite commits into their own worktrees. The designated integrator combines
their own product commits into one release branch/app, and review checks that
exact result. Stock isolated worktrees do not automatically merge the product.

**Wait for Mayor's Release 1 final report before Step 4.** It must identify one
integrated app path, feature branch and commit, passing Node/browser checks,
actual artwork screenshots and an independent review. If work is still running
or a check failed, remain on Release 1 and resolve the reported blocker.
**Blocked is not finished: do not run npm or paste the Release 2 prompt.** If
Mayor cannot find the accepted Studio Board brief in an older preferences demo,
use the older-run help below before launching Release 1. The starter's files
and checks are not Release 1 acceptance.

## Step 4: inspect the finished Release 1 app

**No finished, checked app report yet? Stay at Step 3. Do not run npm yet.**
The `city` folder is for Gas City; it is not an app folder. The starter is not the
finished release either. Get the actual integrated location from Mayor.

**Mayor chat — after the final report, paste this request:**

```text
For the current Studio Board release, identify the completed, integrated app's
absolute path, branch and commit, with passing checks, browser evidence and
review. Only if it is ready, print one ready-to-copy terminal block for ME to
run; do not execute it as part of this request. Start with its actual absolute
cd, followed by npm run check, npm run browser:check and
python3 -m http.server 4173 --bind 127.0.0.1. Join the commands with && so the
block stops if any command fails. If the accepted brief is missing, work is
blocked/unfinished, or no verified integrated result exists, report the blocker
and give no check/preview commands. Do not substitute the starter paint-app
folder for the integrated Studio Board release.
```

**App preview terminal — open a second terminal.** Copy Mayor's entire returned
block, including its absolute `cd`, and run it there. Continue only if that `cd`
succeeds. This leaves the original city terminal and Mayor available for Release 2.
Do not guess a folder or run just the npm lines from the city folder.

Open <http://127.0.0.1:4173>. Make a layered poster, undo/redo and reload it; try
JSON import and JSON/PNG/SVG export. Check the report and numbered, named
screenshots in `artifacts/release-1/`. They must show real artwork and tested
states. The automated browser check uses headless Playwright in an isolated
context, without taking over your browser. Blank-canvas screenshots or passing
artifact schemas do not establish product acceptance.

`check` and `browser:check` are scripts produced by the build, not capabilities
assumed in the starter. Missing scripts, failed checks or missing integration/
review evidence block the next step. Return to Mayor with the actual issue.

Stop the viewing server with Ctrl+C. **Keep the city running and return to the
Mayor chat in the original terminal.** Proceed only after Release 1 is complete,
its checks/review pass and you have inspected the result.

## Step 5: ask Mayor to build Release 2 on Release 1

**Mayor chat — only after Step 4 passes, paste this text.**

```text
Continue with accepted Studio Board Release 2 from ../paint-app/context.md.
First verify Release 1's integrated app/branch/commit and passing checks/review.
Perform the one local release handoff in the brief: preserve clean state and
bring that commit onto the paint launcher's feature branch; verify it in
launcher HEAD. Then launch native build-basic for Release 2 with headless
interaction, drain_policy=separate, agent reviews, max_iterations=2, push=false
and open_pr=false, using plans/studio-board/release-2. Keep conversation
available. Report the integrated result, task count, elapsed time,
regression/browser checks and screenshots.
```

Mayor performs one local handoff of verified Release 1 onto the paint launcher's
feature branch. New native worktrees then start from that launcher HEAD. The
graph's integrator owns task assembly, and review targets its exact result.
Starting Release 2 from the original tiny app would omit the prerequisite code.

Wait for the Release 2 final integrated app report, regression/browser checks,
screenshots and independent review. If Mayor says Release 1 is missing or the
brief still describes paint preferences, use the help section below. Do not
invent a Release 1 result or bypass its checks.

## Step 6: inspect the final app and finish

Use the Step 4 location request in Mayor chat again for **Release 2**. In the
app preview terminal, run its returned block with Release 2's actual app path.
Try its project/page, selection, template, zoom/pan and keyboard flow from the
brief. Inspect `artifacts/release-2/` and the Release 1 regression evidence.
Stop the viewing server with Ctrl+C when finished.

**Original city terminal — detach from Mayor with Ctrl+B, then D, and stop the demo:**

```bash
gc stop --timeout 30s
gc supervisor stop --wait
```

## Help: an earlier demo already exists

**Already using Studio Board?** Keep that city. With Mayor chat open, if Release
1 has not started, go to Step 2. If it is underway, go to Step 3 and wait; do not
archive it, rerun setup or launch it again. If it is complete and checked,
inspect it in Step 4 before Release 2. An open Mayor alone does not show a completed Release 1.

**Older paint-preferences demo or a failed setup?** Source changes do not update
an existing demo. If Mayor cannot locate the accepted Studio Board brief, do not
launch either release or treat the starter as finished. Save any app files,
reports or Git history you want to keep outside `.demo-runs/04` first. Follow
[Optional cleanup](#optional-cleanup-start-fresh), which shuts down and deletes
only this demo, then return to **Step 1** for a fresh Studio Board setup. Start
Release 1 first and wait for its checked, integrated result before Release 2.

## Help: npm cannot find package.json

An error naming `.demo-runs/04/city/package.json` means app commands were run in
the city folder. Do not install packages there. Moving to the starter paint-app
folder does not complete Studio Board; a missing `preferences.js` there is not
evidence of a completed Release 1. If Mayor is blocked on the accepted brief or
has no integrated result, return to Step 3 or the older-run help. Only a verified
finished release proceeds to Step 4 and its actual `cd`/check/preview block.

## Help: terminal observation

For additional terminal observation, detach from Mayor with Ctrl+B, then D.
**Terminal — run from the city folder:**

```bash
gc session list
gc --rig paint convoy list
gc --rig paint bd list --all
gc events --follow
```

Ctrl+C stops the event viewer; `gc session attach mayor` returns to the chat.
The event viewer is observation; running it does not itself configure selective
notifications into Mayor. Status records do not establish browser acceptance or
guaranteed recovery.

For Claude Code, install and authenticate it, then change `provider=codex` to
`provider=claude` at the top of setup.sh before creating the fresh demo.

## Who generates and advances the graph?

This setup selects the stock `gascity` template and asks Mayor to launch
`build-basic` with an accepted brief. Its formulas define the lifecycle stages;
the configured planning and decomposition agents generate the implementation
tasks from that brief. You do not supply a handwritten implementation graph.
The formulas materialize steps and dependencies as persisted Beads work, and
the native runtime advances ready work and manages its agent sessions.

Mayor remains the user-facing conversation. The supplied headless prompt
delegates planning stages rather than asking Mayor to brainstorm requirements
with you. Session status, work records and the event viewer expose execution;
their presence alone does not establish live steering, escalation delivery or
successful recovery. Check the mechanisms and the observed result. See
[Planner and Controller](reference.md#planner-and-controller).

**Rehearsal limit:** Gas City 1.4.2 / Beads 1.3.0 startup, planning and design-review
dispatch were observed earlier for the small exercise. The enlarged setup,
complete two-release implementation/integration, browser acceptance, local
release handoff and Claude alternative have not been rehearsed. The city Git
isolation fix is also unrehearsed; static checks do not prove startup. The brief's
commit-import/integration rules are requirements, not a claim that stock Gas City
automatically implements or has verified them.

Native references: [Gas City](https://github.com/gastownhall/gascity) and
[build-basic workflow](https://github.com/gastownhall/gascity-packs).

## Optional cleanup: start fresh

This deletes only `.demo-runs/04`: its app, tasks, logs, tooling and local Git
history. Stop the preview server with Ctrl+C. If attached to Mayor, detach with
Ctrl+B, then D.

**Terminal — CAP repository root.** Replace `~/code/cap` if your checkout is elsewhere.
If you already shut down in Step 6, skip this stop block; the supervisor is not running.

```bash
cd ~/code/cap &&
  export GC_HOME="$PWD/.demo-runs/04/.gc-home" &&
  gc stop .demo-runs/04/city --timeout 30s &&
  gc supervisor stop --wait
```

Only after shutdown, delete the demo from the CAP repository root:

```bash
cd ~/code/cap && rm -rf .demo-runs/04
```

Bare `rm -rf` has no target and removes nothing. Run Step 1 to prepare a new demo.
