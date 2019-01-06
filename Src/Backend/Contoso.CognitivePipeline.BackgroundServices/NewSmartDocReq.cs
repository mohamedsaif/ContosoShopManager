using Contoso.CognitivePipeline.BackgroundServices.Data;
using Contoso.CognitivePipeline.BackgroundServices.Services;
using Contoso.CognitivePipeline.SharedModels.Models;
using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Functions
{
    public static class NewSmartDocReq
    {
        private const string CosmosDbNameSetting = "ContosoShopManagerDb";
        private const string CosmosCollectionName = "smartdocs";

        //Another option to implement Cosmos DB client
        //private static string endpointUrl = GlobalSettings.GetKeyValue(""); //ConfigurationManager.AppSettings["cosmosDBAccountEndpoint"];
        //private static string authorizationKey = GlobalSettings.GetKeyValue(""); //ConfigurationManager.AppSettings["cosmosDBAccountKey"];
        //private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);

        //For performance consideration, we will create the cosmos db client repo as static so it can be shared across multiple functions calls
        //Empty constructor to load configurations from the Functions settings
        private static CosmosDBRepository<SmartDoc> smartDocsDbClient = new CosmosDBRepository<SmartDoc>();

        private static CosmosDBRepository<CognitivePipeline.SharedModels.Models.User> usersDbClient = new CosmosDBRepository<CognitivePipeline.SharedModels.Models.User>();
        private static ILogger log;

        [FunctionName("NewSmartDocReq")]
        public static async Task<IActionResult> Run(
            //HTTP Trigger
            //Sample of used typed input parameter
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "NewSmartDocReq/{docId}")]HttpRequestMessage newReq,

            //Input
            string docId,

            //Output
            [Queue("newreq", Connection = "NewRequestQueue")]ICollector<string> outputQueueItem,

            //Logger
            ILogger logger)
        {
            var newSmartDocRequestJson = await newReq.Content.ReadAsStringAsync();
            var newSmartDocRequest = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(newSmartDocRequestJson);
            
            log = logger;

            log.LogInformation($"***NewSmartDocReq function http triggered for: {newSmartDocRequestJson}");

            string result = "";

            try
            {
                newSmartDocRequest.Status = SmartDocStatus.InProcessing.ToString();

                //In case used strongly typed trigger paramater
                //string newSmartDocRequestJson = JsonConvert.SerializeObject(newSmartDocRequest);

                //Check if request is Async (a new queue item will be added) or Sync (direct call to functions)
                //Async execution
                if (newSmartDocRequest.IsAsync)
                {
                    //TODO: Implement Async execution through durable functions and queues
                    //outputQueueItem.Add(JsonConvert.SerializeObject(newSmartDocRequest));
                    //result = newSmartDocRequestJson;
                    //return (ActionResult)new OkObjectResult(result);
                    return (ActionResult)new BadRequestObjectResult("NOT IMPLEMENTED YET :)");
                }
                else //Sync exection
                {
                    //TODO: FUTURE IMPROVEMENT the processing here directly from here throw call to HTTP Orchestrator Function
                    //Assess what type of processing instruction needed and execute the relevant functions
                    return await CognitivePipelineSyncProcessing(newSmartDocRequest);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"***EXCEPTION*** in NewSmartDocReq: {ex.Message}, {ex.StackTrace}");
                return new BadRequestObjectResult($"{ex.Message}");
            }
        }

        public static async Task<IActionResult> CognitivePipelineSyncProcessing(NewRequest<SmartDoc> newSmartDocRequest)
        {
            log.LogInformation($"***Starting Sync CognitivePipelineSyncProcessing***");
            
            IActionResult executionResult = null;

            if (newSmartDocRequest.Instructions.Contains(InstructionFlag.AnalyzeText.ToString()))
            {
                var stepName = InstructionFlag.AnalyzeText.ToString();
                log.LogInformation($"***Starting {stepName}***");
                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + "/NewCognitiveOCR";
                var content = new StringContent(JsonConvert.SerializeObject(newSmartDocRequest), Encoding.UTF8, "application/json");
                try
                {
                    executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                    if (executionResult is OkObjectResult)
                    {
                        //TODO: Update the request processing step
                        var result = executionResult as OkObjectResult;
                        string updatedDocJson = result.Value.ToString();
                        NewRequest<SmartDoc> updatedDoc = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(updatedDocJson);
                        newSmartDocRequest.Steps.Add(updatedDoc.RequestItem.CognitivePipelineActions[0]);
                    }
                    else
                    {
                        //TODO: Better error information to be implemented
                        newSmartDocRequest.Steps.Add(new ProcessingStep
                        {
                            LastUpdatedAt = DateTime.UtcNow,
                            Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                            StepName = InstructionFlag.AnalyzeText.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"***EXCEPTION*** in {stepName}: {ex.Message}, {ex.StackTrace}");
                    newSmartDocRequest.Steps.Add(new ProcessingStep
                    {
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                        StepName = InstructionFlag.AnalyzeText.ToString()
                    });
                }
            }

            if (newSmartDocRequest.Instructions.Contains(InstructionFlag.FaceAuthentication.ToString()))
            {
                var stepName = InstructionFlag.FaceAuthentication.ToString();
                log.LogInformation($"***Starting {stepName}***");
                CognitivePipeline.SharedModels.Models.User owner = await usersDbClient.GetItemAsync(newSmartDocRequest.OwnerId);
                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + $"/NewCognitiveFaceAuth/{owner.FacePersonId}";
                var content = new StringContent(JsonConvert.SerializeObject(newSmartDocRequest), Encoding.UTF8, "application/json");
                try
                {
                    executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                    if (executionResult is OkObjectResult)
                    {
                        //TODO: Update the request processing step
                        var result = executionResult as OkObjectResult;
                        string updatedDocJson = result.Value.ToString();
                        NewRequest<SmartDoc> updatedDoc = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(updatedDocJson);
                        newSmartDocRequest.Steps.Add(updatedDoc.RequestItem.CognitivePipelineActions[0]);
                    }
                    else
                    {
                        //TODO: Better error information to be implemented
                        newSmartDocRequest.Steps.Add(new ProcessingStep
                        {
                            LastUpdatedAt = DateTime.UtcNow,
                            Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                            StepName = stepName
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"***EXCEPTION*** in {stepName}: {ex.Message}, {ex.StackTrace}");
                    newSmartDocRequest.Steps.Add(new ProcessingStep
                    {
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                        StepName = stepName
                    });
                }
            }

            if (newSmartDocRequest.Instructions.Contains(InstructionFlag.ShelfCompliance.ToString()))
            {
                var stepName = InstructionFlag.ShelfCompliance.ToString();
                log.LogInformation($"***Starting {stepName}***");
                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + "/NewCognitiveShelfCompliance";
                var content = new StringContent(JsonConvert.SerializeObject(newSmartDocRequest), Encoding.UTF8, "application/json");
                try
                {
                executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                    if (executionResult is OkObjectResult)
                    {
                        //TODO: Update the request processing step
                        var result = executionResult as OkObjectResult;
                        string updatedDocJson = result.Value.ToString();
                        NewRequest<SmartDoc> updatedDoc = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(updatedDocJson);
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(updatedDoc.RequestItem.CognitivePipelineActions[0]);
                    }
                    else
                    {
                        //TODO: Better error information to be implemented
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                        {
                            LastUpdatedAt = DateTime.UtcNow,
                            Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                            StepName = InstructionFlag.AnalyzeText.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"***EXCEPTION*** in {stepName}: {ex.Message}, {ex.StackTrace}");
                    newSmartDocRequest.Steps.Add(new ProcessingStep
                    {
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                        StepName = stepName
                    });
                }
            }

            if (newSmartDocRequest.Instructions.Contains(InstructionFlag.Thumbnail.ToString()))
            {
                var stepName = InstructionFlag.Thumbnail.ToString();
                log.LogInformation($"***Starting {stepName}***");

                //Currently the 2 Thumbnail sizes are loaded from Functions settings.
                var thumbnailConfig = GlobalSettings.GetKeyValue("CognitiveServices-Thumbnail-Config").Split(',');

                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + $"/NewCognitiveThumbnail/{thumbnailConfig[0]}/{thumbnailConfig[1]}/{thumbnailConfig[2]}/{thumbnailConfig[3]}";
                var content = new StringContent(JsonConvert.SerializeObject(newSmartDocRequest), Encoding.UTF8, "application/json");
                try
                {
                    executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                    if (executionResult is OkObjectResult)
                    {
                        //TODO: Update the request processing step
                        var result = executionResult as OkObjectResult;
                        string updatedDocJson = result.Value.ToString();
                        NewRequest<SmartDoc> updatedDoc = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(updatedDocJson);
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(updatedDoc.RequestItem.CognitivePipelineActions[0]);
                    }
                    else
                    {
                        //TODO: Better error information to be implemented
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                        {
                            LastUpdatedAt = DateTime.UtcNow,
                            Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                            StepName = InstructionFlag.AnalyzeText.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"***EXCEPTION*** in {stepName}: {ex.Message}, {ex.StackTrace}");
                    newSmartDocRequest.Steps.Add(new ProcessingStep
                    {
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                        StepName = stepName
                    });
                }
            }

            if (newSmartDocRequest.Instructions.Contains(InstructionFlag.AnalyzeImage.ToString()))
            {
                var stepName = InstructionFlag.AnalyzeImage.ToString();
                log.LogInformation($"***Starting {stepName}***");
                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + $"/NewCognitiveAnalyzeImage";
                var content = new StringContent(JsonConvert.SerializeObject(newSmartDocRequest), Encoding.UTF8, "application/json");
                try
                {
                    executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                    if (executionResult is OkObjectResult)
                    {
                        //TODO: Update the request processing step
                        var result = executionResult as OkObjectResult;
                        string updatedDocJson = result.Value.ToString();
                        NewRequest<SmartDoc> updatedDoc = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(updatedDocJson);
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(updatedDoc.RequestItem.CognitivePipelineActions[0]);
                    }
                    else
                    {
                        //TODO: Better error information to be implemented
                        newSmartDocRequest.RequestItem.CognitivePipelineActions.Add(new ProcessingStep
                        {
                            LastUpdatedAt = DateTime.UtcNow,
                            Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                            StepName = InstructionFlag.AnalyzeImage.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"***EXCEPTION*** in {stepName}: {ex.Message}, {ex.StackTrace}");
                    newSmartDocRequest.Steps.Add(new ProcessingStep
                    {
                        LastUpdatedAt = DateTime.UtcNow,
                        Status = SmartDocStatus.ProcessedUnsuccessfully.ToString(),
                        StepName = stepName
                    });
                }
            }

            //Validate cognitive processing and return relevant details
            log.LogInformation($"***Final Results Processing***");
            var processedResult = await CognitivePipelineResultProcessor.ProcessFinalResult(newSmartDocRequest, smartDocsDbClient, usersDbClient);

            if (!string.IsNullOrEmpty(processedResult))
                return (ActionResult)new OkObjectResult(processedResult);
            else
                return (ActionResult)new BadRequestObjectResult(processedResult);
        }
    }
}