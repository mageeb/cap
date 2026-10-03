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

Create a **new** nested demo repository. The commands stop if this location already exists. Keep an existing run intact and choose a new directory name if necessary. These commands create runtime files only when you execute them; the curriculum contains this Markdown runbook.

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

### Independent checks

The checks are Human-owned. Workers receive isolated app copies and publish only their declared files; the Controller invokes these checks from outside those copies. Changed, added, deleted, or symlinked unowned app paths are rejected before checks, preventing a candidate from passing against unpublished helper edits.

```bash
cd "$RUNDIR"
cat > browser-check.mjs <<'JS'
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = await import('playwright');
import assert from 'node:assert/strict';
import fs from 'node:fs';
const [url, out] = process.argv.slice(2);
fs.mkdirSync(out, {recursive:true});
const browser = await chromium.launch({headless:true});
try {
  const page = await browser.newPage({viewport:{width:900,height:500}});
  await page.goto(url);
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.getByLabel('Tool', {exact:true}).count(), 1);
  assert.equal(await page.getByLabel('Color', {exact:true}).count(), 1);
  await page.selectOption('#tool', 'pencil');
  await page.selectOption('#color', '#188D91');
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'pencil');
  assert.equal(await page.locator('#color').inputValue(), '#188D91');
  async function dragStroke() {
    const box = await page.locator('#paint').boundingBox();
    await page.mouse.move(box.x + 41, box.y + 41);
    await page.mouse.down();
    await page.mouse.move(box.x + 111, box.y + 41, {steps:8});
    await page.mouse.up();
  }
  const readPixel = () => page.locator('#paint').evaluate(c => [...c.getContext('2d').getImageData(75,40,1,1).data]);
  const assertTeal = pixel => { assert.deepEqual(pixel.slice(0,3), [24,141,145]); assert.equal(pixel[3],255); };
  await dragStroke();
  assertTeal(await readPixel());
  await page.screenshot({path:`${out}/pencil-teal-reload.png`});
  await page.selectOption('#tool', 'eraser');
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'eraser');
  assert.equal(await page.locator('#color').inputValue(), '#188D91');
  // Canvas contents are not persisted: draw a new stroke after this reload.
  await page.selectOption('#tool', 'pencil');
  await dragStroke(); assertTeal(await readPixel());
  await page.selectOption('#tool', 'eraser');
  await dragStroke();
  const erased = await readPixel();
  assert.ok(erased[3] === 0 || erased.every(v => v === 255), `Expected white/transparent erased pixel: ${erased}`);
  await page.screenshot({path:`${out}/eraser-after-reload.png`});
  await page.evaluate(() => localStorage.setItem('cap.paint.preferences.v1', '{broken'));
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'pencil');
  assert.equal(await page.locator('#color').inputValue(), '#152536');
  await page.screenshot({path:`${out}/invalid-state-defaults.png`});
  console.log(JSON.stringify({reload:'passed',drawing:'passed',eraser:'passed',invalid_state:'passed'}));
} finally { await browser.close(); }
JS
cat > checks.py <<'PY'
from pathlib import Path
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
import json, subprocess, sys, threading
ROOT = Path(__file__).resolve().parent
criterion, app = sys.argv[1], Path(sys.argv[2]).resolve()
assert criterion in {'schema','ui','storage','integration','review'}, 'Unknown criterion'

def node(script):
    result = subprocess.run(['node', '--input-type=module', '-e', script], cwd=ROOT,
                            capture_output=True, text=True, timeout=30)
    if result.returncode:
        raise AssertionError(result.stderr or result.stdout)
    return result.stdout

prefix = "import assert from 'node:assert/strict';\n"
uri = lambda name: (app / name).as_uri()
if criterion == 'schema':
    assert json.loads((app / 'schema.json').read_text()) == {
        'tools': ['pencil', 'eraser'], 'colors': ['#152536','#188D91','#E8A64C'], 'default': {'tool': 'pencil', 'color': '#152536'}}
if criterion == 'ui':
    node(prefix + f"import {{renderToolbar}} from {json.dumps(uri('toolbar.js'))};\n" + r"""
const html=renderToolbar({tool:'eraser',color:'#188D91'});
for (const [id,value] of [['tool','eraser'],['color','#188D91']]) {
  const select=html.match(new RegExp(`<select\\b[^>]*id=["']${id}["'][^>]*>([\\s\\S]*?)</select>`,'i'));
  assert.ok(select,`Missing select#${id}`);
  const options=select[1].match(/<option\b[^>]*>/gi)||[];
  assert.ok(options.some(o => new RegExp(`value=["']${value}["']`).test(o) && /\bselected\b/i.test(o)));
}
""")
if criterion in ('storage', 'integration', 'review'):
    node(prefix + f"import {{loadPreferences,savePreferences}} from {json.dumps(uri('settings.js'))};\n" + r"""
