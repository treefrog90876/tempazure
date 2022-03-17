@description('Base name of all resources (invalid characters will be stripped when required).')
param name string = resourceGroup().name

@description('Optional objectId to grant an identity access to the key vault.')
param developerObjectIdKeyVaultAccessPolicy string = ''

@description('Location of all resources.')
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

var devAccessPolicy = {
  objectId: developerObjectIdKeyVaultAccessPolicy
  principalType: 'User'
}

module keyVault 'modules/key-vault.bicep' = {
  name: 'key-vault'
  params: {
    name: name
    location: location
    additionalSecrets: additionalSecrets
    additionalAccessPolicies: skip([
      devAccessPolicy
    ], empty(developerObjectIdKeyVaultAccessPolicy) ? 1 : 0)
  }
}

module sshKey 'modules/ssh-key.bicep' = {
  name: 'ssh-key-setup'
  params: {
    name: name
    location: location
    keyVaultName: keyVault.outputs.keyVaultName
    sshKeySecretName: 'generated-sshkey'
  }
}

output sshPublicKey string = sshKey.outputs.sshPublicKey
