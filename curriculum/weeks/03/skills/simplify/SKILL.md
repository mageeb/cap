---
name: simplify
description: Reduce unnecessary complexity after behavior works, while preserving the approved requirements and verified behavior.
---

# Simplify

Simplification happens **after** the feature works and before final delivery.

## Procedure

1. Read the spec and current verification evidence.
2. Inspect the diff for:
   - duplicated logic;
   - custom helpers where repository primitives already exist;
   - unnecessary state;
   - premature abstractions;
   - one-use indirection;
   - complicated control flow;
   - comments that explain code complexity instead of removing it.
3. Do not remove required edge-case handling merely to make code shorter.
4. Propose simplifications before applying them.
5. Prefer changes that reduce concepts/files/branches while preserving behavior.
6. After applying a simplification, rerun the most relevant checks.

## Output

- proposed simplifications with rationale
- which were applied / rejected
- verification rerun and results
- any remaining complexity that is justified by requirements
