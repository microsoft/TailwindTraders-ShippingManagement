using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services.Contracts;

namespace TailwindTraders.ShippingManagement
{
    public class Scan
    {
        private readonly IRequestService _svcRequest;
        private readonly IAnalysisService _svcAnalisys;
        private readonly IResponseService _svcResponse;

        public Scan(IRequestService svcRequest,
                    IAnalysisService svcAnalysis,
                    IResponseService svcResponse
                    )
        {
            _svcRequest = svcRequest;
            _svcAnalisys = svcAnalysis;
            _svcResponse = svcResponse;
        }

        [FunctionName("Scan")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
             HttpRequest req,
             ILogger logger)
        {
            try
            {
                PackagingSlip model = null;

                var tempFilePath = _svcRequest.CreateTempFile(req.Body);

                using (FileStream fStream = new FileStream(tempFilePath, FileMode.Open))
                {
                    var fileMimeType = _svcRequest.GetMimeTypeFromFilePath(tempFilePath);
                    var analyzeResult = await _svcAnalisys.AnalyzeAsync(fileMimeType, fStream);
                    if (analyzeResult != null)
                    {
                        model = _svcResponse.Parse(analyzeResult);
                        model.LocationMatchs = _svcResponse.ChecksLocation(_svcRequest.Request.CurrentLocation);
                    }
                }

                _svcRequest.DeleteTempFile(tempFilePath);

                return HttpStatusCode.OK == ChecksResult(model, logger, out string jsonResponse)
                      ? (ActionResult)new OkObjectResult(jsonResponse)
                      : new BadRequestObjectResult(jsonResponse);
            }
            catch (Exception ex)
            {
                var error = $"Unexpected exception scanning image. ex:{ex.ToString()}";
                logger.LogError(error);
                var response = new Response<string>()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = error
                };
                
                return new BadRequestObjectResult(JsonConvert.SerializeObject(response));
            }
        }

        private HttpStatusCode ChecksResult(PackagingSlip model, ILogger logger, out string jsonResponse) 
        {
            if (model != null)
            {
               
                if (String.IsNullOrEmpty(model.Provider)) 
                {
                    var error = "Incorrect result. Provider not found.";
                    logger.LogError(error);
                    var responseProv = new Response<string>()
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Data = error
                    };

                    jsonResponse = JsonConvert.SerializeObject(responseProv);

                    return HttpStatusCode.BadRequest;
                }
                
                logger.LogInformation($"Correct result. The image [{model.ID}] was scanned and analyzed successfully");
                var response = new Response<PackagingSlip>()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = model
                };

                jsonResponse = JsonConvert.SerializeObject(response);

                return HttpStatusCode.OK;
            }
            else
            {
                var error = "Incorrect result. Please pass a correct immage to the request.";
                logger.LogError(error);
                var response = new Response<string>()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = error
                };

                jsonResponse = JsonConvert.SerializeObject(response);

                return HttpStatusCode.BadRequest;
            }
        }
    }
}
