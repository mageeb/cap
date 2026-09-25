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
