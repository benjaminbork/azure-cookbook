@description('Name of the App Service plan.')
param planName string

@description('Name of the Web App (globally unique).')
param appName string

@description('Azure region.')
param location string

@description('Resource ID of the user-assigned managed identity.')
param identityId string

@description('Client ID of the user-assigned managed identity.')
param identityClientId string

@description('Cosmos DB account endpoint, e.g. https://<account>.documents.azure.com:443/.')
param cosmosEndpoint string

@description('Cosmos DB database name.')
param cosmosDatabaseName string

@description('Cosmos DB container name.')
param cosmosContainerName string

@description('App Service plan SKU. F1 (Free) needs no VM quota; use B1 or higher for production.')
param skuName string = 'F1'

@description('Tags applied to the resources.')
param tags object = {}

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: skuName
  }
  properties: {
    reserved: true
  }
}

resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  tags: union(tags, { 'azd-service-name': 'api' })
  kind: 'app,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: skuName != 'F1'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'Cosmos__Endpoint'
          value: cosmosEndpoint
        }
        {
          name: 'Cosmos__DatabaseName'
          value: cosmosDatabaseName
        }
        {
          name: 'Cosmos__ContainerName'
          value: cosmosContainerName
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: identityClientId
        }
      ]
    }
  }
}

output appName string = app.name
output url string = 'https://${app.properties.defaultHostName}'
