---
name: code-review
description: Review a code diff against its requirements and repository context. Classify evidence-backed findings; a clean review is a valid result.
---

# Code Review

Review the change; do not search for criticism merely to produce output.

## Inputs

- current Git diff
- goal/spec/acceptance criteria
- nearby code and tests
- verification results already run

## Procedure

1. Read the goal/spec first.
2. Inspect the diff and enough surrounding code to understand behavior.
3. Check:
   - requirement mismatches;
   - correctness and edge cases;
   - unintended behavior/regressions;
   - security/privacy/data risks;
   - error handling and state restoration;
   - tests that are missing or assert the wrong thing;
   - unnecessary scope or architectural inconsistency.
4. Validate important claims with code references or commands when practical.
5. Classify each finding:
   - **Blocking** — should be fixed before merge;
   - **Important** — meaningful issue, likely should be fixed;
   - **Optional** — maintainability/style suggestion with low risk.
6. Explicitly say **no blocking findings** when the evidence supports a pass.
7. Do not make code changes unless asked after the review.

## Output

For every finding include:

- severity
- file/symbol
- evidence
- why it matters
- smallest reasonable fix

End with verification gaps and an overall factual summary, not a score.
