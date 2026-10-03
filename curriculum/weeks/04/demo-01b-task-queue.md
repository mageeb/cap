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

## 2. Agent and browser helpers

The browser helper runs cumulative criteria: task 02 includes 01, and task 03 includes both earlier tasks. It saves the actual browser state on successful or failed checks whenever a page exists.

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
cat > .harness/check.cjs <<'EOF'
// Run task 01 checks, then add 02 and 03 checks cumulatively.
// The screenshot is evidence from the real browser, even after a failed check.
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = require('playwright'), assert = require('node:assert/strict');
const http = require('node:http'), fs = require('node:fs'), path = require('node:path');
const root = path.resolve(process.argv[2]), task = Number(process.argv[3]), image = process.argv[4];
const server = http.createServer((req,res) => {
  const file = path.resolve(root,'.'+new URL(req.url,'http://localhost').pathname);
  const target = file === root ? path.join(root,'index.html') : file;
  if (!target.startsWith(root+path.sep)) { res.writeHead(403); return res.end(); }
  try { res.setHeader('Content-Type', {'.html':'text/html','.js':'text/javascript','.css':'text/css'}[path.extname(target)] || 'text/plain');
    res.end(fs.readFileSync(target)); } catch { res.writeHead(404); res.end(); }
});
(async () => {
  // Port 0 selects an unused local port for this check's short-lived server.
  await new Promise(r => server.listen(0,'127.0.0.1',r));
  let browser, page;
  try {
    browser = await chromium.launch(); page = await browser.newPage({viewport:{width:1280,height:900}});
    const errors = []; page.on('pageerror',e => errors.push(e.message));
    await page.goto(`http://127.0.0.1:${server.address().port}`,{waitUntil:'networkidle'});
    const value = id => page.locator('#'+id).inputValue();
    assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#152536');
    assert.deepEqual(await page.locator('#tool option').evaluateAll(a => a.map(x=>x.value)),['pencil','eraser']);
    assert.deepEqual(await page.locator('#color option').evaluateAll(a => a.map(x=>x.value)),['#152536','#188D91','#E8A64C']);
    const canvas = page.locator('#paint');
    async function draw() {
      const b = await canvas.evaluate(c => {const r=c.getBoundingClientRect(); return {x:r.x+c.clientLeft,y:r.y+c.clientTop,w:c.clientWidth,h:c.clientHeight,cw:c.width,ch:c.height};});
      await page.mouse.move(b.x+50*b.w/b.cw,b.y+60*b.h/b.ch); await page.mouse.down();
      await page.mouse.move(b.x+150*b.w/b.cw,b.y+60*b.h/b.ch,{steps:15}); await page.mouse.up();
    }
    const before = await canvas.evaluate(c => c.toDataURL()); await draw();
    assert.notEqual(await canvas.evaluate(c => c.toDataURL()),before,'Default drag drew nothing');
    // Task 02 adds reload persistence and verifies a real teal pencil stroke.
    if (task >= 2) {
      await page.selectOption('#tool','pencil'); await page.selectOption('#color','#188D91');
      await page.reload(); assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#188D91');
      await draw();
      assert.equal(await canvas.evaluate(c => {
        const p=c.getContext('2d').getImageData(100,60,1,1).data;
        return p[0]===24 && p[1]===141 && p[2]===145 && p[3]===255;
      }),true,'Pencil stroke is not teal');
    }
    // Task 03 preserves both earlier checks, then adds eraser and bad-data safety.
    if (task >= 3) {
      await page.selectOption('#tool','eraser'); await page.reload();
      assert.equal(await value('tool'),'eraser'); assert.equal(await value('color'),'#188D91');
      // Canvas pixels do not persist: draw a NEW stroke after reload, then erase it.
      await page.selectOption('#tool','pencil'); await draw();
      await page.selectOption('#tool','eraser'); const painted=await canvas.evaluate(c => c.toDataURL());
      await draw(); assert.notEqual(await canvas.evaluate(c => c.toDataURL()),painted,'Eraser did not change pixels');
      assert.equal(await canvas.evaluate(c => {
        const p=c.getContext('2d').getImageData(100,60,1,1).data;
        return p[3]===0 || (p[0]===255 && p[1]===255 && p[2]===255);
      }),true,'Eraser did not clear the stroke');
      for (const raw of ['{broken','null','[]','{"tool":"bad","color":"#188D91"}',
        '{"tool":"eraser","color":"purple"}']) {
        await page.evaluate(raw => localStorage.setItem('cap.paint.preferences.v1',raw),raw);
        await page.reload(); assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#152536');
      }
      assert.equal(await page.evaluate(async () => {
        const m = await import('./settings.js'), s = {getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
        const p=m.loadPreferences(s); m.savePreferences(s,p);
        return p.tool==='pencil' && p.color==='#152536';
      }),true,'Denied storage not safe');
    }
    assert.deepEqual(errors,[]);
    // A successful task requires its screenshot; capture errors fail the gate.
    await page.screenshot({path:image,fullPage:true});
    console.log(`PASS task ${String(task).padStart(2,'0')}: cumulative browser criteria.`);
  } catch (error) {
    // Preserve best-effort failure evidence without replacing the original error.
    if (page) await page.screenshot({path:image,fullPage:true}).catch(()=>{});
    throw error;
  } finally {
    try { if (browser) await browser.close(); }
    finally { await new Promise(r => server.close(r)); }
  }
})().catch(e => {console.error(e);process.exitCode=1;});
EOF
```

## 3. The completion-loop controller

Read this during class. The queue is a text file, and each item gets at most two attempts. A failed check becomes the next attempt's explicit repair feedback. Task order and checks are external to the LLM.

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
cat > .harness/journal.cjs <<'EOF'
// Called only after the cumulative browser criterion passes.
const fs = require('node:fs');
const [directory, task] = process.argv.slice(2);
const report = JSON.parse(fs.readFileSync(`${directory}/report.json`, 'utf8'));
for (const key of ['implemented', 'envisioned', 'limitations']) {
  if (typeof report[key] !== 'string' || !report[key].trim()) {
    throw Error(`Missing ${key} in the agent report`);
  }
}
const checks = fs.readFileSync(`${directory}/checks.txt`, 'utf8').trim();
fs.appendFileSync('ledger.md', `\n## Task ${task}\n\n` +
  `Agent implemented: ${report.implemented}\n\n` +
  `Agent envisioned: ${report.envisioned}\n\n` +
  `Agent limitations: ${report.limitations}\n\n` +
  `Controller observed: ${checks}\n\n` +
  `Evidence: ${directory}/screen.png. Human acceptance pending.\n`);
