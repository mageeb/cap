#!/usr/bin/env bash
# Run this COPY from an isolated demo's .harness/, never from CAP's source tree.
# Bash chooses the next pass. The fresh agent only changes the paint app.
set -euo pipefail
cd "$(dirname "$0")/.."

# ${NAME:-default} means: use the environment setting, or this default.
passes="${ITERATIONS:-3}"
seconds="${PASS_SECONDS:-180}"
check_seconds="${CHECK_SECONDS:-60}"
case "$passes" in ''|*[!0-9]*) echo 'ITERATIONS must be 1..100'; exit 1;; esac
if [ "$passes" -lt 1 ] || [ "$passes" -gt 100 ]; then
  echo 'ITERATIONS must be 1..100'; exit 1
fi

# Refuse CAP itself, a published repo, or unaccepted changes from a prior pass.
if [ "$(git rev-parse --show-toplevel)" != "$PWD" ] ||
   [ "$(git branch --show-current)" != 'feature/paint-loop' ] ||
   [ -n "$(git remote)" ] || [ ! -f .harness/prompt.txt ]; then
  echo 'STOP: use the isolated fixture created by the runbook.'; exit 1
fi
if [ -n "$(git status --porcelain -- app ledger.md)" ]; then
  echo 'STOP: inspect unaccepted app or ledger changes before starting.'; exit 1
fi

run="$(date -u +%Y%m%dT%H%M%SZ)-$$"
worker=''
# $! is the last background process's PID. On exit, stop it and await cleanup.
cleanup() {
  if [ -n "$worker" ]; then
    kill -TERM "$worker" 2>/dev/null || true
    wait "$worker" 2>/dev/null || true
  fi
}
trap cleanup EXIT
trap 'exit 130' INT TERM

# One trip through this loop is one NEW conversation, with no resume command.
for ((pass=1; pass<=passes; pass++)); do
  id="$run-$(printf '%03d' "$pass")"
  dir="$PWD/artifacts/$id"
  mkdir -p "$dir"

  echo "[1/6] Pass $pass/$passes: same request + current ledger"
  cat .harness/prompt.txt ledger.md > "$dir/prompt.txt"

  echo '[2/6] Call a fresh agent; wait for its result'
  python3 .harness/agent.py "$PWD/app" "$dir/prompt.txt" "$dir/report.json" \
    "$seconds" > "$dir/events.jsonl" 2> "$dir/agent.stderr" &
  worker=$!
  if ! wait "$worker"; then
    echo "STOP: agent failed; inspect $dir and working code"; exit 1
  fi
  worker=''

  echo '[3/6] Save the diff and enforce the 1,000-line budget'
  [ -f app/index.html ] || { echo 'STOP: app/index.html is missing'; exit 1; }
  git add -N app # Include new files in the diff without accepting their content.
  git diff -- app > "$dir/app.diff"
  lines="$(python3 .harness/changes.py)"
  if [ "$lines" -gt 1000 ]; then
    echo "STOP: $lines changed lines; inspect $dir"; exit 1
  fi

  echo '[4/6] Launch the app, check drawing, and save a screenshot'
  # Track the check helper too: a stuck browser must not block this loop forever.
  python3 .harness/check.py "$check_seconds" node .harness/capture.cjs app \
    "$dir/screen.png" > "$dir/checks.txt" 2>&1 &
  worker=$!
  if ! wait "$worker"; then
    echo "STOP: inspect $dir/checks.txt and working code"; exit 1
  fi
  worker=''

  echo '[5/6] Record agent claims separately from observed checks'
  node .harness/journal.cjs "$dir" "$id"

  echo '[6/6] Save a local checkpoint; then start the next pass'
  git add app ledger.md
  git commit -m "Paint iteration $id; human acceptance pending"
  git rev-parse HEAD > "$dir/checkpoint.txt"
  echo "CHECKPOINT $id: $lines changed lines; $dir/screen.png"
done
