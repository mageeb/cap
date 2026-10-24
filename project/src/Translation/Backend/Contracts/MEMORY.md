# Contracts rationale

## 2026-10-23: Framework-independent outcomes

- **Choice / why:** [OperationResult](OperationResult.cs) gives services explicit outcomes without coupling them to HTTP. ReviewerContext carries server-validated identity; adapters still enforce resource scope.
- **Status:** Confirmed interface boundary in [DEC-032](../../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).
- **Evidence:** [Validation and access regressions](../Tests/Program.cs).
- **Limits:** This type does not authenticate callers or map errors to HTTP responses.
