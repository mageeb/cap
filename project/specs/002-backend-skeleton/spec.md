# 002: Reviewable Translation backend skeleton

Status: Interface review completed and boundaries approved. This historical proposal
scope is followed by [003: backend implementation](../003-backend-implementation/spec.md).

User direction: put implementation under `project/src/Translation/`, separated into
Backend and Frontend folders, and focus on a C# .NET backend for review.

- **AC-1:** Backend declarations compile with nullable checks and warnings as errors.
- **AC-2:** Import/read, draft translation and correction interfaces expose cancellation,
  validated caller context and explicit failure outcomes without framework/vendor types.
- **AC-3:** Proposal describes access, invalid input, save conflicts and dependency failures;
  it does not claim these safeguards are implemented.
- **AC-4:** Frontend is reserved; no database, provider, host or completion policy is built.

Review scope and open choices are in the [backend proposal](../../src/Translation/Backend/README.md).
