#! /usr/bin/pwsh
Function CreateSkillset($resourceGroupName, $functionAppName, $functionName, $skillsetName, $cognitiveServiceName)
{
    $azSearchService = Get-AzSearchService -ResourceGroupName $resourceGroupName
    $azSearchAdminApiKey = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $azSearchService.name

    $headers = @{
    'api-key' = $azSearchAdminApiKey.Primary
    'Content-Type' = 'application/json'
    'Accept' = 'application/json' }

    $subscription = az account show | Convertfrom-json
    $subscriptionId = $subscription.id
    $resourceId = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$functionAppName"

    $accessToken = az account get-access-token --query accessToken -o tsv

    $listFunctionDetailUrl = "https://management.azure.com/$resourceId/functions/$functionName/?api-version=2019-08-01"
    $functionDetails = Invoke-RestMethod -Method GET -Uri $listFunctionDetailUrl ` -Headers @{ Authorization="Bearer $accessToken"; "Content-Type"="application/json" }
    $invokeUrl = $functionDetails.properties.invoke_url_template

    $listFunctionKeysUrl = "https://management.azure.com/$resourceId/functions/$functionName/listKeys?api-version=2019-08-01"
    $functionKeys = Invoke-RestMethod -Method POST -Uri $listFunctionKeysUrl ` -Headers @{ Authorization="Bearer $accessToken"; "Content-Type"="application/json" }
    $functionKey = $functionKeys.default

    $functionurl = "$invokeUrl/?code=$functionKey"

    $keys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -name $cognitiveServiceName

    $a = Get-Content -Raw -Path skillsets/imageskillset.json | Convertfrom-json
    $a.name = $skillsetName
    $a.cognitiveServices.description = $cognitiveServiceName
    $a.cognitiveServices.key = $keys.Key1
    $a.skills[0].uri = $functionurl

    $body = ConvertTo-Json -InputObject $a -Depth 10
    $url = "https://{0}.search.windows.net/skillsets/{1}?api-version=2019-05-06" -f ($azSearchService.name, $skillsetName)
    Invoke-RestMethod -Uri $url -Headers $headers -Method Put -Body $body | ConvertTo-Json
}