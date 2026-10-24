# Integrations rationale

## 2026-10-23: Narrow translation-provider port

- **Choice / why:** [ITranslationProvider](ITranslationProvider.cs) translates one paragraph per call so provider changes do not alter review/storage contracts or introduce ambiguous batch alignment.
- **Status:** Port confirmed in [DEC-032](../../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries); vendor selection remains open.
- **Evidence:** [Provider-fake orchestration checks](../Tests/Program.cs).
- **Limits:** No real provider adapter or credential/configuration path is implemented.
