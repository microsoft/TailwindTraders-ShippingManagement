#! /usr/bin/pwsh
Function CreateDatasource($resourceGroupName, $azsearchDatasourceName, $cosmosAccountName, $cosmosDataset)
{
    $azSearchService = Get-AzSearchService -ResourceGroupName $resourceGroupName
    $azSearchAdminApiKey = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $azSearchService.name
    
    $headers = @{
    'api-key' = $azSearchAdminApiKey.Primary
    'Content-Type' = 'application/json' 
    'Accept' = 'application/json' }
    
    $cosmosdbProperties = Get-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts" `
        -ApiVersion "2015-04-08" -ResourceGroupName $resourceGroupName `
        -Name $cosmosAccountName | Select-Object Properties
    
    $keys = Invoke-AzResourceAction -Action listKeys -ResourceType "Microsoft.DocumentDb/databaseAccounts" `
            -ApiVersion "2015-04-08" -ResourceGroupName $resourceGroupName `
            -Name $cosmosAccountName -Force
    
    $cosmosdbConnectionString = "AccountEndpoint={0};AccountKey={1};Database={2}" `
            -f ($cosmosdbProperties.Properties.documentEndpoint, $keys.primaryMasterKey, $cosmosDataset)
    
    $a = Get-Content -Raw -Path datasource/datasource.json | Convertfrom-json
    $a.name = $azsearchDatasourceName
    $a.description = $azsearchDatasourceName
    $a.type = "cosmosdb"
    $a.credentials.connectionString = $cosmosdbConnectionString
    $a.container.name = "PackageSlips"
    
    $body = ConvertTo-Json -InputObject $a -Depth 10
    $url = "https://{0}.search.windows.net/datasources/{1}?api-version=2019-05-06" `
            -f ($azSearchService.name, $azsearchDatasourceName)
    Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $body | ConvertTo-Json
}