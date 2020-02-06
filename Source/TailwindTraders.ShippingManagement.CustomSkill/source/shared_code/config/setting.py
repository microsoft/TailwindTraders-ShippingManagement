import json
import os
from singleton_decorator import singleton

@singleton
class Settings:
    def __init__(self):
        self.__config = self.__load_configuration()

    def __load_configuration(self):
        with open('./shared_code/config/settings.json') as file:
            return json.load(file)
    
    def get_storage_account(self):
        return os.environ.get("STORAGE_ACCOUNTNAME", self.__from_config("STORAGE_ACCOUNTNAME"))

    def get_storage_key(self):
        return os.environ.get("STORAGE_KEY", self.__from_config("STORAGE_KEY"))
    
    def get_storage_container_name(self):
        return os.environ.get("STORAGE_CONTAINER_NAME", self.__from_config("STORAGE_CONTAINER_NAME"))

    def get_computervision_key(self):
        return os.environ.get("COMPUTERVISION_KEY", self.__from_config("COMPUTERVISION_KEY"))

    def get_computervision_endpoint(self):
        return os.environ.get("COMPUTERVISION_ENDPOINT", self.__from_config("COMPUTERVISION_ENDPOINT"))
    
    def __from_config(self, key):
        return self.__config[key]