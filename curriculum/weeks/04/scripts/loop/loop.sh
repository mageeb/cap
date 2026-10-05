#!/usr/bin/env bash
# A fresh conversation each time; only the files carry context forward.
set -euo pipefail
ITERATIONS=3
mkdir -p app artifacts
python3 -m http.server 4173 --bind 127.0.0.1 --directory app > artifacts/server.log 2>&1 &
server=$!
trap 'kill "$server" 2>/dev/null || true' EXIT
printf '<!doctype html><title>Paint history</title><h1>Paint history</h1>\n' > artifacts/history.html
for ((pass=1; pass<=ITERATIONS; pass++)); do
  number=$(printf '%03d' "$pass")
  echo "Pass $number: fresh agent call"
  # Workers skip personal browser tools; only the harness captures headless screenshots.
  cat prompt.txt | codex exec --ephemeral --sandbox workspace-write --ignore-user-config \
    --disable plugins --disable apps --disable browser_use --disable computer_use - \
    2>&1 | tee "artifacts/$number.log"
  kill -0 "$server"
  npx playwright screenshot --full-page http://127.0.0.1:4173 "artifacts/$number.png"
  printf '<h2>Pass %s</h2><img width="100%%" src="%s.png">\n' \
    "$number" "$number" >> artifacts/history.html
  echo "Saved artifacts/$number.png"
done
# Harden here: verify drawing, enforce file/line limits, and add deadlines.
