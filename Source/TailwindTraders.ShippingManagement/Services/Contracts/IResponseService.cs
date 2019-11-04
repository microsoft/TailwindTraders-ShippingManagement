using Microsoft.Azure.CognitiveServices.FormRecognizer.Models;
using TailwindTraders.ShippingManagement.Models;

namespace TailwindTraders.ShippingManagement.Services.Contracts
{
    public interface IResponseService
    {
        PackagingSlip Parse(AnalyzeResult result);

        bool ChecksLocation(string requestLocation);
    }
}
