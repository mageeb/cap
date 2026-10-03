# Demo 3 — A Codex Planner and an external DAG Controller

**Live class slot:** 38–48 minutes. Prepare and rehearse before class.

**CLI used here:** the default instructions use Codex CLI. If you use Claude Code, see [the optional Claude path](#if-you-use-claude-code) before running setup; changing a command name alone does not adapt a controller.

**Watch for:** planning chat stays available while a separate program dispatches ready tasks, checks each result, and starts a fresh agent only when work or judgment is needed.

**Deliverable:** a small paint app with persistent `{tool, color}` preferences, a reviewed PRD and JSON dependency graph, recorded Controller state, real check outputs, and Playwright screenshots.

The paint app starts from the scaffold below. This is a small teaching Controller, not Gas City. Its five task types and checks are deliberately fixed so the control code remains readable. It calls Codex for Workers, one conditional Supervisor, and a Reviewer. Selecting ready tasks, watching processes, persisting state, and enforcing limits are ordinary Python operations.

## 1. Prepare the environment outside class

Use three terminals during the live segment:

- **Planner:** an interactive Codex conversation.
- **Controller:** the external Python program.
- **Observer:** state, checks, screenshots, and the prepared failure boundary.

### Install missing runtime tools (macOS)

These executable instructions use an **already installed Codex CLI**. You need macOS 14+, a supported Node.js release (22, 24, or 26) with npm, Python 3.10+, and Git 2.28+. Use a currently supported Node LTS release when installing. Each runbook is independent; do not borrow another demo's dependencies.

If those runtime tools are already available, skip this installation block. If any are missing or older, the following installs the runtimes through an existing Homebrew installation and selects them in this terminal:

```bash
brew install node@24 python git
export PATH="$(brew --prefix node@24)/bin:$(brew --prefix)/bin:$PATH"
```

If `brew` is missing, follow the [official Homebrew installation instructions](https://brew.sh/) first, including its printed **Next steps** for adding Homebrew to your shell, then run the block above. The Homebrew installer explains its machine changes before applying them. Alternatively, use the official [Node.js macOS LTS installer](https://nodejs.org/en/download) and [Python macOS installer](https://www.python.org/downloads/macos/), and [Git's macOS installation instructions](https://git-scm.com/install/mac).

Open a fresh terminal after installing. If you used the Homebrew block, repeat its `export PATH=...` line in every new demo terminal so they select the same runtimes. This guide's setup commands target macOS. The browser requirements follow [Playwright's supported platforms](https://playwright.dev/docs/intro#system-requirements).

### Verify the tools and account

Copy this complete block. It checks versions and authentication without running a model job:

This Controller requires Codex for its Workers even when Claude is the interactive Planner. The main preflight always checks Codex. For the optional mixed path, check Claude separately in [the Claude section](#if-you-use-claude-code); both CLIs are required.

```bash
bash <<'PREFLIGHT'
set -euo pipefail
for tool in node npm python3 git codex; do
  command -v "$tool" >/dev/null || { printf 'Missing tool: %s. Install it before continuing.\n' "$tool" >&2; exit 1; }
done
node -e "if (![22,24,26].includes(Number(process.versions.node.split('.')[0]))) { console.error('Need supported Node 22, 24, or 26'); process.exit(1); }"
python3 - <<'PY_CHECK'
import platform, re, subprocess, sys
assert platform.system() == 'Darwin' and int(platform.mac_ver()[0].split('.')[0]) >= 14, 'This setup requires macOS 14+'
assert sys.version_info >= (3, 10), 'Need Python 3.10+'
version = subprocess.check_output(['git', '--version'], text=True)
match = re.search(r'(\d+)\.(\d+)', version)
assert match and tuple(map(int, match.groups())) >= (2, 28), 'Need Git 2.28+'
PY_CHECK
node --version
npm --version
python3 --version
git --version
git -C /Users/michaelmurray/code/cap rev-parse --show-toplevel
codex --version
codex login status
CAP_DEMO_HELP=$(codex exec --help)
for flag in --ephemeral --sandbox --json --output-schema --ignore-user-config --disable; do
  case "$CAP_DEMO_HELP" in
    *"$flag"*) ;;
    *) printf 'Codex CLI lacks %s. Update your CLI before continuing.\n' "$flag" >&2; exit 1 ;;
  esac
done
codex features list
printf 'PASS: prerequisites ready. Continue to fixture setup.\n'
PREFLIGHT
```

**Continue only when the final `PASS` line appears.** If login fails, run `codex login`, complete the browser sign-in, and rerun the preflight. Use your normal approved account; no API key belongs in demo files. Authentication being present does not prove account quota or sandbox permissions: rehearse one real demo run before class. [Codex authentication](https://learn.chatgpt.com/docs/auth)

This runbook uses `--ephemeral`, `--sandbox`, `--json`, `-o`, `--output-schema`, `--ignore-user-config`, `-c project_doc_max_bytes=0`, `--disable memories`, `--disable multi_agent`, `-C`, and stdin `-`. The author inspected these options on Codex CLI 0.159.2; verify them on your installed version. Disabling memories and native multi-agent dispatch keeps each external job's input explicit. Every job gets a fresh context; the code and recorded artifacts carry progress. The explicit fixture contract replaces inherited project instructions for these isolated teaching calls; project_doc_max_bytes=0 prevents CAP AGENTS.md from adding unrelated repository workflows.

Create a **new** nested demo repository. The commands stop if this location already exists. Keep an existing run intact and choose a new directory name if necessary. These commands create runtime files only when you execute them. The curriculum contains this runbook and the commented [Controller and checks](scripts/external-controller/controller.py) that you copy into the isolated fixture.

**Git isolation:** this setup does not switch CAP's curriculum branch or create commits in CAP. It adds `/.demo-runs/` to CAP's local `.git/info/exclude`, preserving existing lines. That rule is not tracked or shared; this runbook configures it even on a fresh checkout. The fixture gets its own Git repository, feature branch and no remote. Run subsequent demo commands inside `$RUNDIR`.

**Use your actual fixture path throughout.** If the default `dag` directory already exists, change the `export RUNDIR=...` line below to a fresh directory name. Replace **every later** `/Users/michaelmurray/code/cap/.demo-runs/dag` path in this runbook with that same printed repository root before copying a block, including the optional Claude block. Never direct a later block back to an earlier run.

```bash
cd /Users/michaelmurray/code/cap || exit 1
CAP_DEMO_EXCLUDE=$(git rev-parse --git-path info/exclude)
if ! grep -qxF '/.demo-runs/' "$CAP_DEMO_EXCLUDE"; then
  printf '\n/.demo-runs/\n' >> "$CAP_DEMO_EXCLUDE"
fi
export RUNDIR="$PWD/.demo-runs/dag"
bash <<'SETUP'
set -euo pipefail
if [ -e "$RUNDIR" ]; then
  printf 'Existing run found: %s. Choose a fresh RUNDIR.\n' "$RUNDIR" >&2
  exit 1
fi
mkdir -p "$RUNDIR/app" "$RUNDIR/plan" "$RUNDIR/artifacts" "$RUNDIR/jobs"
git -C "$RUNDIR" init -b feature/paint-dag
cd "$RUNDIR"
if [ "$(git -C "$RUNDIR" rev-parse --show-toplevel)" != "$RUNDIR" ] ||
   [ "$(git -C "$RUNDIR" branch --show-current)" != "feature/paint-dag" ] ||
   [ -n "$(git -C "$RUNDIR" remote)" ] ||
   ! git -C /Users/michaelmurray/code/cap check-ignore -q "$RUNDIR/"; then
  printf 'Git isolation check failed. Stop here; do not run later blocks.\n' >&2
  exit 1
fi
git -C "$RUNDIR" rev-parse --show-toplevel
git -C "$RUNDIR" branch --show-current
git -C "$RUNDIR" remote -v
git -C /Users/michaelmurray/code/cap check-ignore -v "$RUNDIR/"
git -C /Users/michaelmurray/code/cap status --short
cat > package.json <<'JSON'
{"name":"cap-week4-dag","private":true,"type":"module"}
JSON
cat > .gitignore <<'EOF'
node_modules/
artifacts/
jobs/
EOF
npm install --save-dev --save-exact playwright
PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium
PLAYWRIGHT_BROWSERS_PATH=0 node <<'BROWSER_CHECK'
const {chromium} = require('playwright');
(async () => {
  const browser = await chromium.launch({headless:true});
  try { console.log('PASS: Playwright ' + require('playwright/package.json').version + '; Chromium ' + browser.version()); }
  finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
BROWSER_CHECK
node -p "require('playwright/package.json').version" > artifacts/playwright-version.txt
codex --version > artifacts/codex-version.txt
printf 'Prepared pause before integration.\n' > PAUSE_INTEGRATION
SETUP
```

Playwright and its matching Chromium are installed in this fixture's ignored `node_modules/`, independently of other demos or your system Chrome. **Stop if installation or the `PASS: Playwright ...; Chromium ...` launch check fails.** To repair a missing browser, run `cd "$RUNDIR"` then `PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium`. [Playwright library setup](https://playwright.dev/docs/library), [matching browser installation](https://playwright.dev/docs/browsers)

The generated lockfile records the Playwright version used in this run. Only CAP’s local `.git/info/exclude` is updated; no tracked ignore file changes. The demo repository uses `feature/paint-dag`.

During setup, verify the printed results before continuing:

- Repository root: `/Users/michaelmurray/code/cap/.demo-runs/dag`.
- Branch: `feature/paint-dag`.
- Remotes: no output from `git remote -v`.
- CAP ignore check: `.git/info/exclude` supplies the `/.demo-runs/` rule; its line number may vary.
- CAP status: no `.demo-runs/` entry. Existing curriculum changes may still appear.

**If a check fails, stop:** keep the fixture intact, open a new terminal, choose a fresh `RUNDIR`, and repeat setup. Use that same new absolute path in all three terminals. Do not run the remaining blocks until the repository root, branch, remote and ignore checks pass. For later terminals, restore `$RUNDIR` to the printed repository root and `cd "$RUNDIR"` before following commands.

### App scaffold

Run this block in the Observer terminal. The same `RUNDIR` value is repeated in each terminal so no shell state is assumed. If you chose a different fixture directory, replace the shown default path with your printed repository root before pasting.

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
cat > app/index.html <<'HTML'
<!doctype html>
<html lang="en">
<head><meta charset="utf-8"><title>CAP Paint</title>
<style>
body { margin: 32px; background: #F4F0E7; color: #152536; font: 20px Arial; }
label { margin-right: 24px; } canvas { display: block; margin-top: 24px; border: 1px solid #152536; background: white; }
</style></head>
<body>
<h1>CAP Paint</h1>
<div id="toolbar"></div>
<canvas id="paint" width="600" height="240"></canvas>
<script type="module" src="main.js"></script>
</body></html>
HTML
cat > app/main.js <<'JS'
// Integration Worker will connect toolbar, storage, and drawing.
JS
```

### Copy the real teaching files

The scripts are checked in beside this runbook. Copy them into your isolated demo repository; run them there so logs, app changes, and state stay out of the curriculum branch.

In the Observer terminal, after creating the scaffold:

```bash
# This is the tracked source folder. It is not the working demo repository.
DEMO_SCRIPTS=/Users/michaelmurray/code/cap/curriculum/weeks/04/scripts/external-controller

# Copy the program and its Human-owned checks into this run.
cp "$DEMO_SCRIPTS/controller.py" "$RUNDIR/controller.py"
cp "$DEMO_SCRIPTS/checks.py" "$RUNDIR/checks.py"
cp "$DEMO_SCRIPTS/browser-check.mjs" "$RUNDIR/browser-check.mjs"
cp "$DEMO_SCRIPTS/supervisor-schema.json" "$RUNDIR/supervisor-schema.json"
cp "$DEMO_SCRIPTS/review-schema.json" "$RUNDIR/review-schema.json"

# The Planner will turn this fixed template into the accepted task graph.
cp "$DEMO_SCRIPTS/task-graph-template.json" "$RUNDIR/plan/task-graph-template.json"

# Keep the prepared defect separate until the safe injection boundary.
cp "$DEMO_SCRIPTS/prepared-broken-settings.js" "$RUNDIR/artifacts/prepared-broken-settings.js"
```

Check that every copy command completed before continuing. If a source file is missing, stop and check that you are using the complete Week 4 branch and the correct CAP path.

| File | What to explain to students |
|---|---|
| [controller.py](scripts/external-controller/controller.py) | Pick ready tasks, start fresh agents, check output, publish owned files, retry or stop. |
| [checks.py](scripts/external-controller/checks.py) | The program checks the actual schema, modules, storage behavior, and Reviewer report. |
| [browser-check.mjs](scripts/external-controller/browser-check.mjs) | Reload, draw teal pixels, erase a fresh stroke, and recover from bad saved data. |
| [task-graph-template.json](scripts/external-controller/task-graph-template.json) | Five tasks, their prerequisites, owned files, checks, and two-attempt budgets. |
| [supervisor-schema.json](scripts/external-controller/supervisor-schema.json) / [review-schema.json](scripts/external-controller/review-schema.json) | Restrict the decisions and reports an agent can return. |
| [prepared-broken-settings.js](scripts/external-controller/prepared-broken-settings.js) | An intentionally wrong `colour` field for the labeled repair demonstration. |

### Independent checks

The checks are Human-owned. Workers receive isolated app copies and publish only their declared files; the Controller invokes these checks from outside those copies. Changed, added, deleted, or symlinked unowned app paths are rejected before checks, preventing a candidate from passing against unpublished helper edits.

### The graph template

Each task has `id`, `deps`, `owned`, `task`, `check`, and `max_attempts`. `deps` are prerequisite task IDs. `check` is a whitelisted independent criterion, not an arbitrary shell command supplied by a model. Open the actual JSON before asking the Planner to write the plan:

```bash
python3 -m json.tool "$RUNDIR/plan/task-graph-template.json"
```

### Walk through the external Controller

Open [controller.py](scripts/external-controller/controller.py) in your editor. Read the five numbered comments in the **MAIN LOOP** first:

1. Keep the approved plan fixed and honor STOP.
2. Poll running processes without calling a model.
3. Find tasks whose prerequisites passed.
4. Start those tasks; UI and storage can run together.
5. Finish when every task passed, then ask for Human acceptance.

Then read `start()` and `finish()`. `start()` launches a fresh context into an isolated app copy. `finish()` runs a Human-owned check before publishing declared output files. Failed attempts preserve their candidate files for the next fresh context; they never publish unchecked code to the live app.

The shell commands in this runbook only set paths, copy files, and start programs. The **Python main loop** does the orchestration; students do not need to understand a large Bash launcher. The smaller functions explain process cleanup, the plan hash, bounded repair, and the optional notification back to the Planner.

**Control boundary:** only this program chooses ready tasks. Each job has a separate workspace and fresh context. The Controller publishes declared regular files after a real check passes. Copy isolation plus the Codex workspace sandbox limits conflicting writes; this is not a general security boundary for arbitrary untrusted code. The five fixed roles, fixed checks, and maximum of two attempts are intentional teaching constraints.

## 2. Live: brainstorm and accept the plan (38–40)

In the Planner terminal, replace the shown default path with your actual fixture root if it differs:

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
codex -C "$RUNDIR" --sandbox workspace-write --disable memories --disable multi_agent --ignore-user-config -c project_doc_max_bytes=0
```

Paste this prompt into the interactive chat:

```text
You are the Planner for this paint app. Discuss the smallest useful persistent-preferences feature with me before writing files. The accepted behavior is: pencil/eraser selection and one of #152536, #188D91, or #E8A64C persist after reload; pencil uses the selected color for new strokes; malformed or unsupported saved data restores pencil and #152536. The interface is {tool,color}.

Explain the task ordering schema -> (UI and storage in parallel) -> integrate -> review. The external Python Controller will perform dispatch, checks, bounded retries, and state monitoring; do not launch agents or implementation jobs from this conversation.

After I say "write the plan", write plan/prd.md with these acceptance criteria and plan/task-graph.json based on plan/task-graph-template.json. Preserve version 1, the five IDs, dependencies, owned files, check IDs, and max_attempts=2. You may make task prose clearer without expanding scope. Do not change app/, checks.py, controller.py, or any schema/check files. Ask about any conflict instead.
```

Discuss briefly, then send:

```text
Write the plan. Keep the accepted scope and the template's execution fields unchanged. Summarize the acceptance criteria and ordering for my review. Do not start work.
```

In the Observer terminal, read the concrete output and validate it. Replace the shown default path with your actual fixture root if it differs:

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
cat plan/prd.md
python3 -m json.tool plan/task-graph.json
python3 controller.py --validate
```

Inspect the PRD, task prose, prerequisites, owned files, criterion IDs, and two-attempt budgets. If they do not match the accepted contract, ask the Planner to fix them and repeat validation. When you accept this exact version, run:

```bash
python3 controller.py --approve
```

This explicit Human command records a hash of both the PRD and graph. The Controller rejects later plan changes. Planning chat remains a place for discussion; changing accepted scope during a run requires a separate reviewed run.

## 3. Live: fan out while planning remains available (40–43)

For optional automatic completion/blockage notifications, first send this documented command in the open Planner chat (choose a unique name and use the same name below):

```text
/rename cap-week4-planner-20260929-01
```

In the Controller terminal, preflight the version-specific queue command. Replace the shown default path with your actual fixture root if it differs. If the queue command is unavailable, leave the two notification variables unset and use notification.txt.

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
unset PLANNER_THREAD PLANNER_QUEUE_READY
codex queue --help
```

Only if that help command succeeds, enable notification delivery:

```bash
export PLANNER_THREAD=cap-week4-planner-20260929-01
export PLANNER_QUEUE_READY=1
```

Start the Controller in that terminal:

```bash
python3 controller.py
```

Keep it running. In the Observer terminal, replace the shown default path with your actual fixture root if it differs:

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
python3 -m json.tool state.json
ls jobs
```

Repeat the state command as needed. Expected observations after a successful rehearsal:

1. `schema` passes before UI or storage starts.
2. `ui` and `storage` both become running before either needs to finish. They use separate job directories and owned files.
3. Passing outputs are copied to `app/`. The Controller pauses before integration because `PAUSE_INTEGRATION` exists.
4. No agent is called merely to poll state. Routine selection and monitoring are Python code.

These are expected observations, not recorded results. Actual calls can be slower or fail; show the real state and use the fallback below.

While the Controller is busy, paste this into the original Planner chat:

```text
While the accepted preference work runs, brainstorm a future undo feature with me. Give two useful user stories and explain what state undo would need. Keep these as conversational proposals. Do not edit files, change the accepted plan, dispatch work, or poll the Controller.
```

The Planner can answer while execution continues in another process. The Controller's accepted graph remains unchanged. If you enabled the optional notification below, the Planner receives a queued message only when the Controller completes or blocks, not on each Started/Passed event.

## 4. Live: inject one labeled mismatch at the pause (43–46)

Wait until `state.json` says **paused before integration**, with UI and storage **passed** and no Worker running. This is the only injection boundary. Do not overwrite a file while a Worker owns it.

In the Observer terminal, verify the boundary and the successful storage check before injecting the prepared defect:

```bash
bash <<'INJECT'
set -euo pipefail
cd "$RUNDIR"
python3 - <<'PY'
import json
s=json.load(open('state.json'))
assert s['phase']=='paused before integration'
assert s['tasks']['ui']['status']=='passed'
assert s['tasks']['storage']['status']=='passed'
assert s['tasks']['storage']['attempts']==1, 'Repair requires the remaining storage attempt'
assert not any(t['status']=='running' for t in s['tasks'].values())
print('Approved prepared injection boundary reached')
PY
python3 checks.py storage app
cp app/settings.js artifacts/settings.before-injection.js
# Inject the labeled defect only after the boundary checks above pass.
cp artifacts/prepared-broken-settings.js app/settings.js
printf 'Prepared colour/color mismatch injected after storage passed, before integration.\n' > artifacts/prepared-injection.txt
rm PAUSE_INTEGRATION
INJECT
```

Watch the Controller terminal and inspect actual evidence:

```bash
python3 -m json.tool state.json
cat jobs/integrate-1/check.txt
cat jobs/supervisor/assignment.json
```

Wait for those artifacts to exist before reading them. The integration gate checks the actual exported storage interface, so `colour` fails even if an Integration Worker tries to normalize it locally. On the first integration failure, the Controller invokes one fresh, read-only Supervisor. Its structured assignment can name the bounded `storage` repair or decline with `blocked`; the program does not let that judgment change the accepted graph or owned paths.

Storage gets its second attempt. Integration then gets its second attempt after storage passes. Both remain bounded. If another defect exceeds a limit or the Supervisor cannot return a supported assignment, the Controller stops blocked and writes `notification.txt`. Show the blocker instead of claiming success.

## 5. Live: independently check and accept (46–48)

After `integrate` passes, the Controller starts the read-only Reviewer with the actual integration-check output. The Reviewer reports criterion status and gaps. The Controller independently repeats browser and storage checks before marking review passed; a model's verdict alone cannot complete the run.

In the Observer terminal:

```bash
cd "$RUNDIR"
python3 -m json.tool state.json
cat notification.txt
cat artifacts/integration-check.txt
cat jobs/review-1/review.json
open artifacts/screenshots/pencil-teal-reload.png
open artifacts/screenshots/invalid-state-defaults.png
open artifacts/screenshots/eraser-after-reload.png
```

If the Reviewer required a second attempt, inspect `jobs/review-2/review.json` instead. Read the actual `job` path recorded for `review` in `state.json`.

The Human accepts only after inspecting:

- the PRD and unchanged plan hash;
- UI/storage ownership and published code;
- the labeled injection and conditional repair history;
- real storage, integration, and browser outputs;
- screenshots and pixel checks showing teal pencil drawing after reload, retained teal with eraser selection, actual erasure of a fresh stroke, and default recovery;
- Reviewer findings and any remaining gaps.

For an additional visible browser check, run the app in the Observer terminal and open the printed URL:

```bash
python3 -m http.server 5179 --bind 127.0.0.1 --directory app
```

Select pencil and teal, reload, and draw a teal stroke. Select eraser, reload again, and verify both eraser selection and the retained teal choice. Because canvas contents are not persisted, switch to pencil and draw a fresh teal stroke; switch back to eraser and erase that same stroke. Confirm the erased area is white/transparent. Stop this server with Ctrl-C. The accepted feature persists preferences, not canvas contents.

## 6. Notification, stop, and recovery

### Notification to the planning conversation

`notification.txt` is the fallback handoff for every run. If PLANNER_THREAD and PLANNER_QUEUE_READY were set after a successful preflight, the Controller automatically queues exactly one terminal notice when the run completes or blocks. Started/Passed events never queue a message and routine state polling never calls an LLM.

`codex queue --thread NAME --message TEXT` was visible in Codex CLI 0.159.2 help. It is a version-specific local capability; do not assume it exists on every installation. The interactive `/rename` command is documented. Rehearse both the unique session name and delivery before class.

Queue failure does not erase the completed/blocked state: the Controller prints the delivery error and saves notification_delivery in state.json. If queueing is absent, disabled, or fails, paste the actual notification into the open Planner chat:

```bash
cat "$RUNDIR/notification.txt"
```

Do not resume a Worker session as the Planner. This optional notification sends a message to the existing planning session; it does not make the Planner the polling Controller.

### Stop without losing the record

In the Observer terminal:

```bash
cd "$RUNDIR"
touch STOP
```

The Controller terminates active CLI process groups at its next poll, records pending tasks and a blocked reason, and exits. It also honors STOP while waiting for the conditional Supervisor. Ctrl-C and SIGTERM use the same cleanup path, sending termination to each job process group and killing it after a bounded wait if needed. An interrupted attempt still consumes its recorded budget. Inspect state and logs before recovery:

```bash
python3 -m json.tool state.json
cat notification.txt
```

If the accepted plan is unchanged and pending tasks still have remaining attempts, remove only the stop request and restart the same Controller:

```bash
rm -f STOP
python3 controller.py
```

Completed tasks stay completed. An interrupted running job becomes pending; its attempt is already consumed. Its artifacts remain under jobs/, but it is not accepted or reused as a candidate. The next permitted attempt starts from the published app. A check-failed candidate can be reused only for its owned files by the bounded repair loop; that differs from an interrupted job. Do not delete `state.json` to manufacture a fresh budget. If attempts are exhausted, preserve this run and prepare a new reviewed run in a fresh directory.

### Fallback for the ten-minute live slot

Rehearse the full successful path before class. Retain that run's real state, reports, code, and screenshots in a separately named directory. If live calls run long, stop at a recorded boundary and show the labeled rehearsal artifacts. Explain which checkpoint was live, which was prepared, and which condition remains unmet. Do not present expected observations as actual results.

## Sources and limits

- [Codex noninteractive mode](https://learn.chatgpt.com/docs/non-interactive-mode): CLI calls suitable for program control.
- [Codex native agents](https://learn.chatgpt.com/docs/agent-configuration/subagents): the separate native Supervisor demo uses this mechanism; this external Controller instead starts independent CLI calls.
- [Codex CLI commands](https://learn.chatgpt.com/docs/developer-commands?surface=cli): interactive controls including session naming.
- [Gas City execution model](https://docs.gascity.com/getting-started/how-gas-city-works): the deck's real architecture example, not a dependency of this custom demo.

Prepared on 2026-09-29. No live Codex jobs or browser runs are claimed by the runbook author. Install the prerequisites and rehearse actual dispatch, permissions, checks, repair, and Planner availability before teaching.

This Controller supports one reviewed five-node graph, optional version-specific terminal notifications, at most two attempts per task, one conditional storage-repair judgment, one running program, and copy-based publication. It does not implement arbitrary graph editing during execution, a distributed queue, crash-proof child-process recovery, permission escalation, or general merge resolution. Stop the active program before resuming; if a terminal/process crashes, inspect and stop any surviving Codex child processes before restarting. The durable state explains progress, while the Human remains responsible for final acceptance.

## If you use Claude Code

You can use Claude for the **interactive Planner** while retaining this runbook's **Codex Workers and Controller**. That mixed path still requires both installed CLIs: leave the main prerequisite check on Codex, then check Claude separately below. The Node/Python/Git/Playwright setup and Controller checks remain identical.

In the Planner terminal, replace the section 2 launch command with the block below. Replace the shown default path with your actual fixture root if it differs:

```bash
export RUNDIR=/Users/michaelmurray/code/cap/.demo-runs/dag
cd "$RUNDIR"
claude --version
claude auth status
claude
```

If sign-in is missing, run `claude auth login` before launching. Paste the same Planner prompts; only planning files are authorized for edits. Keep the accepted graph unchanged during execution, and make the same Human `--validate` / `--approve` calls in the Observer terminal. You can continue chatting with this Planner while the external program runs.

Leave `PLANNER_THREAD` and `PLANNER_QUEUE_READY` unset in the Controller terminal and use `notification.txt` for completion or blockage. The `codex queue` delivery code does not notify a Claude session.

An **all-Claude** version is a controller port, not a command-name substitution. `command()` and its callers must start fresh `claude -p` jobs with the correct allowed tools/permissions; normal JSON output is a result envelope and schema data lives in `structured_output`, unlike the Codex `-o` report file. Worker, read-only Supervisor and Reviewer outputs/status handling all need adapters. Preserve isolated candidate copies, ownership checks, independent browser gates, process-group cleanup, retries and plan hashes. Replace notification delivery separately or keep the file handoff. These adapters are not implemented in this runbook; use the native [Supervisor demo](demo-02-supervisor.md#if-you-use-claude-code) for a runnable Claude-only exercise.

[Claude programmatic calls and result envelopes](https://code.claude.com/docs/en/headless), [Claude CLI authentication](https://code.claude.com/docs/en/cli-reference)
