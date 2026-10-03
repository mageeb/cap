# Demo 1 — A fresh-context programming loop

**Goal:** turn an empty folder into a browser paint app, then let an agent choose small improvements. Start with three passes. Inspect the code, saved images and ledger to decide whether the improvements are useful. A longer experiment can use up to 100 passes in a fresh fixture.

**What you might see:** a first pass could add a drawable canvas; a later pass could add a color picker. These are examples, not a promised feature sequence. The agent chooses its next improvement from the current app and ledger.

**How it runs:**

1. A Bash controller starts a new `codex exec` conversation with the **same request plus the current ledger**. The agent reads the code left by earlier passes.
2. The agent changes the app within **1,000 added plus deleted app lines per pass**. The conversation is fresh; the code and ledger persist.
3. The controller checks the diff, opens a real headless browser (no window), checks that a pointer drag changes canvas pixels, rejects page errors and requires a screenshot.
4. Only a pass that clears those gates gets a ledger entry and local Git checkpoint. The ledger separates agent claims from observed checks; human acceptance remains pending.
5. The controller starts the next pass, or stops on a failure or timeout with the attempted code and evidence preserved. There is no automatic repair attempt.

**Where to work:** begin in a terminal opened at your CAP checkout. Setup prints a separate `.demo-runs/loop-...` folder and you enter it. Run its copied helpers there. During a run, `[1/6]` through `[6/6]` messages show the current stage, and `CHECKPOINT` marks a pass that cleared the smoke checks. Stage 2 can look quiet because agent output goes to files; section 5 shows how to follow it in another terminal.

| Inside your printed fixture folder | What to observe |
|---|---|
| `app/` | The evolving paint-app source |
| `.harness/` | The copied controller/helpers and fixed prompt |
| `ledger.md` | Completed-pass claims and observed check results |
| `artifacts/TIMESTAMP-PASS/events.jsonl`, `agent.stderr` | The current agent's output and errors |
| `artifacts/TIMESTAMP-PASS/app.diff`, `checks.txt` | The attempted change and browser result |
| `artifacts/TIMESTAMP-PASS/screen.png`, `checkpoint.txt` | One image after a successful browser check and the completed local checkpoint |

**Time and usage:** setup downloads Playwright and its matching Chromium, so allow several minutes depending on your connection and machine. Each pass can take seconds or minutes and consumes model usage; default limits are 180 seconds for the agent and 60 seconds for the browser check. Three passes are a starting point, and 100 can take hours. No number of passes guarantees a better app.

This is the Ralph-style programming-loop pattern implemented with an ordinary script; no Ralph plugin is required. Its browser gates are smoke checks, so inspect the product yourself.

