# DEV application telemetry

The API exports OTLP HTTP/protobuf to a collector sidecar on `127.0.0.1:4318`.
The collector forwards requests, structured logs and custom/runtime metrics to
workspace-based Application Insights. The receiver is not exposed through a
Kubernetes Service. Health traffic is deliberately excluded from application
request telemetry.

## Azure portal

Open resource group **Translation-Dev**, then **translation-dev-appinsights**:

- **Investigate > Search / Transaction search**: individual requests and correlated logs.
- **Investigate > Failures**: failed requests, including the current intentional 503s.
- **Investigate > Performance**: request counts and durations.
- **Monitoring > Logs**: run the queries below; set the time range to the last hour.
- **Monitoring > Metrics**: choose the Application Insights custom metrics namespace
  for exported request counters/durations and runtime metrics.

[Open Application Insights](https://portal.azure.com/#resource/subscriptions/3df3fa8b-637a-4277-a7d3-2f86e4606a3e/resourceGroups/Translation-Dev/providers/Microsoft.Insights/components/translation-dev-appinsights/overview)

[Open Log Analytics workspace](https://portal.azure.com/#resource/subscriptions/3df3fa8b-637a-4277-a7d3-2f86e4606a3e/resourceGroups/Translation-Dev/providers/Microsoft.OperationalInsights/workspaces/translation-dev-logs/overview)

The exporter does not supply Application Insights Live Metrics/Profiler. Use stored
telemetry queries. Container Insights collection is not enabled by this release;
application log records are forwarded through OTLP, not `ContainerLogV2`.

## Application Insights Logs queries

```kusto
requests
| where timestamp > ago(1h)
| where cloud_RoleName == "translation-backend"
| project timestamp, name, resultCode, success, duration, operation_Id
| order by timestamp desc
```

```kusto
traces
| where timestamp > ago(1h)
| where cloud_RoleName == "translation-backend"
| project timestamp, message, operation_Id
| order by timestamp desc
```

```kusto
customMetrics
| where timestamp > ago(1h)
| where cloud_RoleName == "translation-backend"
| summarize Records=count() by name
```

At workspace scope the corresponding table names are `AppRequests`, `AppTraces`,
`AppMetrics`, with columns such as `TimeGenerated`, `AppRoleName` and `OperationId`.

## Generate DEV traffic

Use the API request in [backend.http](backend.http). The localhost address tunnels
to the deployed AKS pod; it does not run a local copy of the backend. Restart
[Start-PortForward.ps1](Start-PortForward.ps1) after pod replacement if needed.
The business route currently returns 503 because production dependencies and
identity remain unregistered. API mappings are deployed; successful business
operations are not enabled by this release.

## Configuration and limits

- [Telemetry infrastructure](../../infra/translation-dev/telemetry.bicep) defines
  Application Insights and the dedicated 30-day Log Analytics workspace with a
  1 GB/day ingestion cap. The cap is not a strict billing ceiling and can stop
  ingestion; monitor usage if telemetry disappears.
- [Collector configuration](telemetry.yaml) configures all three signal pipelines.
- The Application Insights connection is held in Kubernetes Secret
  `translation-applicationinsights`, not committed to source or printed in logs.
  This DEV exporter uses connection-string ingestion, not workload-identity auth.
- Both application and collector run non-root with read-only root filesystems.
- Collector image v0.164.0 is pinned by digest. The Azure Monitor exporter is beta;
  validate ingestion again when upgrading. In-memory buffering can lose telemetry
  during failures/restarts; this is a development configuration.
- The single-replica DEV rollout permits replacing the old unready pod. It does
  not provide production availability guarantees.
