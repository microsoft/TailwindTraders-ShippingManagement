import azure.functions as func
from TailwindTraderFunc.tailwindtrader import TailwindTraders


def main(req: func.HttpRequest) -> func.HttpResponse:
    twt = TailwindTraders(req)
    content = twt.readRequest()

    for i in range(len(content["Items"])):
        image_url = twt.getBlobUrlById(content["Items"][i]["Reference"]["Value"])
        visualfeatures = twt.getVisualFeaturesByImage(image_url)
        twt.updateItemField(content["Items"][i], visualfeatures)

    result = twt.generateResult(content)
    return func.HttpResponse(body=result, mimetype="application/json")