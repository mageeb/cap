# Reviews rationale

## 2026-10-23: Versioned correction saves

- **Choice / why:** [ReviewService](ReviewService.cs) requires an expected version to prevent silent overwrites. Empty reviewed text is allowed so clearing a translation is representable.
- **Status:** Version safeguard confirmed in [DEC-032](../../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries); empty-edit policy remains provisional under DEC-033 in the same log.
- **Evidence:** [Validation and conflict regressions](../Tests/Program.cs).
- **Limits:** Real ownership enforcement and compare-and-save atomicity belong to the unimplemented store adapter.
