
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.CognitivePipeline.BackgroundServices.Services;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Microsoft.Extensions.Logging;

namespace Contoso.CognitivePipeline.BackgroundServices.Functions
{
    public static class NewCognitiveAnalyzeImage
    {
        private static HttpClient httpClient;

        [FunctionName("NewCognitiveAnalyzeImage")]
        public static async Task<IActionResult> Run(
            //HTTP Trigger (Functions allow only a single trigger)
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]NewRequest<SmartDoc> newRequest,

            // Inputs
            [Blob("smartdocs/{RequestItem.DocName}", FileAccess.Read, Connection = "SmartDocsStorageConnection")] byte[] smartDocImage,
            
            // Logger
            ILogger log)
        {
            string stepName = InstructionFlag.AnalyzeImage.ToString();

            log.LogInformation($"***New {stepName} Direct-HTTP Request triggered: {{InstructionFlag.FaceAuthentication.ToString()}}");

            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            try
            {
                var result = await ComputerVisionService.AnalyzeImage(httpClient, smartDocImage);
                var resultJson = JsonConvert.SerializeObject(result);

                //Update the request information with the newly processed data
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = stepName,
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = resultJson,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString()
                });
                
                return (ActionResult)new OkObjectResult(newRequest);
            }
            catch(Exception ex)
            {
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = InstructionFlag.AnalyzeText.ToString(),
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = ex.Message,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString()
                });
                return (ActionResult)new BadRequestObjectResult(newRequest);
            }
        }
    }
}
