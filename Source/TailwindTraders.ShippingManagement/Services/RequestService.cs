using System;
using System.IO;

using MimeTypes;
using Newtonsoft.Json;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services.Contracts;

namespace TailwindTraders.ShippingManagement.Services
{
    public class RequestService : IRequestService
    {
        private const string C_BASE64 = ";base64,";

        public Request Request { get; private set; }

        public RequestService() { }

        public string CreateTempFile(Stream streamBody)
        {
            using (StreamReader reader = new StreamReader(streamBody))
            {
                Request = JsonConvert.DeserializeObject<Request>(reader.ReadToEndAsync().Result);                
                if (!string.IsNullOrEmpty(Request.Source))
                {
                    string parseFile = ParseFile(Request.Source);
                    string fileExtension = GetExtensionFromMimeType(Request.Source);

                    byte[] bytes = Convert.FromBase64String(parseFile);
                    var filePath = $"{Path.GetTempPath()}{DateTime.Now.ToString("yyyyMMddmmssfff")}{fileExtension}";

                    DeleteTempFile(filePath); //If exists file, it must be deleted

                    File.WriteAllBytes(filePath, bytes);

                    return filePath;
                }
                
                return null;
            }
        }
        public string GetMimeTypeFromFilePath(string filePath)
        {
            return MimeTypeMap.GetMimeType(Path.GetExtension(filePath));
        }

        public void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        
        private string ParseFile(string originalFile)
        {
            int position = originalFile.IndexOf(C_BASE64) + C_BASE64.Length;

            if (position > C_BASE64.Length)
            {
                return originalFile.Substring(position);
            }
            else
            {
                return originalFile;
            }
        }

        private string GetExtensionFromMimeType(string dataFile)
        {
            int intPosition = dataFile.IndexOf(":");
            int endPosition = dataFile.IndexOf(C_BASE64);

            if (endPosition > 0 && intPosition > 0)
            {
                string contentType = dataFile.Substring(intPosition + 1, (endPosition - intPosition) - 1);
                return MimeTypeMap.GetExtension(contentType);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
