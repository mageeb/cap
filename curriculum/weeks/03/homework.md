# Week 03 Homework — Agentic Development Workflow

## Objective
Use either Claude Code or Codex CLI to complete one small but non-trivial feature or fix in a repository you own or are authorized to modify. Practice the workflow, not just code generation.

## Required workflow

1. **Goal** — create a concise goal with success criteria and non-goals.
2. **Spec** — write observable requirements and edge cases.
3. **Explore** — ask the agent to inspect relevant code before planning.
4. **Plan** — map the spec to existing files/symbols and verification.
5. **Tasks** — create a checkable task list.
6. **Worktree** — perform the implementation on a feature branch in a separate Git worktree.
7. **Implement** — use the agent, but keep the diff scoped.
8. **Verify** — run relevant automated checks and one manual success/failure path when applicable.
9. **Code review** — review the diff against the spec. A clean review is acceptable.
10. **Simplify** — remove unnecessary complexity without changing the behavior.
11. **Durable context** — update `PROJECT_CONTEXT.md` (or equivalent).
12. **Fresh session test** — start a new AI session and ask it to reconstruct current state from the repository artifacts and Git evidence.
13. **PR** — commit, push, and create a pull request in a repository/fork you are authorized to use.

## Deliverables

Submit:

- repository / PR link;
- `GOAL.md`;
- `SPEC.md`;
- `PLAN.md`;
- `TASKS.md`;
- `PROJECT_CONTEXT.md`;
- a short note answering:
  1. What did the agent initially misunderstand or assume?
  2. What evidence changed the plan or implementation?
  3. What did the review or simplify step improve?
  4. Could a fresh session accurately continue from the saved context?

## Constraints

- Do not use a repository you are not authorized to modify.
- Do not put secrets, credentials, private production data, or confidential employer code into a public AI service.
- Do not merge purely because the AI says the change is correct.
- Do not count a test as “passed” unless it was actually run and its result observed.

## Evaluation rubric

| Area | Strong evidence |
|---|---|
| Goal/spec quality | observable behavior, clear non-goals, useful edge cases |
| Repository exploration | plan references real existing architecture rather than invented structure |
| Scope control | diff is focused; dependencies/architecture are not expanded casually |
| Verification | checks and manual evidence are documented honestly |
| Review judgment | findings are evidence-backed; optional suggestions are not treated as mandatory |
| Simplification | complexity reduced without losing requirements |
| Durable context | fresh session can reconstruct state and next step |
| Git workflow | isolated branch/worktree, clean commit, real PR |

## Optional challenge

Repeat the fresh-session handoff using the **other** coding CLI (Claude → Codex or Codex → Claude). Compare what information transferred well and what depended on tool-specific project instructions.
