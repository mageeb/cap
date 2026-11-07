targetScope = 'resourceGroup'

@description('Azure region for the approved development environment.')
param location string = resourceGroup().location

param clusterName string = 'translation-dev-aks'
param registryName string = 'translationdev${uniqueString(resourceGroup().id)}'

@description('Administrator public IPv4 CIDR; no unrestricted API access.')
param adminCidr string

@description('Entra object ID of the operator performing the initial deployment.')
param operatorObjectId string

// Automatic supplies managed node configuration, upgrades and platform safeguards.
// Use the newest generally available resource API advertised by this subscription.
resource aks 'Microsoft.ContainerService/managedClusters@2026-08-01' = {
  name: clusterName
  location: location
  tags: { environment: 'development', application: 'translation' }
  sku: { name: 'Automatic', tier: 'Standard' }
  identity: { type: 'SystemAssigned' }
  properties: {
    dnsPrefix: clusterName
    enableRBAC: true
    disableLocalAccounts: true
    aadProfile: {
      managed: true
      enableAzureRBAC: true
      tenantID: subscription().tenantId
    }
    apiServerAccessProfile: {
      enablePrivateCluster: false
      authorizedIPRanges: [adminCidr]
    }
    networkProfile: {
      networkPlugin: 'azure'
      networkPluginMode: 'overlay'
      networkDataplane: 'cilium'
      loadBalancerSku: 'standard'
      serviceCidr: '10.0.0.0/16'
      dnsServiceIP: '10.0.0.10'
      podCidr: '10.244.0.0/16'
    }
    oidcIssuerProfile: { enabled: true }
    securityProfile: { workloadIdentity: { enabled: true } }
    hostedSystemProfile: { enabled: true }
  }
}

// Private image content; public registry endpoint uses Entra authentication.
resource registry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: registryName
  location: location
  tags: { environment: 'development', application: 'translation' }
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
    roleAssignmentMode: 'LegacyRegistryPermissions'
  }
}

var acrPullRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
resource imagePull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, aks.id, acrPullRole)
  scope: registry
  properties: {
    roleDefinitionId: acrPullRole
    principalId: aks.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
  }
}

// Cluster-scoped access permits initial namespace and workload creation using Entra.
var clusterAdminRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b1ff04bb-8a4e-4dc4-8eb5-8693973ce19b')
resource operatorAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aks.id, operatorObjectId, clusterAdminRole)
  scope: aks
  properties: {
    roleDefinitionId: clusterAdminRole
    principalId: operatorObjectId
    principalType: 'User'
  }
}

output clusterName string = aks.name
output registryName string = registry.name
output registryLoginServer string = registry.properties.loginServer
output clusterId string = aks.id
