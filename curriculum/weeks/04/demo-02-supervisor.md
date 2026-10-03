# Demo 2 — Chat with a Supervisor while its Workers run

## What you are building

The starting program is a small browser paint app. The baseline draws with a **Pencil / Navy** and has no working preference controls. Your goal is to add **Pencil / Eraser** and three-color controls, keep **Pencil / Teal** or **Eraser / Teal** after reload, and recover safely if saved data is broken.

You will ask one main **Supervisor** conversation to coordinate four native agents. UI and Storage Workers edit separate files at the same time. An Integrator waits for both reports and connects their work. A separate Reviewer checks the combined result. The Supervisor decides when those steps start, and you can keep chatting with it while the Workers run.

The setup script creates an independent disposable copy of the app, its accepted specification, checks, and named agent roles. It does not launch the Workers. You will brainstorm the graph, approve execution, steer the work, and check the final app yourself.

## What you should see

- **Main terminal:** a short plan, two real Worker threads, their reports, then Integration and Review. Use `/agent` in Codex or `/tasks` in Claude to inspect progress and return to the main chat.
- **Files:** UI changes `app/toolbar.js`, Storage changes `app/settings.js`, and Integration changes `app/main.js`. `spec.md` is their shared contract. `git diff -- app` shows what actually changed.
- **Browser:** keep the app open on port **4174**. After the agents finish, your choices should survive a reload; drawing and erasing should still work.
- **Evidence:** `node checks.mjs` checks the modules. Your browser checkpoints check the user experience. A Review report alone does not mean either check passed.

A successful run has overlapping UI and Storage work, an Integrator that waits for both, a separate Review, passing module checks, and the browser behavior above. If a step fails, the Supervisor should name the responsible Worker and route one bounded repair.

## 1. Install the prerequisites

Assume Codex CLI or Claude Code is installed. You also need **Node 20+**, npm, **Python 3.10+**, and **Git 2.28+**. No Playwright or application packages are required.

On macOS with Homebrew, install any missing tools:

```bash
brew install node@24 python git
export PATH="$(brew --prefix node@24)/bin:$(brew --prefix)/bin:$PATH"
```

