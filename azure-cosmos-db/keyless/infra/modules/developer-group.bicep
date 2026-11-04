extension microsoftGraphV1

@description('Display name of the security group.')
param displayName string

@description('Client-provided key that makes the deployment idempotent. Cannot be changed once set.')
param uniqueName string

@description('Object ID of a user (or CI principal) to add as a member. Use "" to add nobody.')
param memberPrincipalId string = ''

resource group 'Microsoft.Graph/groups@v1.0' = {
  uniqueName: uniqueName
  displayName: displayName
  mailEnabled: false
  mailNickname: uniqueName
  securityEnabled: true
  members: {
    relationshipSemantics: 'append'
    relationships: empty(memberPrincipalId) ? [] : [memberPrincipalId]
  }
}

@description('Object ID of the group – used for the Cosmos DB role assignment.')
output id string = group.id
