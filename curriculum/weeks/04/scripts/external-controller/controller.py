# Demo 03: a program runs the accepted task graph; the Planner stays in chat.
# Read the main loop at the bottom, then start() and finish().
# Every attempt is a fresh agent with its own app copy. Only checked output
# from declared files reaches the shared app. No model is called to poll state.

from pathlib import Path
import hashlib
import json
import os
import shutil
import signal
import subprocess
import sys
import time
ROOT = Path(__file__).resolve().parent
APP, PLAN = (ROOT / 'app', ROOT / 'plan')
GRAPH = PLAN / 'task-graph.json'
EXPECTED = {
    'schema': ([], ['schema.json'], 'schema'),
    'ui': (['schema'], ['toolbar.js'], 'ui'),
    'storage': (['schema'], ['settings.js'], 'storage'),
    'integrate': (['ui', 'storage'], ['main.js'], 'integration'),
    'review': (['integrate'], [], 'review'),
}

# Hash both planning files so a changed plan needs new Human approval.
def digest():
    return hashlib.sha256(GRAPH.read_bytes() + (PLAN / 'prd.md').read_bytes()).hexdigest()

# This lesson has five fixed task types. The model can write clearer task
# prose, but it cannot choose arbitrary commands, ownership, or retry budgets.
def validate():
    graph = json.loads(GRAPH.read_text())
    if graph['version'] != 1:
        raise ValueError('Expected accepted version 1')
    tasks = graph['tasks']
    if len(tasks) != 5 or {t['id'] for t in tasks} != set(EXPECTED):
        raise ValueError('Five known task IDs required')
    used = set()
    for t in tasks:
        deps, owned, check = EXPECTED[t['id']]
        if sorted(t['deps']) != sorted(deps) or t['owned'] != owned or t['check'] != check:
            raise ValueError('Dependencies, ownership, or criterion violate this teaching contract')
        if type(t['max_attempts']) is not int or not 1 <= t['max_attempts'] <= 2:
            raise ValueError('Attempt limit must be 1 or 2')
        if not isinstance(t['task'], str) or not t['task'].strip():
            raise ValueError('Missing task instruction')
        for name in t['owned']:
            if Path(name).is_absolute() or '..' in Path(name).parts or name in used:
                raise ValueError('Unsafe or overlapping owned path')
            used.add(name)
    done = set()
    while len(done) < len(tasks):
        ready = {t['id'] for t in tasks if set(t['deps']) <= done} - done
        if not ready:
            raise ValueError('Dependency cycle or missing prerequisite')
        done |= ready
    return {t['id']: t for t in tasks}
# --validate and --approve prepare the run without starting any agents.
TASKS = validate()
if '--validate' in sys.argv:
    print('Valid graph and PRD hash:', digest())
    sys.exit(0)
if '--approve' in sys.argv:
    (PLAN / 'APPROVED').write_text(digest())
    print('Human-approved version 1:', digest())
    sys.exit(0)
if (PLAN / 'APPROVED').read_text().strip() != digest():
    raise ValueError('Human approval missing or stale')
# State survives a restart; an interrupted attempt still uses its budget.
STATE = ROOT / 'state.json'
state = json.loads(STATE.read_text()) if STATE.exists() else {
    'plan_hash': digest(),
    'phase': 'ready',
    'supervisor_calls': 0,
    'tasks': {i: {'status': 'pending', 'attempts': 0} for i in TASKS},
}
if state['plan_hash'] != digest():
    raise ValueError('Plan changed. Keep this run intact and start a new run.')
for record in state['tasks'].values():
    if record['status'] == 'running':
        record['status'] = 'pending'
        record.pop('candidate', None)
running = {}

# Replace the state file atomically, so readers see a complete JSON record.
def save():
    tmp = ROOT / 'state.tmp'
    tmp.write_text(json.dumps(state, indent=2))
    tmp.replace(STATE)

# Progress goes to the terminal and file. Only completion/blockage can wake
# the existing Planner chat, and file handoff still works if queue delivery fails.
def notify(message, terminal=False):
    (ROOT / 'notification.txt').write_text(message + '\n')
    print(message, flush=True)
    thread = os.environ.get('PLANNER_THREAD')
    if not terminal or not thread:
        return
    try:
        if os.environ.get('PLANNER_QUEUE_READY') != '1':
            raise RuntimeError('Queue command not preflighted')
        result = subprocess.run(['codex', 'queue', '--thread', thread, '--message', message], capture_output=True, text=True, timeout=15)
        if result.returncode:
            raise RuntimeError(result.stderr.strip() or result.stdout.strip())
        state['notification_delivery'] = 'queued to ' + thread
    except Exception as error:
        state['notification_delivery'] = 'failed: ' + str(error)
        print('Queue delivery failed; paste notification.txt into Planner:', error, flush=True)
    save()

