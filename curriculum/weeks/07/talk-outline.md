# Week 7: Planning, Verification, and Backend Practice

## Learning goals

Derive an explainable MVP plan from source requirements and architecture. Review hierarchy, dependencies and completion checks; ground verification in representative domain cases; review and implement backend boundaries; keep code, rationale and evidence organized for future agents.

## Teaching through-line

Weeks 5–6 established product and architecture context. Week 7 connects that context to execution, and asks what evidence makes the result trustworthy. Use the [notes](session-notes.md), [raw transcript](transcript.txt), [reference](reference.md) and [homework](homework.md). The [canonical backend](../../../project/src/Translation/Backend/README.md) is the classroom practice; do not duplicate application source into the curriculum.

## Delivery formats

An 80-minute [presentation](presentation.md) follows the recorded progression. Add the 45-minute guided exercise and 55-minute workshop below for a 180-minute class. No timing implies the full backend can be implemented during the demonstration.

## Core discussion — 80 minutes

| Time | Instructor move | Evidence to retain |
| --- | --- | --- |
| 0–10 | Recap scope/design and distinguish translation generation from review. Ground the assistant in Weeks 5–6. | Corrected product summary and unresolved choices. |
| 10–20 | Generate an epic/story/task hierarchy and review it. Discuss bounded agent work and first-principles reasoning. | Proposed work items with source, completion checks and dependencies. |
| 20–35 | Ask how to prove behavior. Compare unit/system checks with representative domain inputs and expected outputs. | A small dataset plan, provenance and reviewed expectations. |
| 35–43 | Review the generated hierarchy and dependency sequence; identify justified parallelism. | Reviewed execution order and integration owner. |
| 43–53 | Separate curriculum from application; inspect C# interfaces and the proposed local onebox. | Boundary proposal and reproducibility gaps. |
| 53–62 | Distinguish proposal, approval, implementation and passing checks. Inspect the existing practice and small-change review workflow. | Actual check results and explicit unimplemented behavior. |
| 62–75 | Explain scoped instructions, rationale and reusable procedures. Keep context concise and review comments traceable. | Useful guidance and feature decision records. |
| 75–80 | Restate minimal-backend/unit/component-test homework; optional basic frontend. | Student scope, acceptance cases and honest evidence plan. |

## Guided practice walkthrough — 45 minutes

1. **Ground and plan (10m).** Supply the reviewed PRD/design. Ask for a hierarchy of bounded units with requirements, dependencies and completion checks. Review a few consequential items instead of accepting the whole workbook.
2. **Define the truth (10m).** Prepare a small synthetic parallel-text case, an edit and its expected retrieval result, plus a failure case. Explain who reviewed the expected answer and what the cases do not cover.
3. **Review the contract (10m).** Open `IBookService`, `IReviewService`, `IDraftTranslationService`, `IReviewStore` and `ITranslationProvider` in the canonical backend. Trace a save and a draft request. Explain unresolved real adapters and HTTP behavior.
4. **Run and challenge (10m).** Run the documented build and service regressions if prerequisites are ready. Trace one assertion to a requirement. Show why a fake-backed pass is not a database or live-provider integration result. Debug actual failures rather than claiming success in advance.
5. **Preserve the handoff (5m).** Link task status, checks, rationale and remaining questions. Explain which local instructions apply and how a fresh session finds the next step.

Current repository check during material preparation: build succeeded with zero warnings/errors; all 17 service regressions passed. This is recorded follow-up evidence, not a claim that every test ran in the recording or that an HTTP service is complete.

## Student workshop — 55 minutes

- **10m:** Review the work hierarchy and choose one backend operation with a clear finish.
- **10m:** Specify representative input/output and failure cases; confirm the expected result.
- **25m:** Implement or extend a bounded unit and its checks in the student's repository. Start with known decisions; leave unresolved provider/input/storage choices explicit.
- **10m:** Peer challenge, revise, and record status. Identify whether the evidence is a unit check, component check, API request, or real integration.

## Misconceptions and source cautions

- A generated plan or interface does not mean implementation has started.
- A finite dataset does not prove all possible behavior; test expectations can be wrong too.
- More agents, more examples, more generated code, or a coverage percentage do not automatically establish quality.
- Example productivity/model/vendor claims are not validated benchmarks or current recommendations.
- The exact instruction filename here is `AGENTS.md`. `MEMORY.md` is the classroom rationale convention; automatic discovery depends on the actual tool/setup. A reusable skill uses `SKILL.md`, not a universal `skills.md` memory mechanism.
- Do not require duplicate guidance in every small directory merely because that was the quick classroom choice.
- Onebox setup was proposed, not demonstrated as a complete frontend/API/database environment.
- The canonical practice has real application-service code and fake-backed tests; HTTP, adapter durability and full integration remain incomplete.

## Homework and next step

Use the [homework](homework.md): implement the bounded backend in the student's own repository, with unit/component checks and usable setup. The recording allows a small frontend, API client or runnable tests as the demonstration surface. Full visual design and deployment are not required.

Continue from the verified checkpoint and unresolved requirements. Later infrastructure and release lessons must start from actual implementation evidence; this outline does not revise the remaining course schedule or promise a live service that does not exist yet.
