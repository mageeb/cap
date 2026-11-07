# Translation development backend

Prepared for resource group `Translation-Dev` in `eastus`.
The latest release adds mapped business APIs and Azure application telemetry;
see [telemetry.md](telemetry.md). Business dependencies/readiness remain gated.
Initial inspection on 2026-10-30 found an empty resource group and no AKS clusters
in the active subscription (`SPSD.Sub1`). The user subsequently authorized
provisioning and deployment; see [deployment-status.md](deployment-status.md).

## Initial image-build handoff (before the API/telemetry update)

Read from local session `01a18fc3-504f-7e21-8a83-be9ae08566f2` and its
deployment worktree, `C:\Repos\cap\outputs\worktrees\deployment`:

- Image `translation-backend:local` has been built and smoke-tested on Docker
  context `desktop-linux`; it has not been pushed to a registry.
- Source: `project/src/Translation/Dockerfile` and `Host/Program.cs`.
  Runtime is .NET 8 Linux, entry point `Translation.Host.dll`, port 8080, UID 1654.
- The build session verified non-root execution, a read-only filesystem, and
  dropped capabilities. This session has not repeated those container tests.
- `/health/live` returns 200; `/health/ready` intentionally returns 503 with
  `business_dependencies_not_configured`. No business HTTP endpoints exist yet.
- The manifests preserve HTTP readiness. A deployed pod will remain unready and
  rollout will time out until real dependency adapters/readiness are implemented.
  Do not substitute a TCP or liveness readiness probe to bypass that behavior.

## Workload configuration

- Linux ASP.NET Core HTTP host, now including mapped business APIs with a
  fail-closed dependency gate. Source is in the separate
  worktree above; these Azure-specific files do not modify that worktree.
- One development replica. DEV updates permit replacing the old unready pod
  (`maxUnavailable: 1`, `maxSurge: 0`); production availability is not guaranteed.
- HTTP on port 8080, configured using `ASPNETCORE_URLS`.
- Internal ClusterIP service on port 80. No public endpoint is provisioned.
- Numeric user/group 1654, matching the built image. Read-only root filesystem
  and writable `/tmp` match the build session's verified restrictions.
- Initial CPU/memory requests and limits are estimates; tune using measurements.
- Startup/liveness use `/health/live`; readiness uses `/health/ready`.
- No secrets, database configuration, storage, ingress, or workload identity
  bindings are assumed. Add these when the backend requirements are known.
- `ASPNETCORE_ENVIRONMENT=Production` avoids developer exception pages. The Azure
  resource group name does not require the ASP.NET Development environment.

## New development cluster

The user requested a new development-cluster plan. See
[cluster-plan.md](cluster-plan.md) for the recommended configuration, verified
prerequisites, remaining choices and provisioning sequence.

## Published image

`kustomization.yaml` now references the published ACR repository by digest:

```text
translationdevku67727xgop2y.azurecr.io/translation-backend@sha256:70d18f43bbd93bd976b413236e254d4818f005eeb3ba95c203fca96099319387
```

The build tag is `dev-70d18f43bbd9`. For subsequent releases, push a new immutable
build tag and update the digest in `kustomization.yaml`. Always apply using `-k`;
`workload.yaml` retains its base image placeholder and is not a standalone release.

## Validate and deploy once a cluster and image exist

Install `kubectl` and, for Entra-enabled AKS authentication, `kubelogin`. Fetch
non-admin credentials into a dedicated kubeconfig after confirming the target:

```powershell
$env:KUBECONFIG = Join-Path $env:TEMP 'translation-dev.kubeconfig'
az aks get-credentials --resource-group Translation-Dev --name translation-dev-aks --file $env:KUBECONFIG
kubelogin convert-kubeconfig -l azurecli
kubectl config current-context
kubectl kustomize deploy/translation-dev
```

Check the rendered image and target context. Then perform server validation. The
namespace must exist for namespaced server validation, so create it explicitly:

```powershell
kubectl create namespace translation-dev --dry-run=client -o yaml | kubectl apply -f -
kubectl apply --dry-run=server -k deploy/translation-dev
kubectl diff -k deploy/translation-dev
kubectl apply -k deploy/translation-dev
kubectl -n translation-dev rollout status deployment/translation-backend --timeout=300s
kubectl -n translation-dev get pods,services
kubectl -n translation-dev port-forward service/translation-backend 8080:80
```

The temporary kubeconfig file is reused if it already exists; choose a different
filename if needed. `kubectl diff` returns exit code 1 when differences exist.
After readiness is implemented and rollout succeeds, open
`http://localhost:8080/health/live` while port-forwarding. In the current health-only
state, the Service has no ready endpoints and Service port-forwarding may fail;
for diagnostics only, forward `deployment/translation-backend 8080:8080` and
confirm `/health/ready` is still 503. If the service needs external
clients, design Gateway API routing, TLS and authentication before exposure.
The ClusterIP service does not restrict access from other cluster workloads.

For a failed update, inspect pod events/logs and use
`kubectl -n translation-dev rollout undo deployment/translation-backend` to restore
a prior revision, if one exists. The first rollout has no prior revision.

## Validation performed

`kubectl v1.35.0 kustomize deploy/translation-dev` rendered the Namespace,
ServiceAccount, Deployment and Service successfully. The temporary validation
binary was downloaded from `dl.k8s.io` and checked against its SHA-256 checksum.
For subsequent cloud validation and deployment results, see
[deployment-status.md](deployment-status.md).

## References

- [ASP.NET Core container port configuration](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port)
- [Non-root .NET containers](https://devblogs.microsoft.com/dotnet/securing-containers-with-rootless/)
- [Kubernetes startup and readiness probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [AKS Automatic](https://learn.microsoft.com/en-us/azure/aks/automatic/quick-automatic-managed-network)
