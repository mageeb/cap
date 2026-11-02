# Backend rationale

## 2026-10-23: Capability boundaries

- **Choice / why:** One C# assembly with book, review and draft services plus provider/storage ports keeps the MVP understandable without premature layer projects.
- **Status:** Confirmed in [DEC-032](../../../../DECISIONS.md#dec-032-proposed-translation-backend-boundaries).
- **Evidence:** [Accepted ADR](../../../docs/adrs/001-backend-boundaries.md); [service checks](Tests/Program.cs).
- **Limits:** Local .NET 8 compilation is not a deployment-runtime choice. Real adapters, identity and HTTP hosting remain open.

## 2026-10-23: Local instructions and rationale

- **Choice / why:** The user requested AGENTS.md and MEMORY.md in Backend, Frontend and each backend capability, so future agents can recover both rules and their reasons locally.
- **Status:** Directory scope and filenames explicitly confirmed by the user. The [root log](../../../../DECISIONS.md) remains authoritative for decision approval; memory is not automatic approval.


## 2026-10-30: Container host boundary

- **Choice / why:** Keep a separate health-only HTTP host beside Backend so packaging does
  not couple domain code to ASP.NET or invent real adapters. Readiness deliberately fails.
- **Status:** Pending review in [DEC-037](../../../../DECISIONS.md#dec-037-container-hosting-preparation).
- **Evidence:** [004 tasks](../../../specs/004-container-deployment/tasks.md); no business endpoints implemented.

## 2026-10-30: HTTP contracts and telemetry

- **Choice / why:** Keep HTTP/telemetry in the separate host, using the approved service
  interfaces. User authorized API contract tests with doubles while production access stays
  blocked until real adapters and identity are configured. Domain logic stays independent.
- **Status:** OpenTelemetry framework explicitly authorized; details tracked by DEC-038/039.
- **Evidence:** [005 tasks](../../../specs/005-api-observability/tasks.md) and
  [006 tasks](../../../specs/006-business-api/tasks.md) contain current verification.

## 2026-10-30: Unit coverage and component workflows

- **Choice / why:** Keep the executable runner and measure all production backend code with unit execution alone. Separate component workflows exercise real services together against stateful boundary doubles; they do not substitute for future adapter tests.
- **Status:** User requested 80% unit coverage and passing component tests; tooling/scope details are pending review in [DEC-041](../../../../DECISIONS.md#dec-041-backend-unit-coverage-and-component-workflows).
- **Evidence:** [Feature task record](../../../specs/003-backend-implementation/tasks.md#2026-10-30-unit-and-component-tests); [reproducible gate](Tests/Test.ps1).

