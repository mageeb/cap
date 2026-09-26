# Week 03 Reference — CLI Agents, Hooks, Subagents, Specs, Skills & Worktrees

Prepared against public docs/repositories checked **September 25, 2026**. Tool behavior changes quickly; use the linked official documentation as the source of truth for installation/configuration details.

---

# 1. CLI agent mental model

A terminal coding agent combines:

1. a language/reasoning model;
2. tools such as file read/write, shell, Git, web/MCP integrations;
3. a loop that chooses actions, observes results, and continues;
4. context/instructions that define the task and project constraints.

The terminal itself is not the intelligence. Its value is being a common interface to the engineering environment.

## Chat vs. CLI agent

A chat can still be excellent for explanation, comparing approaches, design discussion, or reviewing pasted artifacts. A CLI agent becomes especially useful when the task needs repeated interaction with the live repository and tools.

Do not turn this into a false binary: developers often use CLI, IDE, desktop/web chat, and specialized agents together.

## Coding harness, model, and CLI

A coding harness is the software around a model that manages its work in a development environment. It assembles context, calls the model, checks and executes requested tools, feeds results back, and maintains session state. The model reasons and proposes text or tool calls; the harness determines which tools are available and how their execution is controlled.

The CLI is the terminal interface to this system. Products such as Claude Code and Codex combine that interface with agent software; graphical interfaces can use similar runtimes. Changing the harness, instructions, or tools can change outcomes even when the model stays the same.

