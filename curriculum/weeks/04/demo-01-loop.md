# Demo 1 — A fresh-context programming loop

**Prepare before class:** let the loop run for three passes, or up to 100 for a longer experiment. **Class slot:** share minutes 16–20 with the task-queue demo: read the small controller, compare saved screenshots, and inspect the ledger. A fresh pass is optional; 100 passes are not a live-class promise.

**CLI used here:** the default instructions use Codex CLI. If you use Claude Code, see [the optional Claude path](#if-you-use-claude-code) before running setup; changing a command name alone does not adapt a controller.

An EMPTY application folder becomes a paint app. Each pass starts a new `codex exec` conversation. The agent chooses an improvement; a Bash controller checks the change size, launches the app, records a screenshot, and decides whether to continue. Code and an implementation/planning ledger carry forward. No conversation is resumed.

This is the Ralph-style programming-loop pattern, implemented with an ordinary script. It does not require a Ralph plugin. More iterations can also produce regressions or unhelpful scope; use the evidence to discuss that.

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

Rehearse with your account and the scoped `workspace-write` sandbox. This example ignores user configuration, disables memory and subagents, and uses an ephemeral session. It uses the CLI's default model; optionally supply `MODEL` below. These flags control this invocation, not the existence of other personal instructions or saved account authentication. If your installation enforces additional instructions or denies an operation, resolve that during rehearsal; the loop stops rather than widening its permissions.

## 2. Make the isolated fixture

Copy this block into one terminal. All generated demo material stays under CAP. The nested repository has a feature branch and no remote. The application directory is empty: the controller, dependencies and journal live beside it.

**Git isolation:** this setup does not switch CAP's curriculum branch or create commits in CAP. It adds `/.demo-runs/` to CAP's local `.git/info/exclude`, preserving existing lines. That rule is not tracked or shared; this runbook configures it even on a fresh checkout. The fixture gets its own Git repository, feature branch and no remote. Run subsequent demo commands inside `$CAP_LOOP_DEMO`.

```bash
cd /Users/michaelmurray/code/cap || exit 1
CAP_DEMO_EXCLUDE=$(git rev-parse --git-path info/exclude)
if ! grep -qxF '/.demo-runs/' "$CAP_DEMO_EXCLUDE"; then
  printf '\n/.demo-runs/\n' >> "$CAP_DEMO_EXCLUDE"
fi
export CAP_LOOP_DEMO="$PWD/.demo-runs/loop-$(date +%Y%m%d-%H%M%S)"
if [ -e "$CAP_LOOP_DEMO" ]; then
  printf 'Existing run found: %s. Choose a fresh demo directory.\n' "$CAP_LOOP_DEMO" >&2
  exit 1
fi
mkdir -p "$CAP_LOOP_DEMO/app" "$CAP_LOOP_DEMO/.harness" "$CAP_LOOP_DEMO/artifacts"
cd "$CAP_LOOP_DEMO" || exit 1
git init -b feature/paint-loop || exit 1
if [ "$(git -C "$CAP_LOOP_DEMO" rev-parse --show-toplevel)" != "$CAP_LOOP_DEMO" ] ||
   [ "$(git -C "$CAP_LOOP_DEMO" branch --show-current)" != "feature/paint-loop" ] ||
   [ -n "$(git -C "$CAP_LOOP_DEMO" remote)" ] ||
   ! git -C /Users/michaelmurray/code/cap check-ignore -q "$CAP_LOOP_DEMO/"; then
  printf 'Git isolation check failed. Stop here; do not run later blocks.\n' >&2
  exit 1
fi
# Identity is set only in this isolated repository when no identity exists.
git config user.name >/dev/null || git config --local user.name "CAP Demo"
git config user.email >/dev/null || git config --local user.email "cap-demo@example.invalid"
git var GIT_AUTHOR_IDENT >/dev/null
git -C "$CAP_LOOP_DEMO" rev-parse --show-toplevel
git -C "$CAP_LOOP_DEMO" branch --show-current
git -C "$CAP_LOOP_DEMO" remote -v
git -C /Users/michaelmurray/code/cap check-ignore -v "$CAP_LOOP_DEMO/"
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
cat > AGENTS.md <<'EOF'
# Isolated classroom fixture
This nested demo repository authorizes its single implementation Worker to edit
app/ directly. It may not edit CAP files, the harness, this contract, or Git.
The supplied per-pass prompt is the complete task. Do not add CAP planning,
review, DECISIONS, commit, or publishing workflows to this fixture.
The external controller owns checks, screenshots, the ledger and local commits.
EOF
cat > ledger.md <<'EOF'
# Paint app iteration ledger
The app folder is empty. No implementation has happened yet.
Agent reports below are claims; controller observations are recorded separately.
EOF
cat > .harness/schema.json <<'EOF'
{"type":"object","additionalProperties":false,"properties":{"implemented":{"type":"string"},"envisioned":{"type":"string"},"limitations":{"type":"string"}},"required":["implemented","envisioned","limitations"]}
EOF
cat > .harness/prompt.txt <<'EOF'
You are the sole implementation Worker in an isolated classroom fixture.
This is a NEW conversation. Read current app source and the ledger below;
those disk artifacts are the carried-forward implementation/planning state.
Choose and implement the next useful improvement to a browser paint app.
The first pass starts from an EMPTY app directory. Start with a usable slice.

Runtime contract:
- Plain static index.html and local HTML/JS/CSS; no packages, remote assets,
  symlinks, network services, build steps, server code or credentials.
- A visible HTML canvas#paint, at least 400 by 240 intrinsic pixels.
- A pointer drag draws a visible stroke immediately after opening the app.
- No full-page opening modal. Layout, tools and later features are your choice.

Ownership and limits:
- Edit only files in this app directory, directly as this fixture's Worker.
- Do not modify the parent/harness/Git, commit, push, spawn agents, install
  packages or start servers. The controller owns those operations.
- Add plus delete at most 1,000 application lines this pass.
- Preserve existing behavior. If the ledger reports a failure, repair it first.
- Do not retrieve previous sessions, transcripts, screenshots or external memory.

Return the supplied JSON schema: implemented = actual changes this pass;
envisioned = the next useful improvement you foresee; limitations = unverified
or incomplete behavior. The controller appends your words to the disk ledger.
EOF
```

Playwright is installed in this fixture's `.harness/`, with its matching Chromium binaries inside ignored `node_modules/`. A separately installed Chrome or Playwright does not replace this step. **Stop if either installation or the `PASS: Playwright ...; Chromium ...` launch check fails.** To repair a missing browser, enter this fixture's `.harness/` and rerun `PLAYWRIGHT_BROWSERS_PATH=0 npx playwright install chromium`; do not install from CAP's root. [Playwright library setup](https://playwright.dev/docs/library), [matching browser installation](https://playwright.dev/docs/browsers)

The setup supplies a local demo commit identity only when you have none; it leaves your global Git identity unchanged. If a checkpoint commit fails because your existing signing setup is unavailable, stop and resolve it before running the loop. For disposable demo commits only, `git config --local commit.gpgsign false` disables signing in this nested repository.

During setup, verify the printed results before continuing:

- Repository root: `/Users/michaelmurray/code/cap/.demo-runs/loop-YYYYMMDD-HHMMSS` (the timestamp is your actual run).
- Branch: `feature/paint-loop`.
- Remotes: no output from `git remote -v`.
- CAP ignore check: `.git/info/exclude` supplies the `/.demo-runs/` rule; its line number may vary.
- CAP status: no `.demo-runs/` entry. Existing curriculum changes may still appear.

**If a check fails, stop:** keep the fixture intact, open a new terminal, and repeat setup with a fresh demo directory. Do not run the remaining blocks until the repository root, branch, remote and ignore checks pass. For later terminals, restore `$CAP_LOOP_DEMO` to the printed repository root and `cd "$CAP_LOOP_DEMO"` before following commands.

## 3. Create the helpers

The helpers keep process management, browser mechanics and journal formatting out of the readable loop. Agent calls and browser checks have separate overall time limits, with process-group cleanup on timeout or interruption. The browser check is a small smoke check, not a full acceptance suite.

```bash
cat > .harness/agent.py <<'EOF'
"""One fresh Codex call, with a time limit and process-group cleanup."""
import os
import signal
import subprocess
import sys

app, prompt, output, seconds = sys.argv[1:]
try:
    seconds = int(seconds)
    if seconds <= 0:
        raise ValueError
except ValueError:
    raise SystemExit('PASS_SECONDS must be a positive integer')
command = [
    'codex', 'exec', '--ignore-user-config', '--ephemeral',
    '--sandbox', 'workspace-write', '--disable', 'memories',
    '--disable', 'multi_agent', '-c', 'project_doc_max_bytes=0',
    '--json', '--output-schema', os.path.abspath('.harness/schema.json'),
    '-o', output,
]
if os.getenv('MODEL'):
    command += ['-m', os.environ['MODEL']]
command += ['-']  # Read the complete request from standard input.
process = None


def stop(*_):
    """Ask the whole call to stop; force it after a short grace period."""
    if process:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            pass
        # The leader can exit while a descendant ignores TERM. Clean its group
        # regardless of the leader's exit status, then reap the leader.
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        process.wait()
    sys.exit(124)


signal.signal(signal.SIGTERM, stop)
signal.signal(signal.SIGINT, stop)
# Automatic parent instructions are disabled: the explicit prompt is the task.
with open(prompt) as request:
    process = subprocess.Popen(
        command, cwd=app, stdin=request, start_new_session=True,
    )
try:
    sys.exit(process.wait(timeout=seconds))
except subprocess.TimeoutExpired:
    stop()
EOF
cat > .harness/capture.cjs <<'EOF'
// Serve the local app on an available port, draw once, and capture the result.
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = require('playwright');
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(process.argv[2]);
const output = process.argv[3];
const mime = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css' };

const server = http.createServer((request, response) => {
  const file = path.resolve(root, '.' + new URL(request.url, 'http://localhost').pathname);
  const target = file === root ? path.join(root, 'index.html') : file;
  if (!target.startsWith(root + path.sep)) {
    response.writeHead(403); return response.end();
  }
  try {
    response.setHeader('Content-Type', mime[path.extname(target)] || 'text/plain');
    response.end(fs.readFileSync(target));
  } catch {
    response.writeHead(404); response.end();
  }
});

(async () => {
  // Port 0 asks the OS for an unused port, so separate fixtures can coexist.
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  let browser;
  try {
    browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.goto(`http://127.0.0.1:${server.address().port}`, { waitUntil: 'networkidle' });
    const canvas = page.locator('#paint');
    await canvas.waitFor({ state: 'visible' });
    const size = await canvas.evaluate(canvas => [canvas.width, canvas.height]);
    if (size[0] < 400 || size[1] < 240) throw Error('Canvas must be at least 400x240');

    // Compare actual canvas pixels before and after a pointer drag.
    const before = await canvas.evaluate(canvas => canvas.toDataURL());
    const box = await canvas.boundingBox();
    await page.mouse.move(box.x + 30, box.y + 30);
    await page.mouse.down();
    await page.mouse.move(box.x + 150, box.y + 80, { steps: 12 });
    await page.mouse.up();
    if (before === await canvas.evaluate(canvas => canvas.toDataURL())) {
      throw Error('Drag drew no pixels');
    }
    if (errors.length) throw Error(errors.join('\n'));
    await page.screenshot({ path: output, fullPage: true });
    console.log('PASS: visible canvas, default drag changes pixels, no page errors.');
  } finally {
    if (browser) await browser.close();
    await new Promise(resolve => server.close(resolve));
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
EOF
cat > .harness/journal.cjs <<'EOF'
// A report is the agent's claim. checks.txt is the controller's observation.
const fs = require('node:fs');
const [directory, id] = process.argv.slice(2);
const report = JSON.parse(fs.readFileSync(`${directory}/report.json`, 'utf8'));
for (const key of ['implemented', 'envisioned', 'limitations']) {
  if (typeof report[key] !== 'string' || !report[key].trim()) {
    throw Error(`Missing ${key} in the agent report`);
  }
}
const checks = fs.readFileSync(`${directory}/checks.txt`, 'utf8').trim();
fs.appendFileSync('ledger.md', `\n## ${id}\n\n` +
  `Agent implemented: ${report.implemented}\n\n` +
  `Agent envisioned: ${report.envisioned}\n\n` +
  `Agent limitations: ${report.limitations}\n\n` +
  `Controller observed: ${checks}\n\n` +
  `Screenshot: ${directory}/screen.png. Product acceptance: pending human inspection.\n`);
EOF
```

## 4. The controller — read this with the class

It calls a fresh session, caps scope, captures evidence, checkpoints, then repeats. There is no hidden LLM scheduler. A failure stops with the working files and logs preserved for a human.

```bash
cat > .harness/changes.py <<'EOF'
"""Count added PLUS deleted app lines since the last accepted checkpoint."""
import subprocess

total = 0
summary = subprocess.check_output(['git', 'diff', '--numstat', '--', 'app'], text=True)
for entry in summary.splitlines():
    added, deleted, _ = entry.split('\t', 2)
    if added == '-' or deleted == '-':
        raise SystemExit('STOP: binary app changes cannot satisfy a line budget')
    total += int(added) + int(deleted)
print(total)
EOF
cat > .harness/check.py <<'EOF'
"""Run a browser check with an overall timeout, including stuck page code."""
import os
import signal
import subprocess
import sys

seconds, *command = sys.argv[1:]
try:
    seconds = int(seconds)
    if seconds <= 0 or not command:
        raise ValueError
except ValueError:
    raise SystemExit('CHECK_SECONDS must be positive, followed by a check command')
process = None


def stop(*_):
    """Stop Node and its browser group, even if Node already exited."""
    if process:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            pass
        # A browser descendant can outlive Node; always finish group cleanup.
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        process.wait()
    sys.exit(124)


signal.signal(signal.SIGTERM, stop)
signal.signal(signal.SIGINT, stop)
process = subprocess.Popen(command, start_new_session=True)
try:
    sys.exit(process.wait(timeout=seconds))
except subprocess.TimeoutExpired:
    print(f'STOP: browser check exceeded {seconds} seconds', file=sys.stderr)
    stop()
EOF
cat > .harness/loop.sh <<'EOF'
#!/usr/bin/env bash
# Run this COPY from an isolated demo's .harness/, never from CAP's source tree.
# Bash chooses the next pass. The fresh agent only changes the paint app.
set -euo pipefail
cd "$(dirname "$0")/.."

# ${NAME:-default} means: use the environment setting, or this default.
passes="${ITERATIONS:-3}"
seconds="${PASS_SECONDS:-180}"
check_seconds="${CHECK_SECONDS:-60}"
case "$passes" in ''|*[!0-9]*) echo 'ITERATIONS must be 1..100'; exit 1;; esac
if [ "$passes" -lt 1 ] || [ "$passes" -gt 100 ]; then
  echo 'ITERATIONS must be 1..100'; exit 1
