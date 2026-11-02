# Implementation tasks and evidence

- [x] I-1 (AC-1): Implement review validation and persistence delegation; run regression checks.
- [x] I-2 (AC-2–4): Implement translation ownership, complete publication and failure/cancellation handling; run regression checks.
- [x] I-3 (AC-6): Add deterministic behavioral checks and perform independent review.
- [ ] I-4 (AC-5): Implement import once format/size are confirmed.
- [ ] I-5 (AC-6): Implement chosen storage/provider adapters and verify actual integration.

Pending input: UTF-8 input limit, database/local persistence and provider selection.
Confirmed boundaries: [DEC-032](../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).
Pending implementation choices: [DEC-033](../../../DECISIONS.md#dec-033-backend-implementation-verification).

Initial evidence: package-free restore with `dotnet restore .../Tests/Translation.Backend.Tests.csproj
--configfile outputs/dotnet-check/NuGet.Config -p:NuGetAudit=false`, then
`dotnet run --project project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj
--no-restore`: 17/17 service checks passed, including book ordering/reader failures/stream
ownership. Independent review found no service blockers. Affected local links in seven
documents, source whitespace and `git diff --check` passed. Concrete store durability and
restart recovery remain unverified.

Next: await the pending user answer before implementing format/limit or real storage/provider
adapters. Generic BookService orchestration is implemented and tested; AC-5 remains partial
until a real reader enforces the agreed input policy. No HTTP service is runnable yet.

Documentation follow-up (2026-10-23): user confirmed AGENTS.md/MEMORY.md in Backend,
Frontend and the six backend capability folders. Added 16 files with local instructions,
dated rationale and links to approval/evidence records. Scope, local links and whitespace
checks passed; independent documentation review recorded no supported blockers.

## 2026-10-30: Unit and component tests

- [x] T-1 (AC-1–6): Establish whole-production-assembly unit coverage and prioritize access, request ownership, complete publication, alignment, corrections, conflicts and cancellation.
- [x] T-2 (AC-1–6): Expand isolated boundary regressions; enforce at least 80% line and branch coverage using unit execution alone.
- [x] T-3 (AC-1–4, AC-6; orchestration part of AC-5): Add combined import/read/review/translate/status workflows, duplicate requests, denial, failure, stale edits and cancellation.
- [x] T-4: Complete adversarial review and record the result.

Setup paths/base: [DEC-040](../../../DECISIONS.md#dec-040-tests-and-deployment-worktree-setup).
Tooling, metric and component-boundary choice: [DEC-041](../../../DECISIONS.md#dec-041-backend-unit-coverage-and-component-workflows), pending user review.

Verification from the tests worktree root:
`powershell -NoProfile -ExecutionPolicy Bypass -File project/src/Translation/Backend/Tests/Test.ps1`
passed: build zero warnings/errors, **28/28 unit tests**, **145/146 lines (99.31%)**,
**77/78 branches (98.71%)**, and **6/6 component tests**. Baseline was 17 unit tests,
94.52% line / 87.17% branch. Coverlet measures the entire `Translation.Backend` assembly,
with no production source/type exclusions. The uncovered line/branch is the defensive
unrecognized-result fallback in BookService; the closed result hierarchy has only success/failure.
Machine-readable reports are generated under ignored `outputs/coverage/`.

The coverage target was already met by the baseline; additions address substantive gaps:
pre-cancelled entry points, null/missing input, failed ownership, cleanup failure preserving
cancellation, persistence failure propagation, exact caller/token scope, reversed source order,
and stopping immediately on failed/null provider responses.
Component tests verify observable read-back state and correction preservation rather than
call counts alone. Their store is a test double; I-4/I-5 remain open and no real adapter,
restart durability, concurrent request race, identity system or HTTP integration is verified.

Review-with-refuter: independent review of unit tests, runner and coverage tooling accepted
with no supported blockers; component author self-review and a separate parent review of
the component tests also found no blockers. Optional multi-argument CLI handling feedback
was addressed and verified to return usage exit code 2. A deliberately impossible 100%
threshold failed with both line and branch errors, confirming the coverage gate rejects
insufficient coverage. Final documentation sanity review, all 101 local links/anchors
across five affected documents, tool-manifest JSON, new-file whitespace and
`git diff --check` passed. No production code changed. On 2026-11-01 the user authorized
publishing this tests branch for PR review; publication validation reruns the same gate.
