---
name: commit-push-pr
description: Safely finish reviewed work by checking repository state, verifying the change, committing only intended files, pushing the feature branch, and creating a pull request.
---

# Commit, Push, PR

Do not treat delivery as a blind sequence of Git commands.

## Procedure

1. Confirm repository, branch, and remotes.
2. Ensure the branch is not the protected/default branch unless explicitly intended.
3. Inspect:
   - `git status`
   - `git diff --stat`
   - full diff or staged diff
4. Confirm the required verification has actually run; do not claim tests that were not run.
5. Exclude unrelated/unreviewed files and secrets.
6. Stage only intended files.
7. Create a concise commit message describing the behavior change.
8. Push the current feature branch to the user-owned `origin`.
9. Create a PR against the intended repository/base branch.
10. PR body should include:
    - problem/outcome;
    - implementation summary;
    - verification performed;
    - manual test steps;
    - known limitations / follow-ups.
11. Return the PR URL and final Git status.

## Safety checks for this course

- Never push to `excalidraw/excalidraw` upstream.
- The PR target should be the instructor/student’s own Excalidraw fork.
- Do not use `--force` unless the human explicitly approves and understands why.
- Do not stage generated secrets, `.env` files, or unrelated local artifacts.
