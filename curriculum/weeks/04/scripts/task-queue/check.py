"""Run a browser check with an overall timeout, including stuck page code."""
import os
import signal
import subprocess
import sys

seconds, *command = sys.argv[1:]
try:
    seconds = int(seconds)
    if seconds <= 0 or not command:
        raise ValueError
except ValueError:
    raise SystemExit('CHECK_SECONDS must be positive, followed by a check command')
process = None


def stop(*_):
    """Stop Node and its browser group, even if Node already exited."""
    if process:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            pass
        # A browser descendant can outlive Node; always finish group cleanup.
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        process.wait()
    sys.exit(124)


signal.signal(signal.SIGTERM, stop)
signal.signal(signal.SIGINT, stop)
process = subprocess.Popen(command, start_new_session=True)
try:
    sys.exit(process.wait(timeout=seconds))
except subprocess.TimeoutExpired:
    print(f'STOP: browser check exceeded {seconds} seconds', file=sys.stderr)
    stop()
