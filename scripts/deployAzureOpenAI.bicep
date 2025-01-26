@description('Complete resource name for both Bot Service and App Registration.')
param resourceBaseName string

param location string = 'eastus'
param modelName string = 'gpt-4o-mini'
param modelVersion string = '2024-07-18'
param modelDeploymentName string = 'gpt-4o-mini'

resource account 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: resourceBaseName
  location: location
  kind: 'OpenAI'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: resourceBaseName
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    disableLocalAuth: false
  }
  sku: {
    name: 'S0'
  }
}

resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: account
  name: modelDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: modelName
      version: modelVersion
    }
  }
  sku: {
    name: 'Standard'
    capacity: 10
  }
}

output SECRET_AZURE_OPENAI_API_KEY string = listKeys(account.id, '2022-12-01').key1
output AZURE_OPENAI_ENDPOINT string = account.properties.endpoint
