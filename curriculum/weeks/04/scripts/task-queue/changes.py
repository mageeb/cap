"""Count added PLUS deleted app lines since the last accepted checkpoint."""
import subprocess

total = 0
summary = subprocess.check_output(['git', 'diff', '--numstat', '--', 'app'], text=True)
for entry in summary.splitlines():
    added, deleted, _ = entry.split('\t', 2)
    if added == '-' or deleted == '-':
        raise SystemExit('STOP: binary app changes cannot satisfy a line budget')
    total += int(added) + int(deleted)
print(total)
