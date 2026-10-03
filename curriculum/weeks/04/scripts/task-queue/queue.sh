#!/usr/bin/env bash
# Run this COPY from an isolated demo's .harness/, never from CAP's source tree.
# Outer loop: next task. Inner loop: one attempt, then at most one repair.
set -euo pipefail
cd "$(dirname "$0")/.."
seconds="${PASS_SECONDS:-180}"
check_seconds="${CHECK_SECONDS:-60}"

# Require the disposable repository and reject unaccepted prior changes.
if [ "$(git rev-parse --show-toplevel)" != "$PWD" ] ||
   [ "$(git branch --show-current)" != 'feature/paint-task-queue' ] ||
   [ -n "$(git remote)" ] || [ ! -f .harness/queue.txt ]; then
  echo 'STOP: use the isolated fixture created by the runbook.'; exit 1
fi
if [ -n "$(git status --porcelain -- app ledger.md)" ]; then
  echo 'STOP: inspect unaccepted app or ledger changes before starting.'; exit 1
fi

run="$(date -u +%Y%m%dT%H%M%SZ)-$$"
worker=''
# The active Python helper stops its agent or browser group on interruption.
cleanup() {
  if [ -n "$worker" ]; then
    kill -TERM "$worker" 2>/dev/null || true
    wait "$worker" 2>/dev/null || true
  fi
}
trap cleanup EXIT
trap 'exit 130' INT TERM

# read takes the next line of queue.txt. No LLM chooses the task order.
while read -r task; do
  passed=0
  feedback='No prior check output for this task.'
  for attempt in 1 2; do
    dir="$PWD/artifacts/$run-task$task-attempt$attempt"
    mkdir -p "$dir"

    echo "[1/6] Task $task, attempt $attempt/2: request + prior feedback"
    cat .harness/contract.txt ledger.md ".harness/tasks/$task.txt" > "$dir/prompt.txt"
    printf '\nPrevious check feedback:\n%s\n' "$feedback" >> "$dir/prompt.txt"

    echo '[2/6] Call a fresh agent; wait for its result'
    python3 .harness/agent.py "$PWD/app" "$dir/prompt.txt" "$dir/report.json" \
      "$seconds" > "$dir/events.jsonl" 2> "$dir/agent.stderr" &
    worker=$!
    if ! wait "$worker"; then
      echo "STOP: agent failed; inspect $dir"; exit 1
    fi
    worker=''

    echo '[3/6] Measure all app changes since this task began'
    git add -N app # New files must also count against the budget.
    git diff -- app > "$dir/app.diff"
    lines="$(python3 .harness/changes.py)"
    # A failed first attempt has no commit, so its repair shares this budget.
    if [ "$lines" -gt 1000 ]; then
      echo "STOP: $lines changed lines exceed the task budget; inspect $dir"; exit 1
    fi

    echo '[4/6] Run this task AND all earlier browser criteria'
    # Track the browser check and give it an overall time limit.
    python3 .harness/check.py "$check_seconds" node .harness/check.cjs app \
      "$task" "$dir/screen.png" > "$dir/checks.txt" 2>&1 &
    worker=$!
    if wait "$worker"; then
      worker=''
      echo '[5/6] Record claims, observed checks, and the evidence path'
      node .harness/journal.cjs "$dir" "$task"

      echo '[6/6] Save a local checkpoint; then move to the next task'
      git add app ledger.md
      git commit -m "Queued task $task passed its browser criteria"
      git rev-parse HEAD > "$dir/checkpoint.txt"
      passed=1
      echo "DONE task $task: $dir"
      break # Leave the attempt loop: this task is now complete.
    fi
    worker=''

    # A check failure supplies facts to one fresh repair attempt, not a resume.
    feedback="$(cat "$dir/checks.txt")"
    echo "FAILED CHECK task $task: $dir/checks.txt"
  done

  # Do not advance the queue after two failed attempts.
  if [ "$passed" -ne 1 ]; then
    echo "STOP: task $task exhausted two attempts; human handoff"; exit 1
  fi
done < .harness/queue.txt
