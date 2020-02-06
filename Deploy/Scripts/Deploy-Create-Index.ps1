#! /usr/bin/pwsh
Function Createindex($resourceGroupName, $azSearchIndexName)
{
    $azSearchService = Get-AzSearchService -ResourceGroupName $resourceGroupName
    $azSearchAdminApiKey = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $azSearchService.name

    $headers = @{
    'api-key' = $azSearchAdminApiKey.Primary
    'Content-Type' = 'application/json'
    'Accept' = 'application/json' }

    $a = Get-Content -Raw -Path indexes/index_docs.json | Convertfrom-json
    $a.name = $azSearchIndexName

    $body = ConvertTo-Json -InputObject $a -Depth 10
    $url = "https://{0}.search.windows.net/indexes/{1}?api-version=2019-05-06" -f ($azSearchService.name, $azSearchIndexName)
    Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $body | ConvertTo-Json
}