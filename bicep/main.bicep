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

@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GZRS'
  'Standard_RAGZRS'
])
param storageAccountSku string = 'Standard_LRS'

var appServicePlanName = name
var appServicePlanSku = {
  name: 'B1'
}
var webAppName = name
var storageAccountName = toLower(take(replace(replace(name, '-', ''), '_', ''), 24))
var appInsightsName = name
var functionsAppName = '${name}-functions'
var keyVaultName = toLower(take(replace(name, '_', ''), 24))

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  kind: 'StorageV2'
  location: location
  sku: {
    name: storageAccountSku
  }
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value}'

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  sku: appServicePlanSku
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2021-03-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      healthCheckPath: 'healthcheck'
    }
    httpsOnly: true
  }
}

resource webApp_appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    KeyVaultNameFromDeployment: keyVault.name
    FunctionsAppHostName : functionsApp.properties.defaultHostName
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

var devAccessPolicy = {
  objectId: developerObjectIdKeyVaultAccessPolicy
  principalType: 'User'
  canWrite: true
}

var webAppAccessPolicy = {
  objectId: webApp.identity.principalId
  principalType: 'ServicePrincipal'
  canWrite: false
}

var functionsAccessPolicy = {
  objectId: functionsApp.identity.principalId
  principalType: 'ServicePrincipal'
  canWrite: true
}

module keyVault 'modules/key-vault.bicep' = {
  name: keyVaultName
  params: {
    name: keyVaultName
    location: location
    additionalSecrets: {
      secrets: concat([
        {
          name: 'AzureStorageSettings--ConnectionString'
          secret: storageAccountConnectionString
        }
        {
          name: 'ApplicationInsights--InstrumentationKey'
          secret: appInsights.properties.InstrumentationKey
        }
      ], additionalSecrets.secrets)
    }
    additionalAccessPolicies: skip([
      devAccessPolicy
      webAppAccessPolicy
      functionsAccessPolicy
    ], empty(developerObjectIdKeyVaultAccessPolicy) ? 1 : 0)
  }
}

resource functionsApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionsAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlan.id
    reserved: true
    siteConfig: {
      healthCheckPath: 'api/healthcheck'
      appSettings: [
        {
          // I would prefer this comes from KeyVault, but the functions runtime consumes this before KV can be applied right now
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'KeyVaultNameFromDeployment'
          value: keyVaultName
        }
        {
          name: 'AzureWebJobsSecretStorageType'
          value: 'keyvault'
        }
        {
          name: 'AzureWebJobsSecretStorageKeyVaultUri'
          value: 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/'
        }
      ]
    }
  }
}

output storageEndpoint object = storageAccount.properties.primaryEndpoints
output webAppName string = webApp.name
output webAppEndpoint string = 'https://${webApp.properties.defaultHostName}/'
output webAppHealthCheckEndpoint string = 'https://${webApp.properties.defaultHostName}/healthcheck'
output functionsAppName string = functionsApp.name
output functionsAppHealthCheckEndpoint string = 'https://${functionsApp.properties.defaultHostName}/api/healthcheck'
