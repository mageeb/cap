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
