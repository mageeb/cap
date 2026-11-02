# 005: API-host observability

Instrument the existing host using OpenTelemetry and built-in .NET logging. Business
APIs remain blocked until adapter and identity choices are resolved.

- AC-1: Structured JSON request-completion logs include safe route, method, status,
  duration, trace/span correlation; never raw paths, queries, headers, bodies or identity.
- AC-2: ASP.NET Core and outbound HttpClient traces plus request/runtime metrics export
  through OTLP when explicitly configured. Export is optional; console logging works alone.
- AC-3: Health probes are excluded from request traffic logs/traces/custom request metrics;
  unmatched paths and nonstandard methods use bounded labels.
- AC-4: Real HTTP integration checks verify telemetry export/correlation, sensitive-data
  exclusion, health filtering and unchanged fail-closed readiness/business access.
- AC-5: Missing/unreachable telemetry backend does not prevent serving HTTP; invalid
  endpoint configuration fails startup with a non-sensitive actionable message.
