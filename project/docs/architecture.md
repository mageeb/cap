# Current architecture

Status: Approved boundaries with application-service implementations and deterministic
regression checks. A separate HTTP host has business API contracts and OpenTelemetry instrumentation. Concrete adapters and production identity remain unimplemented, so business access is blocked.

The user selected C# .NET and `src/Translation/Backend/` plus `src/Translation/Frontend/`.
The [backend proposal](../src/Translation/Backend/README.md) describes three application
services and persistence/provider ports in one assembly. Other technology and product
choices remain open. See the [accepted ADR](adrs/001-backend-boundaries.md) and
[implementation tasks](../specs/003-backend-implementation/tasks.md).

## Present structure

- `src/`: Reserved for application code.
- `tests/`: Reserved for behavior checks.
- `infrastructure/`: Docker verification runbook, HTTP smoke checks and Kubernetes Deployment/Service template.
- `specs/`: Feature requirements, implementation plans, and task checklists.

## First implementation

As the first workflow is built, document the chosen runtime, components, data flow, persistence, and external dependencies here. Describe what exists and label proposed changes explicitly.

Record significant decisions and their alternatives in `adrs/`; link them here when created.

## Container hosting preparation

`src/Translation/Host` references the backend library and exposes liveness, fail-closed readiness and gated business API contracts. The multi-stage Linux image runs non-root on 8080.
Kubernetes uses distinct startup/liveness and readiness probes, a read-only root
filesystem, bounded resources and ClusterIP access. No business services are wired
until real adapters and validated identity exist. Readiness stays 503 and rollout
cannot complete yet. See [deployment runbook](../infrastructure/README.md),
[feature 004](../specs/004-container-deployment/spec.md) and
[DEC-037](../../DECISIONS.md#dec-037-container-hosting-preparation).

Hosting rationale: [ADR 002](adrs/002-container-hosting.md) (pending user review).

## API and telemetry boundaries

Business HTTP handlers map the approved application ports to JSON/multipart contracts.
They derive reviewer context from an authenticated principal and check prerequisite
registrations before reading bodies or invoking services. The production host registers
no adapters or identity provider, so mapped business calls return 503. Contract tests
provide explicit doubles and test identities; they do not prove production integrations.

OpenTelemetry collects request and outbound HTTP traces, safe request metrics and runtime
metrics. Structured completion logs use route templates, bounded methods, status, duration
and trace/span IDs. Health probes are filtered from normal request telemetry. OTLP export
is optional; structured console logging works without a collector. See the
[observability runbook](../infrastructure/observability.md),
[005 feature](../specs/005-api-observability/spec.md) and
[006 feature](../specs/006-business-api/spec.md).

Implementation rationale: [ADR003](adrs/003-host-observability.md) and
[API contract documentation](../src/Translation/Host/API.md).

Business API decisions: [ADR004](adrs/004-gated-business-api.md).
