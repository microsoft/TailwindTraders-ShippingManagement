import json
import requests
import itertools
import logging
from shared_code.config.setting import Settings
from TailwindTraderFunc.cognitiveservices import CognitiveServices
from shared_code.storage.storage import BlobStorageService


class TailwindTraders():

    def __init__(self, req):
        self._settings = Settings()
        self._cs = CognitiveServices()
        self._storage = BlobStorageService(self._settings.get_storage_connection_string())
        self._reqbody = req.get_json()

    def readRequest(self):
        content = self._reqbody["values"][0]["data"]["content"]
        return content

    def getBlobUrlById(self, image_id):
        image = list(self._storage.list_blobs(self._settings.get_storage_container_name(),
                                            prefix=f'{image_id}.jpg'))
        image_url = self._storage.make_blob_url(self._settings.get_storage_container_name(),
                                                image[0].name)
        return image_url

    def getVisualFeaturesByImage(self, image_url):
        response_analyze = self._cs.getVisualFeaturesByImage(image_url, "analyze", {'visualFeatures': 'Description, Tags'})
        response_ocr = self._cs.getOCRByImage(image_url, "recognizeText")
        return {"analyze":response_analyze, "ocr":response_ocr}
    
    def updateItemField(self, item, content):
        item["Tags"] = content["analyze"]["tags"]
        item["VisualDetail"] = content["analyze"]["description"]
        recognition_result = content["ocr"]["recognitionResult"]
        item["OCRText"] = [line["text"] for line in recognition_result["lines"]]

    def generateResult(self, content):
        result = {"values": [{"recordId": self._reqbody["values"][0]["recordId"],
                            "data" : {"Items": content["Items"]}}]}
        result = json.dumps(result, sort_keys=True, indent=4, separators=(',', ': '))
        return result