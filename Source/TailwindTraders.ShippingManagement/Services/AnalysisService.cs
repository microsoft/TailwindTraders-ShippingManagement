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
        private readonly FormTrainingClient _formTrainingClient;
        private const int C_MinNumTrainning = 3;

        public AnalysisService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
            _formTrainingClient = CreateFormTrainingClient();
        }

        private FormTrainingClient CreateFormTrainingClient()
        {
            FormTrainingClient formTrainingClient = new FormTrainingClient(new Uri(_settings.FormRecognizedEndPoint), new AzureKeyCredential(_settings.FormRecognizedSubscriptionKey));
            return formTrainingClient;
        }
        private async Task<string> TrainModelAsync()
        {
            if (Uri.IsWellFormedUriString(_settings.FormRecognizedTrainningDataUrl, UriKind.Absolute))
            {
                try
                {
                    CustomFormModel result = await _formTrainingClient.StartTrainingAsync(new Uri(_settings.FormRecognizedTrainningDataUrl), useTrainingLabels: false).WaitForCompletionAsync();

                    await _formTrainingClient.GetCustomModelAsync(result.ModelId);

                    return result.ModelId;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public async Task<RecognizedFormCollection> AnalyzeAsync(Stream fileStream)
        {

            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException(nameof(fileStream) + " can't be null or empty");
            }

            try
            {
                string modelId = await TrainModelAsync();
                return (modelId != null) ? await _formTrainingClient.GetFormRecognizerClient().StartRecognizeCustomFormsAsync(modelId, fileStream).WaitForCompletionAsync() : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
