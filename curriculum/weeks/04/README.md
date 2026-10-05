# Week 4: Orchestration — Building and Using Harnesses

- [Talk outline and classroom agenda](talk-outline.md)
- [Homework and assessment](homework.md)
- [Agent contracts and harness workflow templates](reference.md)

## Slides and student experiments

- [Orchestration slides](presentation.md)
- [Demo 1: fresh-context programming loop](demo-01-loop.md)
- [Demo 1B: fixed task queue with completion loops](demo-01b-task-queue.md)
- [Demo 2: native supervisor and delegated workers](demo-02-supervisor.md)
- [Demo 3: Planner and external task-graph Controller](demo-03-external-controller.md)
- [Demo 4: native Gas City workspace](demo-04-gas-city.md)

## Choosing how to coordinate

A single interactive session can plan, delegate to native subagents, and
supervise a bounded workflow. The demos add repeated calls, an ordered queue,
native delegation, a program that schedules an explicit graph, and a native
orchestration runtime. Choose by who should own execution and what state and
checks the work needs. The [planner and controller reference](reference.md#planner-and-controller)
explains the two-way conversation and compares the patterns, including their limits.
Persisting the graph and execution state outside agent processes lets planners
be replaced and controllers recover work; shared claiming/result mechanisms can
also support workers on several machines. The reference uses GitHub Issues as
an example, without changing either demo's backend. Demo 3 saves its graph but
keeps completion in memory; Demo 4's Beads graph is durable, while full recovery
and multi-machine execution remain unverified here.

Demos 1–3 use small drawing examples. Demo 4 expands the starter into **Studio
Board**, a local creative workspace delivered in two releases. Native planning
creates each release's implementation graph; roughly 60–80 meaningful tasks per
release is an estimate, below the pinned runtime's 100-member drain limit.
One explicit integration owner assembles and checks each release before the next
starts. A completed isolated task graph is not an integrated application.

## Shared prerequisites

Use an installed, signed-in **Codex CLI or Claude Code CLI**, plus
**Node.js with npm** and **Python 3**. Demos 1, 1B and 4 install test-only Playwright
in their demo folders. Demo 4 also prepares native Gas City dependencies, a local
PyYAML environment and stock artifact checks; its setup limits the city to six
active sessions and the implementation role to four. The expanded two-release
run has not been rehearsed end to end; do not promise an elapsed time.
If the runtimes are missing on macOS with Homebrew:

```bash
brew install node python
```

From the CAP checkout, prepare the shared location once:

```bash
mkdir -p .demo-runs
grep -qxF '/.demo-runs/' .git/info/exclude || echo '/.demo-runs/' >> .git/info/exclude
```

Run one demo at a time. Each guide copies its files into a fixed folder under
`.demo-runs/` and includes setup and run commands. Start each guide from your CAP
repository root. Demo 3 uses port `4303`; the other demos use `4173` to view the app.
After stopping a demo, delete only its numbered `.demo-runs/` folder and repeat
setup for a clean start.

### Using Claude Code

Use an installed, signed-in Claude Code CLI for the native-agent demo. For Demo 3,
change `AGENT` to `"claude"` in `controller.py`; follow its guide. For Demo 1 or
1B, replace the entire `codex exec` invocation, including all its options and
final `-`, with:

```bash
claude -p --permission-mode dontAsk --allowedTools Read,Edit,Write,Glob,Grep
```

Keep its input pipe or `<` redirect, `2>&1`, and `tee` unchanged. Each call starts
a fresh conversation.
This replacement follows the [official CLI documentation](https://code.claude.com/docs/en/headless)
and has not been rehearsed here. Native CLI permissions still apply.
