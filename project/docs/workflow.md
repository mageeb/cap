# Project workflow and session handoffs

Use this guide to take the classroom application from an idea to verified code. Follow the [repository instructions](../../AGENTS.md) and [project instructions](../AGENTS.md) throughout.

**Sequence:** Load context → brainstorm → write the spec → plan tasks → implement and review → save a handoff.

## Before starting a new session

Open this repository in your local session. The complete setup currently lives on `feature/panel-skill`, including the scaffold and panel. Start from that branch or a later branch containing the setup, then create a new feature branch for product work. A local session in the same directory can see uncommitted files; another worktree or clone needs them committed first. Push the branch before using a remote checkout.

Superpowers must be installed and enabled in the environment running the agent; installing it on one machine does not put the plugin in Git. Ask the session to confirm skill availability. The prompts below can also be followed directly if the plugin is unavailable.

Before escalating a question, the agent uses [panel](../../skills/panel/SKILL.md): three independent answers, followed by one exchange of first answers if they do not align. The coordinator decides whether to proceed provisionally or ask you. Required user input and approval still go through Ask User Question, with dependent work paused until you answer. Autonomous choices and panel reasoning go into the shared [decision log](../../DECISIONS.md) as pending user review; successful checks and panel agreement do not count as your confirmation.

Round 1 runs three Codex generalists in parallel with fresh conversation contexts. Each considers the whole question using the same neutral packet; only its own private emphasis differs: user value, implementation, or assumptions and risks. None learns about the other emphases, peers, or the coordinator's preferred direction. These perspectives come from one model family and do not guarantee independent errors. For a complete panel, if all three align under compatible conditions without a material objection, skip round 2; otherwise, the same agents exchange all complete first answers once, optionally with an identical, clearly labeled coordinator view. The coordinator makes the final decision.

The tracked skill is in `skills/panel/SKILL.md`. This machine discovers it through a link at `~/.codex/skills/panel`; that link depends on this checkout remaining at its current path on a branch containing the skill. A new environment can read the tracked file directly or install the skill there.

## 1. Load context and brainstorm

Paste this into a fresh session, replacing the idea placeholder:

```text
We are building one classroom application in this repo.
Read AGENTS.md, project/AGENTS.md, project/README.md,
project/docs/brief.md, project/docs/roadmap.md, and the files in
project/specs/001-first-workflow/ (or the active feature folder).
Check Git status and the branch. Preserve existing work and use a
feature branch based on the committed scaffold.
Briefly summarize the current state, applicable rules, and open decisions.
Use Superpowers brainstorming if available. Ask focused questions and
recommend the smallest complete user workflow. My idea is: [your idea].
Keep feature records in project/specs/NNN-short-name/ and product context
in project/docs/. Use these paths instead of duplicate plugin documents.
We are brainstorming; do not implement application code yet.
```

**Check:** The session identifies the existing scaffold and distinguishes agreed requirements from open questions. Correct its understanding before choosing an approach.

## 2. Write and review the specification

```text
The proposed direction looks right. Update the product brief and roadmap.
Write the first feature's spec.md in its existing feature folder, with
scope, exclusions, and numbered success and failure criteria.
Show me the written spec for review before planning implementation.
```

**Check:** You can explain the user workflow and judge each acceptance criterion by observing behavior. Resolve material ambiguities and review the written spec before approving it. Rename the starter feature folder when its purpose is known and repair its links.

## 3. Plan the approved feature

```text
I approve this written spec. Use Superpowers writing-plans if available
to update this feature's plan.md and tasks.md. Each task must take an
estimated 2–5 minutes including verification, name its affected files,
and link to acceptance criteria. Use stable task IDs shared by both files;
keep detailed steps in the plan and completion status in tasks.md.
Recommend the execution approach and show me the plan before coding.
```

**Check:** Every criterion has planned implementation and verification. Split tasks estimated over five minutes. The repo's task-size rules apply even if a plugin normally assigns 2–5 minutes to individual steps inside a larger task.

## 4. Implement and review

```text
I approve the plan. Implement the agreed feature using subagents where
they save time, with independent review and an adversarial pass.
Follow our testing, branch, and commit rules. Keep tasks.md current.
Continue through the approved scope; ask for clarification only when a
material decision cannot be resolved from the spec and existing context.
```

**Check:** Compare the working behavior with the spec, inspect actual check results, and review the diff. Each commit needs its own review and validation. Keep commits at or below 100 changed lines when practical; record justified exceptions.

## 5. Save a handoff or resume

Before leaving a session:

```text
Update the active feature's tasks.md with completed and remaining work,
decisions, unresolved issues, actual check results, branch, and exact next
step. Commit the relevant work following our rules. Report any remaining
uncommitted files. Record autonomous choices in DECISIONS.md and link
relevant pending decision IDs from tasks.md. Do not rely on chat history
as the handoff.
```

To resume later:

```text
Read both AGENTS.md files and the active feature's spec, plan, and tasks.
Read the relevant pending entries in DECISIONS.md.
Check Git status and recent commits. Summarize the recorded state and
resume the next unfinished task within the previously agreed scope.
```

To review choices the agent made while you were away:

```text
Read DECISIONS.md and show me the decisions awaiting my review.
Reuse any completed panel for each decision; do not restart it merely
to ask for my confirmation. Include its reasoning and any dissent.
Use Ask User Question for one decision at a time. Explain the choice,
reason, and effect, then let me confirm, redirect, or defer it.
Wait for my answer before showing the next decision. Record my response
and any follow-up work; keep technical checks separate from my approval.
```

## Checking documentation correctness

- **Accuracy:** Compare claims with the actual files, commands, plugin availability, and Git state. Label drafts and planned behavior clearly.
- **Coherence:** Check that instructions, spec, plan, and tasks agree about scope, paths, sequence, and completion conditions.
- **Usability:** Walk through the instructions as a fresh session would. Confirm that each step has sufficient inputs and a checkable outcome.
- **Adversarial review:** Look for ways a reasonable reader could follow the words and get the wrong result, such as starting from a branch without the scaffold or treating a placeholder as a requirement.
- **Mechanical checks:** Validate relative links and whitespace. Before committing, inspect and check the staged snapshot; files present only in the working directory cannot satisfy a committed link.

These checks provide evidence and catch specific defects; they do not prove that prose has only one interpretation. Record what was checked and any unresolved uncertainty in the commit or task evidence.
