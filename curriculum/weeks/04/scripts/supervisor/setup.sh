#!/usr/bin/env bash
# Prepare one disposable, independent repository. This script runs no agents.
set -euo pipefail
unset GIT_DIR GIT_WORK_TREE GIT_INDEX_FILE

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
cap_root=$(git -C "$script_dir" rev-parse --show-toplevel)
[[ "$script_dir" == "$cap_root/curriculum/weeks/04/scripts/supervisor" ]] || { echo "Run the checked-in scripts/supervisor/setup.sh." >&2; exit 1; }
agent=codex
if [[ ${1:-} == --agent ]]; then
  agent=${2:?Choose codex or claude after --agent}
  shift 2
fi
case "$agent" in codex|claude) ;; *) echo 'Choose codex or claude.' >&2; exit 1 ;; esac
if [[ $# != 1 ]]; then
  echo 'Usage: bash setup.sh [--agent codex|claude] /absolute/path/to/fresh-demo' >&2
  exit 1
fi
run_dir=$1
command -v python3 >/dev/null || { echo "Missing tool: python3" >&2; exit 1; }
# A strict location rule keeps this demo out of the curriculum Git repository.
case "$run_dir" in "$cap_root"/.demo-runs/*) ;; *) echo 'Choose a path inside CAP/.demo-runs/.' >&2; exit 1 ;; esac
run_dir=$(python3 - "$cap_root" "$run_dir" <<'PY_PATH'
from pathlib import Path
import sys
root=Path(sys.argv[1]).resolve()/'.demo-runs'
if root.resolve() != root:
    raise SystemExit('CAP/.demo-runs must not be a symlink outside its expected location.')
selected=Path(sys.argv[2]).resolve()
if selected.parent != root.resolve() or not selected.name.startswith("supervisor-"):
    raise SystemExit('Choose a fresh supervisor-* direct child of CAP/.demo-runs; no escaping paths.')
print(selected)
PY_PATH
)
[[ ! -e "$run_dir" ]] || { echo "Existing run found: $run_dir. Choose a fresh path." >&2; exit 1; }

# Check installed tools and account before creating the fixture.
for tool in node npm python3 git "$agent"; do
  command -v "$tool" >/dev/null || { echo "Missing tool: $tool" >&2; exit 1; }
done
node -e 'if (+process.versions.node.split(".")[0] < 20) { console.error("Need Node 20+"); process.exit(1); }'
python3 - <<'PY'
import re, subprocess, sys
assert sys.version_info >= (3, 10), 'Need Python 3.10+'
match = re.search(r'(\d+)\.(\d+)', subprocess.check_output(['git','--version'],text=True))
assert match and tuple(map(int,match.groups())) >= (2, 28), 'Need Git 2.28+'
PY
"$agent" --version
if [[ "$agent" == codex ]]; then
  codex login status
else
  claude auth status
fi

# Ignore all demo runs locally. Existing exclusions are retained.
exclude=$(git -C "$cap_root" rev-parse --git-path info/exclude)
[[ "$exclude" == /* ]] || exclude="$cap_root/$exclude"
mkdir -p "$(dirname "$exclude")"
touch "$exclude"
if ! grep -qxF '/.demo-runs/' "$exclude"; then printf '\n/.demo-runs/\n' >> "$exclude"; fi
mkdir -p "$run_dir"
run_dir=$(cd "$run_dir" && pwd)
cp -R "$script_dir/fixture/." "$run_dir/"
if [[ "$agent" == codex ]]; then rm -rf "$run_dir/.claude" "$run_dir/CLAUDE.md"; else rm -rf "$run_dir/.codex"; fi
cd "$run_dir"
git init -b feature/paint-preferences
# Disposable commits use only a local identity and no signing dependency.
git config --local user.name 'CAP Demo'
git config --local user.email 'cap-demo@example.invalid'
git config --local commit.gpgsign false
[[ $(git rev-parse --show-toplevel) == "$run_dir" ]]
[[ $(git branch --show-current) == feature/paint-preferences ]]
[[ -z $(git remote) ]]
git -C "$cap_root" check-ignore -q "$run_dir/"
git add .
git commit -m 'Prepare native-agent paint-preferences classroom fixture'
printf '\nPASS: independent fixture ready.\nPath: %s\nBranch: %s\nAgent: %s\nRemotes: none\n' "$run_dir" "$(git branch --show-current)" "$agent"
echo 'The baseline intentionally does not pass the preference checks yet.'
printf 'Copy these commands into each new terminal:\nexport CAP_ROOT=%q\nexport CAP_SUPERVISOR_DEMO=%q\n' "$cap_root" "$run_dir"
