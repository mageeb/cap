# Drafts rationale

## 2026-10-23: Request ownership and complete publication

- **Choice / why:** [DraftTranslationService](DraftTranslationService.cs) uses Created to avoid duplicate provider work and collects all paragraphs before publishing, avoiding partially replaced drafts.
- **Status:** Ownership safeguard confirmed in [DEC-032](../../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries); orchestration policy remains provisional under DEC-033 in the same log.
- **Evidence:** [Duplicate/failure/cancellation regressions](../Tests/Program.cs).
- **Limits:** Cancellation cleanup is best effort; process crashes may leave Pending operations. No recovery worker exists.
