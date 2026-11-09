# Translation development infrastructure

`main.bicep` provisions the user-approved AKS Automatic cluster, ACR Basic registry,
kubelet image-pull role and operator Entra cluster access in `Translation-Dev`.
It uses managed identities, disables local cluster accounts and registry passwords,
and restricts the Kubernetes API to the operator's public IPv4 CIDR.

Parameters: `location`, `clusterName`, `registryName`, `adminCidr`, `operatorObjectId`.
Defaults use the resource group's region and a deterministic globally unique
registry name. The operator must be an Entra user. CIDRs are explicit in the
template; review overlap before connecting this cluster to other networks.

Provisioned through Azure Resource Manager, independently of the old CLI's
`az aks create` feature set. Bicep compilation and Azure what-if/validate passed
before creation. The current Bicep type bundle cannot validate the newest stable
AKS API locally; remote Azure validation provides the resource schema check.

For a future update, use the same deployment name, review what-if and validate
before create. Preserve the established parameter values unless intentionally
changing the configuration. Never widen `adminCidr` to bypass a connectivity issue.

Workload deployment and current evidence:
[deployment-status.md](../../deploy/translation-dev/deployment-status.md).
