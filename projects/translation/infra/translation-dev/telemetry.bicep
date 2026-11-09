targetScope = 'resourceGroup'
param location string = resourceGroup().location
param workspaceName string = 'translation-dev-logs'
param applicationInsightsName string = 'translation-dev-appinsights'

// Dedicated DEV retention and daily ingestion cap keep the initial scope bounded.
resource workspace 'Microsoft.OperationalInsights/workspaces@2026-03-01' = {
  name: workspaceName
  location: location
  tags: { environment: 'development', application: 'translation' }
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
    workspaceCapping: { dailyQuotaGb: 1 }
  }
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  tags: { environment: 'development', application: 'translation' }
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    IngestionMode: 'LogAnalytics'
    // Collector connection string is supplied using a Kubernetes Secret, never source.
    DisableLocalAuth: false
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output applicationInsightsId string = insights.id
output workspaceId string = workspace.id
