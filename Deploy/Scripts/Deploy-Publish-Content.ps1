Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$storageName,
    [parameter(Mandatory=$true)][string]$containerName,
    [parameter(Mandatory=$true)][string]$appFunctionName
)

$folderSourceSolution = "TailwindTraders.ShippingManagement"

Push-Location $($MyInvocation.InvocationName | Split-Path)

Write-Host "Begining the Uploading model files to Storage: $storageName" -ForegroundColor Yellow
& ./Deploy-Dataset-Azure.ps1 -resourceGroup $resourceGroup -storageName $storageName -containerName $containerName
Write-Host "Files uploaded..."
 
Write-Host "Begining deploying Azure function: $appFunctionName" -ForegroundColor Yellow
& ./Deploy-AppFunction-Azure.ps1 -resourceGroup $resourceGroup -appFunctionName $appFunctionName -folderSourceSolution $folderSourceSolution
Write-Host "Azure function updated..."

Pop-Location