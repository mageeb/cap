#!/usr/bin/env bash
# Run this checked-in script to prepare a new isolated Demo 01 fixture.
# No model job starts here. Runtime code and prompts are written beside app/.
set -euo pipefail
source_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cap_root="$(git -C "$source_dir" rev-parse --show-toplevel)"
if [ "$source_dir" != "$cap_root/curriculum/weeks/04/scripts/loop" ]; then
  echo 'STOP: run CAP curriculum/weeks/04/scripts/loop/setup.sh from its source location.' >&2
  exit 1
fi

preflight() {
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
git -C "$cap_root" rev-parse --show-toplevel
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
}

if [ "${1:-}" = '--preflight' ] && [ "$#" -eq 1 ]; then
  preflight
  exit 0
fi
if [ "$#" -ne 1 ]; then
  echo 'Usage: bash setup.sh --preflight | /absolute/CAP/.demo-runs/loop-NAME' >&2
  exit 1
fi
preflight
CAP_LOOP_DEMO=$(python3 - "$1" "$cap_root" <<'FIXTURE_PATH'
from pathlib import Path
import sys
candidate = Path(sys.argv[1])
root = Path(sys.argv[2]).resolve()
assert candidate.is_absolute(), 'Choose an absolute fixture path'
candidate = candidate.resolve()
assert candidate.parent == root / '.demo-runs' and candidate.name.startswith('loop-'), 'Choose CAP/.demo-runs/loop-NAME'
print(candidate)
FIXTURE_PATH
)
cd "$cap_root" || exit 1
CAP_DEMO_EXCLUDE=$(git rev-parse --git-path info/exclude)
if ! grep -qxF '/.demo-runs/' "$CAP_DEMO_EXCLUDE"; then
  printf '\n/.demo-runs/\n' >> "$CAP_DEMO_EXCLUDE"
fi
if [ -e "$CAP_LOOP_DEMO" ]; then
  printf 'Existing run found: %s. Choose a fresh demo directory.\n' "$CAP_LOOP_DEMO" >&2
  exit 1
fi
mkdir -p "$CAP_LOOP_DEMO/app" "$CAP_LOOP_DEMO/.harness" "$CAP_LOOP_DEMO/artifacts"
cd "$CAP_LOOP_DEMO" || exit 1
git init -b feature/paint-loop || exit 1
if [ "$(git -C "$CAP_LOOP_DEMO" rev-parse --show-toplevel)" != "$CAP_LOOP_DEMO" ] ||
   [ "$(git -C "$CAP_LOOP_DEMO" branch --show-current)" != "feature/paint-loop" ] ||
   [ -n "$(git -C "$CAP_LOOP_DEMO" remote)" ] ||
   ! git -C "$cap_root" check-ignore -q "$CAP_LOOP_DEMO/"; then
  printf 'Git isolation check failed. Stop here; do not run later blocks.\n' >&2
  exit 1
fi
# Identity is set only in this isolated repository when no identity exists.
git config user.name >/dev/null || git config --local user.name "CAP Demo"
git config user.email >/dev/null || git config --local user.email "cap-demo@example.invalid"
git var GIT_AUTHOR_IDENT >/dev/null
git -C "$CAP_LOOP_DEMO" rev-parse --show-toplevel
git -C "$CAP_LOOP_DEMO" branch --show-current
git -C "$CAP_LOOP_DEMO" remote -v
git -C "$cap_root" check-ignore -v "$CAP_LOOP_DEMO/"
git -C "$cap_root" status --short
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
cat > AGENTS.md <<'EOF'
# Isolated classroom fixture
This nested demo repository authorizes its single implementation Worker to edit
app/ directly. It may not edit CAP files, the harness, this contract, or Git.
The supplied per-pass prompt is the complete task. Do not add CAP planning,
review, DECISIONS, commit, or publishing workflows to this fixture.
The external controller owns checks, screenshots, the ledger and local commits.
EOF
cat > ledger.md <<'EOF'
# Paint app iteration ledger
The app folder is empty. No implementation has happened yet.
Agent reports below are claims; controller observations are recorded separately.
EOF
cat > .harness/schema.json <<'EOF'
{"type":"object","additionalProperties":false,"properties":{"implemented":{"type":"string"},"envisioned":{"type":"string"},"limitations":{"type":"string"}},"required":["implemented","envisioned","limitations"]}
EOF
cat > .harness/prompt.txt <<'EOF'
You are the sole implementation Worker in an isolated classroom fixture.
This is a NEW conversation. Read current app source and the ledger below;
those disk artifacts are the carried-forward implementation/planning state.
Choose and implement the next useful improvement to a browser paint app.
The first pass starts from an EMPTY app directory. Start with a usable slice.

Runtime contract:
- Plain static index.html and local HTML/JS/CSS; no packages, remote assets,
  symlinks, network services, build steps, server code or credentials.
- A visible HTML canvas#paint, at least 400 by 240 intrinsic pixels.
- A pointer drag draws a visible stroke immediately after opening the app.
- No full-page opening modal. Layout, tools and later features are your choice.

Ownership and limits:
- Edit only files in this app directory, directly as this fixture's Worker.
- Do not modify the parent/harness/Git, commit, push, spawn agents, install
  packages or start servers. The controller owns those operations.
- Add plus delete at most 1,000 application lines this pass.
- Preserve existing behavior. If the ledger reports a failure, repair it first.
- Do not retrieve previous sessions, transcripts, screenshots or external memory.

Return the supplied JSON schema: implemented = actual changes this pass;
envisioned = the next useful improvement you foresee; limitations = unverified
or incomplete behavior. The controller appends your words to the disk ledger.
EOF

# Copy exactly the six runtime helpers; setup and Claude examples stay in CAP.
for helper in agent.py capture.cjs changes.py check.py journal.cjs loop.sh; do
  cp "$source_dir/$helper" .harness/
done
git add AGENTS.md .gitignore .harness ledger.md
git commit -m "Prepare isolated paint-loop classroom fixture"
if [ -n "$(find app -type f -print -quit)" ]; then
  echo 'STOP: expected an empty application directory.' >&2
  exit 1
fi
printf '\nPASS: fixture ready; no model job has run.\nFixture: %s\n' "$CAP_LOOP_DEMO"
printf 'In your terminal, run: cd %q\n' "$CAP_LOOP_DEMO"
printf 'For a later terminal, copy these exports:\n'
printf 'export CAP_ROOT=%q\n' "$cap_root"
printf 'export CAP_LOOP_DEMO=%q\n' "$CAP_LOOP_DEMO"
