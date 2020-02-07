#! /usr/bin/pwsh
param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$true)][string]$resourcePrefixName,
    [parameter(Mandatory=$true)][string]$subscription
)

Write-Host "Login in your account" -ForegroundColor Yellow
az login

#Write-Host "Choosing your subscription" -ForegroundColor Yellow
az account set --subscription $subscription

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location Scripts

& ./Deploy-ARMs.ps1 -resourceGroup $resourceGroup -location $location -resourcePrefixName $resourcePrefixName

$storageName = $(az resource list --resource-group $resourceGroup --resource-type Microsoft.Storage/storageAccounts -o json | ConvertFrom-Json)[0].name
$containerName = "$resourcePrefixName-trainmodel"
$containerNameProducts = "$resourcePrefixName-products"
$appFunctionName = $(az resource list --resource-group $resourceGroup --resource-type Microsoft.Web/sites -o json | ConvertFrom-Json)[0].name

& ./Deploy-Publish-Content.ps1 -resourceGroup $resourceGroup -storageName $storageName -containerName $containerName -containerNameProducts $containerNameProducts -appFunctionName $appFunctionName

Pop-Location
Pop-Location