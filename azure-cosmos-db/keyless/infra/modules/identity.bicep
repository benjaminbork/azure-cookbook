@description('Name of the user-assigned managed identity.')
param name string

@description('Azure region.')
param location string

@description('Tags applied to the resource.')
param tags object = {}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
  tags: tags
}

@description('Resource ID – used to attach the identity to compute.')
output id string = identity.id

@description('Object (principal) ID – used for the Cosmos DB role assignment.')
output principalId string = identity.properties.principalId

@description('Client (application) ID – passed to the app as AZURE_CLIENT_ID.')
output clientId string = identity.properties.clientId
