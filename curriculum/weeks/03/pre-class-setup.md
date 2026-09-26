# Module 03 — Pre-Class Setup

**Complete this before class.** Install the tools and dependencies in your own Excalidraw fork. In class, follow the instructor through one feature workflow; everyone uses their own branch. We will debug feature problems together as they arise.

You need:

- macOS or Windows
- Git
- GitHub CLI (`gh`)
- your own GitHub account
- your **own fork** of Excalidraw
- Node.js 18+ and Yarn 1.22.22
- **one** AI coding CLI: **Claude Code OR Codex CLI**

You do **not** need Docker, both AI CLIs, or custom course skills. The live workflow uses built-in Plan mode, file/search/shell tools, and ordinary prompts.

---

## 0. Choose your AI CLI

Pick one path:

- **Claude Code** — used for the instructor’s primary live demo.
- **Codex CLI** — fully acceptable for following the workflow.

Both can inspect a repository, use shell/Git tools, edit files, and run engineering commands. Exact commands, hooks, skills, and agent features are not identical.

---

# 1. Install Git

## macOS

Check first:

```bash
git --version
```

If Git is missing, installing Apple command-line tools is usually the simplest route:

```bash
xcode-select --install
```

Then verify again:

```bash
git --version
```

## Windows

In PowerShell:

```powershell
winget install --id Git.Git -e
```

Close and reopen your terminal, then verify:

```powershell
git --version
```

### Configure your Git identity

Use the name/email you want attached to commits:

```bash
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
```

Verify:

```bash
git config --global user.name
git config --global user.email
```

---

# 2. Install GitHub CLI

GitHub CLI lets us authenticate Git and later create a real pull request from the terminal.

## macOS — Homebrew

```bash
brew install gh
```

If you do not use Homebrew, use the installer from https://cli.github.com/.

## Windows — WinGet

```powershell
winget install --id GitHub.cli -e
```

Close/reopen the terminal if necessary.

### Verify

```bash
gh --version
```

---

# 3. Authenticate your terminal with GitHub

Run:

```bash
gh auth login
```

Recommended choices for this class:

1. **GitHub.com**
2. **HTTPS** for Git operations
3. Authenticate through the **browser**
4. Allow `gh` to authenticate Git when prompted

Then run:

```bash
gh auth status
gh auth setup-git
```

You should see your GitHub username and no authentication error.

> `gh auth setup-git` configures Git to use GitHub CLI as a credential helper. This is what lets commands such as `git push` authenticate without manually pasting a token.

---

# 4. Fork and clone Excalidraw

Create a fork of `excalidraw/excalidraw` on GitHub. Replace `YOUR_USERNAME` with your GitHub username. From the parent folder where you keep projects, clone that fork:

```bash
git clone https://github.com/YOUR_USERNAME/excalidraw.git
cd excalidraw
git remote -v
```

`origin` must point to your fork. If you already have a local clone, open that folder and check its remotes instead. Add the upstream remote only if it is missing:

```bash
git remote add upstream https://github.com/excalidraw/excalidraw.git
git remote -v
```

All class pushes and pull requests go to your fork. Set GitHub CLI's default explicitly; replace `YOUR_USERNAME` with your GitHub username:

```bash
gh repo set-default YOUR_USERNAME/excalidraw
```

---

# 5. Install Node.js

The current Excalidraw repository declares **Node.js >= 18**. Use a current Node LTS release if possible.

## macOS — Homebrew

```bash
brew install node
```

## Windows — WinGet

```powershell
winget install OpenJS.NodeJS.LTS
```

Close/reopen the terminal, then verify:

```bash
node --version
npm --version
```

Your Node version should be 18 or newer.

---

# 6. Install the Yarn version used by Excalidraw

The Excalidraw root `package.json` currently declares:

```text
yarn@1.22.22
```

Install it:

```bash
npm install -g yarn@1.22.22
```

Verify:

```bash
yarn --version
```

Expected version: `1.22.22`.

# 7A. Install Claude Code — choose this OR Codex

Official docs: https://code.claude.com/docs/en/setup

## macOS — recommended native installer

```bash
curl -fsSL https://claude.ai/install.sh | bash
```

Alternative with Homebrew:

```bash
brew install --cask claude-code
```

## Windows PowerShell — native installer

```powershell
irm https://claude.ai/install.ps1 | iex
```

Alternative with WinGet:

```powershell
winget install Anthropic.ClaudeCode
```

Claude Code supports native Windows. Git for Windows is recommended because it also gives Claude access to Bash tooling; without it, Claude Code can use PowerShell.

### Verify

```bash
claude --version
claude doctor
```

### Authenticate

Run:

```bash
claude
```

Follow the browser login flow.

> Claude Code requires an account/plan that includes Claude Code access. If your account does not include it, use Codex CLI instead for this course.

---

# 7B. Install Codex CLI — choose this OR Claude

Official repository: https://github.com/openai/codex

## macOS

Recommended installer:

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
```

Alternative with Homebrew:

```bash
brew install --cask codex
```

Alternative with npm:

```bash
npm install -g @openai/codex
```

## Windows PowerShell

```powershell
powershell -ExecutionPolicy ByPass -c "irm https://chatgpt.com/codex/install.ps1 | iex"
```

If your Windows environment has compatibility issues, using WSL2 is also a supported development path in the Codex repository documentation.

### Verify and authenticate

```bash
codex
```

Choose **Sign in with ChatGPT** when prompted, then complete the browser flow.

---

# 8. Update your fork's base branch

In your local clone, bring your fork up to date with the official Excalidraw default branch before class:

```bash
git fetch upstream
git switch master
git merge --ff-only upstream/master
git push origin master
git status --short --branch
```

The exercise branch will be created live after the instructor introduces the selected feature. Do not create feature code in advance.

---

# 9. Install dependencies

From the root of your local Excalidraw clone:

```bash
yarn
```

Wait for installation to finish successfully.

---

# 10. Start Excalidraw locally

```bash
yarn start
```

Open the local URL printed by the terminal. Leave this terminal running for the live exercise.

---

# 11. Verify your AI CLI

Open a second terminal in the same Excalidraw clone. Authenticate and verify your selected CLI:

```bash
claude --version
claude doctor
```

Or:

```bash
codex --version
codex login status
```

During the exercise, the instructor will lead you through creating the feature branch and starting Plan mode.

---

# Ready for class

- [ ] My own fork is cloned, with `origin` pointing to that fork.
- [ ] `upstream` points to `excalidraw/excalidraw` and local `master` matches `upstream/master`.
- [ ] Node.js 18+ and Yarn 1.22.22 are installed.
- [ ] `yarn` completed and `yarn start` runs locally.
- [ ] One AI CLI is installed and authenticated.
- [ ] GitHub CLI is authenticated and defaults to my fork.
- [ ] The running app and second terminal are ready.

## Sources / version notes

This setup was prepared against the public documentation/repository state checked on **September 25, 2026**:

- Claude Code setup: https://code.claude.com/docs/en/setup
- Codex CLI: https://github.com/openai/codex
- GitHub CLI: https://cli.github.com/
- Fork-and-clone command: https://cli.github.com/manual/gh_repo_fork
- Excalidraw development guide: https://github.com/excalidraw/excalidraw/blob/master/dev-docs/docs/introduction/development.mdx
- Excalidraw root package metadata: https://github.com/excalidraw/excalidraw/blob/master/package.json

CLI installers and repository requirements evolve. If a command no longer matches the linked official documentation, follow the current official documentation and tell the instructor before class.