If Homebrew is missing, follow [its official installation instructions](https://brew.sh/), including the printed shell setup. Alternatively, use the [Node installer](https://nodejs.org/en/download), [Python installer](https://www.python.org/downloads/macos/) and [Git installation guide](https://git-scm.com/install/mac). Repeat the PATH command in new terminals if needed.

Sign in using your normal account if necessary:

```bash
codex login
```

For Claude, use `claude auth login` instead. The setup checks the selected CLI's version and authentication without starting a model job. Authentication does not establish quota or permissions; the exercise checks actual delegation when you launch Workers.

## 2. Prepare a fresh isolated fixture

Open a terminal in the **CAP curriculum checkout**, where `curriculum/weeks/04` exists. This is different from the disposable app repository that setup will print. If you are at the root of a printed `.demo-runs/supervisor-*` fixture, `cd ../..` returns to CAP; derive `CAP_ROOT` only after returning. Then copy:

```bash
export CAP_ROOT="$(git rev-parse --show-toplevel)"
ls "$CAP_ROOT/curriculum/weeks/04/scripts/supervisor/setup.sh" && \
export CAP_SUPERVISOR_DEMO="$CAP_ROOT/.demo-runs/supervisor-$(date +%Y%m%d-%H%M%S)" && \
bash "$CAP_ROOT/curriculum/weeks/04/scripts/supervisor/setup.sh" "$CAP_SUPERVISOR_DEMO" && \
cd "$CAP_SUPERVISOR_DEMO"
```

### If you use Claude Code

Add `--agent claude` before the path in the setup command. This installs Claude's native role definitions instead of Codex's.

If `ls` reports the setup script is missing, stop: your terminal is not at the CAP curriculum checkout. Return there and repeat this block. Continue only after **`PASS: independent fixture ready`**. The output must show your fixture path, branch `feature/paint-preferences`, and **Remotes: none**. If setup fails, resolve the printed error and choose a fresh path.

[Setup script](scripts/supervisor/setup.sh) contains the preparation steps and comments. [Fixture assets](scripts/supervisor/fixture/spec.md) contain the accepted specification, app, module checks, and native agent definitions. The baseline intentionally lacks working preference persistence.

**Git isolation:** the setup preserves CAP's current branch and adds `/.demo-runs/` to CAP's local `.git/info/exclude`. Your fixture has its own repository and feature branch, no remote, and a local baseline commit. Only its disposable repository gets a demo Git identity and signing disabled. No command pushes anything.

Setup prints copyable `export CAP_ROOT=...` and `export CAP_SUPERVISOR_DEMO=...` commands with your actual paths. Paste **both printed exports into every new terminal** before using this guide's commands. Each demo uses its own folder and server port.

## 3. Open the baseline app

In a second terminal, paste both export commands printed by setup, then run:

```bash
cd "$CAP_SUPERVISOR_DEMO"
python3 -m http.server 4174 --bind 127.0.0.1 --directory app
```

Open <http://127.0.0.1:4174>. The baseline shows only **Pencil / Navy**. Draw one navy stroke; preference persistence and its checks are expected to fail until the Workers implement them. Leave the server running while you use the first terminal for the main conversation.

## 4. Brainstorm and approve the task graph

In the first terminal:

```bash
codex --sandbox workspace-write
```

For Claude, use this command instead:

```bash
claude --agents "$(cat .claude/agents.json)"
```

Review any project trust prompt before proceeding. Paste this into the main conversation:

```text
You are the Supervisor for this classroom fixture. Read AGENTS.md and spec.md.
Let's briefly reason about remembering paint preferences. Give one concrete
example of Pencil/Teal after reload and a separate Eraser persistence example.
Show the accepted task graph and why UI and Storage can run concurrently.
Do not edit files or launch Workers until I say GO. Keep the response short.
```

**Checkpoint:** UI owns `app/toolbar.js`; Storage owns `app/settings.js`; Integration owns `app/main.js`. Both Workers must finish before Integration; Review follows Integration. Correct mistakes in chat before starting.

## 5. Fan out, then intervene in the same chat

Paste:

```text
GO. Spawn ui_worker and storage_worker concurrently using the named native
roles. Give each its owned file and accepted spec. Do not do their edits in
the main session. Keep other files fixed. Show the real launched threads.
Wait for both actual reports before delegating app/main.js to integrator.
After Integration's actual report, spawn reviewer separately. For Codex,
close completed child threads when needed to free capacity. Do not commit
or publish. Route one bounded repair to the responsible owner if a check
fails, then review again. If that repair fails, stop and report the failure.
```

For Claude, add:

```text
Run UI and Storage in the background so I can keep chatting. Use the four
named Claude subagents. The Reviewer cannot run commands; I will run the
checks. Omit the Codex instructions about closing threads and freeing capacity.
```

While both Workers are active, paste:

```text
Steering update: keep our three-color palette. Do not add a color picker or
more tools. Preserve the saved color when Eraser is selected. Tell me which
Workers are actually running and pass this constraint to the affected Worker.
Keep going with the accepted graph.
```

Use **`/agent`** in Codex or **`/tasks`** in Claude to inspect actual Workers, then return to the main conversation. Show one task, its owned file, and its actual report.

**Checkpoint:** two real Workers overlap in time and edit separate files. No integration starts before their reports. If delegation is skipped, say: `Use the named native agents now; show actual child threads rather than simulated role messages.`

## 6. Review the combined result

After Integration and Reviewer report, paste:

```text
Summarize actual UI, Storage and Integration handoffs, plus independent
Reviewer findings. Include commands run and real results; name unrun checks.
Module checks do not establish browser acceptance. Give me the remaining
browser steps. If blocked, identify the owner and concrete next action.
```

In a terminal inside the fixture:

```bash
node checks.mjs
git diff --stat
git diff -- app
```

Successful checks start **`PASS: module defaults...`**. Route failures to the relevant owner.

Then check in the browser:

1. Choose **Pencil / Teal**, reload, confirm both choices remain, and draw a visible teal stroke.
2. Choose **Eraser / Teal**, reload, and confirm both remain. The canvas is cleared by reload. Switch to Pencil and draw a teal stroke; switch to Eraser and erase part; switch back to Pencil and draw teal again.
3. In the browser developer console, run `localStorage.setItem('cap.paint.preferences.v1', '{broken'); location.reload();`.
4. Confirm **Pencil / Navy** defaults and working drawing.

Report the actual browser outcome in the main chat. For a failure, paste:

```text
Browser acceptance failed: [PASTE ACTUAL OBSERVATION]. Delegate one bounded
repair to the faulty file's owner, preserving the accepted contract and other
files. Get a separate Reviewer report. Stop if the repair fails. Acceptance
remains pending until I repeat the browser check and report it passed.
```

## 7. Explain the pattern and stop

**Explain what you observed:** “The Supervisor is an LLM session. It chooses when to launch Workers, wait, integrate and review. I can steer it during the work. A larger task graph puts more coordination into that conversation.”

Best fit: a small feature with changing requirements and a human nearby. Native roles help separate work and review; thread capacity, permissions, and handoffs still need attention.

Quit the agent normally. Press **Ctrl+C** in the server terminal. All edits remain in the isolated fixture.

## Ask an agent to prepare a similar demo

Copy this prompt into your normal CLI conversation:

```text
Prepare a small native multi-agent demonstration for a paint app that remembers
its tool and color after reload. Put it in a fresh independent Git repository
on a feature branch with no remote; keep my source checkout untouched. Create
one accepted spec and exact shared module interfaces. Define native UI and
Storage Workers with separate files, an Integrator that waits for both real
reports, and a read-only Reviewer after integration. Set enough thread capacity
and close completed Codex threads when needed. Keep the main Supervisor chat
available for steering. Include a runnable baseline, module checks, and exact
human browser acceptance steps for Pencil/Teal, Eraser and malformed storage.
Put setup implementation in actual scripts and assets. Give me instructions
and copyable launch, steering and review prompts. Do not run agents or publish
until I approve the concrete setup.
```

## What has been checked

The setup script and extracted assets have syntax and static parity checks. Native model jobs and browser acceptance are **not claimed as rehearsed here**. Complete the Worker and browser checkpoints above to verify them on your account. Ownership is a cooperation contract; the Reviewer's read-only permissions depend on the parent runtime.

Official references: [Codex subagents and custom roles](https://learn.chatgpt.com/docs/agent-configuration/subagents), [Codex authentication](https://learn.chatgpt.com/docs/auth), [Claude subagents](https://code.claude.com/docs/en/sub-agents), [Claude CLI](https://code.claude.com/docs/en/cli-reference).
