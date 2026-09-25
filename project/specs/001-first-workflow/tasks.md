# 001: Tasks and evidence

Status: Not started — product definition comes next.

## Product definition tasks

Each task is estimated at 2–5 minutes of agent work, including its check; user discussion may take longer.

- [ ] D-1: Capture the intended user and problem in [the brief](../../docs/brief.md). Check that confirmed facts and assumptions are distinct.
- [ ] D-2: Describe one success scenario for AC-1 in [the spec](spec.md). Check that starting state, action, and observable outcome are explicit.
- [ ] D-3: Define a failure scenario for AC-2. Check that expected feedback and resulting state are explicit.
- [ ] D-4: Define included and excluded behavior. Check that the scope supports the selected workflow.
- [ ] D-5: Review the draft for contradictions and unresolved requirements. Record open questions and present the spec for user review.

After the written spec is approved, complete [the implementation plan](plan.md) and add implementation tasks sized at 2–5 minutes each, with acceptance-criterion IDs and verification steps. Product definition alone does not complete the feature.

## Evidence

None yet. Add links to relevant code, tests, and logs as work is completed.

## Next step and unresolved issues

Next step: Define the product and first user workflow. The app's behavior and technology stack remain undecided.

Pending collaboration choices: [DEC-001: log location](../../../DECISIONS.md#dec-001-location-of-the-decision-log) and [DEC-002: entry format](../../../DECISIONS.md#dec-002-decision-entry-format). Review these one at a time through Ask User Question when requested.

Panel setup choices still await review in [DEC-003: source and installation](../../../DECISIONS.md#dec-003-panel-skill-source-and-installation) and [DEC-004: alignment threshold](../../../DECISIONS.md#dec-004-panel-alignment-threshold). [DEC-005: panelist lenses](../../../DECISIONS.md#dec-005-panelist-lenses) preserves the redirection history and latest user-approved design: blind generalists with private emphases on user value, implementation, and assumptions and risks. That design needs no repeat approval; its skill validation, independent review, and prompt-replay evidence are recorded there.
