using Azure.AI.FormRecognizer.Models;
using TailwindTraders.ShippingManagement.Models;

namespace TailwindTraders.ShippingManagement.Services.Contracts
{
    public interface IResponseService
    {
        PackagingSlip Parse(RecognizedFormCollection result);

        bool ChecksLocation(string requestLocation);
    }
}
