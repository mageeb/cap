"""A program dispatches the planner's graph; every task is a fresh agent."""
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path
import json
import subprocess

AGENT = "codex"  # Change to "claude" to use Claude Code.
ROOT = Path(__file__).resolve().parent
TASKS = json.loads((ROOT / "graph.json").read_text())["tasks"]
LOGS = ROOT / "logs"
LOGS.mkdir(exist_ok=True)


def run(task):
    name = task["id"]
    report = LOGS / f"{name}.txt"
    prompt = (ROOT / task["prompt"]).read_text()
    prompt += "\nRead prd.md. Keep all work inside this demo folder."
    prompt += f"\nEdit only: {', '.join(task['files']) or 'no files (review only)'}."
    prompt += "\nDo not run Git, install dependencies, or change the plan or prompts."
    prompt += "\nEnd with DONE or TLDR: DONE if requirements are met; otherwise end with BLOCKED."
    command = [
        "codex", "exec", "--ephemeral", "--sandbox",
        "workspace-write" if task["files"] else "read-only",
        "--skip-git-repo-check", "-o", str(report), "-",
    ]
    print(f"START {name}", flush=True)
    with (LOGS / f"{name}.log").open("w") as log:
        if AGENT == "claude":
            tools = "Read,Glob,Grep" + (",Edit,Write" if task["files"] else "")
            command = ["claude", "-p", "--no-session-persistence",
                       "--permission-mode", "dontAsk", "--tools", tools,
                       "--allowedTools", tools, "--disallowedTools", "mcp__*",
                       "Follow the task supplied on stdin."]
            with report.open("w") as final:
                result = subprocess.run(command, input=prompt, text=True,
                                        cwd=ROOT, stdout=final, stderr=log)
        else:
            result = subprocess.run(command, input=prompt, text=True,
                                    cwd=ROOT, stdout=log, stderr=subprocess.STDOUT)
    reply = report.read_text().strip() if report.exists() else ""
    files_exist = all((ROOT / file).is_file() for file in task["files"])
    status = reply.splitlines()[-1] if reply else ""
    if result.returncode or not files_exist or status not in {"DONE", "TLDR: DONE"}:
        raise RuntimeError(f"{name} incomplete; read logs/{name}.log and .txt")
    print(f"DONE  {name}", flush=True)
    return name


# Ready nodes run together; joining this batch releases dependent nodes.
done = set()
while len(done) < len(TASKS):
    ready = [task for task in TASKS
             if task["id"] not in done and set(task["deps"]) <= done]
    if not ready:
        raise SystemExit("Graph stalled: check dependency names and cycles.")
    with ThreadPoolExecutor(max_workers=len(ready)) as pool:
        jobs = [pool.submit(run, task) for task in ready]
        for job in as_completed(jobs):
            done.add(job.result())

print("Graph completed. Read logs/review.txt, then try the app.")
# A larger controller could add retries, durable state and cancellation later.
