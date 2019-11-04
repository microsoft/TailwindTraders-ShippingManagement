using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.FormRecognizer;
using Microsoft.Azure.CognitiveServices.FormRecognizer.Models;
using Microsoft.Extensions.Options;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services.Contracts;

namespace TailwindTraders.ShippingManagement.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly Settings _settings;
        private readonly IFormRecognizerClient _formClient;
        private const int C_MinNumTrainning = 3;

        public AnalysisService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
            _formClient = CreateFormRecognizerClient();
        }

        private IFormRecognizerClient CreateFormRecognizerClient()
        {
            IFormRecognizerClient formClient = new FormRecognizerClient(
                new ApiKeyServiceClientCredentials(_settings.FormRecognizedSubscriptionKey))
            {
                Endpoint = _settings.FormRecognizedEndPoint
            };

            return formClient;
        }

        private async Task<Guid> TrainModelAsync()
        {
            if (Uri.IsWellFormedUriString(_settings.FormRecognizedTrainningDataUrl, UriKind.Absolute))
            {
                try
                {
                    TrainResult result = await _formClient.TrainCustomModelAsync(new TrainRequest(_settings.FormRecognizedTrainningDataUrl));
                    ModelResult model = await _formClient.GetCustomModelAsync(result.ModelId);

                    return result.ModelId;
                }
                catch (Exception)
                {
                    return Guid.Empty;
                }
            }
            return Guid.Empty;
        }

        private async Task<Guid> GetLastModelIDAsync()
        {
            try
            {
                ModelsResult models = await _formClient.GetCustomModelsAsync();

                if (models == null || (models != null && models.ModelsProperty.Count < C_MinNumTrainning)) 
                {
                    for (var i = 0; i < C_MinNumTrainning; i++)
                    {
                        await TrainModelAsync();
                    }

                    models = await _formClient.GetCustomModelsAsync();
                }

                return models.ModelsProperty.First().ModelId;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AnalyzeResult> AnalyzeAsync(string fileMimeType, Stream fileStream)
        {
           
            if (fileStream == null || fileStream.Length == 0) 
            {
                throw new ArgumentException("Request can´t no be null.");
            }

            try
            {
                Guid modelId = await GetLastModelIDAsync();
                return (modelId != Guid.Empty) ? await _formClient.AnalyzeWithCustomModelAsync(modelId, fileStream, fileMimeType) : null;
            }
            catch (ErrorResponseException exr)
            {
                throw exr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
