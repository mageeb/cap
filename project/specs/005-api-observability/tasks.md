# Tasks and evidence

- [x] O-1 (AC-1-3): Implement safe telemetry registration and request instrumentation.
- [x] O-2 (AC-4-5): Verify real HTTP, export, correlation, privacy and failure boundaries.
- [x] O-3: Document commands/configuration and perform adversarial review.

Choice: [DEC-038](../../../DECISIONS.md#dec-038-opentelemetry-host-instrumentation).
Business API contracts are implemented separately in feature 006; production access remains blocked until adapters and validated identity are configured.

## Verification (2026-10-30)

`dotnet build project/src/Translation/Host/Translation.Host.csproj --no-restore` passed
with zero warnings/errors. Pinned compatible packages: OpenTelemetry core/exporter/hosting,
ASP.NET Core and HttpClient instrumentation 1.20.1; runtime instrumentation 1.20.0.
Restore used the official NuGet feed; no business library dependency was added.

`dotnet run --project project/src/Translation/Host.Tests/Translation.Host.Tests.csproj
--no-restore` passed all six grouped integration checks. A real loopback OTLP HTTP/protobuf
receiver received nonempty traces, logs, request metrics and runtime metrics. Tests verified
inbound trace-ID/log correlation, outbound HttpClient scope and traceparent propagation,
route templates, bounded unknown routes/methods, absence of synthetic sensitive path/query/
Authorization/baggage/tracestate/exception markers in exported payloads and JSON logs,
health exclusion, sanitized failure and deterministic outbound HTTP failure. Standalone
operation and an unreachable exporter preserve HTTP behavior. Credential-bearing exporter
configuration is rejected without reflecting its value. Receiver shutdown is bounded.
The HttpListener receiver requires a shell allowed to bind loopback; restricted sandbox
startup returned an invalid-handle error, while escalated runs passed.

Independent review-with-refuter accepted the implementation and independently reran all
six telemetry groups and 13 API groups. Review feedback strengthened the outbound span
existence assertion; the final tests additionally verify parent trace propagation and
an HTTP503 dependency failure. Privacy hardening suppresses Microsoft.AspNetCore and
System.Net.Http framework logs and strips unapproved server/client trace tags before
export. No source text, raw URI or caller identifiers are added by the host telemetry.

Parent rebuilt the Linux image successfully:
`sha256:4e51b80a241de0dcf5a3b6d9937e9e43ce3c568585fd43ac87b7d9dbaf7dd51c`.
Restricted-container smoke passed live200, ready503 and API503, correlated JSON logs,
route templating, sensitive marker exclusion and health filtering. Container evidence:
`outputs/deployment-check/otel-container-smoke.log` (local generated output, not source).
No registry push or Kubernetes mutation occurred.

## Configuration and limits

JSON console logging is always enabled. Optional `OTEL_EXPORTER_OTLP_ENDPOINT` is an
HTTP(S) base URL; the host appends /v1/traces, /v1/logs and /v1/metrics and uses HTTP/protobuf.
Service name is fixed to translation-backend. No endpoint means no OTLP export. Export
attempt timeout is two seconds; exporter failures do not become HTTP dependencies. No
monitoring storage/viewer, vendor, credentials, sampling override or business operation
instrumentation is selected. Business operation events await concrete API/adapters;
current request telemetry includes blocked API requests and excludes all /health traffic.

SDK custom request metrics are translation.http.requests and translation.http.request.duration
(seconds). Runtime metrics are provided by the runtime instrumentation package. Built-in
ASP.NET per-request metrics are not separately subscribed, avoiding duplicate counts and
health traffic pollution of these traffic metrics. Trace export retains only normalized
HTTP method, status and route template; outbound URL/host/path attributes are omitted.

Parent final integration: existing backend regressions17/17; all147 local file links at
time of check, PowerShell parser checks and tracked diff whitespace passed. Independent
review accepted API13/13 and telemetry6/6; review feedback was resolved before handoff.
All changes remain uncommitted on feature/deployment; no registry or cluster mutation.


## Publication verification (2026-11-01)

Before the authorized branch push/PR, reran host telemetry6/6, API contracts13/13 and
backend regressions17/17 successfully with the same host/container sources. Prior image
build and restricted-container evidence above remain applicable. Image publishing and
cluster deployment are not part of the branch publication. Local generated outputs and
credentials are excluded from the commit; tests-worktree changes remain separate.
