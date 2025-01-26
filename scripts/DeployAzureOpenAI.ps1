param(
    [Parameter(Mandatory=$true)]
    [string]$resourceSuffix
)
$ResourceGroup = 'rg-custom-engine-agent-local'
$Location = 'eastus'
$botName="bot-" + $resourceSuffix + "-local"
# $BotTemplateFile = 'deployBotService.bicep'
$AzureOpenAITemplateFile = 'deployAzureOpenAI.bicep'

Write-Host("================================================")
Write-Host("Logging in to Azure...")
Write-Host("================================================")
# Login to Azure
az login

# Create the Azure Resource Group
Write-Host("================================================")
Write-Host("Creating the Azure Resource Group...")
Write-Host("================================================")
$resourceGroupExists = az group exists --name $ResourceGroup
if ($resourceGroupExists -eq $false) {
    az group create --name $ResourceGroup --location $Location
}

# # Deploy the Azure Bot Service
# Write-Host("================================================")
# Write-Host("Deploying Azure Bot Service...")
# Write-Host("================================================")
# $deploymentResult = az deployment group create `
#     --resource-group $ResourceGroup `
#     --template-file $BotTemplateFile `
#     --only-show-errors `
#     --output json | ConvertFrom-Json

# $appId = $deploymentResult.properties.outputs.appId.value
# $tenantId = $deploymentResult.properties.outputs.tenantId.value
# $botName = $deploymentResult.properties.outputs.botApp.value
  
# Generate a new client secret for the returned App ID
# $deploymentSecretResult = az ad app credential reset --id $appId --output json | ConvertFrom-Json
# $secret = $deploymentSecretResult.password

Write-Host("================================================")
Write-Host("Deploying Azure OpenAI...")
Write-Host("================================================")

# Deploy the Azure OpenAI service
$deploymentOpenAIResult = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file $AzureOpenAITemplateFile `
    --parameters resourceBaseName=$botName `
    --only-show-errors `
    --output json | ConvertFrom-Json
  
$azureOpenAIEndpoint = $deploymentOpenAIResult.properties.outputs.AZURE_OPENAI_ENDPOINT.value
$azureOpenAIApiKey = $deploymentOpenAIResult.properties.outputs.SECRET_AZURE_OPENAI_API_KEY.value

Write-Host("================================================")
# Write-Host ("App Id: $appId")
# Write-Host ("Tenant Id: $tenantId")
# Write-Host ("Secret: $secret")
Write-Host ("Azure OpenAI Endpoint: $azureOpenAIEndpoint")
Write-Host ("Azure OpenAI ApiKey: $azureOpenAIApiKey")
Write-Host ("Azure OpenAI deployment name: gpt-4o-mini")
Write-Host("================================================")

# App Id: $appId
# Tenant Id: $tenantId
# Secret: $secret

$logContent = @"
Azure OpenAI Endpoint: $azureOpenAIEndpoint
Azure OpenAI ApiKey: $azureOpenAIApiKey
"@
Set-Content -Path "$Env:USERPROFILE\Desktop\Credentials.txt" -Value $logContent