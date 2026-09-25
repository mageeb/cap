# Autonomous decisions awaiting user review

This log separates agent choices from user-confirmed requirements. It starts with the introduction of this workflow; it is not a retrospective audit of earlier sessions. Follow the [repository rules](AGENTS.md#autonomous-decisions-and-user-review).

Each entry has a stable `DEC-###` ID, task or scope, status, choice, reason, alternatives, effects or links, technical verification, and user response. Add new entries without reusing IDs. Keep reviewed entries as history.

Statuses: **Pending user review**, **Confirmed**, or **Redirected**. Passing checks does not change user-review status. A deferred decision stays pending. For a redirect, record the replacement direction and follow-up work separately from its completion evidence.

## DEC-001: Location of the decision log

- **Task / scope:** Collaboration rules; repository and classroom application.
- **Status:** Pending user review.
- **Choice:** Keep a single log in the repository root at `DECISIONS.md`.
- **Reason:** Fresh sessions can find both repository and application choices through the root instructions.
- **Alternative:** Separate logs for repository work and application work.
- **Effects:** [Root instructions](AGENTS.md), [project instructions](project/AGENTS.md), and [handoff guide](project/docs/workflow.md) link here. Feature task records link to relevant decision IDs.
- **Technical verification:** Independent sanity and adversarial review passed; all 24 local links in the staged changes, including three anchors, resolve.
- **User response:** Not reviewed.

## DEC-002: Decision entry format

- **Task / scope:** Collaboration rules; decision tracking.
- **Status:** Pending user review.
- **Choice:** Use Markdown entries with stable IDs and separate user-review status, verification evidence, and response fields.
- **Reason:** This records whether a choice works without implying the user has agreed to it, and preserves corrections across sessions.
- **Alternative:** A compact table with the same fields.
- **Effects:** New entries follow the fields above; confirmation or redirection updates the existing entry while preserving the original choice.
- **Technical verification:** Independent review and mechanical checks passed; decision IDs are unique, and both choices remain pending user review.
- **User response:** Not reviewed.

## DEC-003: Panel skill source and installation

- **Task / scope:** Panel skill; source control and local discovery.
- **Status:** Pending user review.
- **Question / options:** Maintain the skill globally only (A), track it in this repo and link it globally (B), track it and keep a separate global copy (C), or justify another approach.
- **Facts:** The user requested a reusable skill; skill-creator defaults to `~/.codex/skills`; no existing `panel` installation was found.
- **Round 1:** Three agents received the same packet with no inherited conversation; no peer answers were supplied, and reads were restricted to the packet. A (user outcome) chose B for one reviewable source; B (implementation/evidence) chose B to avoid installation drift; C (risks/alternatives) chose B to retain shared history without synchronizing copies.
- **Alignment / dissent:** All three agreed, conditional on verifying discovery. All flagged that moving this checkout or switching to a branch without the skill can break the link. No dissent; round 2 skipped.
- **Coordinator choice / reason:** B: maintain `skills/panel/SKILL.md` and link `~/.codex/skills/panel` to its directory. One maintained source meets repository and personal discovery needs; accept the documented checkout dependency provisionally.
- **Alternative:** A separate personal installation if independence from this checkout becomes necessary.
- **Effects:** The skill source is versioned here; this machine's link is not distributed by Git. Other environments can read the tracked skill directly or install it locally. Keep the checkout at its current path on a branch containing the skill while using this link.
- **Technical verification:** Skill validator passed; a fresh native Codex `skills/list` returned one enabled `panel` skill at the tracked source. Independent skill review passed; the integration review corrected the wording about shared-context isolation.
- **Later clarification:** This earlier exercise withheld peer answers but disclosed panel participation and assigned lenses. It does not establish compliance with the user's stricter blind-round requirement recorded in DEC-005.
- **User response:** Not reviewed.

## DEC-004: Panel alignment threshold

- **Task / scope:** Panel skill; deciding whether to run round 2.
- **Status:** Pending user review.
- **Choice:** Alignment means all three recommend the same action under compatible conditions with no unresolved material objection; a 2–1 split triggers round 2.
- **Reason:** This preserves independent dissent before using the user's second and final round.
- **Alternative:** Treat a simple majority as aligned.
- **Effects:** The coordinator still decides after either alignment or two rounds, and may reject a unanimous recommendation.
- **Technical verification:** Independent replay passed a 2–1 split through round 2, stopped with dissent, and retained coordinator judgment. It also preserved requested user review and required spec approval, and rejected consensus with a missing reply. This was a replay, not a live two-round run.
- **User response:** Not reviewed.

## DEC-005: Panelist lenses

- **Task / scope:** Panel skill; the three default perspectives.
- **Status:** Redirected.
- **Choice:** Use user outcome, implementation/evidence, and risks/alternatives as complementary lenses; each agent evaluates all choices.
- **Reason:** These cover practical tradeoffs without assigning agents positions to defend.
- **Alternative:** Three generalists with identical roles.
- **Effects:** Round 1 shares the same question and evidence with all three, but gives each a different lens. Dissent is optional.
- **Technical verification:** A live round-1 exercise used all three lenses and reached alignment; independent skill review found no blocker in their roles or the blind-context instructions.
- **User response:** The user clarified that round-1 agents must receive an unsteered question, without knowing about each other or the coordinator's direction. Round 2 may reveal all first answers and the coordinator's tentative view; the coordinator retains the final decision.
- **Replacement / follow-up:** Remove assigned lenses and panel-revealing language from round-1 dispatch. Send identical neutral packets; preserve full answer exchange only for a needed second round and label any coordinator view separately.
- **Follow-up verification:** Independent review and prompt-generation replay passed: identical neutral first requests omitted panel and coordinator cues; second requests included all supplied first answers and a labeled coordinator view. Skill validation, staged links, anchors, and whitespace passed. This verified authored prompts, not live runtime isolation.
- **Latest user-approved direction:** Use three generalists with private emphases on user value, implementation, and assumptions and risks. Each considers the whole question; the neutral round-1 packet differs only in the recipient's own emphasis and reveals no peers, other emphases, or coordinator preference. Different private emphases are compatible with a blind first round; removing all emphases was stricter than the user intended. This supersedes the earlier replacement while preserving its history.
- **Latest implementation:** Updated the canonical skill, repository instructions, workflow guide, and task handoff to the approved direction. Preserved the alignment threshold, two-round limit, coordinator authority, and required user input. Corrected the introducing unmerged skill and integration commits instead of adding a later patch; this history records successive user clarifications, not a request to approve this design again.
- **Latest verification:** Skill validation, authored-prompt comparisons, and independent adversarial skill and integration reviews passed. Six fixture replays covered first-round alignment, two-round dissent with a supported minority choice, binding approval, incompatible conditions, a failed worker, and essential user-only information. First requests differed only in private emphasis; second requests preserved the packet, original agents, and complete first answers. These are prompt-authoring and decision replays, not live runtime tests or proof of filesystem isolation.
