# Demo 2: a native Supervisor with delegated workers

[Shared prerequisites](README.md#shared-prerequisites) · [Slides](presentation.md)

The queue in Demo 1B works through a fixed list. Here, one main chat decides
when to delegate and when to join the results. Native subagents run the
independent UI and Storage tasks in parallel; Integration and Review follow.

## Who owns the conversation and execution?

The main chat remains your place to explain intent, clarify the specification
and judge escalations. Native subagents have separate contexts and return
reports; their implementation detail need not fill the main conversation.
The Supervisor still decides when to launch, join, integrate and request review.
Keeping the user chat available does not itself move those decisions into code.

This is sufficient for small, bounded work. Native CLI runtimes already manage
parts of worker execution; an additional Controller is useful when specific
scheduling or state rules need an implemented owner. The distinction is
execution ownership and state, rather than agent count. See
[Planner and Controller](reference.md#planner-and-controller).

## Real scenarios where you might use this

This paint exercise is small enough for one session; it illustrates the mechanism.

- **Settings release:** Build the settings UI and server permission checks in
  parallel under an agreed interface, then let the main chat judge integration.
- **Browser/server bug:** Investigate each side independently; the main chat
  chooses a fix from the reports and asks a separate worker to review it.

Use one session when the work is small or tightly coupled; the main chat still
judges worker reports and review findings.

## What you are building

A small paint app should remember its tool and color after reload. Two files
can change independently: the toolbar and the preference storage helpers.
Expect four named child agents and a final report from the main Supervisor.

```text
UI Worker ──┐
            ├── Integrator ── Reviewer
Storage ────┘
```

## Set up

Start in your CAP repository root after the shared prerequisites. Setup copies
a small unfinished app and native role definitions into a fresh demo folder.

```bash
mkdir -p .demo-runs/02
cp -R curriculum/weeks/04/scripts/supervisor/fixture/. .demo-runs/02/
cd .demo-runs/02
codex --sandbox workspace-write
```

The checked-in [roles](scripts/supervisor/fixture/.codex/agents/) tell each child
which file it owns. The main chat coordinates the work. No custom controller
script is needed for this demo.

## Give the main chat this prompt

```text
You are the Supervisor. Read spec.md and use the configured native roles.
Delegate ui_worker and storage_worker in parallel. Wait for both actual reports.
Then delegate integrator to connect their modules. After integration finishes,
delegate reviewer to independently review the combined change without edits.
If review finds a defect, send it to the responsible worker and request one
follow-up review. Finish with the changed files, actual check results and any
remaining issues. Do not commit or push.
```

Watch the child agents in Codex's `/subagents` view. The main chat stays available:
you can ask it for progress or add a small clarification while children work.
For example: `Keep the existing canvas size; only add remembered preferences.`

## View the result

Exit the main agent, then reuse the same terminal:

```bash
node checks.mjs
python3 -m http.server 4173 --bind 127.0.0.1 --directory app
```

Open <http://127.0.0.1:4173>. Select Pencil and Teal, reload, then draw a visible
teal stroke. Select Eraser, reload, and confirm erasing still works. Switch back
to Pencil and confirm Teal remains selected. The module checks and separate
review are useful evidence; these browser actions check the integrated app.

Read the changed source files and Reviewer report to see each role's contribution.
Stop the server with Ctrl+C before running the next demo.

## Optional: Claude Code

Claude Code is an alternative native supervisor. Start it in the same fixture:

```bash
claude --agents "$(cat .claude/agents.json)"
```

Use the same prompt and `/tasks` to inspect background workers. These role
instructions and the complete live browser flow have not been rehearsed here;
the Claude alternative is also unverified.

Native role references: [Codex subagents](https://learn.chatgpt.com/docs/agent-configuration/subagents)
and [Claude Code subagents](https://code.claude.com/docs/en/sub-agents).
