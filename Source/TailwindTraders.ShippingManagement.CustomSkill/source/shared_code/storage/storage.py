import os, logging, shutil, datetime
from ..utils.mime import mime_content_type
from azure.storage.blob import BlobServiceClient, BlobSasPermissions, ContentSettings, generate_blob_sas, ResourceTypes
from singleton_decorator import singleton
from datetime import timedelta

@singleton
class BlobStorageService:
    def __init__(self, connection_string):
        self.__blob_service_client = BlobServiceClient.from_connection_string(
            conn_str=connection_string)

    def create_container(self, container_name):
        self.__blob_service_client.create_container(name=container_name)

    def delete_container(self, container_name):
        self.__blob_service_client.delete_container(container=container_name)

    def upload_file(self, container_name, filename, local_file, delete_local_file=False):
        self.create_container(container_name)
        return self.__upload_file(container_name, filename, local_file, delete_local_file)

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

    def list_blobs(self, container_name, prefix=None):
        container = self.__blob_service_client.get_container_client(container=container_name)
        return container.list_blobs(name_starts_with=prefix)

    def download_blob(self, container_name, blob_name, local_file=None):
        local_file = blob_name if local_file == None else local_file
        self.__create_local_dir(os.path.split(local_file)[0])
        blob_client = self.__blob_service_client.get_blob_client(container=container_name, blob=blob_name)

        with open(local_file, "wb") as my_blob:
            blob_data = blob_client.download_blob()
            blob_data.readinto(my_blob)

    def download_blobs(self, container_name, local_path="", blob_path=""):
        blobs = self.__get_blobs_in_path(container_name, blob_path)
        return blobs

    def download_all_blobs(self, container_name, local_path="", blob_path=""):
        blobs = self.__get_blobs_in_path(container_name, blob_path)
        base = self.__create_local_dir(local_path)

        list(map(lambda blob: self.download_blob(container_name, blob.name,
                                                 os.path.join(base, blob.name)), blobs))

    def delete_blob(self, container_name, blob_name):
        container = self.__blob_service_client.get_container_client(container=container_name)
        container.delete_blob(blob_name)

    def __upload_file(self, container_name, filename, local_file, delete_local_file=False):
        blob_client = self.__blob_service_client.get_blob_client(container=container_name, blob=filename)

        with open(local_file, "rb") as data:
            blob = blob_client.upload_blob(data, content_settings=ContentSettings(content_type=self.__get_mime_type(local_file)))

        if delete_local_file:
            os.remove(local_file)
        return blob

    def copy_blob(self, container_name, blob_name, blob_url):
        blob_client = self.__blob_service_client.get_blob_client(container=container_name, blob=blob_name)
        blob_client.start_copy_from_url(source_url=blob_url)

    def make_blob_url(self, container_name, blob_name):
        blob_client = self.__blob_service_client.get_blob_client(container=container_name, blob=blob_name)
        return blob_client._format_url(self.__blob_service_client.primary_hostname)

    def generate_blob_shared_access_signature(self, account_name, container_name, blob_name, account_key):
        permission = BlobSasPermissions(read=True, write=True)
        sas_token = generate_blob_sas(
            account_name=account_name,
            container_name=container_name,
            blob_name=blob_name,
            account_key=account_key,
            resource_types=ResourceTypes(object=True),
            permission=permission,
            start=datetime.datetime.utcnow(),
            expiry=datetime.datetime.utcnow() + timedelta(days=1)
        )

        return sas_token

    def set_blob_metadata(self, container_name, blob_name, metadata):
        blob_client = self.__blob_service_client.get_blob_client(container=container_name, blob=blob_name)
        return blob_client.set_blob_metadata(metadata)

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