EOF
cat > .harness/queue.sh <<'EOF'
#!/usr/bin/env bash
# Run this COPY from an isolated demo's .harness/, never from CAP's source tree.
# Outer loop: next task. Inner loop: one attempt, then at most one repair.
set -euo pipefail
cd "$(dirname "$0")/.."
seconds="${PASS_SECONDS:-180}"
check_seconds="${CHECK_SECONDS:-60}"

# Require the disposable repository and reject unaccepted prior changes.
if [ "$(git rev-parse --show-toplevel)" != "$PWD" ] ||
   [ "$(git branch --show-current)" != 'feature/paint-task-queue' ] ||
   [ -n "$(git remote)" ] || [ ! -f .harness/queue.txt ]; then
  echo 'STOP: use the isolated fixture created by the runbook.'; exit 1
fi
if [ -n "$(git status --porcelain -- app ledger.md)" ]; then
  echo 'STOP: inspect unaccepted app or ledger changes before starting.'; exit 1
fi

run="$(date -u +%Y%m%dT%H%M%SZ)-$$"
worker=''
# The active Python helper stops its agent or browser group on interruption.
cleanup() {
  if [ -n "$worker" ]; then
    kill -TERM "$worker" 2>/dev/null || true
    wait "$worker" 2>/dev/null || true
  fi
}
trap cleanup EXIT
trap 'exit 130' INT TERM

# read takes the next line of queue.txt. No LLM chooses the task order.
while read -r task; do
  passed=0
  feedback='No prior check output for this task.'
  for attempt in 1 2; do
    dir="$PWD/artifacts/$run-task$task-attempt$attempt"
    mkdir -p "$dir"

    echo "[1/6] Task $task, attempt $attempt/2: request + prior feedback"
    cat .harness/contract.txt ledger.md ".harness/tasks/$task.txt" > "$dir/prompt.txt"
    printf '\nPrevious check feedback:\n%s\n' "$feedback" >> "$dir/prompt.txt"

    echo '[2/6] Call a fresh agent; wait for its result'
    python3 .harness/agent.py "$PWD/app" "$dir/prompt.txt" "$dir/report.json" \
      "$seconds" > "$dir/events.jsonl" 2> "$dir/agent.stderr" &
    worker=$!
    if ! wait "$worker"; then
      echo "STOP: agent failed; inspect $dir"; exit 1
    fi
    worker=''

    echo '[3/6] Measure all app changes since this task began'
    git add -N app # New files must also count against the budget.
    git diff -- app > "$dir/app.diff"
    lines="$(python3 .harness/changes.py)"
    # A failed first attempt has no commit, so its repair shares this budget.
    if [ "$lines" -gt 1000 ]; then
      echo "STOP: $lines changed lines exceed the task budget; inspect $dir"; exit 1
    fi

    echo '[4/6] Run this task AND all earlier browser criteria'
    # Track the browser check and give it an overall time limit.
    python3 .harness/check.py "$check_seconds" node .harness/check.cjs app \
      "$task" "$dir/screen.png" > "$dir/checks.txt" 2>&1 &
    worker=$!
    if wait "$worker"; then
      worker=''
      echo '[5/6] Record claims, observed checks, and the evidence path'
      node .harness/journal.cjs "$dir" "$task"

      echo '[6/6] Save a local checkpoint; then move to the next task'
      git add app ledger.md
      git commit -m "Queued task $task passed its browser criteria"
      git rev-parse HEAD > "$dir/checkpoint.txt"
      passed=1
      echo "DONE task $task: $dir"
      break # Leave the attempt loop: this task is now complete.
    fi
    worker=''

    # A check failure supplies facts to one fresh repair attempt, not a resume.
    feedback="$(cat "$dir/checks.txt")"
    echo "FAILED CHECK task $task: $dir/checks.txt"
  done

  # Do not advance the queue after two failed attempts.
  if [ "$passed" -ne 1 ]; then
    echo "STOP: task $task exhausted two attempts; human handoff"; exit 1
  fi
done < .harness/queue.txt
EOF
git add .gitignore AGENTS.md package.json .harness app ledger.md
git commit -m "Prepare independent paint task-queue fixture"
```

A task's criterion may pass while the product still has other defects. The queue does not create parallel Workers, discover dependencies, or independently review code. The two-attempt cap is a classroom bound, not an assurance that a difficult task can finish in two attempts.

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
