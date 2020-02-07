import os, logging, shutil, datetime
from ..utils.mime import mime_content_type
from azure.storage.blob import BlockBlobService, ContainerPermissions, ContentSettings
from azure.storage.blob import BlobPermissions
from singleton_decorator import singleton
from datetime import timedelta

@singleton
class BlobStorageService:
    def __init__(self, account_name, key):
        self.__blockblob_service = BlockBlobService(
            account_name=account_name, account_key=key)

    def create_container(self, container_name):
        self.__blockblob_service.create_container(container_name)

    def delete_container(self, container_name):
        self.__blockblob_service.delete_container(container_name)

    def upload_file(self, container_name, filename, local_file, delete_local_file=False):
        self.create_container(container_name)
        return self.__upload_file(container_name, filename, local_file, delete_local_file)
    
    def upload_file_from_bytes(self, container_name, filename, blob):
        self.create_container(container_name)
        return self.__upload_file_from_bytes(container_name, filename, blob)

    def upload_directory(self, container_name, directory, storage_path=""):
        self.create_container(container_name)
        files = self.__get_files(directory)
        directories = self.__get_directories(directory)

        blobs = list(map(lambda file: self.__upload_file(container_name,
                                                         os.path.join(
                                                             storage_path, os.path.basename(file)),
                                                         os.path.join(directory, file)), files))

        return blobs + list(map(lambda dir: self.upload_directory(
            container_name, os.path.join(directory, dir), storage_path), directories))

    def list_blobs(self, container_name, prefix):
        return self.__blockblob_service.list_blobs(container_name, prefix)

    def download_blob(self, container_name, blob_name, local_file=None):
        local_file = blob_name if local_file == None else local_file
        self.__create_local_dir(os.path.split(local_file)[0])
        self.__blockblob_service.get_blob_to_path(
            container_name, blob_name, local_file)
    
    def download_blob_bytes(self, container_name, blob_name, local_file=None):
        b = self.__blockblob_service.get_blob_to_bytes(container_name, blob_name)
        return b

    def download_blobs(self, container_name, local_path="", blob_path=""):
        blobs = self.__get_blobs_in_path(container_name, blob_path)
        return blobs
    
    def download_all_blobs(self, container_name, local_path="", blob_path=""):
        blobs = self.__get_blobs_in_path(container_name, blob_path)
        base = self.__create_local_dir(local_path)

        list(map(lambda blob: self.download_blob(container_name, blob.name,
                                                 os.path.join(base, blob.name)), blobs))

    def delete_blob(self, container, blob_name):
        self.__blockblob_service.delete_blob(
            container, blob_name)

    def __upload_file(self, container_name, filename, local_file, delete_local_file=False):
        blob = self.__blockblob_service.create_blob_from_path(container_name,
                                                              filename,
                                                              local_file,
                                                              content_settings=ContentSettings(content_type=self.__get_mime_type(local_file)))
        if delete_local_file:
            os.remove(local_file)
        return blob
    
    def __upload_file_from_bytes(self, container_name, filename, blob):
        blob = self.__blockblob_service.create_blob_from_bytes(container_name,
                                                            filename, 
                                                            blob,
                                                            content_settings=ContentSettings(content_type=self.__get_mime_type(filename)))
    
    def copy_blob(self, container_name, blob_name, blob_url):
        self.__blockblob_service.copy_blob(container_name, blob_name, blob_url)

    def make_blob_url(self, container_name, blob_name, sas_token=''):
        return self.__blockblob_service.make_blob_url(container_name, blob_name, sas_token=sas_token)

    def generate_blob_shared_access_signature(self, container_name, blob_name):
        permission = ContainerPermissions(read=True, write=True)
        return self.__blockblob_service.generate_blob_shared_access_signature(container_name, blob_name,
                                                                            permission, protocol='https',
                                                                            start=datetime.datetime.utcnow(), expiry=datetime.datetime.utcnow() + timedelta(days=1))
    def set_blob_metadata(self, container_name, blob_name, metadata):
        return self.__blockblob_service.set_blob_metadata(container_name, blob_name, metadata)

    def __get_mime_type(self, file_path):
        return mime_content_type(file_path)

    def __get_blobs_in_path(self, container_name, blob_path):
        blobs = self.list_blobs(container_name)
        if not blob_path:
            return blobs
        return list(filter(lambda blob: blob.name.startswith(blob_path), blobs))

    def __create_local_dir(self, local_path):
        if local_path:
            os.makedirs(local_path, exist_ok=True)
        return os.path.join(os.getcwd(), local_path)

    def __get_directories(self, local_path):
        return [file for file in os.listdir(local_path) if os.path.isdir(
            os.path.join(local_path, file))]

    def __get_files(self, local_path):
        return [file for file in os.listdir(local_path) if os.path.isfile(
            os.path.join(local_path, file))]
