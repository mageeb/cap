# Translation development deployment evidence

Date: 2026-10-30 (America/Los_Angeles).
Status: infrastructure provisioned; manifests applied; backend container running.
Application availability remains intentionally blocked by readiness HTTP 503.

## API and telemetry update

- New API image digest: `sha256:70d18f43bbd93bd976b413236e254d4818f005eeb3ba95c203fca96099319387`;
  ACR build tag `dev-70d18f43bbd9`.
- Source: current deployment worktree, including uncommitted Host API/telemetry files.
  No application source was changed in this deployment session.
- Existing tests passed: 6/6 telemetry groups and 13/13 API checks.
- New restricted-container smoke check passed: live=200, ready=503, mapped GET
  `/api/books/{bookId:guid}`=503 and a structured correlated request log emitted.
- Provisioned `translation-dev-appinsights` and `translation-dev-logs` after
  successful Azure what-if/validation. Retention 30 days, ingestion cap 1 GB/day.
- Collector configuration validated locally using pinned official image v0.164.0.
- Server-side manifest validation passed. Applied API digest, ConfigMap, collector
  sidecar and Secret reference. Secret contents never written to source or output.
- Updated pod: `translation-backend-57b5ffb4c6-jw87m`. Both containers running;
  collector ready, backend readiness remains intentionally false.
- DEV HTTP verification: liveness 200, readiness 503, business GET 503 with
  `business_dependencies_not_configured`.
- Verification trace ID: `a4ed237d11bf4d9b93adcba1e9cb46d6`.
- Queried Application Insights and verified that exact operation ID has both a
  request record named `GET /api/books/{bookId:guid}` with resultCode 503 and its
  correlated structured completion log. Verified stored `customMetrics` records
  for `translation.http.requests`, `translation.http.request.duration` and .NET
  runtime metrics. This establishes cloud ingestion of all three signal types.
- Collector counters reported 1 exported span, 5 exported log records and 72
  exported metric points at the verification snapshot, without export errors.
- Restarted the user-facing localhost:18081 tunnel to the replacement pod.
  Closed the temporary collector-metrics diagnostic tunnel after verification.
- See [telemetry.md](telemetry.md) for Azure portal links, queries and limitations.

The remaining sections record the initial deployment before the API/telemetry update.

## Authorized configuration

- Subscription: `SPSD.Sub1`; resource group: `Translation-Dev`; region: `eastus`.
- User approved AKS Automatic with public API restricted to this machine's public
  IP and acknowledged AKS/ACR recurring charges.
- Cluster: `translation-dev-aks`; registry: `translationdevku67727xgop2y`.
- API allowlist: `50.125.88.33/32`; application Service: internal ClusterIP.

## Image and preflight checks

- Exact local image: `translation-backend:local`, Linux/amd64, UID 1654.
- Repeated restricted container smoke test: read-only root, dropped capabilities,
  no privilege escalation, writable temporary mount; live=200, ready=503 as designed.
- Published successfully to ACR as `translation-backend:dev-64bae0c5b9ce`.
- Image digest: `sha256:64bae0c5b9ce512b0cafdd1e1321b9738710ee338d062e507bd25bdbc29d05c2`.
- Release Kustomization pins that digest.
- Missing Azure providers registered. No existing VNets were found in the subscription.
- Bicep compilation passed, with a warning that the newest AKS resource API is
  absent from the local type bundle. Azure what-if and deployment validation passed.
- What-if showed only the intended cluster, registry and two scoped role assignments.
- Azure deployment `translation-dev-platform` completed successfully, including
  cluster, registry, kubelet `AcrPull` and operator cluster-scoped RBAC assignments.

## Cluster verification

- AKS SKU Automatic / Standard tier; Kubernetes nodes report v1.36.5.
- Server-side dry-run passed for the Namespace, ServiceAccount, Service and
  Deployment. Reviewed diff and applied all four resources.
- AKS deployment safeguards injected preferred anti-affinity and topology spread.
- Automatic provisioned application node `aks-default-7q5nm` (Standard_D4als_v7).
- Pod `translation-backend-74c756669c-jmth8` is Running with zero restarts.
- Startup probe passed; observed runtime UID/GID 1654 and the expected image digest.
- A temporary localhost-only port-forward verified actual cluster responses:
  `/health/live` = 200 `{"status":"alive"}`;
  `/health/ready` = 503 `{"status":"not_ready","reason":"business_dependencies_not_configured"}`.
  The port-forward was closed after the check.
- Service `translation-backend` is ClusterIP `10.0.162.75`, port 80, without an
  external IP. EndpointSlice reports `ready=false`, `serving=false` as intended.
- One endpoint query initially hit an authorization-service timeout; its retry passed.

## Access notes

- Dedicated non-admin kubeconfig: `%TEMP%\translation-dev.kubeconfig`.
- Temporary clients: `%TEMP%\translation-dev-aks-tools\kubectl.exe` and
  `%TEMP%\translation-dev-aks-tools\kubelogin-package\bin\windows_amd64\kubelogin.exe`.
  Add these directories to the current shell's PATH when using that kubeconfig.
- This machine's default DNS resolver cached the new API hostname as nonexistent.
  Cloudflare DNS and Google DNS independently resolved it to `172.171.162.31`.
  During diagnosis the dedicated temporary kubeconfig used that IP, with TLS server name
  `translation-dev-aks-w8q46vba.hcp.eastus.azmk8s.io`; certificate checking remains
  enabled. No machine DNS/hosts settings or API allowlists were widened.
- Default DNS subsequently resolved correctly. Refreshed the dedicated kubeconfig
  and reconverted it with `kubelogin -l azurecli`; normal hostname-based API access
  was restored and a final pod/Service query passed. No IP override remains.
- Kubernetes API access remains limited to `50.125.88.33/32`; update the Bicep
  parameter and review what-if if the administrator's public IP changes.
- Azure resources are running and incur charges, including managed networking and
  nodes. No public application endpoint was created.

## Application limitation

The image is a health-only host. Readiness intentionally returns HTTP 503 with
`business_dependencies_not_configured`, so Service traffic remains disabled.
Application adapters and business endpoints must be implemented before a healthy
application rollout is possible. Do not replace readiness with liveness/TCP.

## Remaining application work

Implement real business dependencies and readiness, publish the next image and
update the digest. Only then can rollout reach Available and the Service route
traffic. With this image, rollout status eventually reports its progress deadline
exceeded; that is the expected consequence of the fail-closed application gate.