# Build one fresh Codex call. Reviewer and Supervisor calls are read only.
# The external program, rather than native subagents, owns dispatch here.
def command(job, output, readonly=False, schema=None):
    args = [
        'codex', 'exec', '-C', str(job), '--ephemeral', '--sandbox',
        'read-only' if readonly else 'workspace-write',
        '--disable', 'memories', '--disable', 'multi_agent',
        '--ignore-user-config', '-c', 'project_doc_max_bytes=0',
        '--json', '-o', str(output),
    ]
    if schema:
        args += ['--output-schema', str(schema)]
    return args + ['-']

# Remember every unowned path and reject symlinks anywhere in the candidate.
# A Worker cannot change helpers to make its own unpublished code pass checks.
def snapshot(app, owned):
    result = {}
    for path in app.rglob('*'):
        name = path.relative_to(app).as_posix()
        if path.is_symlink():
            raise RuntimeError('Symlink in candidate: ' + name)
        if name not in owned:
            result[name] = hashlib.sha256(path.read_bytes()).hexdigest() if path.is_file() else 'directory'
    return result

# Stop the whole child process group, with a bounded wait before force-kill.
def stop(process):
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        return
    try:
        process.wait(timeout=5)
    except subprocess.TimeoutExpired:
        pass
    try:
        os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError:
        pass
    process.wait()

# Ctrl-C and termination follow the same state-saving cleanup as STOP.
def interrupt(signum, frame):
    raise RuntimeError('Human interrupt signal ' + str(signum))
signal.signal(signal.SIGINT, interrupt)
signal.signal(signal.SIGTERM, interrupt)

# Launch one attempt in an isolated candidate directory. On a check failure,
# only that task's owned files may carry forward into its fresh repair attempt.
def start(task_id):
    t, r = (TASKS[task_id], state['tasks'][task_id])
    if r['attempts'] >= t['max_attempts']:
        raise RuntimeError('Attempt budget exhausted: ' + task_id)
    r['attempts'] += 1
    job = ROOT / 'jobs' / f"{task_id}-{r['attempts']}"
    job.mkdir(exist_ok=False)
    shutil.copytree(APP, job / 'app')
    (job / 'package.json').write_text('{"type":"module"}')
    if r.get('candidate'):
        for name in t['owned']:
            candidate = Path(r['candidate']) / 'app' / name
            if candidate.is_file():
                shutil.copy2(candidate, job / 'app' / name)
    output = job / ('review.json' if task_id == 'review' else 'report.txt')
    evidence = (ROOT / 'artifacts' / 'integration-check.txt').read_text() if task_id == 'review' else ''
    prompt = (PLAN / 'prd.md').read_text() + '\nACCEPTED TASK:\n' + json.dumps(t) + '\n'
    prompt += 'Work only inside app/. Publish only declared owned files. Human-owned checks are outside this sandbox.\n'
    prompt += 'Do not edit, add, delete, or link any unowned app path; those changes fail before checks.\n'
    prompt += 'Prior failure: ' + r.get('error', 'none') + '\nIndependent evidence:\n' + evidence
    before = snapshot(job / 'app', t['owned'])
    log = open(job / 'events.jsonl', 'w')
    process = subprocess.Popen(command(job, output, task_id == 'review', ROOT / 'review-schema.json' if task_id == 'review' else None), stdin=subprocess.PIPE, stdout=log, stderr=log, text=True, start_new_session=True)
    running[task_id] = (process, job, output, log, time.monotonic(), before)
    r.update(status='running', job=str(job))
    save()
    process.stdin.write(prompt)
    process.stdin.close()
    notify('Started ' + task_id)

# The program runs Human-owned checks outside the Worker's app copy.
# Review checks repeat actual storage and browser checks, not just model claims.
def check(t, app, output):
    args = [sys.executable, str(ROOT / 'checks.py'), t['check'], str(app)]
    if t['id'] == 'review':
        args += [str(output)]
    process = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, start_new_session=True)
    try:
        started = time.monotonic()
        while process.poll() is None:
            if (ROOT / 'STOP').exists() or time.monotonic() - started > 100:
                raise RuntimeError('Independent check stopped or timed out')
            time.sleep(0.3)
        out, err = process.communicate()
        return subprocess.CompletedProcess(args, process.returncode, out, err)
    finally:
        stop(process)

