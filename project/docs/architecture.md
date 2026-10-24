# Current architecture

Status: Approved boundaries with application-service implementations and deterministic
regression checks. No runnable HTTP host or concrete adapters implemented.

The user selected C# .NET and `src/Translation/Backend/` plus `src/Translation/Frontend/`.
The [backend proposal](../src/Translation/Backend/README.md) describes three application
services and persistence/provider ports in one assembly. Other technology and product
choices remain open. See the [accepted ADR](adrs/001-backend-boundaries.md) and
[implementation tasks](../specs/003-backend-implementation/tasks.md).

## Present structure

- `src/`: Reserved for application code.
- `tests/`: Reserved for behavior checks.
- `infrastructure/`: Reserved for application deployment configuration.
- `specs/`: Feature requirements, implementation plans, and task checklists.

## First implementation

As the first workflow is built, document the chosen runtime, components, data flow, persistence, and external dependencies here. Describe what exists and label proposed changes explicitly.

Record significant decisions and their alternatives in `adrs/`; link them here when created.
