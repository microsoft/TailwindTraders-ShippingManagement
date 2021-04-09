using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using Microsoft.Extensions.Options;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services.Contracts;

namespace TailwindTraders.ShippingManagement.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly Settings _settings;
        private readonly FormTrainingClient _formTrainedClient;
        private const int C_MinNumTrainning = 3;

        public AnalysisService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
            _formTrainedClient = CreateFormTrainingClient();
        }

        private FormTrainingClient CreateFormTrainingClient()
        {
            FormTrainingClient formTrainedClient = new FormTrainingClient(new Uri(_settings.FormRecognizedEndPoint), new AzureKeyCredential(_settings.FormRecognizedSubscriptionKey));
            return formTrainedClient;
        }
        private async Task<string> TrainModelAsync()
        {
            if (Uri.IsWellFormedUriString(_settings.FormRecognizedTrainningDataUrl, UriKind.Absolute))
            {
                try
                {
                    CustomFormModel result = await _formTrainedClient.StartTrainingAsync(new Uri(_settings.FormRecognizedTrainningDataUrl), useTrainingLabels: false).WaitForCompletionAsync();

                    CustomFormModel model = await _formTrainedClient.GetCustomModelAsync(result.ModelId);

                    return result.ModelId;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        private async Task<string> GetLastModelIDAsync()
        {
            try
            {
                IAsyncEnumerator<CustomFormModelInfo> models =  _formTrainedClient.GetCustomModelsAsync().GetAsyncEnumerator();
                if (models.Current == null || !(await models.MoveNextAsync() && await models.MoveNextAsync())) 
                {
                    for (var i = 0; i < C_MinNumTrainning; i++)
                    {
                        await TrainModelAsync();
                    }

                    models = _formTrainedClient.GetCustomModelsAsync().GetAsyncEnumerator();
                }

                return models.Current.ModelId;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RecognizedFormCollection> AnalyzeAsync(string fileMimeType, Stream fileStream)
        {
           
            if (fileStream == null || fileStream.Length == 0) 
            {
                throw new ArgumentException("Request can´t no be null.");
            }

            try
            {
                string modelId = await GetLastModelIDAsync();
                return (modelId != null) ? await _formTrainedClient.GetFormRecognizerClient().StartRecognizeCustomFormsAsync(modelId, fileStream).WaitForCompletionAsync() : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