See the [conceptual diagram](assets/coding-harness.svg) and its [Mermaid source](assets/coding-harness.mmd). The diagram describes responsibilities rather than exact product internals. Further reading: [Anthropic's harness engineering article](https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents).

## Model Context Protocol (MCP)

MCP is an open protocol that gives an AI application a standard way to connect to external systems. The **host** (for example, Claude Code or Codex) contains an MCP client; an **MCP server** exposes capabilities such as tools, resources, and prompt templates. A server may run as a local process (stdio) or be reached remotely over HTTP.

- **Tools** let the model ask the host to perform an operation or retrieve data.
- **Resources** provide information for the model's context.
- **Prompts** provide reusable server-authored templates where supported.

This makes service integrations reusable across compatible AI applications. Examples include documentation search, GitHub issues and pull requests, Figma designs, and monitoring systems. MCP connects tools/context; a skill provides reusable instructions for the agent.

### Connect a server in the CLI

The slide uses the read-only OpenAI developer documentation server. Choose the command for your CLI, then inspect its status:

```bash
# Claude Code
claude mcp add --transport http openai-docs https://developers.openai.com/mcp
claude mcp list

# Codex CLI
codex mcp add openaiDeveloperDocs --url https://developers.openai.com/mcp
codex mcp list
```

Start the CLI and use `/mcp` to inspect the connected tools. Ask a bounded task that needs those docs. For servers that require authentication, follow their documented sign-in step. Review trust, permissions, and available operations; a server can expose write-capable tools.

Sources: [MCP introduction](https://modelcontextprotocol.io/introduction), [Claude Code MCP](https://code.claude.com/docs/en/mcp), and [Codex MCP](https://learn.chatgpt.com/docs/extend/mcp).

---

## Planning mode and planning in chat

Plan mode organizes investigation and a proposed approach before implementation. Review scope, assumptions, affected files, and verification steps before proceeding. It is separate from model choice and reasoning effort.

- **Claude Code:** cycle with `Shift+Tab` until Plan appears, use `/plan`, or launch `claude --permission-mode plan`. Source edits are normally blocked until plan approval; plan-file writes and permitted exploration commands can still occur. Permission and bypass settings matter, so this is not an OS-level read-only sandbox.
- **Codex CLI:** use `/plan`, optionally followed by the task. Review the proposed approach before moving to implementation; do not assume Claude's permission semantics apply unchanged.
- **Chat:** asking for a plan works conversationally. Without repository tools, the plan depends on material supplied in the conversation. Dedicated Plan modes also exist in agent interfaces outside the CLI, including Claude Code desktop's Code tab and its web interface. Compare actual repository access, tools, and approval controls rather than terminal versus graphical UI.

Sources: [Claude Plan mode and interface controls](https://code.claude.com/docs/en/permission-modes), [Codex commands](https://learn.chatgpt.com/docs/developer-commands?surface=cli), [planning with local project context](https://learn.chatgpt.com/docs/prompting).

### Compare approaches with built-in capabilities

This demo step uses **Plan mode and tools**, with no additional skill installation:

1. **Enter Plan mode:** in Claude Code, press `Shift+Tab` until Plan appears, or launch `claude --permission-mode plan`. In Codex CLI, enter `/plan`.
2. **Gather evidence:** ask the agent to find relevant files and read existing implementations. Claude uses `Read` for file contents and `Bash` for search commands. Dedicated `Glob`/`Grep` tools are available in some configurations; the current defaults on macOS/Linux/WSL use shell search instead. Codex can use its shell tools for read-only commands such as `rg` and `sed` when those utilities are installed. The agent invokes these tools in response to your prompt; they are not slash commands to type.
3. **Compare:** request 2–3 approaches, citing files/symbols and weighing reuse, complexity, UX risks, affected files, and tests. Choose an approach before any implementation.
4. **Allow document edits:** leave Plan mode using the CLI's mode control. In Claude, cycle `Shift+Tab` to the normal permission mode; in Codex, use the mode switch shown in the CLI. Confirm that Plan is no longer active. Request: “Create only GOAL.md and SPEC.md for the chosen approach. Do not change application code.” Avoid accepting a broad implementation proposal merely to save these documents.

The later implementation plan and tasks still require review before code changes. A documentation-only prompt limits the requested scope; ordinary tool permissions still govern writes.

Tool reference: [Claude Code built-in tools](https://code.claude.com/docs/en/tools-reference). Mode controls: the official CLI documentation linked above.

---

## YOLO and permission bypass

“YOLO” (“You Only Live Once”) is slang for running an agent with execution safeguards bypassed. People use it to reduce approval interruptions during long tasks, batch edits, or repeated test/fix cycles.

| Tool | Example | Meaning |
|---|---|---|
| Claude Code | `claude --dangerously-skip-permissions` | Selects `bypassPermissions`; routine permission checks are skipped, while explicit deny/ask rules and some other checks remain |
| Codex CLI | `codex --yolo` | Alias for `--dangerously-bypass-approvals-and-sandbox`; skips approval prompts and the Codex sandbox |

Approval policy and sandboxing are separate controls. For example, Codex `--ask-for-approval never` can retain a configured sandbox; it is not itself permission for unrestricted execution. Neither bypass flag grants OS administrator privileges or removes an external VM/container boundary.

Prefer scoped permissions and sandboxing. If bypass is necessary, use an isolated, disposable environment with limited mounts, credentials, and network access. A worktree isolates files, not process privileges. Verification still matters; Git cannot undo external actions. These examples explain the modes and are not required setup steps.

Sources: [Claude bypass permissions](https://code.claude.com/docs/en/permission-modes#skip-all-checks-with-bypasspermissions-mode), [Codex approvals and sandboxing](https://learn.chatgpt.com/docs/agent-approvals-security).

---

# 2. Claude Code quick reference

Official setup: https://code.claude.com/docs/en/setup

## Install

macOS/Linux/WSL native installer:

```bash
curl -fsSL https://claude.ai/install.sh | bash
```

macOS Homebrew:

```bash
brew install --cask claude-code
```

Windows PowerShell:

```powershell
irm https://claude.ai/install.ps1 | iex
```

Windows WinGet:

```powershell
winget install Anthropic.ClaudeCode
```

Verify:

```bash
claude --version
claude doctor
```

Start/authenticate:

```bash
claude
```

As of the checked documentation, native Windows is supported; Git for Windows is recommended so Claude can use Bash tooling, otherwise PowerShell can be used.

## Resume and select a model

From the same repository/worktree, run `claude --continue` for the latest conversation, `claude --resume` for a picker, or `claude --resume SESSION_ID` for a specific conversation. Inside Claude, `/resume` opens the picker.

Use `/model` to choose an available model and `/effort` to adjust reasoning effort. At startup, for example: `claude --model sonnet --effort high`. The alias follows the provider's model mapping; access and supported effort levels vary. Higher effort can increase time and token usage; evaluate the resulting work.

Sources: [Session resumption](https://code.claude.com/docs/en/common-workflows#resume-previous-conversations), [model and effort controls](https://code.claude.com/docs/en/model-config).

## Project customization map

Official directory reference: https://code.claude.com/docs/en/claude-directory

Common files:

```text
CLAUDE.md                         project guidance loaded each session
.claude/settings.json             project settings, permissions, hooks
.claude/settings.local.json       local-only project settings
.claude/skills/<name>/SKILL.md    reusable skills
.claude/agents/<name>.md          custom subagents
.mcp.json                         project MCP servers
.worktreeinclude                  gitignored files copied into new worktrees
```

A useful principle from Claude’s docs: keep always-loaded project instructions concise and move occasional/reusable workflows into skills.

---

# 3. Codex CLI quick reference

Official repository: https://github.com/openai/codex

## Install

macOS/Linux:

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
```

Windows PowerShell:

```powershell
powershell -ExecutionPolicy ByPass -c "irm https://chatgpt.com/codex/install.ps1 | iex"
```

Alternatives:

```bash
npm install -g @openai/codex
brew install --cask codex
```

Start:

```bash
codex
```

Sign in with ChatGPT when prompted, or use the API-key path documented by OpenAI.

## Resume and select a model

From the same repository/worktree, run `codex resume --last` for the latest conversation, `codex resume` for a picker, or `codex resume SESSION_ID` for a specific conversation. The picker and `--last` filter by directory by default; add `--all` to search across directories. Inside Codex, `/resume` opens the picker.

Use `/model` to select a model and its reasoning effort when supported. At startup, `codex --model MODEL_ID` selects a model; replace `MODEL_ID` with an available identifier. `/status` shows the active configuration.

For either CLI, resuming a conversation does not restore a Git snapshot or restart application processes. Check the branch, working-tree changes, and saved task state. To change tools or begin with fresh context, read the project's saved artifacts instead.

Source: [Codex CLI commands](https://learn.chatgpt.com/docs/developer-commands?surface=cli). Startup flags also checked with local `codex --help` and `codex resume --help`.

## Project guidance

Codex uses `AGENTS.md` as repository/project instructions. Keep rules concrete: build/test commands, conventions, architecture boundaries, and important “never do X” constraints.

## Skills and plugins

A **skill** is one reusable procedure centered on a `SKILL.md` file, with optional scripts, references, and templates. A **plugin** packages capabilities for installation and updates through a marketplace/catalog. Contents depend on the host and plugin format.

- **Claude Code plugins** can bundle skills, commands, subagents, hooks, and MCP servers.
- **Codex plugins** can bundle skills, MCP configuration, and lifecycle hooks; do not assume every Claude plugin component has a Codex equivalent.

### Install the OpenAI Developers example

In Claude Code, add its marketplace and install the plugin:

```text
/plugin marketplace add openai/openai-developers-for-claude
/plugin install openai-developers@openai-developers
```

In Codex CLI, start `codex`, enter `/plugins`, find **OpenAI Developers**, then choose **Install plugin**. The two variants differ: Claude Code's plugin includes developer skills and the OpenAI Docs MCP server; Codex's plugin connects OpenAI Platform tools, while the OpenAI Docs skill is bundled with Codex.

Sources: [Claude Code plugins](https://code.claude.com/docs/en/plugins), [OpenAI Developers plugin](https://developers.openai.com/learn/developers-codex-plugin), [OpenAI plugin architecture](https://developers.openai.com/plugins/concepts/plugins), and [package your plugin](https://developers.openai.com/plugins/build/plugins).

Do not assume Claude’s `.claude/settings.json` hook syntax or skill placement is interchangeable with Codex configuration.

---

# 4. Hooks

Claude Code hooks reference: https://code.claude.com/docs/en/hooks

A hook executes when a lifecycle event occurs. Current Claude events include many stages such as:

- `SessionStart` / `SessionEnd`;
- `UserPromptSubmit`;
- `PreToolUse` / `PostToolUse` / failure/permission events;
- `SubagentStart` / `SubagentStop`;
- `TaskCreated` / `TaskCompleted`;
- `PreCompact` / `PostCompact`;
- `WorktreeCreate` / `WorktreeRemove`;
- configuration/file-change events.

Students do **not** need to memorize them. The useful concept is:

> event → optional matcher → deterministic handler

### Simple project hook example

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "\"${CLAUDE_PROJECT_DIR}\"/.claude/hooks/lint-check.sh"
          }
        ]
      }
    ]
  }
}
```

Claude supports command, HTTP, MCP-tool, prompt, and agent hook handlers in current docs.

## Hook safety

Hooks are code execution. Treat a repository-provided hook like any other script you might run:

- review it before trusting the workspace;
- sanitize/quote input;
- avoid exposing secrets;
- use absolute/project-root-aware paths;
- keep frequent hooks fast;
- make failures visible;
- schedule expensive checks at deliberate verification steps.

---

# 5. Agents and subagents

Claude subagent docs: https://code.claude.com/docs/en/sub-agents

A subagent is useful for a focused task that benefits from separate context or tools.

Good examples:

- codebase exploration that reads many files;
- code review after implementation;
- security review;
- test-failure investigation;
- independent research that returns a concise evidence summary.

Poor examples:

- tiny edits where delegation costs more than the work;
- two agents concurrently editing the same uncommitted files;
- vague “solve the project” delegation with no boundaries.

## Context isolation

Separate context can keep large file reads/logs out of the primary conversation. It does **not** guarantee correctness. The main agent should request references/evidence and verify important conclusions.

---

# 6. Skills

Claude skills: https://code.claude.com/docs/en/skills
OpenAI plugin skills: https://developers.openai.com/plugins/concepts/skills

A skill is a reusable procedure. Good skills define:

- when to use them;
- required inputs;
- ordered steps/review points;
- what evidence to gather;
- expected output.

Comparing implementation approaches uses [built-in Plan mode and repository exploration tools](#compare-approaches-with-built-in-capabilities). A mode controls workflow, a tool performs an operation, and a skill supplies reusable instructions. The demo's comparison step requires no custom skill.

---

# 7. Spec-driven development

There is no requirement that every project use the exact filenames below. The teaching pattern is separation of concerns:

```text
GOAL.md            concise finish line / boundaries
SPEC.md            observable behavior and requirements
PLAN.md            repository-specific implementation approach
TASKS.md           progress checklist
PROJECT_CONTEXT.md durable current state + evidence + next step
DECISIONS.md       optional decision log for larger work
```

## Why explore before planning?

Without codebase evidence, an AI plan can be internally coherent and still be wrong for the repository. Exploration should identify existing primitives, conventions, tests, and architecture before generating a plan.

## What not to persist

Avoid dumping every conversation turn into project context. Store:

- decisions and rationale;
- current state;
- verified evidence;
- important file/symbol references;
- risks;
- next step.

---

# 8. Git worktrees

Git documentation: https://git-scm.com/docs/git-worktree

A worktree lets one repository have multiple checked-out branches in separate directories.

Create:

```bash
git worktree add ../excalidraw-focus -b feature/focus-mode master
```

List:

```bash
git worktree list
```

Remove:

```bash
git worktree remove ../excalidraw-focus
```

Prune stale metadata if necessary:

```bash
git worktree prune
```

## Why agents benefit

Two editing agents working in one directory can overwrite or confuse each other’s changes. Separate worktrees provide filesystem and branch isolation.

Still avoid unnecessary parallelism. Integration conflicts can occur when branches modify the same code.

---

# 9. GitHub auth / PR workflow for the course

GitHub CLI docs:

- https://cli.github.com/manual/gh_auth_login
- https://cli.github.com/manual/gh_auth_setup-git

Authenticate:

```bash
gh auth login
gh auth setup-git
gh auth status
```

Course fork model:

```text
origin    → YOUR_USERNAME/excalidraw
upstream  → excalidraw/excalidraw
```

Create a PR explicitly against your fork:

```bash
gh pr create \
  --repo YOUR_USERNAME/excalidraw \
  --base master \
  --head feature/my-feature \
  --title "Demo: ..." \
  --body-file docs/ai/PR_BODY.md
```

Never use the course to generate unsolicited upstream PRs to Excalidraw.

---

# 10. Excalidraw development facts used by the demo

Official development guide:
https://github.com/excalidraw/excalidraw/blob/master/dev-docs/docs/introduction/development.mdx

At the time of preparation:

- prerequisites: Node.js, Yarn, Git;
- root `package.json`: Node `>=18.0.0`, Yarn `1.22.22`;
- install: `yarn`;
- run: `yarn start`;
- local URL: `http://localhost:3000`;
- no collaboration server is required for ordinary local client work;
- Docker is available but not needed for this class.

Useful root commands in the checked repository:

```bash
yarn start
yarn test
yarn test:typecheck
yarn test:code
yarn fix
```

## Existing concepts relevant to Focus Mode

Repository search confirms existing:

- Zen Mode (`zenModeEnabled`, toggle action/UI);
- View Mode (`viewModeEnabled`, toggle action/UI);
- zoom-to-fit / zoom-to-selection actions;
- canvas renderer and Layer UI patterns.

This is why the demo’s Selection Focus Mode must be explicitly different from Zen/View Mode.

## Existing concepts relevant to Presentation Mode

Repository search confirms:

- Excalidraw frame elements/helpers;
- viewport fitting actions/helpers;
- a presentation icon and a current Excalidraw+ presentation promotion in `excalidraw-app/components/AppSidebar.tsx`.

The class feature is therefore described as a **demo-only frame walkthrough prototype**, not an upstream product proposal.

---

# 11. Benchmarks

## SWE-bench

SWE-bench tasks are based on real software repository issues and patches. Agent performance typically depends on understanding the repository, changing code, and satisfying tests.

### Important 2026 caveat

OpenAI published **“Why SWE-bench Verified no longer measures frontier coding capabilities”** on Feb. 23, 2026, reporting increasing contamination and recommending a move away from Verified:

https://openai.com/index/why-we-no-longer-evaluate-swe-bench-verified/

Then on July 8, 2026, OpenAI reported an audit finding widespread task problems in SWE-Bench Pro as well, estimating roughly 30% of tasks were broken:

https://openai.com/index/separating-signal-from-noise-coding-evaluations/

Teaching point: benchmarks must themselves be evaluated.

## Terminal-Bench

Terminal-Bench is designed around agent performance in terminal environments and is useful for measuring multi-step command-line problem solving. Performance reflects the **agent system**, not only model weights.

## Example comparison table

Meta’s Muse Spark 1.3 model page currently publishes one same-table comparison among Muse Spark 1.3 (max), GPT-5.6 Sol (max), and Anthropic Opus 5 (max):

https://dev.meta.ai/models/muse-spark

Values shown on that page when checked:

| Benchmark | Muse Spark 1.3 | GPT-5.6 Sol | Opus 5 |
|---|---:|---:|---:|
| DeepSWE v1.1 | 75.4 | 73.0 | 74.0 |
| SWEAtlas CodeBase QnA | 59.4 | 53.5 | 52.7 |
| Terminal-Bench 2.1 | 88.8 | 88.8 | 86.7 |

**Interpretation warning:** this is a vendor-published comparison table, not an independent ranking. Exact harness/model settings matter. OpenAI’s own GPT-5.6 release page independently reports GPT-5.6 Sol at 88.8 on Terminal-Bench 2.1, while other benchmark numbers can differ slightly because evaluation setups differ:

https://openai.com/index/gpt-5-6/

---

# 12. Boris Cherny / Claude Code origin and workflow

## Role

Anthropic currently identifies Boris Cherny as **Head of Claude Code**; Anthropic pages also describe him as Claude Code’s inventor/creator.

Official Anthropic origin webinar:

https://www.anthropic.com/webinars/claude-code-live

## Origin story

In the Feb. 17, 2026 Y Combinator interview, Cherny describes the origin as accidental. He wanted to understand Anthropic’s API, created a very small terminal chat app, then gradually gave it tools. He also says the terminal was supposed to be the **starting point**, not necessarily the ending point.

Video:

https://www.youtube.com/watch?v=PQU9o_5rHC4

Chapter markers from the published video description include:

- 02:38 — how he came up with the idea;
- 05:38 — simplicity of terminals;
- 09:00 — his `CLAUDE.md`;
- 23:48 — subagents.

A transcript mirror captures his description that no one asked him specifically to build a CLI: he first built a small terminal chat app to learn the API, then expanded its capabilities.

## “Build for the model six months from now”

Cherny has repeatedly described an Anthropic approach of building for expected future model capabilities. Early Claude Code versions reportedly wrote only a minority of his code; the product became much more useful as later models improved.

A detailed engineering interview on the origin/build process:

https://newsletter.pragmaticengineer.com/p/how-claude-code-is-built

## Reported personal workflow

InfoQ summarized Cherny’s Jan. 2026 public workflow as roughly:

- five local terminal sessions;
- five to ten web sessions;
- separate Git checkouts for local sessions;
- frequent parallel work and handoff;
- some sessions abandoned when they go in an unhelpful direction.

Source:

https://www.infoq.com/news/2026/01/claude-code-creator-workflow/

Do not turn this into a rule that every developer should run 10+ agents. The useful lesson is **cheap isolation + reviewable outputs + willingness to discard bad work**.

## Sequoia AI Ascent 2026

User-provided video:

https://www.youtube.com/watch?v=SlGRN8jh2RI

Public descriptions identify it as a 2026 Sequoia AI Ascent conversation in which Cherny discusses the future of coding, high agent parallelism, and “loops.” Treat productivity numbers and forward-looking claims as his perspective, not universal guarantees.

## Follow

X: https://x.com/bcherny

## Reusable Claude Code workflows from Boris Cherny

These examples combine built-in Claude Code features with personal skills. The current [Claude Code command reference](https://code.claude.com/docs/en/commands) classifies `/simplify`, `/batch`, and `/loop` as bundled skills, and `/goal` as a built-in command. It describes `/simplify` as a parallel cleanup review (not a correctness review) and `/batch` as an orchestrator that splits large changes into worktree-isolated units. `/loop` reruns a prompt while the session remains open.

Cherny’s personal workflows include `/commit-push-pr` (commit, push, open a PR; reportedly used daily for each change), `/go` (end-to-end test, simplify, then open a PR), and `/babysit` (a recurring PR follow-up workflow that responds to review comments, rebases, and pushes). One reported example combines the bundled `/loop` with `/babysit`: `/loop 5m /babysit`. The availability and behavior of these personal skills depend on the user's setup; do not present them as Claude Code built-ins.

`/simplify` targets reuse and cleanup opportunities, not correctness bugs; review bug findings separately.

Sources: [Claude Code command reference](https://code.claude.com/docs/en/commands), [Boris Cherny’s public workflow posts](https://x.com/bcherny), and the [Y Combinator interview](https://www.youtube.com/watch?v=PQU9o_5rHC4). The slide’s description of his exact personal routines follows the examples supplied for this lesson; built-in classifications and feature behavior follow current official documentation.

---

# 13. Instructor cautions

## Do not overclaim “deterministic”

A hook trigger can be deterministic relative to its configured event, but the script it runs may still fail, be environment-dependent, or contain nondeterministic behavior.

## Do not claim tests prove correctness

Tests provide evidence about covered behavior. They do not prove the absence of defects.

## Do not imply a benchmark is a model ranking

Benchmark results depend on benchmark version, model configuration, harness, tools, attempts, and task validity.

## Do not treat saved context as truth

A Markdown context file can become stale. A fresh agent should reconcile it with Git status/diff and the actual code.

## Do not let parallel agents share one dirty working tree

Use worktrees/branches or explicitly sequence edits.

## Debug live-demo failures with the class

Keep the failing output visible. Reproduce the issue, identify the unmet criterion, inspect the relevant code, and agree on one small fix. Rerun a focused check and update the task list with the result. Students follow the same investigation on their own machines.
