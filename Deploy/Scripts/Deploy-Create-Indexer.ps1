#! /usr/bin/pwsh
Function CreateIndexer($resourceGroupName, $indexerName, $datasourceName, $targetIndexName, $skillsetName)
{
    $azSearchService = Get-AzSearchService -ResourceGroupName $resourceGroupName
    $azSearchAdminApiKey = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $azSearchService.name

    $headers = @{
    'api-key' = $azSearchAdminApiKey.Primary
    'Content-Type' = 'application/json'
    'Accept' = 'application/json' }

    $a = Get-Content -Raw -Path indexers/indexer_images.json | Convertfrom-json
    $a.name = $indexerName
    $a.dataSourceName = $datasourceName
    $a.targetIndexName = $targetIndexName
    $a.skillsetName = $skillsetName

    $body = ConvertTo-Json -InputObject $a -Depth 10
    $url = "https://{0}.search.windows.net/indexers/{1}?api-version=2019-05-06" -f ($azSearchService.name, $indexerName)
    Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $body | ConvertTo-Json
}