fi

# Refuse CAP itself, a published repo, or unaccepted changes from a prior pass.
if [ "$(git rev-parse --show-toplevel)" != "$PWD" ] ||
   [ "$(git branch --show-current)" != 'feature/paint-loop' ] ||
   [ -n "$(git remote)" ] || [ ! -f .harness/prompt.txt ]; then
  echo 'STOP: use the isolated fixture created by the runbook.'; exit 1
fi
if [ -n "$(git status --porcelain -- app ledger.md)" ]; then
  echo 'STOP: inspect unaccepted app or ledger changes before starting.'; exit 1
fi

run="$(date -u +%Y%m%dT%H%M%SZ)-$$"
worker=''
# $! is the last background process's PID. On exit, stop it and await cleanup.
cleanup() {
  if [ -n "$worker" ]; then
    kill -TERM "$worker" 2>/dev/null || true
    wait "$worker" 2>/dev/null || true
  fi
}
trap cleanup EXIT
trap 'exit 130' INT TERM

# One trip through this loop is one NEW conversation, with no resume command.
for ((pass=1; pass<=passes; pass++)); do
  id="$run-$(printf '%03d' "$pass")"
  dir="$PWD/artifacts/$id"
  mkdir -p "$dir"

  echo "[1/6] Pass $pass/$passes: same request + current ledger"
  cat .harness/prompt.txt ledger.md > "$dir/prompt.txt"

  echo '[2/6] Call a fresh agent; wait for its result'
  python3 .harness/agent.py "$PWD/app" "$dir/prompt.txt" "$dir/report.json" \
    "$seconds" > "$dir/events.jsonl" 2> "$dir/agent.stderr" &
  worker=$!
  if ! wait "$worker"; then
    echo "STOP: agent failed; inspect $dir and working code"; exit 1
  fi
  worker=''

  echo '[3/6] Save the diff and enforce the 1,000-line budget'
  [ -f app/index.html ] || { echo 'STOP: app/index.html is missing'; exit 1; }
  git add -N app # Include new files in the diff without accepting their content.
  git diff -- app > "$dir/app.diff"
  lines="$(python3 .harness/changes.py)"
  if [ "$lines" -gt 1000 ]; then
    echo "STOP: $lines changed lines; inspect $dir"; exit 1
  fi

  echo '[4/6] Launch the app, check drawing, and save a screenshot'
  # Track the check helper too: a stuck browser must not block this loop forever.
  python3 .harness/check.py "$check_seconds" node .harness/capture.cjs app \
    "$dir/screen.png" > "$dir/checks.txt" 2>&1 &
  worker=$!
  if ! wait "$worker"; then
    echo "STOP: inspect $dir/checks.txt and working code"; exit 1
  fi
  worker=''

  echo '[5/6] Record agent claims separately from observed checks'
  node .harness/journal.cjs "$dir" "$id"

  echo '[6/6] Save a local checkpoint; then start the next pass'
  git add app ledger.md
  git commit -m "Paint iteration $id; human acceptance pending"
  git rev-parse HEAD > "$dir/checkpoint.txt"
  echo "CHECKPOINT $id: $lines changed lines; $dir/screen.png"
