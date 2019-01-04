using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using Contoso.CognitivePipeline.SharedModels.Models;
using System.IO;
using System;
using System.Threading;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Contoso.CognitivePipeline.BackgroundServices.Functions
{
    public static class ClassificationOrchestrator
    {
        static double resultOperationSleep = 500;
        static HttpClient httpClient = new HttpClient();


        /// <summary>
        ///     Queue based trigger to start executing the Cognitive Services processing pipeline based on the requested "InstructionFlag"
        /// </summary>
        /// <param name="newReq">Details about the SmartDoc request</param>
        /// <param name="starter">Orchestration client</param>
        /// <param name="log">Default logger</param>
        /// <returns></returns>
        [FunctionName("ClassificationOrchestrator_QueueStart")]
        public static async Task QueueStart(
            //Triggers
            [QueueTrigger("newreq", Connection = "NewRequestQueue")]NewRequest<SmartDoc> newReq,

            //Durable Function Orchestration Client
            [OrchestrationClient]DurableOrchestrationClientBase starter,

            //Logger
            ILogger log)
        {
            // Function input comes from the request content.
            var newReqJson = JsonConvert.SerializeObject(newReq);
            string instanceId = await starter.StartNewAsync("ClassificationOrchestrator", newReqJson);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'. Document: {newReq}");

            //return newReq;
        }

        /// <summary>
        ///     Durable Orchestrator
        /// </summary>
        /// <param name="context">Orchestration Context</param>
        /// <returns></returns>
        [FunctionName("ClassificationOrchestrator")]
        public static async Task<NewRequest<SmartDoc>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            //Initialize function params and services (if needed)
            var sleepValueRetrieved = double.TryParse(GlobalSettings.GetEnvironmentVariable("ResultOperationSleep"), out resultOperationSleep);
            if (!sleepValueRetrieved) resultOperationSleep = 1000;
            DateTime expiryTime = GetExpiryTime();
            var newAsyncReqJson = context.GetInput<string>();
            var output = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(newAsyncReqJson);
            
            var operationsFlags = output.Instructions;

            if (operationsFlags.Contains(InstructionFlag.Thumbnail.ToString()))
            {
                output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_GenerateThumbnail", output);
            }

            if (operationsFlags.Contains(InstructionFlag.AnalyzeImage.ToString()))
            {
                output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_AnalyzeImage", output);
            }

            if(operationsFlags.Contains(InstructionFlag.AnalyzeText.ToString()))
            {
                output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_RecognizeText", output);
            }

            //if (operationsFlags.Contains(InstructionFlag.AnalyzeTextv2.ToString()))
            //{
            //    //This is to get the result of a Recognize Text operation. 
            //    //When you use the Recognize Text API, the response contains a field called “Operation-Location”. 
            //    //The “Operation-Location” field contains the URL that should be used to get Recognize Text Operation Result from OCR result API.
            //    //Monitor design pattern was used to wait for this API to return the result as it is unpredictable when the OCR operation will be finalized

            //    //This will submit the image and get the Operation-Location result url. This will be stored in the request steps
            //    output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_RecognizeTextv2", output);

            //    //Monitor patter is sleeping for specific interval before checking again and will fail after passing the expiry time
            //    while (context.CurrentUtcDateTime < expiryTime)
            //    {
            //        output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_RecognizeTextResult", output);

            //        //check if the result is saved to request steps indicating the completion
            //        var recognizeTextStep = output.Steps.Where(s => s.StepName == "RecognizeTextResult" && s.Status == "Success").FirstOrDefault();
            //        if (recognizeTextStep != null)
            //        {
            //            break;
            //        }

            //        // Orchestration will sleep until this time
            //        var nextCheck = context.CurrentUtcDateTime.AddSeconds(resultOperationSleep);
            //        await context.CreateTimer(nextCheck, CancellationToken.None);
            //    }
            //}

            //if (operationsFlags.Contains(InstructionFlag.TypeVerification.ToString()))
            //{
            //    // TODO: Implemenation Custom Vision option to verify type
            //    output = await context.CallActivityAsync<NewRequest<SmartDoc>>("ClassificationOrchestrator_TypeVerification", output);
            //}

            if (operationsFlags.Contains(InstructionFlag.FaceAuthentication.ToString()))
            {
                // TODO: Implemenation of Face Verify option to verify Face authentication validity
            }

            return output;
        }

        private static DateTime GetExpiryTime()
        {
            //Any monitor pattern operation will expier of the desired outcome is not obtained
            return DateTime.UtcNow.AddMinutes(1);
        }

        [FunctionName("ClassificationOrchestrator_GenerateThumbnail")]
        public static NewRequest<SmartDoc> GenerateThumbnail([ActivityTrigger] DurableActivityContext docContext,

            //Inputs
            // TODO: Update parameters to use Azure Function settings
            [CosmosDB("ContosoShopManagerDb", "smartdocs", ConnectionStringSetting = "CosmosDb")] DocumentClient docClient,
            //[Blob("smartdocs/{blobName}", FileAccess.Read)] byte[] imageLarge,

            //Outputs
            //[Blob("smartdocs-tile/{blobName}", FileAccess.Write, Connection = "SmartDocsStorageConnection")] Stream imageTileSize,
            //[Blob("smartdocs-icon/{blobName}", FileAccess.Write, Connection = "SmartDocsStorageConnection")] Stream imageIconSize,

            ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New Thumbnail Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_AnalyzeImage")]
        public static NewRequest<SmartDoc> AnalyzeImage([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New AnalyzeImage Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_RecognizeText")]
        public static NewRequest<SmartDoc> RecognizeText([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New RecognizeText Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_RecognizeTextv2")]
        public static NewRequest<SmartDoc> RecognizeTextv2([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New RecognizeTextv2 Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_RecognizeTextResult")]
        public static NewRequest<SmartDoc> RecognizeTextResult([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New RecognizeTextResult Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_TypeVerification")]
        public static NewRequest<SmartDoc> TypeVerification([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New TypeVerification Generation for: {newRequest}.");

            return newRequest;
        }

        [FunctionName("ClassificationOrchestrator_FaceAuthentication")]
        public static NewRequest<SmartDoc> FaceAuthentication([ActivityTrigger] DurableActivityContext docContext, ILogger log)
        {
            var newRequest = docContext.GetInput<NewRequest<SmartDoc>>();

            log.LogInformation($"New FaceAuthentication Generation for: {newRequest}.");

            return newRequest;
        }
    }
}