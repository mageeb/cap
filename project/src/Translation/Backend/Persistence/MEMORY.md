# Persistence rationale

## 2026-10-23: Application-specific store port

- **Choice / why:** [IReviewStore](IReviewStore.cs) expresses required atomic operations directly, rather than generic CRUD that would hide ownership/version races.
- **Status:** Contract confirmed in [DEC-032](../../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries); storage technology remains undecided.
- **Evidence:** [Service-fake checks](../Tests/Program.cs) verify delegation, not adapter guarantees.
- **Limits:** No concrete persistence, migration or durable authorization/atomicity verification exists.
