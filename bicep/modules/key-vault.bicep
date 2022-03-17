@description('Name of the key vault (must be valid).')
param name string = resourceGroup().name

@description('Location of the key vault.')
param location string = resourceGroup().location

@description('Additional secrets to inject into the key vault.')
@secure()
param additionalSecrets object = {
  secrets: [
    {
      name: 'example-secret-guid'
      secret: newGuid()
    }
  ]
}

@description('')
param additionalAccessPolicies array = []

var keyVaultName = toLower(take(replace(name, '_', ''), 24))
var keyVaultSku = {
  family: 'A'
  name: 'standard'
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: false
    enableSoftDelete: false
    sku: keyVaultSku
  }
}

@description('This is the built-in secret reader role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource secretReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

@description('This is the built-in key vault administrator role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#key-vault-secrets-officer')
resource secretOfficerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: resourceGroup()
  name: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2021-04-01-preview' = [for i in range(0, length(additionalAccessPolicies)): {
  name: guid(subscription().id, resourceGroup().id, additionalAccessPolicies[i].objectId, secretOfficerRoleDefinition.id, string(i))
  scope: keyVault
  properties: {
    roleDefinitionId: (additionalAccessPolicies[i].canWrite ? secretOfficerRoleDefinition.id : secretReaderRoleDefinition.id)
    principalId: additionalAccessPolicies[i].objectId
    principalType: additionalAccessPolicies[i].principalType
  }
}]

resource keyVault_additionalSecrets_items 'Microsoft.KeyVault/vaults/secrets@2021-10-01' = [for i in range(0, length(additionalSecrets.secrets)): {
  name: '${keyVault.name}/${additionalSecrets.secrets[i].name}'
  properties: {
    value: additionalSecrets.secrets[i].secret
  }
}]

output keyVaultName string = keyVault.name
