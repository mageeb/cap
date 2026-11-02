# Backend container and Kubernetes preparation

Status: container hosting package with fail-closed business API contracts. Real input/storage/provider/identity adapters are not implemented. `/health/live` returns 200;
`/health/ready` deliberately returns 503. Kubernetes therefore keeps this pod out of
Service endpoints and rollout will time out until application readiness is implemented.
Do not change the readiness probe to liveness to make a rollout appear successful.

Run commands from the deployment worktree root. Requires .NET 8 SDK locally;
Docker with Linux containers and kubectl are needed for image/cluster checks.

## Local verification

```powershell
dotnet publish project/src/Translation/Host/Translation.Host.csproj -c Release -o outputs/host-publish
powershell -NoProfile -ExecutionPolicy Bypass -File project/infrastructure/Test-Host.ps1 -PublishedDirectory outputs/host-publish
dotnet run --project project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj
```

The smoke script checks actual HTTP status/body and rejects a busy test port.
The backend unit/component coverage suite is maintained separately on `feature/tests`.

## Build and smoke the image

```powershell
docker build --pull -t translation-backend:local project/src/Translation
docker run --rm --read-only --cap-drop ALL --security-opt no-new-privileges --tmpfs /tmp:rw,noexec,nosuid,size=64m -p 127.0.0.1:18080:8080 --name translation-backend translation-backend:local
```

In a second terminal, `curl.exe -i http://localhost:18080/health/live` must return 200;
`curl.exe -i http://localhost:18080/health/ready` must return 503 with the documented
reason. Stop with `docker stop translation-backend`. The image contains the backend
assembly and host, runs as .NET's non-root UID 1654 and listens on 8080.
The context excludes tests, build output, frontend and local environment files.

## Prepare a target-specific manifest

`kubernetes/backend.yaml` is a template using a local image, one replica, ClusterIP,
probes, bounded resources and restricted Linux container settings. Resource sizing is
provisional and must be load-tested. No ingress, TLS, credentials or cloud binding is
selected. No public endpoint is created. Use an explicit namespace and Kubernetes context.

Build and push a unique image tag to your chosen registry after registry access is
configured. Substitute a registry digest in the manifest to make rollout reproducible:

```powershell
# Replace this example with the pushed image digest; never apply the example unchanged.
$image = 'registry.example.com/team/translation-backend@sha256:REPLACE_WITH_DIGEST'
powershell -NoProfile -ExecutionPolicy Bypass -File project/infrastructure/Prepare-Manifest.ps1 -Image $image -OutputPath outputs/backend.rendered.yaml
kubectl apply --dry-run=client -f outputs/backend.rendered.yaml
# With an authorized target cluster, validate admission without persisting resources:
kubectl --context YOUR_CONTEXT -n YOUR_NAMESPACE apply --dry-run=server -f outputs/backend.rendered.yaml
```

Private registries require target-specific pull credentials or workload/node identity.
The namespace must already exist. Image publishing and cluster mutation are subsequent
steps; this task does neither. A local cluster must load the local image into its nodes
before using `translation-backend:local`.

## Remaining rollout blockers

Implement the approved real adapters and authentication behind the tested business HTTP contracts, then
replace the fail-closed readiness response with dependency-aware checks and regression
coverage. Resolve durable operation recovery before scaling beyond one replica. Review
the deployment runtime's current support and security patch policy; .NET 8 preserves the
existing library target here and is not a long-term runtime commitment. Pin the approved
base image digests and application digest after pulling/scanning actual images. Verify
image startup under the restricted security settings, run server-side manifest validation,
and measure resource limits before approving rollout.

Feature status and actual verification: [004 tasks](../specs/004-container-deployment/tasks.md).
Design choice: [DEC-037](../../DECISIONS.md#dec-037-container-hosting-preparation).
Container/probe guidance: [Microsoft .NET containers](https://learn.microsoft.com/en-us/dotnet/core/docker/),
[Kubernetes probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/).


API request logging and OpenTelemetry export: [observability runbook](observability.md).
Business contracts are tested with doubles; production routes stay blocked until adapters
and validated identity are configured. Readiness remains deliberately unhealthy.
