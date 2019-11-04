using System.IO;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.FormRecognizer.Models;

namespace TailwindTraders.ShippingManagement.Services.Contracts
{
    public interface IAnalysisService
    {
        Task<AnalyzeResult> AnalyzeAsync(string fileContentType, Stream fileStream);
    }
}