done
EOF
git add AGENTS.md .gitignore .harness ledger.md
git commit -m "Prepare isolated paint-loop classroom fixture"
find app -type f
```

The final command should print nothing: **the first agent still receives an empty app folder**. Local demo commits preserve history; they do not publish anything. The cap measures added plus deleted application lines, not the total project size.

## 5. Run before class

Each agent call has a 180-second limit by default; each browser check has a separate 60-second limit. Optional `CHECK_SECONDS=90` gives checks more time. Both limits must be positive integers. A timed-out check records its failure in `checks.txt`; it may have no screenshot.

Start with three passes. They can take several minutes and consume model usage. A failed pass does not automatically retry.

```bash
ITERATIONS=3 PASS_SECONDS=180 bash .harness/loop.sh
```

For the larger pre-class experiment, use a **new fixture** by repeating setup in section 2. This may run for hours; inspect progress before trusting its output.

```bash
ITERATIONS=100 PASS_SECONDS=180 bash .harness/loop.sh > artifacts/run.log 2>&1 &
echo $! > artifacts/controller.pid
printf 'Controller PID: '; cat artifacts/controller.pid
tail -f artifacts/run.log
```

`Ctrl+C` exits `tail`, while the background controller continues. Stop the controller and its current Codex call or browser check with:

```bash
kill -TERM "$(cat artifacts/controller.pid)"
```

Stopping preserves files and evidence. A started pass may have no screenshot or checkpoint if its agent, size check or browser check failed. The screenshot name contains a UTC timestamp and pass number.

## 6. Classroom walkthrough — approximately two minutes

1. Read `.harness/loop.sh`, especially `codex exec` in the helper, `wait`, checks, and the local checkpoint. Ask learners where scheduling authority lives.
2. Open the first, middle and last **actually completed** screenshots. Pick their directories from the listing; do not assume all 100 completed.
3. Read each corresponding `Agent implemented` and `Agent envisioned` ledger entry. Compare its claims with the screenshot and code diff. Explain what the next fresh session receives.

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

For a quick live continuation, after checking the repository is clean:

```bash
ITERATIONS=1 PASS_SECONDS=180 bash .harness/loop.sh
```

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

## What this demonstrates

- **Best fit:** exploratory work with cheap feedback and a modest change budget.
- **Strength:** small controller, fresh conversations, durable progress evidence.
- **Limit:** no task dependencies, parallelism, independent review, or guarantee of worthwhile improvements. A drawable canvas check can pass while the product is poor.
- **On the loop:** the human owns the goal, examines actual results, changes constraints, and stops or repairs the process.

Current command behavior: [Codex non-interactive mode](https://learn.chatgpt.com/docs/non-interactive-mode), [configuration reference](https://learn.chatgpt.com/docs/config-file/config-reference). These are runnable classroom instructions, not a claim that this experiment has been executed or passed in this repository.

## If you use Claude Code

**Available here:** one fresh Claude Worker call with the same app, prompt, and ledger. The Bash loop above still calls Codex; running all iterations with Claude requires replacing `.harness/agent.py` with an adapter. Start from the runtime/fixture/helper setup above, with `CAP_DEMO_AGENT=claude` during preflight; skip `.harness/loop.sh` for this optional single-pass example.

After sections 1–3, run this in the setup terminal:

```bash
bash <<'CLAUDE_PASS'
set -euo pipefail
cd "$CAP_LOOP_DEMO/app"
claude --version
claude auth status
{ cat ../.harness/prompt.txt; cat ../ledger.md; } | \
  claude -p --safe-mode --no-session-persistence --permission-mode acceptEdits \
    --tools "Read,Glob,Grep,Edit,Write" --output-format json \
    --json-schema "$(cat ../.harness/schema.json)" > ../artifacts/claude-result.json