**CLI used here:** the default instructions use Codex CLI. If you use Claude Code, see [the optional Claude path](#if-you-use-claude-code) before running setup. It provides one fresh Worker pass; the repeated Bash loop requires a Claude adapter.

## 1. Preflight

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

Open a terminal in the CAP checkout that contains `curriculum/weeks/04/`. `CAP_ROOT` names that curriculum checkout; `CAP_LOOP_DEMO` later names a separate nested app repository. Derive `CAP_ROOT` here before entering the fixture, and keep it for later steps. `pwd` below should show your CAP checkout. If you are already at a fixture root, first use `cd ../..` to return to CAP before deriving `CAP_ROOT`. The preflight checks versions and authentication without running a model job:

For the default path, leave `CAP_DEMO_AGENT` unset. Claude Code users can run `export CAP_DEMO_AGENT=claude` first; the preflight then checks Claude instead. This variable changes only the preflight, not the controller scripts. Follow the optional Claude section for execution.

```bash
pwd
export CAP_ROOT="$(git rev-parse --show-toplevel)"
[ -f "$CAP_ROOT/curriculum/weeks/04/scripts/loop/setup.sh" ] && \
  bash "$CAP_ROOT/curriculum/weeks/04/scripts/loop/setup.sh" --preflight || \
  printf 'STOP: check CAP_ROOT is your CAP curriculum checkout and resolve any preflight errors.\n'
```

**Continue only when the final `PASS` line appears.** If login fails, run `codex login` (or `claude auth login` for the optional Claude path), complete the browser sign-in, and rerun the preflight. Use your normal approved account; no API key belongs in demo files. Authentication being present does not prove account quota or sandbox permissions; the first real pass checks whether your account can run the requested work. [Codex authentication](https://learn.chatgpt.com/docs/auth)

Run with your account and the scoped `workspace-write` sandbox. This example ignores user configuration, disables memory and subagents, and uses an ephemeral session. It uses the CLI's default model; optionally supply `MODEL` below. These flags control this invocation, not the existence of other personal instructions or saved account authentication. If your installation enforces additional instructions or denies an operation, resolve the reported issue; the loop stops rather than widening its permissions.

## 2. Make the isolated fixture

The actual [setup script](scripts/loop/setup.sh) checks prerequisites, creates a fresh ignored nested repository on `feature/paint-loop` with no remote, installs this fixture's Playwright and matching Chromium, writes its fixed prompt and ledger, copies its six runtime helpers, and creates a local setup checkpoint. No agent runs during setup. The application directory starts empty.

Run these commands in your setup terminal. The script accepts an explicit fixture path; a child script cannot export that path back into your terminal.

```bash
export CAP_LOOP_DEMO="$CAP_ROOT/.demo-runs/loop-$(date +%Y%m%d-%H%M%S)"
bash "$CAP_ROOT/curriculum/weeks/04/scripts/loop/setup.sh" "$CAP_LOOP_DEMO" && cd "$CAP_LOOP_DEMO"
```

**Continue only after `PASS: fixture ready` appears.** If setup fails, keep its files intact, choose a fresh `loop-NAME` path, and repeat setup. An existing fixture is never overwritten. For a later terminal, copy both `export CAP_ROOT=...` and `export CAP_LOOP_DEMO=...` lines printed by setup, then enter the fixture. Do not derive `CAP_ROOT` again inside this nested repository.

The printed repository root must be your fixture, the branch must be `feature/paint-loop`, remotes must be empty, and CAP's status must have no `.demo-runs/` entry. Setup adds `/.demo-runs/` to CAP's local `.git/info/exclude`, preserving existing lines; the rule is not tracked or shared.

Playwright and its matching Chromium live in this fixture's ignored `.harness/node_modules/`. A separately installed browser does not replace that setup. If browser installation fails, enter the fixture's `.harness/` and run `PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium`, then repeat setup with a fresh fixture. [Playwright library setup](https://playwright.dev/docs/library), [matching browser installation](https://playwright.dev/docs/browsers)

Setup supplies a local demo commit identity only when you have none. If an existing signing configuration prevents its checkpoint, resolve that failure before running the loop. For disposable demo commits, `git config --local commit.gpgsign false` disables signing in this nested repository.

## 3. Inspect the prepared prompt and real scripts

Run the fixture copies of the helpers. The controller refuses CAP's source directory; local checkpoints belong in the nested demo repository.

```bash
cd "$CAP_LOOP_DEMO"
find app -type f
cat .harness/prompt.txt
cat ledger.md
```

The first command prints nothing. Read the exact repeated prompt: the Worker chooses a paint-app improvement, edits only `app/`, preserves drawable `canvas#paint`, and stays within 1,000 added plus deleted application lines. The current ledger is appended to that same request each pass.

| File | Job |
|---|---|
| [setup.sh](scripts/loop/setup.sh) | Prepare the independent fixture, fixed prompt, prerequisites and runtime copies |
| [loop.sh](scripts/loop/loop.sh) | Choose the next pass and decide whether to continue |
| [agent.py](scripts/loop/agent.py) | Start one fresh Codex call; stop its process group on timeout or interruption |
| [check.py](scripts/loop/check.py) | Put an overall time limit around the browser check and handle interruption |
| [changes.py](scripts/loop/changes.py) | Count added plus deleted app lines; reject binary changes |
| [capture.cjs](scripts/loop/capture.cjs) | Launch the app on an available local port, check drawing, save a screenshot |
| [journal.cjs](scripts/loop/journal.cjs) | Record the agent's claims separately from observed checks |

You have only inspected the setup files. No agent has run yet. Next, read the controller in step 4, then start three passes in step 5.

## 4. Read the controller

Open the copied file: `cat .harness/loop.sh`. Its comments and numbered output follow six steps:

1. Combine the **same request** with the current ledger.
2. Call a **fresh agent** and wait for it to finish.
3. Measure the app diff: at most **1,000 added plus deleted lines this pass**.
4. Open the app, check actual drawing, and save an image within a time limit.
5. Append claims and observed checks to the ledger.
6. Commit a **local checkpoint**, then repeat.

There is no hidden LLM scheduler. A failure stops with working files and logs preserved for a human. The browser check is a smoke check, not full product acceptance.

### Bash vocabulary for this script

- `passes`, `seconds` and `dir` are named values. Quoting `"$dir"` keeps a path together.
- `${ITERATIONS:-3}` means “use ITERATIONS if supplied; otherwise use 3.”
- `for` repeats the steps. `if` chooses what happens when a command succeeds or fails.
- `&` starts the agent helper in the background; `$!` remembers its process ID; `wait` waits for it.
- `>` saves output to a file; `2>` saves errors separately.
- `trap` arranges cleanup when the controller exits. The Python helper handles process groups.
- `set -euo pipefail` makes unexpected command failures and missing variables stop the script.

Follow the six-step flow first, then inspect the browser and process-management helpers for the details. When you are ready to start the agent, continue to step 5.

## 5. Run the experiment

Start with three passes. They can take several minutes and consume model usage. A failed pass does not automatically retry.

Each agent call has a 180-second limit by default; each browser check has a separate 60-second limit. Optional `CHECK_SECONDS=90` gives browser checks more time. Both limits must be positive integers. A timed-out check records its failure in `checks.txt`; it may have no screenshot.

```bash
ITERATIONS=3 PASS_SECONDS=180 bash .harness/loop.sh
```

Watch the stage messages in this terminal. `CHECKPOINT` means that pass cleared the controller's gates; `STOP` means inspect the named evidence directory before doing more work. If all three passes finish, expect three new local checkpoints, corresponding ledger entries and screenshots. A stopped pass may have only partial evidence.

To inspect progress while the first terminal is busy, open another terminal, copy the two export lines printed by setup, then run:

```bash
cd "$CAP_LOOP_DEMO"
export CAP_LOOP_PASS="$(ls -dt artifacts/*/ 2>/dev/null | head -n 1)"
[ -n "$CAP_LOOP_PASS" ] && tail -f "$CAP_LOOP_PASS/events.jsonl" "$CAP_LOOP_PASS/agent.stderr"
```

This selects the newest actual pass directory. Wait for stage 2 before running it. `Ctrl+C` stops this observer; the controller in the first terminal continues. The observer follows that pass only, so rerun the block when you want the next pass. A screenshot is saved after the browser check, not during every drawing action.

After exiting the observer, inspect the accumulated results:

```bash
cat ledger.md
find artifacts -name screen.png | sort
git log --oneline -- app
```

The ledger and commit log advance after a successful pass. A local checkpoint means the smoke gates passed; it does not mean you accepted the product.

For a longer experiment, use a **new fixture** by repeating setup in section 2. This may run for hours; inspect progress before trusting its output.

```bash
ITERATIONS=100 PASS_SECONDS=180 bash .harness/loop.sh > artifacts/run.log 2>&1 &
echo $! > artifacts/controller.pid
printf 'Controller PID: '; cat artifacts/controller.pid
tail -f artifacts/run.log
```

`Ctrl+C` exits `tail`, while the background controller continues. Stop the controller and its current agent call or browser check with:

```bash
kill -TERM "$(cat artifacts/controller.pid)"
```

Stopping preserves files and evidence. A started pass may have no screenshot or checkpoint if its agent, size check or browser check failed. The screenshot name contains a UTC timestamp and pass number.

## 6. Compare iterations

1. Read `.harness/loop.sh`, especially `codex exec` in the helper, `wait`, checks, and the local checkpoint. Identify where scheduling decisions happen.
2. Open the first, middle and last **actually completed** screenshots. Pick their directories from the listing; do not assume all 100 completed.
3. Read each corresponding `Agent implemented` and `Agent envisioned` ledger entry. Compare its claims with the screenshot and code diff. Identify what the next fresh session receives.

```bash
cd "$CAP_LOOP_DEMO"
cat .harness/loop.sh
find artifacts -name screen.png | sort
cat ledger.md
git log --oneline -- app
```

On macOS, replace the example directory below with an actual one from the listing:

```bash
open artifacts/ACTUAL-TIMESTAMP-PASS/screen.png
cat artifacts/ACTUAL-TIMESTAMP-PASS/checks.txt
cat artifacts/ACTUAL-TIMESTAMP-PASS/app.diff
```

To run one more pass, after checking the repository is clean:

```bash
ITERATIONS=1 PASS_SECONDS=180 bash .harness/loop.sh
```

### Open the current app yourself

After the loop has finished or stopped, use the same terminal to start a local server:

```bash
python3 -m http.server 4173 --bind 127.0.0.1 --directory app
```

Open <http://127.0.0.1:4173> in your browser and try drawing and any added controls. If port 4173 is occupied, choose another port in both the command and URL. Press **Ctrl+C** in the server terminal when finished. A stopped run shows its attempted code, which may have failed the gates.

The automatic browser check is headless and closes its temporary server after capture; it does not leave an app window open.

## 7. Human intervention and failure checkpoint

If the controller stops, inspect the last directory's `agent.stderr`, `checks.txt`, `report.json` if present, and `app.diff`. No checkpoint means the current working tree still contains the attempted changes. Do not continue as if those changes passed.

A bounded repair uses a new conversation too. Substitute a real observed failure in this prompt. The repair is restricted to the existing app; it does not run another autonomous improvement pass.

```bash
codex exec --ignore-user-config --ephemeral --sandbox workspace-write \
  --disable memories --disable multi_agent -c project_doc_max_bytes=0 \
  --cd "$CAP_LOOP_DEMO/app" \
  'Repair only this observed failure: PASTE THE ACTUAL FAILURE HERE. Edit only this app directory. Preserve other behavior. Keep static index.html and drawable canvas#paint. Do not install, commit, spawn agents or start a server. Explain the repair and what remains unverified.'
```

Then rerun the browser helper into a distinct evidence file, inspect the change size and image, and either checkpoint the repair locally or leave the run stopped:

```bash
cd "$CAP_LOOP_DEMO"
node .harness/capture.cjs app artifacts/human-repair.png
git add -N app
git diff --numstat -- app
git diff -- app
open artifacts/human-repair.png
```

If accepted, add a human-written ledger entry describing the actual failure, repair, and inspection, then `git add app ledger.md` and `git commit -m "Human-reviewed repair"`. No reset or rollback command is hidden in this runbook.

## Optional: start another experiment later

When you have finished inspecting this fixture, return to CAP **from the fixture root**:

```bash
cd "$CAP_LOOP_DEMO"
cd ../..
pwd
```

You should see the checkout containing `curriculum/weeks/04/`. Repeat the preflight block there and choose a fresh fixture for the next experiment. Keep the current fixture and its evidence intact.

## Ask an agent to build a similar harness

To recreate this pattern for another small app, give an agent this prompt:

```text
Build a small external Bash harness for a browser paint app. Each pass must
start a fresh Codex CLI conversation with the same request plus a persistent
ledger, carry app source forward, and cap added plus deleted app lines at 1,000.
Keep setup and helpers in actual script files. Create a fresh ignored nested
Git repository on a feature branch with no remote. Keep CAP source untouched.
Use bounded agent and browser deadlines with process-group cleanup on stop.
After each pass, check a real pointer drag changes canvas pixels, reject page
errors, require a saved screenshot, and record claims separately from observed
checks. Save a local checkpoint only after the gates pass. Stop on failure with
code and evidence preserved for human inspection. Do not resume old sessions,
install app dependencies, push, or claim checks passed without actual results.
Show the short setup/run commands and explain where scheduling decisions live.
```

## What this demonstrates

- **Best fit:** exploratory work with cheap feedback and a modest change budget.
- **Strength:** small controller, fresh conversations, durable progress evidence.
- **Limit:** no task dependencies, parallelism, independent review, or guarantee of worthwhile improvements. A drawable canvas check can pass while the product is poor.
- **On the loop:** the human owns the goal, examines actual results, changes constraints, and stops or repairs the process.

Current command behavior: [Codex non-interactive mode](https://learn.chatgpt.com/docs/non-interactive-mode), [configuration reference](https://learn.chatgpt.com/docs/config-file/config-reference). These instructions describe a student exercise. Setup and its matching Chromium launch have been verified. Full agent iterations and app browser acceptance have not been verified in this repository.

## If you use Claude Code

**Available here:** one fresh Claude Worker call with the same app, prompt, and ledger. The Bash loop above still calls Codex; running all iterations with Claude requires replacing `.harness/agent.py` with an adapter. Start from the runtime/fixture/helper setup above, with `CAP_DEMO_AGENT=claude` during preflight; skip `.harness/loop.sh` for this optional single-pass example.

After sections 1–3, run this in the setup terminal:

```bash
bash "$CAP_ROOT/curriculum/weeks/04/scripts/loop/claude-pass.sh" "$CAP_LOOP_DEMO"
```

Inspect the change count and image yourself. This single pass does not append the ledger, enforce the 1,000-line gate, or create a checkpoint automatically. A full adapter must keep the existing process-group timeout, produce the bare report JSON expected by the journal, return failure on `is_error`/missing output, and leave the controller's size, browser, ledger and Git checks intact. Start a new `claude -p` call each pass; do not use `--continue` or `--resume`.

`--safe-mode` suppresses custom instructions, plugins and memory while retaining normal authentication. `--no-session-persistence` alone only prevents saving the session. The selected tools permit file work; the CLI's own permissions still apply. Do not claim the Codex sandbox and Claude permissions are identical. [Claude CLI flags and authentication](https://code.claude.com/docs/en/cli-reference), [programmatic output and permissions](https://code.claude.com/docs/en/headless)
