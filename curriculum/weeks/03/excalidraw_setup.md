# Excalidraw: clone and run locally

Upstream repo: https://github.com/excalidraw/excalidraw. Work on **your fork**; never open PRs against `excalidraw/excalidraw`.

## 1. Install prerequisites (one time)

| macOS | Windows |
|---|---|
| `brew install node@22 gh` | `winget install OpenJS.NodeJS.LTS` and `winget install --id GitHub.cli` |

Then, on both OSes:

```sh
npm install -g yarn
node --version                   # v18 or higher
yarn --version                   # 1.22.x
gh auth login                    # GitHub.com → HTTPS → authenticate Git → browser
```

## 2. Fork and clone

```sh
gh repo fork excalidraw/excalidraw --clone
cd excalidraw
gh repo set-default <your-github-username>/excalidraw   # PRs go to your fork, not upstream
```

This sets `origin` to your fork and `upstream` to `excalidraw/excalidraw`.

## 3. Install and run

```sh
yarn install      # 1–5 minutes
yarn start        # dev server
```

Open **http://localhost:3001** and draw something.

## 4. Verify hot reload

With `yarn start` still running:

1. In `packages/excalidraw/locales/en.json`, change `"viewMode": "View mode"` to `"viewMode": "View mode!!"` and save.
2. In the browser, open the menu (☰): the label updates without a restart.
3. Undo it with `git restore .`

Stop the server with `Ctrl+C`.

> Just want to try it without a fork? `git clone https://github.com/excalidraw/excalidraw.git` works for steps 3–4. The class demo and homework need the fork.

## Notes

### What does fork do?

A **fork** is your own copy of someone else's repository, on GitHub, under your account (`github.com/<you>/excalidraw`).

- **Why you need one:** you can't push to `excalidraw/excalidraw` (no write access). You can push to your fork, and a PR needs a branch you can push to.
- **What it copies:** all code, branches and history at the moment you fork. After that, the two are independent: your changes don't touch the original, and new upstream commits don't appear until you sync (`gh repo sync`, or `git pull upstream master`).
- **Fork vs clone:** a fork is a copy **on GitHub**; a clone is a copy **on your machine**. `gh repo fork excalidraw/excalidraw --clone` does both, and sets two remotes:
  - `origin` = your fork (you push here)
  - `upstream` = the original (you pull updates from here)
- **PRs:** a PR from a fork normally asks the original project to merge. `gh repo set-default <you>/excalidraw` makes PRs target **your fork** instead, so nothing reaches the real project.

### What is Yarn, and what does it do?

**Yarn** is a **package manager for JavaScript/Node.js**, an alternative to `npm` (which ships with Node). Excalidraw uses **Yarn 1**, so use `yarn`, not `npm`, inside the repo.

- **Installs dependencies:** `yarn install` reads `package.json`, downloads the libraries the project needs into `node_modules/`, and pins exact versions in `yarn.lock`, so everyone gets the same versions.
- **Runs project scripts:** `yarn <name>` runs a command defined under `"scripts"` in `package.json`. In Excalidraw:

| Command | What it does |
|---|---|
| `yarn start` | Starts the Vite dev server with hot reload (http://localhost:3001) |
| `yarn test` | Runs the tests |
| `yarn test:typecheck` | Runs the TypeScript type checks |
