#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$appFunctionName,
    [parameter(Mandatory=$true)][string]$folderSourceSolution
)

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location ..
Push-Location ..
Push-Location Source
Push-Location $folderSourceSolution

$sourceProjectFolder = (Get-Item -Path ".\").FullName

If (-not (Test-path $sourceProjectFolder))
{
    Write-Host "Solution source folder $sourceProjectFolder doesn't exists" -ForegroundColor Red
    exit 1
}

Pop-Location
Pop-Location
Push-Location .\Deploy

$publishOutputFolder = $(Join-Path -Path (Get-Item -Path ".\").FullName -ChildPath publish)
If(Test-path $publishOutputFolder)
{
    Remove-item $publishOutputFolder -Force -Recurse
}

New-Item -ItemType Directory -Force -Path $publishOutputFolder

# Compile source code in Publish folder
Write-Host "Compiling project in $publishOutputFolder..." -ForegroundColor Yellow
dotnet publish $sourceProjectFolder --output $publishOutputFolder
Write-Host "Project compiled"

# Zip compiled folder to Publish
$zipFileChildPath = "Tailwindtraders-shippingmanagement-"+$(Get-Date -Format "yyyyMMddHHmmss")+".zip";

$zipPublishFile = $(Join-Path -Path (Get-Item -Path ".\") -ChildPath $zipFileChildPath);


If(Test-path $zipPublishFile)
{
    Remove-item $zipPublishFile
}

Write-Host "Zipping compiled folder in zip file $zipPublishFile..." -ForegroundColor Yellow
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($publishOutputFolder, $zipPublishFile)
Write-Host "File zipped"

#Deploy ZIP file with Azure CLI
Write-Host "Deploying Azure function $appFunctionName in resource group $resourceGroup..." -ForegroundColor Yellow
az webapp deployment source config-zip --resource-group $resourceGroup --name $appFunctionName --src $zipPublishFile
Write-Host "Azure function deployed" -ForegroundColor Yellow

If(Test-path $zipPublishFile)
{
    Remove-item $zipPublishFile
}

If(Test-path $publishOutputFolder)
{
    Remove-item $publishOutputFolder -Force -Recurse
}