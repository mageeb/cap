---
marp: true
theme: default
paginate: true
size: 16:9
footer: 'CAP · Week 7'
style: |
  section { background: #F4F0E7; color: #152536; font-family: "Avenir Next", Avenir, Arial, sans-serif; font-size: 32px; line-height: 1.3; padding: 48px 68px 65px; justify-content: flex-start; }
  h1 { font-size: 52px; line-height: 1.08; margin: 0 0 28px; color: #152536; }
  h2, strong { color: #188D91; }
  p, li { margin-bottom: 18px; }
  footer { left: 68px; bottom: 24px; font-size: 16px; color: #647276; }
  section::after { color: #647276; bottom: 22px; right: 45px; font-size: 16px; }
  section.cover { padding-top: 100px; }
---
# From design to<br>verifiable execution

**A plan. A useful check.<br>A backend we can explain.**

CAP · Week 7 · Planning, verification, and practice

<!-- 0–3m. Use Weeks5–6 as source context. Today follows the recorded work-item/interface/implementation journey; no complete production service is claimed. -->

---

# What are we building?

A person reviews a machine-translated draft.

Read parallel paragraphs. Edit. Save. Return.

**Translation generation is an external dependency.**

<!-- 3–6m. Revisit the MVP, user and unresolved input/completion choices. Do not let the agent silently broaden the product. -->

---

# What comes before the code?

Use the reviewed scope and design
to produce a hierarchy of work items.

**Give execution an understandable plan.**

<!-- 6–10m. A familiar game prompt differs from a specialized business workflow. The hierarchy is proposed and reviewed, not automatically accepted. -->

---

# Recover the context first

Read the requirements and design discussion.

Summarize decisions, proposals and unknowns.

**The repository holds more than source code.**

<!-- 10–13m. Recordings, transcripts and organized notes were the live input. Concise comprehensive summaries still need human review. -->

---

# Epic → Story → Task

State the need, scope, dependencies
and observable completion checks.

**A workbook or tracker holds the plan.<br>It does not prove the work is done.**

<!-- 13–17m. The class generated Excel; trackers were alternatives. No fixed item count, paid service, or hierarchy depth is required. -->

---

# Challenge the generated plan

What is missing? What is out of scope?
Who integrates the pieces? What blocks this task?

**Review before dependent execution.**

<!-- 17–20m. Human or separate-agent review can expose unsupported assumptions. A generator reviewing its own prose is not sufficient evidence by itself. -->

---

# What would prove it works?

Not just a successful function call.
Not just a clickable screen.

**Check the customer's data and outcome.**

<!-- 20–24m. The session's central question connects business validation with unit and system checks. Include security/setup responsibilities rather than treating them as afterthoughts. -->

---

# Build a small domain dataset

Source text · Draft text · Human correction
Expected saved result · Meaningful failure case

**Representative enough to matter.<br>Small enough to run often.**

<!-- 24–28m. Example1000entries was not a requirement. Dataset preparation was discussed, not completed in the recording. Use synthetic/authorized cases; names alone do not guarantee anonymization. -->

---

# Who checked the expected answer?

A generated test can repeat
a generated implementation's assumption.

**Ground both in reviewed requirements.**

<!-- 28–32m. A finite passing set supports tested behavior, not universal correctness. Distinguish source expectations from an agent-generated answer key. -->

---

# Compare across cases

A change can be faster on one example
and less accurate across the workload.

**Use common cases and explain the trade-off.**

<!-- 32–35m. No model ranking, reasoning setting or productivity multiplier is validated by the recording. Product-output/spec-oriented checks complement unit tests. -->

---

# Sequence before parallelizing

Find prerequisites. Identify independent work.
Name the integration responsibility.

**Agent count is not a quality score.**

<!-- 35–39m. Ask the assistant for dependency order and safe parallelism. Work from the reviewed source rather than invented requirements. -->

---

# Keep the repository organized

curriculum/weeks/07 → teaching material
project/src/Translation → application
Backend and Frontend → separate locations

**Link the practice. Do not copy the code.**

<!-- 39–43m. C#/.NET is the classroom backend direction. Database/provider/frontend choices are not all settled by this directory layout. -->

---

# Review the first abstraction

IBookService · IReviewService
IDraftTranslationService

IReviewStore · ITranslationProvider

**Do these responsibilities fit the product?**

<!-- 43–47m. These were the high-level boundaries inspected and approved. Read actual definitions in the canonical project. ISourceDocumentReader further isolates unresolved import policy. -->

---

# Proposal is not implementation

Proposed → Reviewed → Approved
→ Implemented → Checked

**Keep those states separate.**

<!-- 47–51m. The instructor noticed implementation remained unstarted, then authorized it. An interface file or optimistic summary cannot establish working behavior. -->

---

# A local onebox

Clone. Configure. Start the needed pieces.

Can another developer reproduce your setup?

**The idea was proposed; the full setup was deferred.**

<!-- 51–54m. A health endpoint/basic client were suggested. Do not claim the current classroom project has an HTTP host, real database or running frontend. -->

---

# Inspect the practice evidence

Build: zero warnings/errors.
Service regressions: **17 passed**.

**Deterministic fakes check service behavior.<br>Real adapters still need integration evidence.**

<!-- 54–58m. These are fresh preparation checks, not claims that all tests ran in the recording. No HTTP host, real reader/store/provider adapters or deployed application are verified. -->

---

# Make changes reviewable

One coherent concern. Useful tests.
Definition of done. Clear commit rationale.

**Small changes help people and agents investigate.**

<!-- 58–62m. The instructor acknowledged the demonstration grew large. File count is a signal, not an automatic correctness criterion. -->

---

# Close the review loop

Read a finding. Check the trigger.
Fix it—or explain why it does not apply.
Recheck and record the outcome.

**A resolved comment needs a reason.**

<!-- 62–65m. The session discussed PR comments as collaboration. Do not accept every AI finding or mark it fixed without evidence. -->

---

# Leave useful context behind

AGENTS.md → scoped instructions
MEMORY.md → classroom rationale/history
SKILL.md → reusable procedure

**Discovery behavior depends on the actual tool.**

<!-- 65–69m. Use exact repository filenames. MEMORY.md is a course convention, not universal automatic memory. Do not imply the literal filename skills.md is required. -->

---

# Keep the memory usable

Record the choice, why, status and evidence.
Link the relevant feature and task.
Remove repetition and stale assertions.

**More prose can mean less useful context.**

<!-- 69–72m. Every-folder guidance was a quick classroom choice, not a universal requirement. Feature-level design records can preserve rationale without duplicate instructions. -->

---

# The minimal backend homework

Build in your own repository.
Implement the necessary APIs and backend behavior.
Add unit and component tests. Document setup.

**Use reviewed representative cases.**

<!-- 72–76m. Follow homework.md for curriculum assessment. Instructor offered real reviewed material but did not establish distribution. Use safe synthetic fixtures meanwhile. Record incomplete integrations explicitly. -->

---

# A useful result beats a fancy screen

A tiny frontend is optional.
API-client requests or runnable tests can help.

**Show what works, how you checked it,<br>and what is still missing.**

<!-- 76–80m. Rich UX, cloud deployment and a software factory are not required. A fake-backed service-only checkpoint cannot be described as complete HTTP or durable integration behavior. -->
