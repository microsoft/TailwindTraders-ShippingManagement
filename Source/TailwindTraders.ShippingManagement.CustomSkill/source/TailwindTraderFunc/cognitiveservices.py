import requests
from shared_code.config.setting import Settings
from shared_code.utils.enums import CsResponseStatus

class CognitiveServices():
    
    def __init__(self):
        self._settings = Settings()
        self._headers = {'Ocp-Apim-Subscription-Key': self._settings.get_computervision_key()}

    def getVisualFeaturesByImage(self, image_url, cs_type, params={}):
        data = {'url': image_url}
        response = requests.post(self._settings.get_computervision_endpoint() + cs_type,
                                headers=self._headers, params=params, json=data)
        response.raise_for_status()
        return response.json()

    def getOCRByImage(self, image_url, cs_type, params={"mode": "Printed"}):
        data = {'url': image_url}
        response_recognizetext = requests.post(self._settings.get_computervision_endpoint() + cs_type,
                                headers=self._headers, params=params, json=data)
        response_recognizetext.raise_for_status()
        operation_location = response_recognizetext.headers["Operation-Location"]

        ocr = {"status": CsResponseStatus.NotStarted.value}

        while ocr["status"] == CsResponseStatus.Running.value or ocr["status"] == CsResponseStatus.NotStarted.value:
            response_textoperation = requests.get(operation_location, headers=self._headers)
            response_textoperation.raise_for_status()
            ocr = response_textoperation.json()

        if ocr["status"] == CsResponseStatus.Failed.value:
            ocr["recognitionResult"] = {"lines": []}
        
        return ocr