# Backend skeleton tasks and evidence

- [x] S-1 (AC-2–4): Define proposed boundaries, failure shapes and unresolved product rules.
- [x] S-2 (AC-1): Create contracts-only project in the requested folder structure.
- [x] S-3 (AC-1–4): Compile, independently review interfaces and validate affected links.

Evidence: .NET 8 restore using an empty package-source config, then `dotnet build
project/src/Translation/Backend/Translation.Backend.csproj --no-restore`: zero warnings
and errors. Independent refuter review identified a duplicate-request ownership gap;
`BeginDraftResult.Created` now identifies the sole provider caller. Removed an unused
Running state. Markdown local-link and whitespace checks passed. No behavioral tests
run because the proposal contains declarations only.

Confirmed boundaries: [DEC-032](../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries), explicitly approved by the user on October 23, 2026.

Handoff: service implementation and regression evidence now live in
[003 tasks](../003-backend-implementation/tasks.md). Real adapters and HTTP endpoints
remain unstarted pending the relevant product and infrastructure choices.
