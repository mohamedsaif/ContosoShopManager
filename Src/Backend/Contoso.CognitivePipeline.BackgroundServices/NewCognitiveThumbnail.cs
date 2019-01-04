
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
using Contoso.CognitivePipeline.SharedModels.Cognitive;

namespace Contoso.CognitivePipeline.BackgroundServices.Functions
{
    public static class NewCognitiveThumbnail
    {
        private static HttpClient httpClient;

        [FunctionName("NewCognitiveThumbnail")]
        public static async Task<IActionResult> Run(
            //HTTP Trigger (Functions allow only a single trigger)
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "NewCognitiveThumbnail/{tileWidth}/{tileHeight}/{iconWidth}/{iconHeight}")]NewRequest<SmartDoc> newRequest,

            // Inputs
            [Blob("smartdocs/{RequestItem.DocName}", FileAccess.Read, Connection = "SmartDocsStorageConnection")] byte[] smartDocImage,
            int tileWidth,
            int tileHeight,
            int iconWidth,
            int iconHeight,

            // Outputs
            [Blob("smartdocs-tile/{RequestItem.DocName}", FileAccess.Write)] Stream tileImage,
            [Blob("smartdocs-icon/{RequestItem.DocName}", FileAccess.Write)] Stream iconImage,

            // Logger
            ILogger log)
        {
            log.LogInformation($"New Direct-HTTP Thumbnail Request triggered: {newRequest}");

            string stepName = InstructionFlag.Thumbnail.ToString();

            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            try
            {
                var tileResult = await ComputerVisionService.GetThumbnailAsync(httpClient, smartDocImage, tileImage, tileWidth, tileHeight);
                var iconResult = await ComputerVisionService.GetThumbnailAsync(httpClient, smartDocImage, iconImage, iconWidth, iconHeight);
                
                //Update the request information with the newly processed data
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = stepName,
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = JsonConvert.SerializeObject(new Thumbnail[] { tileResult, iconResult }),
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString()
                });
                
                return (ActionResult)new OkObjectResult(newRequest);
            }
            catch(Exception ex)
            {
                newRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                {
                    StepName = stepName,
                    LastUpdatedAt = DateTime.UtcNow,
                    Output = ex.Message,
                    Status = SmartDocStatus.ProccessedSuccessfully.ToString()
                });
                return (ActionResult)new BadRequestObjectResult(newRequest);
            }
        }
    }
}