# At the first integration failure, ask one fresh read-only agent for judgment.
# It may request the existing storage repair or block; it cannot rewrite the graph.
def supervisor(error):
    job = ROOT / 'jobs' / 'supervisor'
    job.mkdir(exist_ok=False)
    shutil.copytree(APP, job / 'app')
    output = job / 'assignment.json'
    prompt = 'Judge this integration failure read only. Accepted interface is {tool,color}. '
    prompt += 'Return repair_task=storage only if evidence supports that cause. Otherwise return repair_task=blocked with the reason.\n' + error
    with open(job / 'events.jsonl', 'w') as log:
        p = subprocess.Popen(command(job, output, True, ROOT / 'supervisor-schema.json'), stdin=subprocess.PIPE, stdout=log, stderr=log, text=True, start_new_session=True)
        try:
            p.stdin.write(prompt)
            p.stdin.close()
            started = time.monotonic()
            while p.poll() is None:
                if (ROOT / 'STOP').exists() or time.monotonic() - started > 90:
                    raise RuntimeError('Supervisor stopped or timed out')
                time.sleep(0.3)
        finally:
            stop(p)
    if p.returncode:
        raise RuntimeError('Supervisor invocation failed')
    assignment = json.loads(output.read_text())
    if assignment.get('repair_task') != 'storage' or not assignment.get('reason'):
        raise RuntimeError('Supervisor blocked: ' + assignment.get('reason', 'invalid assignment'))
    return assignment

# Collect a finished agent, reject unowned edits, then run the independent gate.
# Publish only regular owned files after success; retain failed candidates/logs.
def finish(task_id):
    p, job, output, log, _, before = running.pop(task_id)
    stop(p)
    log.close()
    t, r = (TASKS[task_id], state['tasks'][task_id])
    unchanged = snapshot(job / 'app', t['owned']) == before
    result = check(t, job / 'app', output) if p.returncode == 0 and unchanged else None
    report = result.stdout + result.stderr if result else 'Unowned paths changed; checks refused' if not unchanged else 'Codex invocation failed; inspect events.jsonl'
    (job / 'check.txt').write_text(report)
    if result and result.returncode == 0:
        for name in t['owned']:
            source = job / 'app' / name
            if not source.is_file() or source.is_symlink():
                raise RuntimeError('Missing or unsafe output: ' + name)
            shutil.copy2(source, APP / name)
        r.update(status='passed', error='')
        if task_id == 'integrate':
            shutil.copy2(job / 'check.txt', ROOT / 'artifacts' / 'integration-check.txt')
            shutil.copytree(job / 'screenshots', ROOT / 'artifacts' / 'screenshots', dirs_exist_ok=True)
        notify('Passed ' + task_id)
    else:
        r.update(status='pending', error=report, candidate=str(job))
        if task_id == 'integrate' and state['supervisor_calls'] == 0:
            state['supervisor_calls'] += 1
            save()
            notify('Integration failed; waking one Supervisor')
            assignment = supervisor(report)
            repair = state['tasks']['storage']
            repair.update(status='pending', error=assignment['reason'])
        if r['attempts'] >= t['max_attempts']:
            raise RuntimeError('Check failed at attempt limit: ' + task_id)
    save()
# MAIN LOOP: ordinary Python chooses work, polls processes, and records progress.
try:
    state['phase'] = 'running'
    save()
    while True:
        # 1. Keep the approved plan fixed, and honor the Human stop request.
        if digest() != state['plan_hash']:
            raise RuntimeError('Accepted plan changed during execution')
        if (ROOT / 'STOP').exists():
            raise RuntimeError('Human STOP requested')
        # 2. Poll existing processes; a poll does not start an LLM session.
        for task_id, (p, job, output, log, started, before) in list(running.items()):
            if time.monotonic() - started > 180 and p.poll() is None:
                stop(p)
            if p.poll() is not None:
                finish(task_id)
        # 3. A pending task is ready only after all prerequisites pass.
        ready = [
            i for i, t in TASKS.items()
            if state['tasks'][i]['status'] == 'pending'
            and all(state['tasks'][d]['status'] == 'passed' for d in t['deps'])
        ]
        # 4. Start each ready task. UI and storage can run at the same time.
        for task_id in ready:
            # The instructor injects the prepared defect only at this boundary.
            if task_id == 'integrate' and (ROOT / 'PAUSE_INTEGRATION').exists():
                state['phase'] = 'paused before integration'
                save()
                continue
            state['phase'] = 'running'
            start(task_id)
        # 5. End after all checks pass; Human acceptance still comes afterward.
        if all((r['status'] == 'passed' for r in state['tasks'].values())):
            state['phase'] = 'completed'
            save()
            notify('Completed: inspect evidence before Human acceptance', terminal=True)
            break
        time.sleep(0.3)
except Exception as error:
    # Any failure stops active jobs and leaves a readable record for the Human.
    for p, job, output, log, started, before in running.values():
        stop(p)
        log.close()
    for record in state['tasks'].values():
        if record['status'] == 'running':
            record['status'] = 'pending'
            record.pop('candidate', None)
    state['phase'] = 'blocked'
    state['reason'] = str(error)
    save()
    notify('Blocked: ' + str(error), terminal=True)
    sys.exit(1)
