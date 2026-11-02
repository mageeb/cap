# API traffic and telemetry

The host uses OpenTelemetry for collection/export and .NET `ILogger` for structured JSON
console logs. Business API contracts are implemented and tested with doubles; production
registrations still omit real adapters and identity. A real request to a mapped business
route returns 503. Readiness stays 503. Successful business traffic requires those missing
dependencies; telemetry does not make the application business-ready.

## See local container traffic

Run these commands from this deployment worktree root. Rebuild after source changes:

```powershell
docker build --pull -t translation-backend:local project/src/Translation
docker run -d --rm --name translation-observability --read-only --cap-drop ALL --security-opt no-new-privileges --tmpfs /tmp:rw,noexec,nosuid,size=64m -p 127.0.0.1:18080:8080 translation-backend:local
curl.exe -i http://localhost:18080/api/books/00000000-0000-0000-0000-000000000001
docker logs translation-observability
docker stop translation-observability
```

The API request should return 503 and create a structured completion log. Inspect its
route template, HTTP status, duration and trace correlation. Health probes remain separate
from normal request traffic. Console output is inspectable using `docker logs` locally
or `kubectl logs` after a separately authorized deployment. No telemetry viewer or storage
backend is bundled into the application image.

## Export to an existing telemetry receiver

An OTLP receiver or collector must be reachable from the container. Configure the host's
`OTEL_EXPORTER_OTLP_ENDPOINT` base URL using an environment variable; the receiver address must be
reachable from Docker/Kubernetes, not just from the host browser. Local host services can
be reached through Docker Desktop's `host.docker.internal`. Select and secure the receiver
before exposing it beyond local development. Do not commit collector credentials.

The service name is `translation-backend`; the exporter uses HTTP/protobuf and appends
`/v1/logs`, `/v1/traces` and `/v1/metrics` to the configured base URL. Without the
endpoint, JSON console logging remains active and no OTLP export is attempted.

For a local receiver listening on host port 4318, add this option before the image name
in the Docker run command: `-e OTEL_EXPORTER_OTLP_ENDPOINT=http://host.docker.internal:4318`.
Only HTTP(S) URLs without embedded credentials, query strings or fragments are accepted.

A collector's debug output can verify receipt; a monitoring backend is needed to retain,
query and visualize signals. Backend selection, retention and cluster collector deployment
remain separate decisions. Exporter connectivity failures must not break business responses.

## Verification

Run host telemetry integration checks and API contract checks from the worktree root:

```powershell
dotnet run --project project/src/Translation/Host.Tests/Translation.Host.Tests.csproj
dotnet run --project project/src/Translation/Host.ApiTests/Translation.Host.ApiTests.csproj
dotnet run --project project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj
```

The tests worktree's separate unit/component coverage suite remains independent.
The host tests use local telemetry capture rather than requiring a cloud monitoring account.
Actual commands/results and limitations belong in [005 tasks](../specs/005-api-observability/tasks.md)
and [006 tasks](../specs/006-business-api/tasks.md). API payloads and mappings are documented
in the [business API contract](../src/Translation/Host/API.md). Never log source/translation/review text, credentials or tokens.

Verification note: the telemetry receiver uses Windows HttpListener. In a restricted
agent sandbox, listener creation can fail; the integration suite passed when run outside
that sandbox. This is a test-environment permission issue, not a required production
administrator privilege.
