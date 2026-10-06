#!/usr/bin/env bash
# Read the next task; allow one attempt and one fresh repair.
set -euo pipefail
mkdir -p app artifacts
python3 -m http.server 4173 --bind 127.0.0.1 --directory app > artifacts/server.log 2>&1 &
server=$!
trap 'kill "$server" 2>/dev/null || true' EXIT
printf '<!doctype html><title>Task history</title><h1>Task history</h1>\n' > artifacts/history.html
while read -r task output; do
  passed=0
  feedback='No earlier failure for this task.'
  for attempt in 1 2; do
    dir="artifacts/$task-$attempt"
    mkdir -p "$dir"
    cat prompt.txt "tasks/$task.txt" > "$dir/prompt.txt"
    printf '\nPrevious result:\n%s\n' "$feedback" >> "$dir/prompt.txt"
    echo "Task $task, attempt $attempt"
    # Workers skip personal browser tools; only the harness captures headless screenshots.
    if codex exec --ephemeral --sandbox workspace-write --ignore-user-config \
      --disable plugins --disable apps --disable browser_use --disable computer_use - \
      < "$dir/prompt.txt" 2>&1 | tee "$dir/agent.log" && test -s "$output"; then
      passed=1
    fi
    echo "Required nonempty file: $output; result=$passed" | tee "$dir/checks.txt"
    kill -0 "$server"
    # Drag the same stroke: it appears when drawing code works.
    node capture.mjs http://127.0.0.1:4173 "$dir/screen.png"
    printf '<h2>Task %s, attempt %s (file check=%s)</h2><img width="100%%" src="%s/screen.png">\n' \
      "$task" "$attempt" "$passed" "$task-$attempt" >> artifacts/history.html
    if [ "$passed" = 1 ]; then
      printf 'Task %s produced %s. Evidence: %s\n' "$task" "$output" "$dir" >> ledger.md
      echo "DONE task $task"
      break
    fi
    feedback="The previous call or file check failed. Produce a nonempty $output."
  done
  [ "$passed" = 1 ] || { echo "STOP: task $task failed twice"; exit 1; }
done < queue.txt
# Harden here: verify behavior with browser tests, preserve earlier checks, add deadlines.
