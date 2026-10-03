# Demo 1B — A task queue with completion loops

**Prepare before class. Class slot:** the second half of minutes 16–20, alongside Demo 1. Read the controller and compare its actual task evidence. This is a small, sequential queue with bounded repair, not a DAG scheduler. Keep the larger Supervisor and Controller demos live.

**CLI used here:** the default instructions use Codex CLI. If you use Claude Code, see [the optional Claude path](#if-you-use-claude-code) before running setup; changing a command name alone does not adapt a controller.

**Real example:** implement three agreed paint-app tasks in order. Each task has a different deterministic exit criterion. The controller moves forward only after the current task's criterion passes. A new Codex conversation performs every attempt; disk code, task instructions and check output carry state.

This fixture is independent of both other demos. No files from them are required.

## 1. Preflight and fixture

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

For the default path, leave `CAP_DEMO_AGENT` unset. Claude Code attendees can run `export CAP_DEMO_AGENT=claude` first; the preflight then checks Claude instead. This variable changes only the preflight, not the controller scripts. Follow the optional Claude section for execution.

```bash
bash <<'PREFLIGHT'
set -euo pipefail
CAP_DEMO_AGENT=${CAP_DEMO_AGENT:-codex}
for tool in node npm python3 git "$CAP_DEMO_AGENT"; do
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
case "$CAP_DEMO_AGENT" in
  codex)
    codex --version
    codex login status
    CAP_DEMO_HELP=$(codex exec --help)
    for flag in --ephemeral --sandbox --json --output-schema --ignore-user-config --disable; do
      case "$CAP_DEMO_HELP" in
        *"$flag"*) ;;
        *) printf 'Codex CLI lacks %s. Update your CLI before continuing.\n' "$flag" >&2; exit 1 ;;
      esac
    done
    ;;
  claude)
    claude --version
    claude auth status
    ;;
  *) printf 'Choose codex or claude for this preflight.\n' >&2; exit 1 ;;
esac
printf 'PASS: prerequisites ready. Continue to fixture setup.\n'
PREFLIGHT
```

**Continue only when the final `PASS` line appears.** If login fails, run `codex login` (or `claude auth login` for the optional Claude path), complete the browser sign-in, and rerun the preflight. Use your normal approved account; no API key belongs in demo files. Authentication being present does not prove account quota or sandbox permissions: rehearse one real demo run before class. [Codex authentication](https://learn.chatgpt.com/docs/auth)

These headless calls use the CLI's default model, or your explicit `MODEL` environment variable, with user configuration ignored and memory/subagents disabled. Rehearse sandbox permissions before class.

**Git isolation:** this setup does not switch CAP's curriculum branch or create commits in CAP. It adds `/.demo-runs/` to CAP's local `.git/info/exclude`, preserving existing lines. That rule is not tracked or shared; this runbook configures it even on a fresh checkout. The fixture gets its own Git repository, feature branch and no remote. Run subsequent demo commands inside `$CAP_QUEUE_DEMO`.

```bash
cd /Users/michaelmurray/code/cap || exit 1
CAP_DEMO_EXCLUDE=$(git rev-parse --git-path info/exclude)
if ! grep -qxF '/.demo-runs/' "$CAP_DEMO_EXCLUDE"; then
  printf '\n/.demo-runs/\n' >> "$CAP_DEMO_EXCLUDE"
fi
export CAP_QUEUE_DEMO="$PWD/.demo-runs/queue-$(date +%Y%m%d-%H%M%S)"
if [ -e "$CAP_QUEUE_DEMO" ]; then
  printf 'Existing run found: %s. Choose a fresh demo directory.\n' "$CAP_QUEUE_DEMO" >&2
  exit 1
fi
mkdir -p "$CAP_QUEUE_DEMO/app" "$CAP_QUEUE_DEMO/.harness/tasks" "$CAP_QUEUE_DEMO/artifacts"
cd "$CAP_QUEUE_DEMO" || exit 1
git init -b feature/paint-task-queue || exit 1
if [ "$(git -C "$CAP_QUEUE_DEMO" rev-parse --show-toplevel)" != "$CAP_QUEUE_DEMO" ] ||
   [ "$(git -C "$CAP_QUEUE_DEMO" branch --show-current)" != "feature/paint-task-queue" ] ||
   [ -n "$(git -C "$CAP_QUEUE_DEMO" remote)" ] ||
   ! git -C /Users/michaelmurray/code/cap check-ignore -q "$CAP_QUEUE_DEMO/"; then
  printf 'Git isolation check failed. Stop here; do not run later blocks.\n' >&2
  exit 1
fi
# Identity is set only in this isolated repository when no identity exists.
git config user.name >/dev/null || git config --local user.name "CAP Demo"
git config user.email >/dev/null || git config --local user.email "cap-demo@example.invalid"
git var GIT_AUTHOR_IDENT >/dev/null
git -C "$CAP_QUEUE_DEMO" rev-parse --show-toplevel
git -C "$CAP_QUEUE_DEMO" branch --show-current
git -C "$CAP_QUEUE_DEMO" remote -v
git -C /Users/michaelmurray/code/cap check-ignore -v "$CAP_QUEUE_DEMO/"
git -C /Users/michaelmurray/code/cap status --short
cd .harness
printf '%s\n' '{"name":"cap-paint-harness","private":true}' > package.json
npm install --save-exact playwright || exit 1
# Keep the package's matching browser binaries inside its node_modules.
PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium || exit 1
PLAYWRIGHT_BROWSERS_PATH=0 node <<'BROWSER_CHECK' || exit 1
const {chromium} = require('playwright');
(async () => {
  const browser = await chromium.launch({headless:true});
  try { console.log('PASS: Playwright ' + require('playwright/package.json').version + '; Chromium ' + browser.version()); }
  finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
BROWSER_CHECK
cd ..
cat > .gitignore <<'EOF'
.harness/node_modules/
artifacts/
EOF
cat > package.json <<'EOF'
{"private":true,"type":"module"}
EOF
cat > AGENTS.md <<'EOF'
# Isolated classroom fixture
A single implementation Worker is authorized to edit app/ directly for the
current queued task. Never edit CAP files, the harness, Git, or this contract.
Do not add CAP planning/DECISIONS workflows. The external controller owns
checking, screenshots, ledger entries and local checkpoints. No publishing.
EOF
cat > ledger.md <<'EOF'
# Completed tasks
No queued task has passed yet. Controller results are observations, not a full
product acceptance decision. Human inspection remains required.
EOF
cat > app/index.html <<'EOF'
<!doctype html>
<html lang="en"><meta charset="utf-8"><title>CAP Paint Queue</title>
<style>
body{font:18px system-ui;background:#f4f1e9;color:#152536;max-width:820px;margin:40px auto}
label{display:inline-block;margin:0 20px 18px 0}select{font:inherit;margin-left:8px}
canvas{display:block;background:white;border:2px solid #152536;touch-action:none}
</style>
<h1>CAP Paint</h1><div id="toolbar"></div><canvas id="paint" width="700" height="360"></canvas>
<script type="module" src="main.js"></script></html>
EOF
cat > app/toolbar.js <<'EOF'
export function renderToolbar() {
  return '<label>Tool <select id="tool"><option value="pencil">Pencil</option></select></label>' +
    '<label>Color <select id="color"><option value="#152536">Navy</option></select></label>';
}
EOF
cat > app/settings.js <<'EOF'
export function loadPreferences() { return {tool:'pencil',color:'#152536'}; }
export function savePreferences() {}
EOF
cat > app/main.js <<'EOF'
import { renderToolbar } from './toolbar.js';
const preferences = {tool:'pencil',color:'#152536'};
document.querySelector('#toolbar').innerHTML = renderToolbar(preferences);
const canvas = document.querySelector('#paint'), ctx = canvas.getContext('2d');
let drawing = false;
function position(e) {
  const b = canvas.getBoundingClientRect();
  return [(e.clientX-b.left)*canvas.width/b.width,(e.clientY-b.top)*canvas.height/b.height];
}
canvas.addEventListener('pointerdown', e => {
  drawing = true; canvas.setPointerCapture(e.pointerId);
  ctx.beginPath(); ctx.moveTo(...position(e));
});
canvas.addEventListener('pointermove', e => {
  if (!drawing) return;
  ctx.strokeStyle = preferences.color; ctx.lineWidth = 5; ctx.lineCap = 'round';
  ctx.lineTo(...position(e)); ctx.stroke();
});
canvas.addEventListener('pointerup', () => drawing = false);
canvas.addEventListener('pointercancel', () => drawing = false);
EOF
cat > .harness/contract.txt <<'EOF'
You are the single implementation Worker for this isolated classroom fixture.
Implement ONLY the current queued task. A new conversation handles each attempt.
Inspect current app files, completed-task ledger, and previous check output.
Change only app/ files. Do not edit parent/harness/Git, commit, spawn agents,
install packages, or start a server. Add plus delete at most 1,000 app lines
across this task, including a repair attempt.
Keep plain static index.html, local JS/CSS, drawable canvas#paint 700x360.
No packages, services, remote assets, symlinks, or build step.

Accepted contract:
- toolbar.js exports renderToolbar(preferences), HTML with labelled select#tool
  and select#color. It performs no persistence or event handling.
- settings.js exports loadPreferences(storage), savePreferences(storage,prefs).
  Storage is injected. Key cap.paint.preferences.v1; JSON {tool,color}.
- main.js renders, handles selections, persists, and draws.
Tools pencil/eraser. Colors #152536 (navy), #188D91 (teal), #E8A64C (amber).
Defaults {tool:'pencil',color:'#152536'}. Task 03 adds invalid-data safety.
Keep color independent of eraser selection. A pencil stroke proves its color.
Return the supplied JSON schema: implemented, envisioned, limitations.
Do not claim a check passed unless you actually observed it. The controller checks.
EOF
cat > .harness/schema.json <<'EOF'
{"type":"object","additionalProperties":false,"properties":{"implemented":{"type":"string"},"envisioned":{"type":"string"},"limitations":{"type":"string"}},"required":["implemented","envisioned","limitations"]}
EOF
cat > .harness/queue.txt <<'EOF'
01
02
03
EOF
cat > .harness/tasks/01.txt <<'EOF'
Task 01: expand renderToolbar to both tools and all three colors, rendering
supplied preferences selected. Keep the default page drawing working.
Exit criterion: browser sees exact choices with Pencil/Navy selected and
canvas#paint changes pixels when a pointer drag occurs.
EOF
cat > .harness/tasks/02.txt <<'EOF'
Task 02: implement saved preferences and connect selection handlers to storage.
Exit criterion: choosing Pencil/Teal then reloading restores both controls;
a new pencil stroke contains visible teal pixels. Preserve Task 01.
EOF
cat > .harness/tasks/03.txt <<'EOF'
Task 03: handle invalid saved data/storage failures safely and finish eraser.
Exit criterion: Eraser/Teal survive reload independently; erasing changes pixels;
malformed JSON or invalid values restore Pencil/Navy; storage exceptions are
safe. Existing Pencil/Teal behavior still passes. Preserve Tasks 01 and 02.
EOF
```

Playwright is installed in this fixture's `.harness/`, with its matching Chromium binaries inside ignored `node_modules/`. A separately installed Chrome or Playwright does not replace this step. **Stop if either installation or the `PASS: Playwright ...; Chromium ...` launch check fails.** To repair a missing browser, enter this fixture's `.harness/` and rerun `PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium`; do not install from CAP's root. [Playwright library setup](https://playwright.dev/docs/library), [matching browser installation](https://playwright.dev/docs/browsers)

The setup supplies a local demo commit identity only when you have none; it leaves your global Git identity unchanged. If a checkpoint commit fails because your existing signing setup is unavailable, stop and resolve it before running the loop. For disposable demo commits only, `git config --local commit.gpgsign false` disables signing in this nested repository.

During setup, verify the printed results before continuing:

- Repository root: `/Users/michaelmurray/code/cap/.demo-runs/queue-YYYYMMDD-HHMMSS` (the timestamp is your actual run).
- Branch: `feature/paint-task-queue`.
- Remotes: no output from `git remote -v`.
- CAP ignore check: `.git/info/exclude` supplies the `/.demo-runs/` rule; its line number may vary.
- CAP status: no `.demo-runs/` entry. Existing curriculum changes may still appear.

**If a check fails, stop:** keep the fixture intact, open a new terminal, and repeat setup with a fresh demo directory. Do not run the remaining blocks until the repository root, branch, remote and ignore checks pass. For later terminals, restore `$CAP_QUEUE_DEMO` to the printed repository root and `cd "$CAP_QUEUE_DEMO"` before following commands.

## 2. Copy the runnable scripts into this fixture

The actual files are checked in under [scripts/task-queue/](scripts/task-queue/queue.sh). **Run their fixture copies only.** The controller refuses CAP's source directory; all app changes and local commits belong in this nested demo repository.

Copy this block after completing section 1. It copies all six small files and checkpoints the prepared fixture. It does not call an agent.

```bash
export CAP_ROOT="/Users/michaelmurray/code/cap"
cd "$CAP_QUEUE_DEMO" || exit 1
cp "$CAP_ROOT/curriculum/weeks/04/scripts/task-queue/"* .harness/
git add .gitignore AGENTS.md package.json .harness app ledger.md
git commit -m "Prepare independent paint task-queue fixture"
```

| File | Job |
|---|---|
| [queue.sh](scripts/task-queue/queue.sh) | Read the next task; allow one attempt and at most one repair |
| [agent.py](scripts/task-queue/agent.py) | Start a fresh Codex call; enforce its timeout and interruption cleanup |
| [check.py](scripts/task-queue/check.py) | Put an overall time limit around the browser check and handle interruption |
| [changes.py](scripts/task-queue/changes.py) | Measure app lines changed across the entire current task |
| [check.cjs](scripts/task-queue/check.cjs) | Run cumulative browser criteria and save actual browser evidence |
| [journal.cjs](scripts/task-queue/journal.cjs) | Append a validated report and the controller's observed checks |

The browser helper runs cumulative criteria: task 02 includes 01, and task 03 includes both earlier tasks. It saves the actual browser state on successful or failed checks whenever a page exists.

## 3. Read the completion loop with the class

Open the copied file: `cat .harness/queue.sh`. The task list is ordinary text, and the Bash script chooses what runs:

1. Read a task from `queue.txt`; build its prompt with current code context and prior failure feedback.
2. Start a **fresh agent** and wait.
3. Measure app changes against the last completed task's checkpoint.
4. Run **this task's browser checks plus all earlier checks** within a time limit.
5. On success, record claims and observed evidence.
6. Save a local checkpoint and move to the next task.

A failed browser check sends its exact output to **one fresh repair attempt**. There is no checkpoint between a failed attempt and its repair, so both share the 1,000-line change budget. Two failed checks stop the queue for a human. Agent-call failures or scope failures stop immediately.

### Bash vocabulary for this script

- `while read -r task` reads one task ID per line. The `done < .harness/queue.txt` line supplies that file.
- `for attempt in 1 2` gives each task at most two attempts.
- `if wait "$worker"` asks whether the browser-check helper succeeded; the agent saying “done” cannot pass this gate.
- `passed=1` remembers success; `break` leaves the attempt loop.
- `feedback` holds the previous check's output and is included in the next prompt.
- `&`, `$!` and `wait` start the helper, remember its process ID, then wait for completion.
- Quotes keep paths together; `>` and `2>` save logs; `trap` cleans up on interruption.
- `set -euo pipefail` stops unexpected failures rather than continuing silently.

A criterion may pass while the product has other defects. This queue does not create parallel Workers, discover dependencies, independently review code, or implement automatic resume. The two-attempt bound is a teaching choice, not a promise that every task can finish in two attempts.

## 4. Run before class and keep real evidence

Each agent call has a 180-second limit by default; each browser check has a separate 60-second limit. Optional `CHECK_SECONDS=90` gives checks more time. Both limits must be positive integers. A timed-out browser check supplies failure feedback for the same bounded repair flow; it may have no screenshot.

```bash
PASS_SECONDS=180 bash .harness/queue.sh > artifacts/run.log 2>&1 &
echo $! > artifacts/controller.pid
tail -f artifacts/run.log
```

`Ctrl+C` exits `tail`. To stop the controller and its current agent call or browser check:

```bash
kill -TERM "$(cat artifacts/controller.pid)"
```

Check what actually finished:

```bash
cat artifacts/run.log
cat ledger.md
find artifacts -name screen.png | sort
git log --oneline -- app
```

If the run stops, leave the attempted app and evidence in place. Inspect the failing `checks.txt`, `agent.stderr`, and `app.diff`. Human intervention should name that exact failure and scope a repair. To replay the whole experiment, create a fresh fixture with section 1; this simple script intentionally does not implement resumable queue state.

## 5. Classroom walkthrough — approximately two minutes

1. Read `queue.txt`, one task instruction and its browser criterion in `check.cjs`.
2. Read `queue.sh`: point to the **two-attempt inner loop**, feedback injection, and the `passed` gate before moving to the next task.
3. Open an actual task-01 screenshot, then a completed task-02 screenshot. Read its check output: a teal stroke is observable evidence, not merely a model assertion.
4. If a real repair occurred, compare its failed and succeeding check logs. If none occurred, say so. Do not label a first-attempt success as a repaired run.
5. Explain task 03's independent eraser-persistence and malformed-state checks. Its final screenshot may show the tested default fallback; use its check log to explain the sequence.

```bash
cd "$CAP_QUEUE_DEMO"
cat .harness/queue.txt
cat .harness/queue.sh
find artifacts -name screen.png | sort
```

Substitute an actual directory from the listing:

```bash
open artifacts/ACTUAL-TIMESTAMP-TASK-ATTEMPT/screen.png
cat artifacts/ACTUAL-TIMESTAMP-TASK-ATTEMPT/checks.txt
```

## What this adds to the programming loop

- **Best fit:** a small backlog with known order and cheap, task-specific checks.
- **Strength:** a clear completion gate and bounded repair feedback; easy to inspect.
- **Limit:** sequential throughput; poor handling of changing dependencies or tasks without executable criteria. Task failure blocks the queue.
- **On the loop:** the human defines task boundaries and checks, inspects evidence, and handles exhausted attempts. An LLM saying "done" does not advance the queue.

Current headless flags: [Codex non-interactive mode](https://learn.chatgpt.com/docs/non-interactive-mode). These instructions have not run model calls or application checks as part of authoring the course material.

## If you use Claude Code

**Available here:** one fresh Claude Worker attempt for task 01. The queue's controller still calls Codex; porting `.harness/agent.py` is needed for an automatic Claude queue. Use `CAP_DEMO_AGENT=claude` during preflight, then complete the fixture and helpers in sections 1–2. Keep the same Node/Playwright installation and browser checks.

Run this optional task attempt instead of starting `.harness/queue.sh`:

```bash
bash <<'CLAUDE_TASK'
set -euo pipefail
cd "$CAP_QUEUE_DEMO/app"
claude --version
claude auth status
{ cat ../.harness/contract.txt; cat ../.harness/tasks/01.txt; cat ../ledger.md; } | \
  claude -p --safe-mode --no-session-persistence --permission-mode acceptEdits \
    --tools "Read,Glob,Grep,Edit,Write" --output-format json \
    --json-schema "$(cat ../.harness/schema.json)" > ../artifacts/claude-task01-result.json
python3 - <<'PARSE_RESULT'
import json
from pathlib import Path
result = json.loads(Path('../artifacts/claude-task01-result.json').read_text())
assert not result.get('is_error'), result
report = result.get('structured_output')
assert isinstance(report, dict) and all(isinstance(report.get(k), str) for k in ('implemented','envisioned','limitations')), 'Missing structured report'
Path('../artifacts/claude-task01-report.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
PARSE_RESULT
cd ..
node .harness/check.cjs "$PWD/app" 01 "$PWD/artifacts/claude-task01.png"
CLAUDE_TASK
```

This checks task 01 only; it does not automatically retry, advance the queue, append the ledger or checkpoint. To run the complete queue with Claude, adapt the helper's invocation, validate the result envelope and extract `structured_output` into its report file, and retain process-group timeouts plus the cumulative checks and retry budgets. Fresh `claude -p` calls must receive the current task and the prior failure output, without `--continue`/`--resume`.

`--safe-mode` retains normal authentication while suppressing custom context; `--no-session-persistence` prevents saving the session. File-edit permissions and the Codex workspace sandbox are different mechanisms. Rehearse the actual CLI permissions, without bypass flags. [Claude CLI reference](https://code.claude.com/docs/en/cli-reference), [structured output and permissions](https://code.claude.com/docs/en/headless)
