using System.IO;
using System.Threading.Tasks;

using Azure.AI.FormRecognizer.Models;

namespace TailwindTraders.ShippingManagement.Services.Contracts
{
    public interface IAnalysisService
    {
        Task<RecognizedFormCollection> AnalyzeAsync(string fileContentType, Stream fileStream);
    }
}
