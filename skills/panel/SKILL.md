---
name: panel
description: Use when considering a question for the user or when the user requests a panel, to assess an unresolved decision before escalating.
---

# Panel

Reduce avoidable interruptions while preserving the user's control. The panel advises; the coordinator decides whether to proceed provisionally or ask the user. Agreement is neither proof nor user confirmation. All three are Codex generalists with different areas of attention within one model family; this provides neither different vendors nor a guarantee of independent errors.

## Prepare the question

1. Read the applicable instructions and relevant confirmed decisions. State the question, available choices, known facts, unknowns, constraints, and what the user has already authorized. Do not manufacture a choice when only the user can supply a fact or approval.
2. Build one self-contained, neutral packet: the question, available choices, confirmed constraints, and minimum evidence or explicit read-only file allowlist. Keep the coordinator's provisional direction private. Do not supply chat history, previous recommendations, this skill, or instructions revealing a panel or other agents.
3. Give all three the same neutral question, options, facts, confirmed constraints, and permitted evidence. Append only the recipient's private emphasis from the list below; that emphasis is the only difference in prompt content. Do not disclose the other emphases or assign positions to defend, preferred-option labels, or an expected answer. Use neutral task names that reveal neither panel nor round.
4. Keep each round a bounded 2–5 minute task including evidence checks. If it grows, narrow the question rather than launching more rounds. Panelists must not ask the user, edit files, invoke this skill recursively, or spawn agents.

## Private emphases — coordinator only

Each generalist evaluates the whole question, every requirement, and all options. Assign one area of attention to each; share only its own emphasis with that worker:

- **User value:** Usefulness, user experience, and whether the scope fits the actual need.
- **Implementation:** Feasibility, the simplest reliable approach, and maintenance cost.
- **Assumptions and risks:** Unsupported assumptions, missing evidence, failure cases, and consequences.

## Round 1: Independent answers

Spawn three agents with no inherited conversation (`fork_turns="none"` where supported). Do not tell them that others are answering the question, what those others are doing, or whether you already favor a direction. Dispatch all three before collecting answers. Give each the same packet and the following instruction, replacing the emphasis slot with only its assigned emphasis sentence:

```text
Evaluate the whole question, every requirement, and all options using the
supplied facts and explicitly permitted evidence.
Your area of emphasis: [insert only this worker's emphasis sentence].
This is an area of attention, not a position to defend. You need not disagree,
invent a concern, or recommend any particular answer.
Do not read unlisted files, conversation logs, or decision notes.
Do not edit files, ask the user, or delegate.
Return: recommended option (or a better alternative); concise reasoning;
evidence versus assumptions; strongest supported objection, if any; what only the user can
resolve; and what would change your recommendation. Aim for 200 words.
```

Collect all three answers without passing answers between agents. Clean conversation context does not isolate a shared filesystem: explicitly restrict reads, and do not claim technical isolation that the tools do not provide.

Treat round 1 as aligned only when all three recommend the same action under compatible conditions, with no unresolved material objection. A 2–1 split, incompatible assumptions, or a missing answer is not alignment. If aligned, skip round 2 and make the coordinator's decision.

## Round 2: Reconsider once

If a worker is unavailable, use the incomplete-panel fallback below. Otherwise, if round 1 is not aligned, reveal the panel to each original agent. Send the unchanged question and packet, their own first answer, and the other two complete first answers labeled A/B/C. Share the actual answers without rewriting them. Retain each agent's own emphasis while allowing it to reconsider the whole problem. With collaboration tools, use `followup_task` on those agents.

You may now add your own tentative view and reasoning in a separate, clearly labeled coordinator block, identical for all three. Treat it as another argument to examine, not a required answer. This view must never appear in round 1.

```text
Reconsider independently after reading all round-1 answers. You may keep or
change your recommendation; agreement is not a goal by itself.
Return your recommendation, what changed or stayed the same and why,
which claims you accept or challenge, and remaining uncertainty or dissent.
Do not ask the user, edit, delegate, or launch further rounds.
```

Dispatch to all three before collecting their second answers. Do not relay second-round answers during the round. Stop after round 2 even if disagreement remains; do not replace panelists or repeat the question until they agree.

## Decide, record, or ask

- Independently assess the evidence and constraints. You may reject a unanimous recommendation or choose a supported minority view; explain why. Do not count votes as authority or accept factual claims just because agents repeat them.
- If the advice resolves an ordinary choice within authorized work, choose an approach, log it as **Pending user review**, and continue. Disagreement after two rounds does not automatically require an interruption if you can responsibly decide.
- If essential user-only information, an unresolved material product requirement, or explicit approval is still needed, use Ask User Question with one focused question, your recommendation, relevant alternatives, and the remaining uncertainty. Use a tool permitted for that type of request: a blocking tool when eligible, otherwise an eligible asynchronous question. Pause dependent work until answered.
- If no user-question tool is available or permitted for the needed request, explain the limitation and leave work requiring an answer paused; do not invent an answer or use an ineligible tool.
- A panel cannot grant user approval, supply unknown private facts, override a confirmed requirement, or turn a draft specification into an approved one. For these questions, the panel can improve the question and options, but the user still answers.
- Reuse the completed panel when later asking the user to confirm the same logged choice. Do not repanel the ledger-review question itself unless new facts create a different substantive decision.
- If the user explicitly requests a decision review, still present that decision for their answer; panel agreement does not cancel the requested review.
- If agents fail or the runtime cannot provide three independent workers, report an incomplete panel and record which responses are missing. Never invent votes or claim consensus. Use available advice under coordinator judgment; ask if the missing evidence prevents a responsible decision.

## Decision record and handoff

Use the repository's decision log, or `DECISIONS.md` in the working project if none is specified. Record the question and options, relevant facts, each round's recommendations and reasoning, any coordinator view shared in round 2, whether alignment occurred, dissent, the coordinator's final choice and rationale, affected work, and verification evidence. Preserve enough detail to reconstruct the decision; link longer panel records if necessary.

Keep status **Pending user review** until the user confirms or redirects. Record the user's response and follow-up work when reviewing one decision at a time through Ask User Question. An unanswered or deferred question stays pending.

The log is the queue for reviewing agent choices. ADRs record significant technical decisions, alternatives, and consequences over time; create or update one when warranted and link it to the corresponding log entry. A routine question does not require an ADR.
