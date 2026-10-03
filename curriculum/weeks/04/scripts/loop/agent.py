"""One fresh Codex call, with a time limit and process-group cleanup."""
import os
import signal
import subprocess
import sys

app, prompt, output, seconds = sys.argv[1:]
try:
    seconds = int(seconds)
    if seconds <= 0:
        raise ValueError
except ValueError:
    raise SystemExit('PASS_SECONDS must be a positive integer')
command = [
    'codex', 'exec', '--ignore-user-config', '--ephemeral',
    '--sandbox', 'workspace-write', '--disable', 'memories',
    '--disable', 'multi_agent', '-c', 'project_doc_max_bytes=0',
    '--json', '--output-schema', os.path.abspath('.harness/schema.json'),
    '-o', output,
]
if os.getenv('MODEL'):
    command += ['-m', os.environ['MODEL']]
command += ['-']  # Read the complete request from standard input.
process = None


def stop(*_):
    """Ask the whole call to stop; force it after a short grace period."""
    if process:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            pass
        # The leader can exit while a descendant ignores TERM. Clean its group
        # regardless of the leader's exit status, then reap the leader.
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        process.wait()
    sys.exit(124)


signal.signal(signal.SIGTERM, stop)
signal.signal(signal.SIGINT, stop)
# Automatic parent instructions are disabled: the explicit prompt is the task.
with open(prompt) as request:
    process = subprocess.Popen(
        command, cwd=app, stdin=request, start_new_session=True,
    )
try:
    sys.exit(process.wait(timeout=seconds))
except subprocess.TimeoutExpired:
    stop()
