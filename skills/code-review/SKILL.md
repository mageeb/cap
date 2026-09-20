---
name: code-review
description: Review code through Superpowers and an independent refuter, then classify ACCEPT, COMMENT, or REJECT with supported blockers and useful improvements. Use before committing executable changes or when asked for code review; ordinary prose changes need only a documentation sanity review unless requested otherwise.
---

# Code review with adjudication

The coordinator owns the final judgment. Use Superpowers to obtain a fresh review, challenge it, and distinguish blockers from worthwhile optional improvements. ACCEPT is a valid outcome; do not manufacture findings.

## 1. Bind the review to the change

- Read applicable repository instructions, the request, acceptance criteria, and relevant surrounding code. Identify required checks and their actual evidence.
- Record the scope before dispatch: staged changes, an explicit commit range, or a working-tree snapshot. Do not silently substitute the last commit for uncommitted work.
- For a staged review, freeze the index with `git write-tree`; record that tree ID and the current HEAD as base. Inspect `git diff <base> <tree>` and `git show <tree>:<path>`. A tree ID can replace the head commit in the Superpowers template's inspection commands.
- For committed work, record explicit base and head IDs. For working-tree work, freeze the relevant tracked and untracked files in a separate temporary directory and record the captured scope and content identities; do not stage or alter the user's files to capture it.
- Give both workers the same frozen source and relevant context. Working-tree fixes absent from the snapshot cannot resolve its defects. Run checks in an isolated materialization when the checkout differs; do not credit results from different content.
- Record excluded files and unavailable evidence. Recheck content identities before delivering a verdict or committing; changed content needs renewed review and relevant checks.

## 2. Invoke Superpowers requesting-code-review

- Locate the installed Superpowers `requesting-code-review` skill through the available skill inventory or plugin installation listing. Read its `SKILL.md` and sibling `code-reviewer.md` template; resolve the installed version rather than hardcoding a cache path.
- Dispatch its reviewer using the actual template, filled with the change description, requirements, base, and reviewed head/tree. Supply applicable instructions, source access, and check evidence. Use a fresh agent with no inherited conversation (`fork_turns: "none"` in Codex); omit the implementer's preferred verdict and session history.
- Add the calibration below to the prompt. Preserve Superpowers' strengths, severity categories, recommendations, assessment, and **Declined to judge** output. Give candidate findings stable IDs, file/line references, a reachable trigger, evidence, impact, and a proposed fix direction.
- The reviewer inspects the snapshot read-only, without editing, changing Git state, asking the user, or delegating. It reports missing context to the coordinator.
- If Superpowers or delegation is unavailable, disclose exactly what could not run. Use a clearly labeled manual fallback only when repository instructions allow it; never claim that Superpowers or an independent review ran. Otherwise report the missing required review as a verification blocker.

## 3. Refute independently

- After the review returns, dispatch a different fresh agent with the same snapshot, requirements, checks, and the complete review. It gets no coordinator preference and does not edit, ask the user, or delegate.
- For each candidate, inspect the code and attempt to disprove it: is it handled elsewhere, unreachable under the contract, pre-existing and unaffected, based on the wrong revision, or merely a preference? Challenge claimed severity and the cost of proposed fixes.
- Return each finding ID as **sustained**, **disproved**, or **uncertain**, with evidence and any corrected severity. Check whether declined judgments conceal real requirements or regressions.
- Independently check acceptance criteria and obvious missed blockers, including when the first review reports none. Challenge any newly discovered concern before reporting it; absence of reviewer findings is not proof of correctness.
- Keep unresolved assumptions explicit. A plausible concern without support is not a confirmed bug; essential missing evidence can still prevent acceptance.

## 4. Adjudicate and classify

- Evaluate both reports against the source and requirements; do not count votes or automatically accept a severity label. Resolve conflicts with focused inspection or a relevant check. The coordinator may reject either worker's conclusion.
- Retain supported findings, drop disproved ones with a short reason, and label remaining uncertainty. Adjudicate every item the reviewer declined to judge. Merge duplicate feedback.
- Apply these final categories, including to Superpowers' recommendations and Critical/Important/Minor labels:
  - **ACCEPT:** No actionable concerns; required checks and review evidence are satisfied.
  - **COMMENT:** Useful non-blocking feedback only; required checks and review evidence are satisfied.
  - **REJECT:** A supported blocker or missing required evidence. Distinguish a demonstrated defect from an unverified acceptance condition.
- An Important label alone does not require rejection: establish the concrete blocking effect. Conversely, a Minor label cannot excuse a demonstrated blocker.
- Return control to the implementer with the recommendation. This skill does not apply fixes, stage, commit, push, or submit an external review. After authorized fixes, review the changed snapshot again before claiming readiness.
- If essential user input remains necessary, follow the repository's panel/question workflow as coordinator. Review workers report gaps; they do not recursively run a panel. A review verdict never substitutes for required user approval.

## Calibration

- Review correctness, regressions, relevant security and data integrity, error handling, contract compatibility, and concrete maintainability costs. Read surrounding code before claiming a defect.
- Treat foreseeable failures seriously even when the spec is silent; label inferred expectations and respect explicit exclusions. Do not invent product features to justify rejection.
- Use SOLID principles or design patterns only to explain a concrete improvement whose benefit outweighs added complexity. Do not demand architectural ceremony or personal style preferences.
- Recommend performance work only with measurements or a clear cost at realistic scale. State the expected benefit and tradeoff of optional improvements.
- Distinguish introduced or exposed problems from unrelated existing debt. Report unrelated debt separately when useful; it is not automatically a blocker for this change.
- Judge tests by the behavior they verify. Report checks as passed, failed, or not run, and say what evidence is still missing. Never infer a passing check from a suggested command.

## Report

Keep the result concise and proportional to the change:

1. **Verdict and scope:** ACCEPT, COMMENT, or REJECT; snapshot/range and a short reason.
2. **Strengths:** Specific sound choices worth preserving, when useful.
3. **Blockers:** Retained findings with file/line, trigger, impact, evidence, and fix direction; separate missing required checks.
4. **Optional improvements:** Concrete benefit, tradeoff, and location; clearly non-blocking.
5. **Refuted or uncertain findings:** What was dropped or remains unresolved, and why; include adjudicated declined judgments where material.
6. **Verification:** Actual checks, outcomes, limitations, and whether Superpowers and the independent refuter ran.

Empty sections can be omitted or say “None.” Do not fill them to meet a quota.
