# Books rationale

## 2026-10-23: Reader boundary and ordering

- **Choice / why:** [BookService](BookService.cs) delegates parsing to a reader because accepted formats/limits are unresolved. It orders and validates paragraphs before a single store create call and never owns the input stream.
- **Status:** Reader isolation is provisional under [DEC-033](../../../../../DECISIONS.md#dec-033-backend-implementation-verification); capability boundary confirmed under DEC-032 in the same log.
- **Evidence:** [Book import/read regressions](../Tests/Program.cs).
- **Limits:** Fake-reader checks do not establish real format/size validation or database atomicity.
