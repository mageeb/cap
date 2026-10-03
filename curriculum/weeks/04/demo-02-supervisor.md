# Demo 2 — A live Supervisor with native Codex agents

**Live slot:** minutes 24–32. Prepare the fixture, authenticate, rehearse permissions and open the app before class. During class, keep the main conversation open: plan, fan out, steer, integrate and review.

**CLI used here:** the default instructions use Codex CLI. If you use Claude Code, see [the optional Claude path](#if-you-use-claude-code) before running setup; changing a command name alone does not adapt a controller.

**Real example:** a paint app forgets its tool and color when the page reloads. One Worker handles the toolbar, another handles storage, an Integrator connects them, and a separate Reviewer checks the combined result. The main session is the Supervisor and still owns execution decisions. This differs from Demo 3, where a controller owns scheduling.

## 1. Preflight and complete independent fixture

### Install missing runtime tools (macOS)

These executable instructions use an **already installed Codex CLI**. You need Node.js 20+ with npm, Python 3.10+, and Git 2.28+. Use a currently supported Node LTS release when installing. Each runbook is independent; do not borrow another demo's dependencies.

If those runtime tools are already available, skip this installation block. If any are missing or older, the following installs the runtimes through an existing Homebrew installation and selects them in this terminal:

```bash
brew install node@24 python git
export PATH="$(brew --prefix node@24)/bin:$(brew --prefix)/bin:$PATH"
```

If `brew` is missing, follow the [official Homebrew installation instructions](https://brew.sh/) first, including its printed **Next steps** for adding Homebrew to your shell, then run the block above. The Homebrew installer explains its machine changes before applying them. Alternatively, use the official [Node.js macOS LTS installer](https://nodejs.org/en/download) and [Python macOS installer](https://www.python.org/downloads/macos/), and [Git's macOS installation instructions](https://git-scm.com/install/mac).

Open a fresh terminal after installing. If you used the Homebrew block, repeat its `export PATH=...` line in every new demo terminal so they select the same runtimes. This guide's setup commands target macOS.

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
node -e "if (Number(process.versions.node.split('.')[0]) < 20) { console.error('Need Node 20+'); process.exit(1); }"
python3 - <<'PY_CHECK'
import re, subprocess, sys
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
    codex --help >/dev/null
    codex features list
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

No application packages, build step, or Playwright installation are needed for this demo. Node's built-in checks and a regular browser are enough.

Native subagent availability is version-dependent. The current CLI documents explicit delegation, custom agent files and `/agent` for inspecting or switching threads. Rehearse the exact setup and sandbox on the presentation machine. Do not replace failed delegation with fabricated Worker reports. [Codex subagents](https://learn.chatgpt.com/docs/agent-configuration/subagents)

Copy this block into a terminal. This fixture is independent of Demo 1.

**Git isolation:** this setup does not switch CAP's curriculum branch or create commits in CAP. It adds `/.demo-runs/` to CAP's local `.git/info/exclude`, preserving existing lines. That rule is not tracked or shared; this runbook configures it even on a fresh checkout. The fixture gets its own Git repository, feature branch and no remote. Run subsequent demo commands inside `$CAP_SUPERVISOR_DEMO`.

```bash
cd /Users/michaelmurray/code/cap || exit 1
CAP_DEMO_EXCLUDE=$(git rev-parse --git-path info/exclude)
if ! grep -qxF '/.demo-runs/' "$CAP_DEMO_EXCLUDE"; then
  printf '\n/.demo-runs/\n' >> "$CAP_DEMO_EXCLUDE"
fi
export CAP_SUPERVISOR_DEMO="$PWD/.demo-runs/supervisor-$(date +%Y%m%d-%H%M%S)"
if [ -e "$CAP_SUPERVISOR_DEMO" ]; then
  printf 'Existing run found: %s. Choose a fresh demo directory.\n' "$CAP_SUPERVISOR_DEMO" >&2
  exit 1
fi
mkdir -p "$CAP_SUPERVISOR_DEMO/app" "$CAP_SUPERVISOR_DEMO/.codex/agents"
cd "$CAP_SUPERVISOR_DEMO" || exit 1
git init -b feature/paint-preferences || exit 1
if [ "$(git -C "$CAP_SUPERVISOR_DEMO" rev-parse --show-toplevel)" != "$CAP_SUPERVISOR_DEMO" ] ||
   [ "$(git -C "$CAP_SUPERVISOR_DEMO" branch --show-current)" != "feature/paint-preferences" ] ||
   [ -n "$(git -C "$CAP_SUPERVISOR_DEMO" remote)" ] ||
   ! git -C /Users/michaelmurray/code/cap check-ignore -q "$CAP_SUPERVISOR_DEMO/"; then
  printf 'Git isolation check failed. Stop here; do not run later blocks.\n' >&2
  exit 1
fi
# Identity is set only in this isolated repository when no identity exists.
git config user.name >/dev/null || git config --local user.name "CAP Demo"
git config user.email >/dev/null || git config --local user.email "cap-demo@example.invalid"
git var GIT_AUTHOR_IDENT >/dev/null
git -C "$CAP_SUPERVISOR_DEMO" rev-parse --show-toplevel
git -C "$CAP_SUPERVISOR_DEMO" branch --show-current
git -C "$CAP_SUPERVISOR_DEMO" remote -v
git -C /Users/michaelmurray/code/cap check-ignore -v "$CAP_SUPERVISOR_DEMO/"
git -C /Users/michaelmurray/code/cap status --short
cat > AGENTS.md <<'EOF'
# Isolated classroom fixture
All edits in this nested repository are delegated to the named Workers or
Integrator. The main session is the Supervisor. CAP source and this fixture's
spec.md, checks.mjs, agent configurations, package.json and AGENTS.md are fixed.
Do not add unrelated CAP planning/DECISIONS workflows. Do not commit or push.
UI Worker owns app/toolbar.js; Storage Worker owns app/settings.js;
Integrator owns app/main.js; Reviewer has no write ownership.
Each report must cite actual files and actual command results, or say not run.
EOF
cat > package.json <<'EOF'
{"private":true,"type":"module"}
EOF
cat > spec.md <<'EOF'
# Accepted product contract: remember paint preferences
A user picks a drawing tool and color, reloads, and resumes with the same choices.

Allowed tools: pencil, eraser.
Allowed colors: #152536 (navy), #188D91 (teal), #E8A64C (amber).
Defaults: {tool:'pencil', color:'#152536'}.
Storage key: cap.paint.preferences.v1. JSON shape: {tool, color} only.
Missing, malformed, or invalid saved preferences fall back to BOTH defaults.
Storage read/write exceptions must not stop drawing.
Pencil/teal must draw a visible teal stroke after reload. Check visible color
with PENCIL: an eraser stroke cannot prove which drawing color is selected.
Eraser selection and the retained drawing color must persist independently.
The page remains usable with keyboard-labelled tool and color controls.
No packages, services, build step, or other product features.

Accepted module contract:
- app/toolbar.js exports renderToolbar(preferences), an HTML string containing
  labelled select#tool and select#color with the supplied choices selected.
  It has no storage access or event handlers.
- app/settings.js exports loadPreferences(storage) and
  savePreferences(storage, preferences). The storage object is injected.
  load returns a fresh valid {tool,color}. save writes normalized valid data.
- app/main.js owns rendering, change handlers, persistence calls, and drawing.

Task graph:
UI toolbar   ──┐
              ├── Integration ── Review ── human browser acceptance
Storage      ──┘

UI and Storage can run concurrently because their owned files do not overlap.
Integration starts only after BOTH actual reports. Review starts after integration.
EOF
cat > app/index.html <<'EOF'
<!doctype html>
<html lang="en"><meta charset="utf-8"><title>CAP Paint</title>
<style>
body{font:18px system-ui;background:#f4f1e9;color:#152536;max-width:820px;margin:40px auto}
label{display:inline-block;margin:0 20px 18px 0}select{font:inherit;margin-left:8px}
canvas{display:block;background:white;border:2px solid #152536;touch-action:none}
</style>
<h1>CAP Paint</h1><p>Choose a tool and color. What happens after reload?</p>
<div id="toolbar"></div><canvas id="paint" width="700" height="360"></canvas>
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
export function savePreferences() { /* Not implemented yet. */ }
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
cat > checks.mjs <<'EOF'
import assert from 'node:assert/strict';
import { loadPreferences, savePreferences } from './app/settings.js';
import { renderToolbar } from './app/toolbar.js';
const defaults = {tool:'pencil',color:'#152536'}, key = 'cap.paint.preferences.v1';
function storage(raw = null) {
  return {getItem: k => {assert.equal(k,key); return raw;},
    setItem: (k,v) => {assert.equal(k,key); raw = v;}};
}
assert.deepEqual(loadPreferences(storage()),defaults);
for (const raw of ['{broken','null','[]','{"tool":"brush","color":"#188D91"}',
  '{"tool":"eraser","color":"purple"}']) assert.deepEqual(loadPreferences(storage(raw)),defaults);
const s = storage();
savePreferences(s,{tool:'pencil',color:'#188D91'});
assert.deepEqual(loadPreferences(s),{tool:'pencil',color:'#188D91'});
savePreferences(s,{tool:'eraser',color:'#188D91'});
assert.deepEqual(loadPreferences(s),{tool:'eraser',color:'#188D91'});
savePreferences(s,{tool:'bad',color:'bad'});
assert.deepEqual(loadPreferences(s),defaults);
const denied = {getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
assert.deepEqual(loadPreferences(denied),defaults);
assert.doesNotThrow(() => savePreferences(denied,defaults));
const html = renderToolbar({tool:'eraser',color:'#188D91'});
for (const [id,value] of [['tool','eraser'],['color','#188D91']]) {
  const select = html.match(new RegExp(`<select\\b[^>]*id=["']${id}["'][^>]*>([\\s\\S]*?)</select>`,'i'));
  assert.ok(select,`Missing select#${id}`);
  const options = select[1].match(/<option\b[^>]*>/gi) || [];
  assert.ok(options.some(o => new RegExp(`value=["']${value}["']`).test(o) && /\bselected\b/i.test(o)),
    `Selected ${id} must be ${value}`);
}
console.log('PASS: module defaults, invalid data, pencil/teal, eraser/teal, denied storage, selected controls.');
EOF
```

The fixture's baseline intentionally fails these preference checks. Do not claim it is a finished app. The checks exercise module behavior; the later browser checkpoint checks integration and visible drawing.

The setup supplies a local demo commit identity only when you have none; it leaves your global Git identity unchanged. If a checkpoint commit fails because your existing signing setup is unavailable, stop and resolve it before running the loop. For disposable demo commits only, `git config --local commit.gpgsign false` disables signing in this nested repository.

During setup, verify the printed results before continuing:

- Repository root: `/Users/michaelmurray/code/cap/.demo-runs/supervisor-YYYYMMDD-HHMMSS` (the timestamp is your actual run).
- Branch: `feature/paint-preferences`.
- Remotes: no output from `git remote -v`.
- CAP ignore check: `.git/info/exclude` supplies the `/.demo-runs/` rule; its line number may vary.
- CAP status: no `.demo-runs/` entry. Existing curriculum changes may still appear.

**If a check fails, stop:** keep the fixture intact, open a new terminal, and repeat setup with a fresh demo directory. Do not run the remaining blocks until the repository root, branch, remote and ignore checks pass. For later terminals, restore `$CAP_SUPERVISOR_DEMO` to the printed repository root and `cd "$CAP_SUPERVISOR_DEMO"` before following commands.

## 2. Define the native roles

Copy the following exact custom-agent configuration. No role pins a model; all inherit the session's model. The file ownership is a cooperation contract, not an operating-system restriction on individual files.

```bash
cat > .codex/config.toml <<'EOF'
[agents]
enabled = true
max_concurrent_threads_per_session = 3
EOF
cat > .codex/agents/ui_worker.toml <<'EOF'
name = "ui_worker"
description = "Implement the accepted paint toolbar in app/toolbar.js only."
developer_instructions = """
Read spec.md. Own only app/toolbar.js. Implement renderToolbar(preferences).
Do not access storage or change other files. Preserve the exact IDs/schema.
Report changed file, interface, checks actually run, limitations, and handoff.
Do not commit, push, or invent passing evidence.
"""
EOF
cat > .codex/agents/storage_worker.toml <<'EOF'
name = "storage_worker"
description = "Implement injected-storage preference helpers in app/settings.js only."
developer_instructions = """
Read spec.md. Own only app/settings.js. Implement loadPreferences(storage) and
savePreferences(storage,prefs), exact defaults/key/schema and safe failures.
Do not touch toolbar/main or other files. Report actual changes and checks,
limitations, and the integration handoff. Do not commit or push.
"""
EOF
cat > .codex/agents/integrator.toml <<'EOF'
name = "integrator"
description = "Connect completed toolbar and settings modules in app/main.js only."
developer_instructions = """
Start after actual UI and Storage reports. Read spec.md and their actual code.
Own only app/main.js. Load preferences, render the toolbar, save every change,
and implement pencil/eraser drawing while retaining the chosen color.
Use the accepted APIs exactly. Run node checks.mjs and report real output.
If another module is wrong, report its owner instead of editing that file.
Do not commit or push. Give evidence and limitations, not assumed acceptance.
"""
EOF
cat > .codex/agents/reviewer.toml <<'EOF'
name = "reviewer"
description = "Independently review the integrated preference change without editing."
sandbox_mode = "read-only"
developer_instructions = """
Read spec.md, actual changed code, and git diff. Do not edit any file.
Run node checks.mjs if the inherited runtime permissions allow it.
Check default/invalid storage, tool/color persistence wiring, visible color
risk, eraser behavior, event handlers, and ownership. Return findings with
file/line evidence and actual commands/results; explicitly list unrun checks.
Human browser acceptance remains pending. Do not rubber-stamp Worker reports.
"""
EOF
git add .
git commit -m "Prepare native-agent paint-preferences classroom fixture"
```

Parent runtime permissions can override an agent's requested sandbox. Confirm the Reviewer is behaving as a reader during rehearsal; both its prompt and role contract prohibit edits. [Custom agents and permissions](https://learn.chatgpt.com/docs/agent-configuration/subagents)

## 3. Open the app before class

In a separate terminal:

```bash
cd "$CAP_SUPERVISOR_DEMO"
python3 -m http.server 4174 --bind 127.0.0.1 --directory app
```

If the environment variable is not set in that new terminal, paste the actual fixture path printed by `pwd` in the setup terminal. Open `http://127.0.0.1:4174`. Draw once to show the baseline is real. Leave the server running. The Python server is an instructor process, not an agent tool.

## 4. Live: brainstorm and accept the graph — about one minute

Start the interactive main session in the setup terminal:

```bash
cd "$CAP_SUPERVISOR_DEMO"
codex --sandbox workspace-write
```

If Codex asks whether to trust this project, review and accept this isolated fixture during rehearsal. Paste:

```text
You are the Supervisor for this classroom fixture. Read AGENTS.md and spec.md.
Let's briefly reason about the user experience of remembering paint preferences.
Give one concrete example of pencil/teal after reload and a separate eraser
persistence example. Then show the accepted task graph and why UI and Storage
can run concurrently. Do not edit files or launch Workers until I say GO.
Keep your response short: this is a live demonstration.
```

**Checkpoint:** the plan uses the exact two APIs and file owners. Correct mistakes in chat before launching anything. This is the human approving a concrete boundary.

## 5. Live: fan out and stay in the conversation — about two minutes

Paste:

```text
GO. Spawn ui_worker and storage_worker concurrently using the native roles.
Give each its owned file and the accepted spec. Do not perform their edits
in the main session. Keep the other files fixed. Report the actual launched
threads and their work, then wait for both real reports before integration.
When both finish, record their actual reports and close completed child threads
if needed to free capacity. Delegate app/main.js to integrator. Record its actual
report and close its completed thread before spawning reviewer separately. Do not publish or commit.
If a check fails, route one bounded repair to the owner and review again.
If that repair fails, stop and hand me the actual failure; do not keep retrying.
```

While the Workers are active, paste this intervention into the **same main conversation**:

```text
Quick steering update: preserve our three-color palette; do not add a color
picker or more tools. Keep the saved color when eraser is selected. Confirm
which Workers are still running using actual thread state, and communicate
this constraint to any affected Worker. Keep going with the accepted graph.
```

Then use the actual CLI command:

```text
/agent
```

Inspect a real Worker thread in the picker, then return to the Supervisor thread. Show its task, owned file and actual report. The main chat is an active coordination surface; the Supervisor LLM is deciding when to launch, wait, route repairs and request review.

**Checkpoint:** two real parallel Workers, disjoint owned files, no Integration before their reports. If the Supervisor proceeds without delegation, paste: `Use the named native agents now; this demonstration requires actual child threads, not simulated role messages.`

## 6. Live: integration, review and human acceptance — about three minutes

Wait for Integration's actual report and the separate Reviewer's findings. Paste:

```text
Summarize actual handoffs: UI report, Storage report, Integration changes,
and independent Reviewer findings. Include checks run and their real results.
Do not treat module-check success as browser acceptance. Give me the exact
remaining browser steps. If blocked, name the owner and concrete next action.
```

In a separate terminal, inspect the real integration:

```bash
cd "$CAP_SUPERVISOR_DEMO"
node checks.mjs
git diff --stat
git diff -- app
```

The expected **successful** check output starts `PASS: module defaults...`; a failing command is evidence to route back to the relevant owner, not something to skip. Run these browser steps:

1. Select **Pencil / Teal**, reload, and confirm both controls still show those values. Draw a new stroke: it must visibly be teal.
2. Select **Eraser** while retaining **Teal**, reload, and confirm **Eraser / Teal** remain selected. Reload clears the canvas: only preferences persist. Switch to **Pencil** and draw a teal stroke; switch to **Eraser** and erase part of that new stroke. Switch back to **Pencil** and draw again: it must still be teal.
3. In the browser developer console, paste:

```js
localStorage.setItem('cap.paint.preferences.v1', '{broken'); location.reload();
```

4. Confirm **Pencil / Navy** defaults and that drawing still works.

Report the real browser outcome to the Supervisor. If something fails, use this exact repair routing prompt with the actual symptom filled in:

```text
Browser acceptance failed: [PASTE ACTUAL OBSERVATION]. Delegate a bounded repair
to the owner of the faulty file. Preserve the accepted contract and other files.
Make only one bounded repair attempt; if it still fails, stop and report it.
Then get a separate Reviewer report again. Do not declare acceptance until I
repeat the browser checkpoint and report it passed.
```

## 7. Presenter debrief and stop

- **Best fit:** a small, changing feature where human steering and parallel specialization help.
- **Strength:** one conversational entry point; explicit roles and a separate review pass.
- **Limit:** the Supervisor's context carries scheduling decisions. Worker availability, reports, integration and permission handling still need attention. More agents do not imply faster or better results.
- **On the loop:** the human accepted the contract, intervened during execution, inspected findings, and performed the final visible behavior check.

Quit Codex normally and press `Ctrl+C` in the Python-server terminal. Changes remain local in the fixture's feature branch. No command above pushes anything.

These are complete runnable instructions, not a claim that the agents or browser checks have been executed here. Current CLI/thread behavior: [Codex subagents](https://learn.chatgpt.com/docs/agent-configuration/subagents).

## If you use Claude Code

This is the easiest demo to follow natively with Claude. Set `CAP_DEMO_AGENT=claude` during the prerequisite check, complete the same fixture and server setup, then use this launch command **instead of `codex`** in section 4. The Codex TOML files are unused by Claude; the session-scoped definitions below supply its roles.

```bash
cd "$CAP_SUPERVISOR_DEMO"
claude --version
claude auth status
claude --agents '{
  "ui_worker": {
    "description": "Implements the paint toolbar only.",
    "prompt": "Read AGENTS.md and spec.md. Edit only app/toolbar.js. Honor renderToolbar(preferences), the exact tool/color contract and selected options. Do not edit other files, commit, start servers or spawn agents. Report actual files and unrun checks.",
    "tools": ["Read", "Glob", "Grep", "Edit", "Write"], "background": true
  },
  "storage_worker": {
    "description": "Implements paint preferences storage only.",
    "prompt": "Read AGENTS.md and spec.md. Edit only app/settings.js. Honor loadPreferences(storage), savePreferences(storage, preferences), exact key/fields/defaults and denied storage behavior. Do not edit other files, commit, start servers or spawn agents. Report actual files and unrun checks.",
    "tools": ["Read", "Glob", "Grep", "Edit", "Write"], "background": true
  },
  "integrator": {
    "description": "Connects the completed toolbar and storage.",
    "prompt": "Read AGENTS.md and spec.md. Edit only app/main.js after UI and Storage finish. Preserve drawing and implement event/persistence wiring. Do not edit other files, commit or start servers. Report actual integration and unrun checks.",
    "tools": ["Read", "Glob", "Grep", "Edit", "Write"]
  },
  "reviewer": {
    "description": "Independently reviews the combined paint feature.",
    "prompt": "Read AGENTS.md, spec.md, checks.mjs and app source. Report supported issues with file evidence, including color/eraser persistence and storage failures. Make no edits. You cannot run shell checks: say not run, and have the Human run node checks.mjs and the browser acceptance steps.",
    "tools": ["Read", "Glob", "Grep"]
  }
}'
```

Paste the same planning prompt from section 4 and the GO prompt from section 5. Add: **"Use the four named Claude subagents above. Run UI and Storage in the background so I can keep chatting. The Reviewer is read-only and cannot run commands; I will run the module and browser checks."** Keep ownership, dependency order and the one-repair limit unchanged.

For Claude, omit the GO prompt's instructions to close child threads or free capacity; those describe the Codex thread lifecycle. Use Claude's completion notices and `/tasks` to follow the subagents while preserving the same task order.

Use `/tasks` to inspect running subagents, rather than Codex's `/agent`; return to the main prompt to steer the Supervisor. Run `node checks.mjs` yourself in the fixture terminal, and perform section 6's browser acceptance steps. Permission prompts can still reach the main session; rehearse and approve the actual fixture operations. Do not report concurrent work unless you observe both real Workers.

[Claude custom subagents and session definitions](https://code.claude.com/docs/en/sub-agents), [CLI authentication and flags](https://code.claude.com/docs/en/cli-reference)
