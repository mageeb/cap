# Demo A — Excalidraw Selection Focus Mode

Follow the instructor one step at a time in your own Excalidraw fork. Each step explains **what we are doing**, **what to run or paste**, and **what you should see**. Run commands individually and read the result before continuing. If something fails, we debug it together.

The terminal examples use macOS and `~/repos/excalidraw`; `~` means your home folder. Use your actual clone path if different. Windows installation commands are in [pre-class setup](pre-class-setup.md). Replace every `YOUR_USERNAME` with your GitHub username. Code blocks labeled **Terminal** go at the shell prompt; blocks labeled **Claude** go inside the agent session. No custom skills are required.

If your fork is already cloned, dependencies are installed, and the canvas works, keep its server running and jump to [step 5](#5-create-your-feature-branch). Otherwise, start below. We build the feature live together.

<!-- Walkthrough organization: [DEC-015](../../../DECISIONS.md#dec-015-make-both-demo-runbooks-usable-one-step-at-a-time). -->

## What we are building

Select one or more canvas elements, fit that selection into the viewport, and visually de-emphasize unrelated elements. Provide a visible Exit Focus control and keyboard exit. Exiting restores normal interaction and visual treatment; existing Zen Mode and View Mode still work. Do not change persisted opacity or scene data. Collaboration additions and reload persistence are out of scope.

## 0. Get the tools ready

**What we are doing:** Check that this terminal can find Git, GitHub CLI, Node.js, npm, and Claude Code.

**Terminal:**

```bash
git --version
gh --version
node --version
npm --version
claude --version
```

**You should see:** A version for each tool. If one is missing, install it using [pre-class setup](pre-class-setup.md). Choose one AI CLI; Claude Code is the primary route below.

**If `brew` is missing on macOS:** Install [Homebrew](https://brew.sh/) with its official command:

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Follow the installer prompts, then run the commands printed under **Next steps** to add Homebrew to your PATH. If asked for a password, use your Mac login password; typing it shows no characters. Verify with `brew --version`. If GitHub CLI is missing, run:

```bash
brew install gh
```

Now connect this terminal to your GitHub account:

```bash
gh auth login --hostname github.com --git-protocol https --web
gh auth setup-git
gh auth status
```

Follow the browser sign-in instructions. `gh auth status` should identify your account. `gh auth setup-git` lets Git use that authentication when pushing; GitHub account passwords do not work as Git HTTPS passwords. If already signed in to the correct account, skip login and run the other two commands.

## 1. Fork Excalidraw on GitHub

**What we are doing:** Create a copy of the repository under your GitHub account so you can push your work there.

**Terminal — any folder:**

```bash
gh repo fork excalidraw/excalidraw --clone=false
```

- `gh` is GitHub's CLI; it uses the account you just signed in with.
- `repo fork` creates your fork on GitHub.
- `excalidraw/excalidraw` means `OWNER/REPOSITORY`: the repository at `https://github.com/excalidraw/excalidraw`.
- `--clone=false` leaves downloading to the next step.

**You should see:** Your fork at `https://github.com/YOUR_USERNAME/excalidraw`. If it already exists, use it. Do not add `--remote=false` to this command; that flag combination is unsupported when a repository argument is supplied.

## 2. Clone your fork and enter its folder

**What we are doing:** Download the code and Git history to your computer. Forking creates the GitHub copy; cloning creates the local copy.

**Terminal:**

```bash
mkdir -p ~/repos
cd ~/repos
git clone https://github.com/YOUR_USERNAME/excalidraw.git
cd excalidraw
pwd
git remote -v
```

`mkdir -p` creates the projects folder if needed. `git clone` creates the `excalidraw` folder, and `cd excalidraw` enters it. If you already cloned this fork, skip cloning and enter that folder instead.

**You should see:** `pwd` ends in `/repos/excalidraw`. Both `origin` entries point to your fork:

```text
origin  https://github.com/YOUR_USERNAME/excalidraw.git (fetch)
origin  https://github.com/YOUR_USERNAME/excalidraw.git (push)
```

`origin` is the local name for that remote URL; fetch downloads history and push uploads commits. If Git says `not a git repository`, you are probably still in the parent `repos` folder. Run `cd ~/repos/excalidraw`, then retry.

Set the default repository for later GitHub CLI commands. This does not change the Git remote URLs:

```bash
gh repo set-default YOUR_USERNAME/excalidraw
```

For an older fork, use [Update your fork's base branch](pre-class-setup.md#8-update-your-forks-base-branch) before proceeding; add the `upstream` remote as described in that guide first if it is missing.

## 3. Install the app's dependencies

**What we are doing:** Install Yarn, then the JavaScript libraries Excalidraw needs to run.

The checkout used for this walkthrough declares Node.js `>=18.0.0` and Yarn `1.22.22` in its root `package.json`. If your checkout declares different requirements, follow that file.

**Terminal — inside `excalidraw`:**

```bash
npm install --global yarn@1.22.22
yarn --version
```

**You should see:** `1.22.22`. If that version is already installed, skip the installation. Then run:

```bash
yarn install
```

**You should see:** Installation completes, typically ending with `Done in ...`. Warnings may appear; a failed install needs diagnosis before continuing.

## 4. Run the original app

**What we are doing:** Start the development server and verify the unmodified canvas works.

**Terminal 1 — inside `excalidraw`:**

```bash
yarn start
```

**You should see:** A local URL in the terminal. Open that exact URL in your browser; the port can vary. Draw a rectangle, add text, and move the rectangle.

Leave this terminal running. It serves the app and shows build errors as code changes. We use a second terminal for Git and Claude; do not type those commands into the running server. `Ctrl+C` stops the server when you are finished with the demo.

## 5. Create your feature branch

**What we are doing:** Give this feature its own branch, separate from the base branch.

Open a second terminal tab (`Cmd+T` in macOS Terminal).

**Terminal 2:**

```bash
cd ~/repos/excalidraw
git status --short --branch
git remote -v
```

**You should see:** A clean base branch, normally `master`, with `origin` pointing to your fork. If you are resuming this same demo branch, continue it below. If you see unexpected changed files or a different feature branch, resolve that with the instructor before proceeding; keep existing work intact.

```bash
git switch -c feature/selection-focus-mode
git branch --show-current
gh repo set-default YOUR_USERNAME/excalidraw
```

`git switch -c` creates and switches to the branch. **You should see:** `feature/selection-focus-mode`. If it already exists from this same exercise, continue it with `git switch feature/selection-focus-mode` instead of recreating it.

## 6. Open Claude in Plan mode

**What we are doing:** Let Claude inspect the repository and discuss an approach before editing application files.

**Terminal 2 — inside `excalidraw`:**

```bash
claude --permission-mode plan
```

Complete login or folder-trust prompts if shown. **You should see:** An interactive Claude session with Plan mode active. From here, paste the **Claude** prompts into that session.

**Codex alternative:** Run `codex`, then enter `/plan`. Use the same feature prompts below; use its mode control to leave Plan mode before requesting file edits. Use one CLI for the exercise.

## 7. Explore the code and compare approaches

**What we are doing:** Ask Claude to find existing code we can reuse and explain the tradeoffs.

**Claude — Plan mode:**

```text
We are building a Selection Focus Mode prototype in this Excalidraw fork.
Fit the selected elements into the viewport and visually de-emphasize unrelated
content without changing persisted scene data. Provide visible and keyboard exit;
restore normal rendering/interaction on exit. With no selection, safely disable or
no-op. Preserve Zen Mode and View Mode. Collaboration additions and reload
persistence are out of scope.

Use built-in file-reading and search tools to inspect existing selection,
viewport, rendering, and UI patterns. Compare 2-3 approaches by reuse, complexity,
UX/keyboard risks, affected files, and tests. Cite files/symbols, recommend one,
and wait for our choice. Do not edit repository files.
```

**You should see:** A comparison grounded in actual files and functions, a recommendation, and no application edits. Discuss the options with the instructor and choose an approach.

## 8. Record the goal

**What we are doing:** Turn the chosen approach into a short statement of the user outcome.

Press `Shift+Tab` until the mode indicator says **Manual**, the normal mode that asks before edits. Confirm Plan mode is off. We are allowing document edits at this stage.

**Claude — replace the bracketed text with your chosen approach:**

```text
We chose [approach name and a one-sentence description]. Create docs/ai/GOAL.md
with the problem, user outcome, success criteria, non-goals, and constraints
we discussed. Write only that document; do not change application code.
Show its contents and stop so we can read it.
```

**You should see:** `docs/ai/GOAL.md` explaining who benefits and what success means. Read it together and correct any change to the agreed scope.

## 9. Make the behavior testable

**What we are doing:** Define observable behavior before choosing implementation details.

**Claude:**

```text
Read docs/ai/GOAL.md. Create docs/ai/SPEC.md with observable acceptance criteria
and edge cases. Cover no selection; fitting one or multiple selected elements;
temporary de-emphasis without persisted opacity/scene changes; a visible exit;
the exact keyboard exit and conflict handling; restored normal interaction;
and compatibility with Zen Mode and View Mode. State what happens if the
selection changes while focus is active. Write only the spec, then show it and
stop for our review. Do not implement.
```

**You should see:** Specific actions and expected results you can try in the browser. Agree on the entry control, exit key, and selection-change behavior before continuing.

## 10. Map the code and plan small tasks

**What we are doing:** Connect the spec to existing code and a short implementation sequence.

**Claude:**

```text
Read docs/ai/GOAL.md and docs/ai/SPEC.md. Delegate a read-only exploration of
selection state, viewport fitting, transient UI/rendering, exit controls,
keyboard handling, localization, and nearby tests. If subagents are unavailable,
do the same exploration in this session. Return files/symbols, reuse points,
and risks. Then create docs/ai/PLAN.md and docs/ai/TASKS.md, mapping each small
task to files, acceptance criteria, and verification. Add no dependencies.
Do not implement yet. Show the plan and stop for our review.
```

**You should see:** Concrete file paths, existing helpers to reuse, and tasks with ways to verify them. Read the plan together and correct scope or missing coverage before implementing.

## 11. Implement the next agreed task

**What we are doing:** Make one small change, check it, then decide whether to continue.

**Claude:**

```text
Implement only the next agreed task in docs/ai/TASKS.md, following docs/ai/PLAN.md
and docs/ai/SPEC.md. Add no dependencies. Read the repository's instructions,
package scripts, and test configuration; run relevant checks and add/run focused
regression tests for the behavior changed. Report exact commands and results,
files changed, and anything unresolved. Update TASKS.md to match actual progress.
Keep the dev server usable. Stop after this task. Do not commit or push.
```

**You should see:** A focused change and actual verification results. Review the diff and output together. Repeat this prompt for each next agreed task; debug failures before building on them.

## 12. Try the feature in the browser

**What we are doing:** Check the user-visible behavior ourselves. The development server in Terminal 1 should update the app as files change; reload the page if needed.

1. Draw 6–10 objects in separate groups; select 2–3 and enter Focus Mode using the control you agreed on.
2. Confirm the selected objects fit in view and unrelated content is de-emphasized.
3. Exit using the visible control. Repeat and exit using the key recorded in the spec.
4. Try no selection; confirm the safe disabled/no-op behavior.
5. Try changing the selection while focused; compare the result with the agreed spec.
6. Confirm normal editing and visual treatment after exit, with no saved opacity/scene changes caused by Focus Mode.
7. Try Zen Mode and View Mode before and after Focus Mode; confirm their existing behavior.

**You should see:** Behavior matching `docs/ai/SPEC.md`. Note what passed and what failed; tell Claude the actual results when saving context. If anything differs, use the [debug prompt](#debug-together-whenever-something-fails).

## 13. Review, fix, and simplify

**What we are doing:** Look for correctness problems and unnecessary complexity before publishing.

**Claude — review first:**

```text
Review the current diff and any new implementation/test files against
docs/ai/SPEC.md. Report evidence-backed findings with file locations and severity;
a clean review is valid. Check state restoration, keyboard conflicts, scene-data
changes, edge cases, and missing regression tests. Do not edit yet.
```

**You should see:** Specific findings you can verify, or a clear statement that none were found. Discuss them, then identify the fixes you accept in the next prompt. If there are no accepted findings, say so.

**Claude — replace the bracketed text:**

```text
Accepted findings: [list the findings to fix, or say none]. Fix only those
findings and simplify unnecessary state, duplicated logic, or custom viewport
math where existing helpers suffice. Preserve docs/ai/SPEC.md behavior. Re-run
affected checks and report actual results. Update docs/ai/TASKS.md; leave
unresolved work clearly marked. Do not commit or push.
```

**You should see:** Targeted changes with fresh check results. Repeat the browser actions from step 12 after changes.

## 14. Save context and prepare the PR description

**What we are doing:** Leave a truthful handoff for the next session and the pull-request reviewer.

**Claude — fill in your browser results:**

```text
Our manual browser results: [actions tried, what passed, what failed, not tried].
Create docs/ai/PROJECT_CONTEXT.md with the goal, current state, decisions,
files/symbols changed, checks actually run and results, known risks, and next steps.
Create docs/ai/PR_BODY.md describing the behavior, actual automated checks,
our manual verification, and anything unfinished. Do not claim unrun checks passed.
Show both documents, git status, and the diff summary. Do not commit or push.
```

**You should see:** Two documents that match what actually happened. Read and correct them before publishing.

## 15. Select the files for your commit

**What we are doing:** Choose exactly what will go into Git history. A commit saves changes locally; pushing uploads the commits later.

In Claude, enter `/exit`. When your normal terminal prompt returns in Terminal 2, run the Git commands below. Leave the server in Terminal 1 running.

**Terminal 2:**

```bash
git branch --show-current
git status --short
git diff --stat
git diff
```

**You should see:** `feature/selection-focus-mode` and the expected feature files. If `git diff` opens a pager, press `q` to return to the prompt.

Stage the intended changes:

```bash
git add -p
git add docs/ai/GOAL.md docs/ai/SPEC.md docs/ai/PLAN.md docs/ai/TASKS.md docs/ai/PROJECT_CONTEXT.md docs/ai/PR_BODY.md
```

`git add -p` asks about each tracked-file change: `y` stages it; `n` leaves it out. It does not include new files. Inspect each new implementation/test file shown by `git status`, then run `git add -- path/to/new-file` using its actual path. Exclude unrelated files.

```bash
git diff --cached --check
git diff --cached --stat
git diff --cached
```

**You should see:** No whitespace errors and exactly the changes you intend to publish. `--cached` shows the staged content that the commit will contain. If you correct a file, stage the correction again and rerun these checks before continuing.

## 16. Commit locally and push to your fork

**What we are doing:** Save the reviewed change, then upload this feature branch to your GitHub fork.

**Terminal 2 — first inspect the author identity Git will use:**

```bash
git var GIT_AUTHOR_IDENT
```

The name and email will appear in your commit. If they are missing or you want to change them, set them for this clone:

```bash
git config user.name "YOUR_PREFERRED_NAME"
git config user.email "YOUR_GITHUB_NOREPLY_ADDRESS"
```

Replace both placeholders. Copy your exact noreply address from [GitHub Settings → Emails](https://github.com/settings/emails), then rerun `git var GIT_AUTHOR_IDENT` to verify. Commit identity is separate from GitHub login.

Save the reviewed changes:

```bash
git commit -m "feat: add selection focus mode prototype"
```

**You should see:** A new commit ID and a summary of changed files. Then push:

```bash
git push -u origin feature/selection-focus-mode
```

**You should see:** The branch uploaded to your fork. `-u` connects this local branch to its remote branch so later pushes can use `git push`.

## 17. Open a pull request in your own fork

**What we are doing:** Put the branch diff and our verification notes into a reviewable GitHub PR. This creates a PR; it does not merge it.

**Terminal 2 — replace `YOUR_USERNAME`:**

```bash
gh pr create --repo YOUR_USERNAME/excalidraw --base master --head feature/selection-focus-mode --title "Demo: add selection focus mode prototype" --body-file docs/ai/PR_BODY.md
```

`--repo` targets your fork, `--base` is the branch receiving the proposed change, `--head` is your feature branch, and `--body-file` supplies the description. The walkthrough uses Excalidraw's `master` base; substitute your fork's base name if it differs.

**You should see:** A PR URL under your own GitHub account. Open it and inspect **Files changed** and the description. All class pushes and PRs stay in your fork.

## Debug together whenever something fails

Read the error before running another command. For setup failures, check these first:

| What you see | What to do |
|---|---|
| `command not found` | Install the named tool; complete its PATH instructions and reopen the terminal. |
| `not a git repository` | Run `cd ~/repos/excalidraw`, then `pwd` and `git status`. |
| Clone destination already exists | Enter the existing clone and check its `origin`; do not clone into it again. |
| GitHub rejects a password or token | Run `gh auth login --hostname github.com --git-protocol https --web`, then `gh auth setup-git`. |
| Push denied with `403` | Check `gh auth status` and `git remote -v`: the account must have write access to the target fork. |
| Browser cannot reach the app | Check Terminal 1 is still running and use its printed local URL. |

For a feature failure, keep or reopen the agent session and paste the evidence. Replace the brackets:

```text
Expected: [what should happen]
Observed: [what happened]
Reproduce: [browser steps, or exact command and output]
Read docs/ai/SPEC.md if it exists. Investigate the cause and explain the smallest
fix consistent with the agreed behavior. Do not edit yet; let us discuss it first.
```

After discussing the diagnosis, ask it to apply the agreed fix, rerun the failed check and relevant regression checks, and report actual results. Try the same browser action again. Update the context and PR description to match what happened.

Command references: [GitHub fork](https://cli.github.com/manual/gh_repo_fork), [GitHub authentication](https://cli.github.com/manual/gh_auth_login), [Git credential setup](https://cli.github.com/manual/gh_auth_setup-git), [PR creation](https://cli.github.com/manual/gh_pr_create), and [Claude Plan mode](https://code.claude.com/docs/en/common-workflows#use-plan-mode-for-safe-code-analysis). Repository requirements come from the checked-out `package.json`; installers and version notes are in [pre-class setup](pre-class-setup.md).
