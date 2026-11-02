# ADR 003: Portable and bounded host telemetry

Status: Pending user review in [DEC-038](../../../DECISIONS.md#dec-038-opentelemetry-host-instrumentation).

Use OpenTelemetry with built-in ILogger, optional OTLP export and JSON stdout logs.
ASP.NET Core/HttpClient traces are scrubbed to approved low-cardinality fields before
export. Request completion metrics/logs use route templates and normalized methods;
health probes do not count as business traffic. Framework request/error logging is
suppressed because it can include raw paths, queries and exception messages.

Alternative: unfiltered automatic logging or vendor-specific SDK. Portable instrumentation
keeps storage/viewer selection separate and conservative fields protect source documents
and credentials. The consequence is reduced default diagnostic detail; explicit safe
operation events can be added when business API contracts exist. Exporter failure must
not affect HTTP availability. See [005 spec](../../specs/005-api-observability/spec.md).
