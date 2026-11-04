@description('Name of the existing Cosmos DB account.')
param accountName string

@description('Object (principal) ID of the identity that gets access.')
param principalId string

@description('GUID of the data-plane role definition (built-in or custom).')
param roleDefinitionGuid string = '00000000-0000-0000-0000-000000000002'

@description('Relative scope inside the account, e.g. "/dbs/todo". Use "" for the whole account.')
param relativeScope string = ''

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' existing = {
  name: accountName
}

var roleDefinitionId = '${account.id}/sqlRoleDefinitions/${roleDefinitionGuid}'
var scope = '${account.id}${relativeScope}'

resource assignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-11-15' = {
  parent: account
  name: guid(roleDefinitionId, principalId, scope)
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    scope: scope
  }
}

output assignmentId string = assignment.id
