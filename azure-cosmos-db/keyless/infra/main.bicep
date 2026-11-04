targetScope = 'resourceGroup'

@description('Short name used to derive all resource names.')
@minLength(3)
@maxLength(12)
param environmentName string

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Object ID of a developer (or CI principal) to add to the developer group for local debugging. Defaults to whoever runs the deployment. Set to "" to skip.')
param developerPrincipalId string = deployer().objectId

@description('Display name of the Microsoft Entra security group that gets data access for local debugging.')
param developerGroupName string = 'Cosmos Developers'

@description('Cosmos DB database name.')
param databaseName string = 'todo'

@description('Cosmos DB container name.')
param containerName string = 'todos'

var suffix = uniqueString(resourceGroup().id, environmentName)
var tags = {
  'azd-env-name': environmentName
  sample: 'cosmos-keyless-managed-identity'
}

var cosmosDataContributor = '00000000-0000-0000-0000-000000000002'

var databaseScope = '/dbs/${databaseName}'

module identity 'modules/identity.bicep' = {
  name: 'identity'
  params: {
    name: 'id-${environmentName}-${suffix}'
    location: location
    tags: tags
  }
}

module cosmos 'modules/cosmos.bicep' = {
  name: 'cosmos'
  params: {
    accountName: 'cosmos-${environmentName}-${suffix}'
    location: location
    databaseName: databaseName
    containerName: containerName
    tags: tags
  }
}

module appDataAccess 'modules/cosmos-role-assignment.bicep' = {
  name: 'cosmos-rbac-app'
  params: {
    accountName: cosmos.outputs.accountName
    principalId: identity.outputs.principalId
    roleDefinitionGuid: cosmosDataContributor
    relativeScope: databaseScope
  }
}

module developerGroup 'modules/developer-group.bicep' = {
  name: 'developer-group'
  params: {
    displayName: developerGroupName
    uniqueName: 'cosmos-developers'
    memberPrincipalId: developerPrincipalId
  }
}

module developerDataAccess 'modules/cosmos-role-assignment.bicep' = {
  name: 'cosmos-rbac-developers'
  params: {
    accountName: cosmos.outputs.accountName
    principalId: developerGroup.outputs.id
    roleDefinitionGuid: cosmosDataContributor
    relativeScope: databaseScope
  }
}

module app 'modules/app-service.bicep' = {
  name: 'app'
  params: {
    planName: 'asp-${environmentName}-${suffix}'
    appName: 'app-${environmentName}-${suffix}'
    location: location
    identityId: identity.outputs.id
    identityClientId: identity.outputs.clientId
    cosmosEndpoint: cosmos.outputs.endpoint
    cosmosDatabaseName: cosmos.outputs.databaseName
    cosmosContainerName: cosmos.outputs.containerName
    tags: tags
  }
  dependsOn: [
    appDataAccess
  ]
}

output COSMOS_ENDPOINT string = cosmos.outputs.endpoint
output COSMOS_ACCOUNT_NAME string = cosmos.outputs.accountName
output COSMOS_DATABASE_NAME string = cosmos.outputs.databaseName
output COSMOS_CONTAINER_NAME string = cosmos.outputs.containerName
output AZURE_CLIENT_ID string = identity.outputs.clientId
output DEVELOPER_GROUP_ID string = developerGroup.outputs.id
output APP_NAME string = app.outputs.appName
output APP_URL string = app.outputs.url
