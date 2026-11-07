# New Translation development cluster plan

Status: user approved AKS Automatic with a public API restricted to this machine's
public IPv4 address. Infrastructure deployment completed and the pod is running; see
[deployment-status.md](deployment-status.md) for current evidence.
Scope: subscription `SPSD.Sub1`, resource group `Translation-Dev`, region `eastus`.

## Verified prerequisites on 2026-10-30

- Interactive Azure sign-in works. `Translation-Dev` exists and contains no resources.
- No AKS cluster exists in the active subscription.
- `Microsoft.ContainerService` and `Microsoft.ContainerRegistry` are **NotRegistered**.
  Register them before creating AKS/ACR. Check `Microsoft.PolicyInsights` for
  Automatic/Azure Policy, plus the networking, compute, identity and monitoring
  providers used by the final configuration.
- East US total regional vCPU usage is 0 of 65. This does not establish VM-family
  quota, regional SKU capacity, or support for a particular node configuration.
- Installed Azure CLI AKS help does not expose an Automatic SKU option. Upgrade
  to a supported CLI before provisioning Automatic (Microsoft's current quickstart
  requires Azure CLI 2.86.0 or later); do not run a Standard creation command and
  assume it creates Automatic.
- Existing local image `translation-backend:local` is Linux .NET 8 and has not been
  published. Readiness intentionally fails until application dependencies exist.

## Recommended configuration

| Area | Proposal | Reason / remaining choice |
|---|---|---|
| Cluster | `translation-dev-aks`, AKS Automatic | Prefer managed defaults for a new cluster. Confirm recurring budget before creation. |
| Alternative | AKS Standard with explicitly bounded node pools | Use if cost controls or custom networking require it; choose node SKU, count and scaling bounds first. |
| Resource group | `Translation-Dev`, East US | Matches the user's scope and existing region. AKS also creates a managed node resource group. |
| Network | Azure CNI Overlay with Cilium | Check proposed CIDRs against connected networks before creation. |
| API access | Private API if a reachable VNet/VPN is available; otherwise public API restricted to approved administrator CIDRs | Must settle before provisioning; do not expose an unrestricted API as a convenience default. |
| Application access | Internal ClusterIP initially | Matches existing application deployment scaffolding. No ingress or public IP for the application yet. |
| Identity | Entra authentication, scoped Azure RBAC, OIDC/workload identity | Human access and future application identities without embedded passwords. |
| Registry | ACR Basic, classic registry RBAC, name chosen at provisioning | Suitable initial development registry; check global name availability. Reuse an existing suitable registry if the image pipeline already has one. |
| Image pull | Grant kubelet managed identity `AcrPull` on the classic-RBAC ACR | If ABAC is selected instead, use repository reader permissions; `AcrPull` is not the applicable ABAC role. |
| Workload | One replica, 100m CPU / 128Mi request, 1 CPU / 256Mi limit | Matches the build worktree's provisional sizing. Measure before increasing scale. |
| Secrets/storage | None for the current health-only host | Add Key Vault, federated identities and storage only when real adapters define the requirements. |
| Operations | Managed maintenance/upgrade configuration; bounded log retention and monitoring scope | Choose a maintenance window and review ongoing monitoring costs. |

The user approved the Automatic configuration and recurring charges on 2026-10-30.
Selected public API allowlist: `50.125.88.33/32`. An IP change requires updating
this allowlist. No existing subscription VNets were found; selected service/pod
ranges are `10.0.0.0/16` and `10.244.0.0/16`. No custom outbound restrictions are
configured. Network isolation policies should follow actual backend dependencies.

The infrastructure is defined in `infra/translation-dev/main.bicep`. Bicep and
Azure what-if/validation are used directly, so upgrading the old Azure CLI's AKS
creation command is unnecessary for this deployment. The Bicep type bundle lacks
the newest stable AKS API schema; Azure-side validation passed for that API.

## Provisioning sequence

1. Settle API access, network overlap, outbound requirements and cluster budget.
   Confirm Automatic versus a bounded Standard development cluster.
2. Upgrade Azure CLI if using Automatic. Register missing providers and verify
   region/version availability, family quota, node capacity and required RBAC.
3. Prepare the final infrastructure definition with explicit network/access
   settings. Run its validation and Azure what-if before applying it.
4. Create the cluster and ACR in `Translation-Dev`; configure Entra access and
   kubelet image-pull permissions. Avoid ACR admin passwords and AKS admin kubeconfigs.
5. Tag and push the local image to ACR with an immutable build tag. Capture the
   registry digest; update `kustomization.yaml` with that repository and digest.
   Match the Linux image architecture to the selected AKS node architecture.
6. Obtain scoped credentials, inspect the target context, create the namespace
   and run server-side manifest validation. Review the diff, then apply.
7. Expect the current image to be live but **not ready**. Record the intentional
   rollout failure; complete business adapters/readiness before declaring success.
   Do not route public traffic or scale beyond one replica until durable operation
   recovery and the backend's real dependencies are addressed.

## Completion criteria

- AKS and ACR exist in the intended scope, with approved access and network settings.
- A digest-pinned image pulls using managed identity and runs under the configured
  non-root/read-only restrictions.
- Startup/liveness succeed; readiness remains fail-closed until real dependencies
  and regression tests establish that the application can accept work.
- A successful application rollout requires readiness 200 and a real business
  request smoke test. The current image cannot satisfy that criterion.

## Sources

- [AKS Automatic setup and prerequisites](https://learn.microsoft.com/en-us/azure/aks/automatic/quick-automatic-managed-network)
- Build session: `01a18fc3-504f-7e21-8a83-be9ae08566f2`.
- Build worktree: `C:\Repos\cap\outputs\worktrees\deployment`, particularly
  `project/infrastructure/README.md`, `project/infrastructure/kubernetes/backend.yaml`,
  `project/src/Translation/Dockerfile` and `project/src/Translation/Host/Program.cs`.
