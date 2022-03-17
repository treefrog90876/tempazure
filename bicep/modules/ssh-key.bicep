@description('Name of the ssh key resource.')
param name string = resourceGroup().name

@description('Location of the ssh key resource.')
param location string = resourceGroup().location

@description('Name of the existing key vault to store the secrets.')
param keyVaultName string

@description('Name of the ssh key secret to put in key vault.')
param sshKeySecretName string

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource sshKey 'Microsoft.Compute/sshPublicKeys@2021-11-01' = {
  name: name
  location: location
}

// UserAssignedIdentity to run the script under
// It needs access to KeyVault which is set by the following role assignments
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: name
  location: location
}

@description('This is the built-in key vault administrator role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#key-vault-administrator')
resource keyVaultAdministratorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: resourceGroup()
  name: '00482a5a-887f-4fb3-b363-3b7fe8e74483'
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(identity.id, keyVault.id, keyVaultAdministratorRoleDefinition.id)
  scope: keyVault
  properties: {
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
    roleDefinitionId: keyVaultAdministratorRoleDefinition.id
  }
}

@description('This is the built-in contributor role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#contributor')
resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: resourceGroup()
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource rgRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, contributorRoleDefinition.id)
  properties: {
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
    roleDefinitionId: contributorRoleDefinition.id
  }
}

resource generateScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  location: location
  name: 'generateSshKeyScript'
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  dependsOn: [
    keyVault
  ]
  properties: {
    azCliVersion: '2.32.0'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'RESOURCE_GROUP'
        value: resourceGroup().name
      }
      {
        name: 'SSHKEY_RESOURCE_NAME'
        value: sshKey.name
      }
      {
        name: 'SSHKEY_NAME'
        value: sshKeySecretName
      }
      {
        name: 'KEY_VAULT_NAME'
        value: keyVault.name
      }
      {
        name: 'SUBSCRIPTION_ID'
        value: subscription().subscriptionId
      }
    ]
    scriptContent: '''
#!/bin/bash
set -euo pipefail

PUBLIC_KEY="$(az sshkey show --resource-group $RESOURCE_GROUP --name $SSHKEY_RESOURCE_NAME --query publicKey --output tsv)"

if [ -z "${PUBLIC_KEY}" ]; then
  echo "creating public/private key and storing in key vault."

  PRIVATE_KEY=$(az rest -m post -u "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.Compute/sshPublicKeys/${SSHKEY_RESOURCE_NAME}/generateKeyPair?api-version=2021-11-01" --query privateKey --output tsv)
  PUBLIC_KEY="$(az sshkey show --resource-group $RESOURCE_GROUP --name $SSHKEY_RESOURCE_NAME --query publicKey --output tsv)"

  az keyvault secret set --vault-name $KEY_VAULT_NAME --name "${SSHKEY_NAME}-privateKey" --value "${PRIVATE_KEY}" > /dev/null
  az keyvault secret set --vault-name $KEY_VAULT_NAME --name "${SSHKEY_NAME}-publicKey" --value "${PUBLIC_KEY}" > /dev/null
else
  echo "public/private key already exists."
fi

PUBLIC_KEY_JSON="$(az sshkey show --resource-group $RESOURCE_GROUP --name $SSHKEY_RESOURCE_NAME --query publicKey --output json)"

# Important variables: https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deployment-script-template#develop-deployment-scripts
echo $PUBLIC_KEY_JSON | jq "{ publicKey : . }" > $AZ_SCRIPTS_OUTPUT_PATH
'''
  }
}

output sshPublicKey string = generateScript.properties.outputs.publicKey
