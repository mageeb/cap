#!/usr/bin/env bash
# Optional single fresh Claude pass; this is not a full loop adapter.
set -euo pipefail
if [ "$#" -ne 1 ]; then
  echo 'Usage: bash claude-pass.sh /absolute/CAP/.demo-runs/loop-NAME' >&2
  exit 1
fi
CAP_LOOP_DEMO="$1"
cd "$CAP_LOOP_DEMO"
if [ "$(git rev-parse --show-toplevel)" != "$PWD" ] ||
   [ "$(git branch --show-current)" != 'feature/paint-loop' ] ||
   [ -n "$(git remote)" ] || [ ! -f .harness/prompt.txt ]; then
  echo 'STOP: use the isolated Demo 01 fixture.' >&2
  exit 1
fi
set -euo pipefail
cd "$CAP_LOOP_DEMO/app"
claude --version
claude auth status
{ cat ../.harness/prompt.txt; cat ../ledger.md; } | \
  claude -p --safe-mode --no-session-persistence --permission-mode acceptEdits \
    --tools "Read,Glob,Grep,Edit,Write" --output-format json \
    --json-schema "$(cat ../.harness/schema.json)" > ../artifacts/claude-result.json
python3 - <<'PARSE_RESULT'
import json
from pathlib import Path
result = json.loads(Path('../artifacts/claude-result.json').read_text())
assert not result.get('is_error'), result
report = result.get('structured_output')
assert isinstance(report, dict) and all(isinstance(report.get(k), str) for k in ('implemented','envisioned','limitations')), 'Missing structured report'
Path('../artifacts/claude-report.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
PARSE_RESULT
cd ..
git add -N app
git diff --numstat -- app
node .harness/capture.cjs "$PWD/app" "$PWD/artifacts/claude-pass.png"
