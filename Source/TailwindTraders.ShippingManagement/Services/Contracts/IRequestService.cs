using System.IO;
using TailwindTraders.ShippingManagement.Models;

namespace TailwindTraders.ShippingManagement.Services.Contracts
{
    public interface IRequestService
    {
        string CreateTempFile(Stream streamBody);

        string GetMimeTypeFromFilePath(string filePath);

        void DeleteTempFile(string filePath);

        Request Request { get; }

    }
}