python3 - <<'PARSE_RESULT'
import json
from pathlib import Path
result = json.loads(Path('../artifacts/claude-result.json').read_text())
assert not result.get('is_error'), result
report = result.get('structured_output')
assert isinstance(report, dict) and all(isinstance(report.get(k), str) for k in ('implemented','envisioned','limitations')), 'Missing structured report'
Path('../artifacts/claude-report.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
PARSE_RESULT
cd ..
git add -N app
git diff --numstat -- app
node .harness/capture.cjs "$PWD/app" "$PWD/artifacts/claude-pass.png"
CLAUDE_PASS
```

Inspect the change count and image yourself. This single pass does not append the ledger, enforce the 1,000-line gate, or create a checkpoint automatically. A full adapter must keep the existing process-group timeout, produce the bare report JSON expected by the journal, return failure on `is_error`/missing output, and leave the controller's size, browser, ledger and Git checks intact. Start a new `claude -p` call each pass; do not use `--continue` or `--resume`.

`--safe-mode` suppresses custom instructions, plugins and memory while retaining normal authentication. `--no-session-persistence` alone only prevents saving the session. The selected tools permit file work; the CLI's own permissions still apply. Do not claim the Codex sandbox and Claude permissions are identical. [Claude CLI flags and authentication](https://code.claude.com/docs/en/cli-reference), [programmatic output and permissions](https://code.claude.com/docs/en/headless)
