#!/bin/sh
# Source from the CAP root. Installs tooling; no city or workers start here.
provider=codex # Change to claude for the optional Claude Code version.
city_sessions=6
implementation_sessions=4
mkdir -p .demo-runs
mkdir .demo-runs/04 || return
export GC_HOME="$PWD/.demo-runs/04/.gc-home"
app="$PWD/.demo-runs/04/paint-app"
city="$PWD/.demo-runs/04/city"
python_env="$PWD/.demo-runs/04/.venv"
cp -R curriculum/weeks/04/scripts/gas-city/fixture "$app" || return
# Native do-work needs a committed launcher HEAD before registering the rig.
git init -b feature/paint-demo "$app" || return
git -C "$app" config user.name "CAP Demo"
git -C "$app" config user.email "cap-demo@example.invalid"
git -C "$app" config commit.gpgsign false
printf 'node_modules/\nartifacts/\nplaywright-report/\ntest-results/\nworktrees/\n' > "$app/.gitignore"
git -C "$app" add .
git -C "$app" commit -m "Studio Board starter" || return
# Isolate the city so initialization cannot inherit CAP's Git repo/remote.
git init -b feature/city-demo "$city" || return
gc init "$city" --name cap-demo-04 --template gascity --default-provider "$provider" --no-start || return
cp "$app/AGENTS.md" "$city/AGENTS.md" || return
gc --city "$city" rig add "$app" --name paint --default-branch feature/paint-demo || return

# Keep the stock validator's scripts/schema layout; .gc/scripts is its legacy path.
stock_scripts=$(find "$GC_HOME/cache/repos" -type d -path '*/gascity/assets/scripts' -print -quit)
[ -n "$stock_scripts" ] || { printf 'Stock Gas City scripts not found.\n'; return 1; }
stock_pack=${stock_scripts%/assets/scripts}
mkdir -p "$app/.gc/gascity/assets"
cp -R "$stock_scripts" "$app/.gc/gascity/assets/scripts" || return
cp -R "$stock_pack/schemas" "$app/.gc/gascity/schemas" || return
ln -s gascity/assets/scripts "$app/.gc/scripts" || return

python3 -m venv "$python_env" || return
"$python_env/bin/python" -m pip install PyYAML || return
# Native gate PATH includes bd's directory. This symlink selects our Python too.
ln -s "$(command -v bd)" "$python_env/bin/bd" || return
export PATH="$python_env/bin:$PATH"
(cd "$app" && npm install --save-dev playwright && npx playwright install chromium) || return

# GC prepends its own bin to worker PATH: give helpers an explicit Python.
# Native gates instead use the sourced supervisor PATH above.
"$python_env/bin/python" - "$city/city.toml" "$PATH" "$city_sessions" "$implementation_sessions" "$python_env/bin/python" <<'PYCONFIG' || return
from pathlib import Path
import json, sys
path = Path(sys.argv[1])
text = path.read_text().replace('[workspace]\n', f'[workspace]\nmax_active_sessions = {int(sys.argv[3])}\n', 1)
text += '\n[workspace.env]\nPATH = ' + json.dumps(sys.argv[2]) + '\n'
text += 'GC_DEMO_PYTHON = ' + json.dumps(sys.argv[5]) + '\n'
text += f'\n[[patches.agent]]\ndir = "paint"\nname = "gc.implementation-worker"\nmax_active_sessions = {int(sys.argv[4])}\n'
path.write_text(text)
PYCONFIG
git -C "$app" add .gc/scripts .gc/gascity package.json package-lock.json .gitignore || return
git -C "$app" commit -m "Add native checks and test tooling" || return
printf '\nReady: cd .demo-runs/04/city, then gc start.\n'