const data = new Map(), storage = {getItem:k=>data.get(k)??null,setItem:(k,v)=>data.set(k,v)};
const defaults={tool:'pencil',color:'#152536'}, good={tool:'eraser',color:'#188D91'};
assert.deepEqual(loadPreferences(storage),defaults);
savePreferences(storage,good); assert.deepEqual(loadPreferences(storage),good);
assert.deepEqual(JSON.parse(data.get('cap.paint.preferences.v1')),good);
for (const bad of ['{broken',JSON.stringify({tool:'laser',color:'#188D91'}),JSON.stringify({tool:'pencil',colour:'#188D91'}),JSON.stringify({tool:'pencil',color:'blue'})]) {
  storage.setItem('cap.paint.preferences.v1',bad); assert.deepEqual(loadPreferences(storage),defaults);
}
const denied={getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
assert.deepEqual(loadPreferences(denied),defaults);
assert.doesNotThrow(()=>savePreferences(denied,defaults));
savePreferences(storage,{tool:'bad',color:'bad'}); assert.deepEqual(loadPreferences(storage),defaults);
""")
if criterion in ('integration', 'review'):
    node(prefix + f"import {{loadPreferences,savePreferences}} from {json.dumps(uri('settings.js'))};\n"
         + f"import {{renderToolbar}} from {json.dumps(uri('toolbar.js'))};\n" + r"""
const data=new Map(), storage={getItem:k=>data.get(k)??null,setItem:(k,v)=>data.set(k,v)};
const good={tool:'pencil',color:'#188D91'};
savePreferences(storage,good); assert.deepEqual(loadPreferences(storage),good);
const html=renderToolbar(loadPreferences(storage)); assert.ok(html.includes('#188D91'));
""")
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(SimpleHTTPRequestHandler, directory=str(app)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    try:
        result = subprocess.run(['node', str(ROOT/'browser-check.mjs'),
            f'http://127.0.0.1:{server.server_port}', str(app.parent/'screenshots')],
            capture_output=True, text=True, timeout=60)
        assert result.returncode == 0, result.stderr or result.stdout
        print(result.stdout.strip())
    finally:
        server.shutdown(); server.server_close()
if criterion == 'review':
    report = json.loads(Path(sys.argv[3]).read_text())
    assert report['verdict'] == 'pass' and report['gaps'] == []
    assert len(report['criteria']) == 3
    assert {item['id'] for item in report['criteria']} == {'reload','drawing','invalid_state'}
    assert all(item['status'] == 'met' for item in report['criteria'])
print(json.dumps({'criterion':criterion,'status':'passed'}))
PY
cat > supervisor-schema.json <<'JSON'
{"type":"object","properties":{"repair_task":{"type":"string","enum":["storage","blocked"]},"reason":{"type":"string"}},"required":["repair_task","reason"],"additionalProperties":false}
JSON
cat > review-schema.json <<'JSON'
{"type":"object","properties":{"verdict":{"type":"string","enum":["pass","needs_work"]},"criteria":{"type":"array","items":{"type":"object","properties":{"id":{"type":"string","enum":["reload","drawing","invalid_state"]},"status":{"type":"string","enum":["met","partial","unmet"]},"evidence":{"type":"string"}},"required":["id","status","evidence"],"additionalProperties":false}},"gaps":{"type":"array","items":{"type":"string"}}},"required":["verdict","criteria","gaps"],"additionalProperties":false}
JSON
```

### The graph template

Each task has `id`, `deps`, `owned`, `task`, `check`, and `max_attempts`. `deps` are prerequisite task IDs. `check` is a whitelisted independent criterion, not an arbitrary shell command supplied by a model.

```bash
cd "$RUNDIR"
cat > plan/task-graph-template.json <<'JSON'
{
  "version": 1,
  "tasks": [
    {"id":"schema","deps":[],"owned":["schema.json"],"check":"schema","max_attempts":2,
     "task":"Write app/schema.json exactly as {\"tools\":[\"pencil\",\"eraser\"],\"colors\":[\"#152536\",\"#188D91\",\"#E8A64C\"],\"default\":{\"tool\":\"pencil\",\"color\":\"#152536\"}}. Do not change other app files."},
    {"id":"ui","deps":["schema"],"owned":["toolbar.js"],"check":"ui","max_attempts":2,
     "task":"Implement app/toolbar.js. Export renderToolbar(preferences), returning an HTML string containing labelled select#tool and select#color. Tools are pencil/eraser; colors are #152536/#188D91/#E8A64C. Mark the supplied tool and color options selected. Do not access storage or attach event handlers. Preserve the field name color. Do not change other app files."},
    {"id":"storage","deps":["schema"],"owned":["settings.js"],"check":"storage","max_attempts":2,
     "task":"Implement app/settings.js with exported function declarations loadPreferences(storage) and savePreferences(storage,preferences). Use the injected storage object and key cap.paint.preferences.v1. Valid tools are pencil/eraser and colors are #152536/#188D91/#E8A64C. Missing, malformed, unsupported, or invalid data recovers to BOTH defaults {tool:'pencil',color:'#152536'}. Return fresh objects and save normalized valid data. Storage read/write exceptions must not stop drawing. Preserve the exact {tool,color} interface. Do not change other app files."},
    {"id":"integrate","deps":["ui","storage"],"owned":["main.js"],"check":"integration","max_attempts":2,
     "task":"Implement app/main.js only. Import renderToolbar from toolbar.js and loadPreferences/savePreferences from settings.js. Call loadPreferences(localStorage); insert renderToolbar(preferences) into #toolbar. Own all change handlers: update the complete preferences object and call savePreferences(localStorage,preferences). Draw on #paint using pointerdown/pointermove/pointerup: pencil has lineWidth 4 and the selected color; eraser draws white. Use canvas-relative coordinates. Set window.appReady=true after wiring. Color changes affect new pencil strokes. Never patch a field mismatch in another owned file; report it. Preserve the accepted color schema."},
    {"id":"review","deps":["integrate"],"owned":[],"check":"review","max_attempts":2,
     "task":"Review the integrated paint app and the supplied independent check evidence read only. Challenge reload, actual colored drawing and erasing, and invalid-state defaults. Return JSON matching the CLI-supplied structured schema, with criteria IDs reload, drawing, invalid_state. Cite actual evidence, identify gaps, and do not edit app files. A report does not replace the independent browser checks."}
  ]
}
JSON
```

### The external Controller

Read `start`, `finish`, and the final loop first. `start` dispatches a fresh context into an isolated copy. `finish` runs a Human-owned check before publishing declared output files. The loop selects only nodes whose dependencies passed. Failed attempts preserve their candidate files for the next fresh context; they never publish unchecked code to the live app.

```bash
cd "$RUNDIR"
cat > controller.py <<'PY'
from pathlib import Path
import hashlib, json, os, shutil, signal, subprocess, sys, time
ROOT = Path(__file__).resolve().parent
APP, PLAN = ROOT/'app', ROOT/'plan'
GRAPH = PLAN/'task-graph.json'
EXPECTED = {'schema':([],['schema.json'],'schema'), 'ui':(['schema'],['toolbar.js'],'ui'),
    'storage':(['schema'],['settings.js'],'storage'),
    'integrate':(['ui','storage'],['main.js'],'integration'), 'review':(['integrate'],[],'review')}

def digest():
    return hashlib.sha256(GRAPH.read_bytes() + (PLAN/'prd.md').read_bytes()).hexdigest()

def validate():
    graph = json.loads(GRAPH.read_text())
    if graph['version'] != 1: raise ValueError('Expected accepted version 1')
    tasks = graph['tasks']
    if len(tasks) != 5 or {t['id'] for t in tasks} != set(EXPECTED): raise ValueError('Five known task IDs required')
    used = set()
    for t in tasks:
        deps, owned, check = EXPECTED[t['id']]
        if sorted(t['deps']) != sorted(deps) or t['owned'] != owned or t['check'] != check:
            raise ValueError('Dependencies, ownership, or criterion violate this teaching contract')
        if type(t['max_attempts']) is not int or not 1 <= t['max_attempts'] <= 2: raise ValueError('Attempt limit must be 1 or 2')
        if not isinstance(t['task'], str) or not t['task'].strip(): raise ValueError('Missing task instruction')
        for name in t['owned']:
            if Path(name).is_absolute() or '..' in Path(name).parts or name in used: raise ValueError('Unsafe or overlapping owned path')
            used.add(name)
    done = set()
    while len(done) < len(tasks):
        ready = {t['id'] for t in tasks if set(t['deps']) <= done} - done
        if not ready: raise ValueError('Dependency cycle or missing prerequisite')
        done |= ready
    return {t['id']:t for t in tasks}

TASKS = validate()
if '--validate' in sys.argv:
    print('Valid graph and PRD hash:', digest()); sys.exit(0)
if '--approve' in sys.argv:
    (PLAN/'APPROVED').write_text(digest()); print('Human-approved version 1:', digest()); sys.exit(0)
if (PLAN/'APPROVED').read_text().strip() != digest(): raise ValueError('Human approval missing or stale')
STATE = ROOT/'state.json'
state = json.loads(STATE.read_text()) if STATE.exists() else {'plan_hash':digest(),'phase':'ready',
    'supervisor_calls':0,'tasks':{i:{'status':'pending','attempts':0} for i in TASKS}}
if state['plan_hash'] != digest(): raise ValueError('Plan changed. Keep this run intact and start a new run.')
for record in state['tasks'].values():
    if record['status'] == 'running':
        record['status'] = 'pending'; record.pop('candidate', None)
running = {}

def save():
    tmp = ROOT/'state.tmp'; tmp.write_text(json.dumps(state, indent=2)); tmp.replace(STATE)

def notify(message, terminal=False):
    (ROOT/'notification.txt').write_text(message + '\n'); print(message, flush=True)
    thread = os.environ.get('PLANNER_THREAD')
    if not terminal or not thread: return
    try:
        if os.environ.get('PLANNER_QUEUE_READY') != '1': raise RuntimeError('Queue command not preflighted')
        result = subprocess.run(['codex','queue','--thread',thread,'--message',message],
            capture_output=True, text=True, timeout=15)
        if result.returncode: raise RuntimeError(result.stderr.strip() or result.stdout.strip())
        state['notification_delivery'] = 'queued to '+thread
    except Exception as error:
        state['notification_delivery'] = 'failed: '+str(error)
        print('Queue delivery failed; paste notification.txt into Planner:', error, flush=True)
    save()

def command(job, output, readonly=False, schema=None):
    args = ['codex','exec','-C',str(job),'--ephemeral','--sandbox',
        'read-only' if readonly else 'workspace-write','--disable','memories','--disable','multi_agent',
        '--ignore-user-config','-c','project_doc_max_bytes=0','--json','-o',str(output)]
    if schema: args += ['--output-schema', str(schema)]
    return args + ['-']

def snapshot(app, owned):
    result = {}
    for path in app.rglob('*'):
        name = path.relative_to(app).as_posix()
        if path.is_symlink(): raise RuntimeError('Symlink in candidate: '+name)
        if name not in owned:
            result[name] = hashlib.sha256(path.read_bytes()).hexdigest() if path.is_file() else 'directory'
    return result

def stop(process):
    try: os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError: return
    try: process.wait(timeout=5)
    except subprocess.TimeoutExpired: pass
    try: os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError: pass
    process.wait()

def interrupt(signum, frame):
    raise RuntimeError('Human interrupt signal '+str(signum))

signal.signal(signal.SIGINT, interrupt)
signal.signal(signal.SIGTERM, interrupt)

def start(task_id):
    t, r = TASKS[task_id], state['tasks'][task_id]
    if r['attempts'] >= t['max_attempts']: raise RuntimeError('Attempt budget exhausted: '+task_id)
    r['attempts'] += 1
    job = ROOT/'jobs'/f"{task_id}-{r['attempts']}"; job.mkdir(exist_ok=False)
    shutil.copytree(APP, job/'app'); (job/'package.json').write_text('{"type":"module"}')
    if r.get('candidate'):
        for name in t['owned']:
            candidate = Path(r['candidate'])/'app'/name
            if candidate.is_file(): shutil.copy2(candidate, job/'app'/name)
    output = job/('review.json' if task_id == 'review' else 'report.txt')
    evidence = (ROOT/'artifacts'/'integration-check.txt').read_text() if task_id == 'review' else ''
    prompt = (PLAN/'prd.md').read_text() + '\nACCEPTED TASK:\n' + json.dumps(t) + '\n'
    prompt += 'Work only inside app/. Publish only declared owned files. Human-owned checks are outside this sandbox.\n'
    prompt += 'Do not edit, add, delete, or link any unowned app path; those changes fail before checks.\n'
    prompt += 'Prior failure: '+r.get('error','none')+'\nIndependent evidence:\n'+evidence
    before = snapshot(job/'app',t['owned']); log = open(job/'events.jsonl','w')
    process = subprocess.Popen(command(job,output,task_id=='review',ROOT/'review-schema.json' if task_id=='review' else None),
        stdin=subprocess.PIPE, stdout=log, stderr=log, text=True, start_new_session=True)
    running[task_id] = (process,job,output,log,time.monotonic(),before)
    r.update(status='running',job=str(job)); save()
    process.stdin.write(prompt); process.stdin.close(); notify('Started '+task_id)

def check(t, app, output):
    args = [sys.executable,str(ROOT/'checks.py'),t['check'],str(app)]
    if t['id'] == 'review': args += [str(output)]
    process = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, start_new_session=True)
    try:
        started = time.monotonic()
        while process.poll() is None:
            if (ROOT/'STOP').exists() or time.monotonic()-started > 100:
                raise RuntimeError('Independent check stopped or timed out')
            time.sleep(.3)
        out, err = process.communicate()
        return subprocess.CompletedProcess(args, process.returncode, out, err)
    finally: stop(process)

def supervisor(error):
    job = ROOT/'jobs'/'supervisor'; job.mkdir(exist_ok=False)
    shutil.copytree(APP,job/'app'); output = job/'assignment.json'
    prompt = 'Judge this integration failure read only. Accepted interface is {tool,color}. '
    prompt += 'Return repair_task=storage only if evidence supports that cause. Otherwise return repair_task=blocked with the reason.\n'+error
    with open(job/'events.jsonl','w') as log:
        p = subprocess.Popen(command(job,output,True,ROOT/'supervisor-schema.json'),stdin=subprocess.PIPE,stdout=log,stderr=log,text=True,start_new_session=True)
        try:
            p.stdin.write(prompt); p.stdin.close(); started = time.monotonic()
            while p.poll() is None:
                if (ROOT/'STOP').exists() or time.monotonic()-started > 90:
                    raise RuntimeError('Supervisor stopped or timed out')
                time.sleep(.3)
        finally: stop(p)
    if p.returncode: raise RuntimeError('Supervisor invocation failed')
    assignment = json.loads(output.read_text())
    if assignment.get('repair_task') != 'storage' or not assignment.get('reason'):
        raise RuntimeError('Supervisor blocked: '+assignment.get('reason','invalid assignment'))
    return assignment

def finish(task_id):
    p, job, output, log, _, before = running.pop(task_id); stop(p); log.close()
    t, r = TASKS[task_id], state['tasks'][task_id]
    unchanged = snapshot(job/'app',t['owned']) == before
    result = check(t,job/'app',output) if p.returncode == 0 and unchanged else None
    report = (result.stdout + result.stderr) if result else ('Unowned paths changed; checks refused' if not unchanged else 'Codex invocation failed; inspect events.jsonl')
    (job/'check.txt').write_text(report)
    if result and result.returncode == 0:
        for name in t['owned']:
            source = job/'app'/name
            if not source.is_file() or source.is_symlink(): raise RuntimeError('Missing or unsafe output: '+name)
            shutil.copy2(source,APP/name)
        r.update(status='passed',error='')
        if task_id == 'integrate':
            shutil.copy2(job/'check.txt',ROOT/'artifacts'/'integration-check.txt')
            shutil.copytree(job/'screenshots',ROOT/'artifacts'/'screenshots',dirs_exist_ok=True)
        notify('Passed '+task_id)
    else:
        r.update(status='pending',error=report,candidate=str(job))
        if task_id == 'integrate' and state['supervisor_calls'] == 0:
            state['supervisor_calls'] += 1; save(); notify('Integration failed; waking one Supervisor')
            assignment = supervisor(report)
            repair = state['tasks']['storage']; repair.update(status='pending',error=assignment['reason'])
        if r['attempts'] >= t['max_attempts']: raise RuntimeError('Check failed at attempt limit: '+task_id)
    save()

try:
    state['phase'] = 'running'; save()
    while True:
        if digest() != state['plan_hash']: raise RuntimeError('Accepted plan changed during execution')
        if (ROOT/'STOP').exists(): raise RuntimeError('Human STOP requested')
        for task_id, (p,job,output,log,started,before) in list(running.items()):
            if time.monotonic()-started > 180 and p.poll() is None:
                stop(p)
            if p.poll() is not None: finish(task_id)
        ready = [i for i,t in TASKS.items() if state['tasks'][i]['status']=='pending'
                 and all(state['tasks'][d]['status']=='passed' for d in t['deps'])]
        for task_id in ready:
            if task_id == 'integrate' and (ROOT/'PAUSE_INTEGRATION').exists():
                state['phase']='paused before integration'; save(); continue
            state['phase']='running'; start(task_id)
        if all(r['status']=='passed' for r in state['tasks'].values()):
            state['phase']='completed'; save(); notify('Completed: inspect evidence before Human acceptance',terminal=True); break
        time.sleep(.3)
except Exception as error:
    for p,job,output,log,started,before in running.values():
        stop(p)
        log.close()
    for record in state['tasks'].values():
        if record['status']=='running':
            record['status']='pending'; record.pop('candidate',None)
    state['phase']='blocked'; state['reason']=str(error); save(); notify('Blocked: '+str(error),terminal=True); sys.exit(1)
PY
```

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
cat > app/settings.js <<'JS'
// PREPARED ADVERSE TEST: deliberately violates the accepted color field.
const defaults = {tool:'pencil',color:'#152536'};
function validatePreferences(value) {
  return value && ['pencil','eraser'].includes(value.tool) && ['#152536','#188D91','#E8A64C'].includes(value.color)
    ? {tool:value.tool,color:value.color} : {...defaults};
}
export function loadPreferences(storage) {
  let value;
  try { value=validatePreferences(JSON.parse(storage.getItem('cap.paint.preferences.v1'))); }
  catch { value={...defaults}; }
  return {tool:value.tool,colour:value.color};
}
export function savePreferences(storage,preferences) {
  try { storage.setItem('cap.paint.preferences.v1',JSON.stringify(validatePreferences(preferences))); }
  catch { /* Keep drawing usable when writes fail. */ }
}
JS
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

An **all-Claude** version is a controller port, not a command-name substitution. `launch()` must call fresh `claude -p` jobs with the correct allowed tools/permissions; normal JSON output is a result envelope and schema data lives in `structured_output`, unlike the Codex `-o` report file. Worker, read-only Supervisor and Reviewer outputs/status handling all need adapters. Preserve isolated candidate copies, ownership checks, independent browser gates, process-group cleanup, retries and plan hashes. Replace notification delivery separately or keep the file handoff. These adapters are not implemented in this runbook; use the native [Supervisor demo](demo-02-supervisor.md#if-you-use-claude-code) for a runnable Claude-only exercise.

[Claude programmatic calls and result envelopes](https://code.claude.com/docs/en/headless), [Claude CLI authentication](https://code.claude.com/docs/en/cli-reference)
