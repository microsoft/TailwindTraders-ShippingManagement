#! /usr/bin/pwsh
param (
    [parameter(Mandatory=$true)][string]$subscriptionId,
    [parameter(Mandatory=$true)][string]$subscriptionName,
    [Parameter(Mandatory=$true)][string]$resourceGroupName,
    [Parameter(Mandatory=$true)][string]$functionAppName,
    [Parameter(Mandatory=$true)][string]$functionName,
    [Parameter(Mandatory=$true)][string]$azsearchDatasourceName,
    [Parameter(Mandatory=$true)][string]$cosmosAccountName,
    [Parameter(Mandatory=$true)][string]$cosmosDataset,
    [Parameter(Mandatory=$true)][string]$skillsetName,
    [Parameter(Mandatory=$true)][string]$cognitiveServiceName,
    [Parameter(Mandatory=$true)][string]$azSearchIndexName,
    [Parameter(Mandatory=$true)][string]$indexerName
)

Write-Host "Login in your account" -ForegroundColor Yellow
az login

az account set --subscription $subscriptionId

Select-AzSubscription -SubscriptionName $subscriptionName

Write-Host "Install Az.Search and Az.CognitiveServices modules" -ForegroundColor Yellow
Install-Module -Name Az.Search -AllowClobber -Scope CurrentUser
Install-Module Az.CognitiveServices -AllowClobber -Scope CurrentUser

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location Scripts

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host "Creating Azure Search CosmosDB Datasource from $cosmosAccountName" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

. ".\Deploy-Create-Datasource.ps1"
CreateDatasource -resourceGroupName $resourceGroupName -azsearchDatasourceName $azsearchDatasourceName -cosmosAccountName `
                $cosmosAccountName -cosmosDataset $cosmosDataset

Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow
Write-Host "Creating Azure Search Skillset - $skillsetName" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

. ".\Deploy-Create-Skillset.ps1"
CreateSkillset -resourceGroupName $resourceGroupName -functionAppName $functionAppName -functionName $functionName `
            -skillsetName $skillsetName -cognitiveServiceName $cognitiveServiceName

Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow
Write-Host "Creating Azure Search Index - $azSearchIndexName" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

. ".\Deploy-Create-Index.ps1"
CreateIndex -resourceGroupName $resourceGroupName -azSearchIndexName $azSearchIndexName

Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow
Write-Host "Creating Azure Search Indexer - $indexerName" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

. ".\Deploy-Create-Indexer.ps1"
CreateIndexer -resourceGroupName $resourceGroupName -indexerName $indexerName -datasourceName $azsearchDatasourceName `
            -targetIndexName $azSearchIndexName -skillsetName $skillsetName

Pop-Location
Pop-Location