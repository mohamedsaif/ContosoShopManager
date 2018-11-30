![Banner](Assets/Banner.png)

# Architecture Options

There are 2 major decisions needed to successfully deliver Contoso Shop Manager platform, Shelves compliance classification algorithm/platform and the Azure backend architecture to support scalable and cost efficient services.

This workshop will focus on [Custom Vision](https://azure.microsoft.com/en-us/services/cognitive-services/custom-vision-service/) as it proved to provide sufficient accuracy for the required scope. This is not to under estimate other approaches to the problem, but to make it simpler and efficient.

You can fund further information about [Advanced Computer Vision - Bonus Track](../Bonus/Advanced-ComputerVision/) where couple more Machine Learning approaches where discussed. Just make sure to bring your Python skills with [Jupyter Notebook](http://jupyter.org/) along the ride :)

As for the Azure architecture for Contoso Shop Manager platform, a ***Serverless-based Cognitive Pipeline*** is brining modular and flexible backend options to the table. The solution was designed to allow future addition of new AI-skills to the pipeline.

## Cognitive Pipeline

AI democratization initiative is enabling developers to take advantage of pre-trained models to bring intelligent to their applications.

[Microsoft Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/) are continuously improved by adding new services or enhance the existing ones.

That is why having a ***Cognitive Pipeline*** will allow adding new AI services to be part of the processing pipeline easily and without any significant changes to the client code (or the server).

## Cognitive Services SDK vs APIs

It was tough choice to select Cognitive Services SDKs vs. APIs. As the matter of fact, all Cognitive Services offer APIs but not all offers solid SDKs. 

The choice to go with SDK ,when possible, made sense as it produces cleaner code and in some cases enhanced performance (like in Speech SDKs).

> ***NOTE:*** All Cognitive Services have API-first architecture. So if you didn't find an SDK that is optimized for the platform you target, you can always fall back to using the service APIs.

## Serverless

Event driven pipeline processing was the option selected to implement the primary component of the backend.

[Azure Functions](https://azure.microsoft.com/en-us/services/functions/) offers great capability to execute complex server less scenario by combining triggers, input/output binding and [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview).

I always like to think of Azure Functions like a Windows Service, it is available on the background listening to potential events to respond.

## Architecture Overview

Below is how the system currently implemented by leveraging several Azure services to deliver reliable and scalable backend architecture.

![ArchitectureOverview](Assets/architecture-guided.png)

Mixing multiple implementations for Azure Functions was used to optimum execution orchestration.

1. Normal Azure Functions were used to execute the specific tasks
2. Durable Azure Functions were used for orchestrating the execution of the different individual functions.

## Request Workflow

### 1. A client (web or mobile) will submit 4 thins to a ASP .NET API

1. OwnerId: currently being fixed to a static values but in the future it will be based on the authenticated user
2. Document Type: which is basically what type of image are being submitted with the following initial values:
    1. Passport, DriverLicense, ID, BirthCertificate, Receipt, Check, Generic, Face, VoiceID, Unidentified
3. Document Image: the actual image that will be processed by the backend
4. IsAsync bool flag to tell the back end how this request should be processed

### 2. Backend APIs

1. [Classification/SubmitDoc API]() will receive the 4 information sent by the client and validate the request

```csharp
[HttpPost("{ownerId}/{docType}/{isAsync}")]
        public async Task<IActionResult> SubmitDoc(string ownerId, string docType, bool isAsync, IFormFile doc)
```

2. Based on the document type, a list of instructions for the (Background Services) will be added to the request (like if it is an ID document, 2 instructions will be added to do both Face auth on the ID image and OCR to extract ID information like name, title and employee number)

```csharp
public class DocumentInstructionsProcessor
{
    public static List<string> GetInstructions(ClassificationType docType)
    {
        List<string> instructions = new List<string>();

        switch (docType)
        {
            case ClassificationType.Passport:
            case ClassificationType.DriverLicense:
            case ClassificationType.ID:
            case ClassificationType.BirthCertificate:
            case ClassificationType.Receipt:
                //result = InstructionFlag.AnalyzeImage | InstructionFlag.AnalyzeText;
                instructions.Add(InstructionFlag.AnalyzeText.ToString());
                break;
            case ClassificationType.Check:
                instructions.Add(InstructionFlag.CustomVision.ToString());
                instructions.Add(InstructionFlag.AnalyzeText.ToString());
                break;
            case ClassificationType.Generic:
                instructions.Add(InstructionFlag.AnalyzeImage.ToString());
                break;
            case ClassificationType.Unidentified:
                instructions.Add(InstructionFlag.AnalyzeImage.ToString());
                break;
            default:
                break;
        }

        return instructions;
    }
}
```

```csharp
public enum InstructionFlag
{
    AnalyzeImage,
    AnalyzeText,
    AnalyzeTextv2,
    Thumbnail,
    TypeVerification,
    FaceAuthentication,
    CustomVision
}
```

3. A new document will be created in CosmosDB ***smartdocs*** collection and the file will be saved to the blob storage with naming standard of ***DocType-GUID.ext***.

```csharp
public class SmartDoc : BaseModel
{
    public string OwnerId { get; set; }
    public string DocName { get; set; }
    public string DocUri { get; set; }
    public string TileSizeUri { get; set; }
    public string IconSizeUrl { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ClassificationType DocType { get; set; }
    public string PrimaryClassification { get; set; }
    public double PrimaryClassificationConfidence { get; set; }

    //Bytes will not stored in the db. Instead will remain in the blob storage and accessed via the DocUri
    //public byte[] DocBytes { get; set; }

    public string ClassificationTagsRaw { get; set; }
    public List<SmartDocTag> ClassificationTags { get; set; }
    public string OCRTextTagsRaw { get; set; }
    public List<SmartDocTag> OCRTextTags { get; set; }
    public string ErrorMessage { get; set; }
    public string Status { get; set; }
}
```

[View in project](/Src/Backend/BackgroundSerivces/NewReq.cs#L22)

> **NOTE:** You always want to expose your APIs behind API proxy ([Like API Management Service](https://docs.microsoft.com/en-us/azure/api-management/)) to retain control and protect your actual APIs endpoints.

```csharp
public class NewRequest<T> : BaseModel
{
    public string OwnerId { get; set; }
    public string ItemReferenceId { get; set; }
    public T RequestItem { get; set; }
    public List<string> Instructions { get; set; }
    public string Status { get; set; }
    public bool IsAsync { get; set; }
    public List<ProcessingStep> Steps { get; set; } = new List<ProcessingStep>();
}
```

### 3. Backend Background Services - New Request Function

1. Once the request was received from the API, a New Request function will collaborate to finalize the required instructions.
2. Based on IsAsync flag, this function will do one of the of the following:
    1. Async: will create a new queue item to be picked up by Cognitive Pipeline Durable Function. The request object will be returned immediately to the caller while the function process this document.
    2. Sync: will post an HTTP request(s) directly to the relevant function(s) and wait for the result to come back. Final cognitive processing result will be updated in CosmosDB and returned back to the caller.

```csharp
[FunctionName("NewSmartDocReq")]
public static async Task<IActionResult> Run(
    //HTTP Trigger
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage newReq,

    //Output
    [Queue("newreq", Connection = "NewRequestQueue")]ICollector<string> outputQueueItem, 

    //Logger
    ILogger log)
{
    var newSmartDocRequestJson = await newReq.Content.ReadAsStringAsync();
    var newSmartDocRequest = JsonConvert.DeserializeObject<NewRequest<SmartDoc>>(newSmartDocRequestJson);

    log.LogInformation($"NewReq function http triggered for: {newSmartDocRequestJson}");

    string result = "";
    IActionResult executionResult = null;
    try
    {
        newSmartDocRequest.Status = SmartDocStatus.InProcessing.ToString();

        //Check if request is Async (a new queue item will be added) or Sync (direct call to functions)
        if (newSmartDocRequest.IsAsync)
        {
            outputQueueItem.Add(JsonConvert.SerializeObject(newSmartDocRequest));
            result = newSmartDocRequestJson;
            return (ActionResult)new OkObjectResult(result);
        }
        else //Sync execution
        {
            //Assess what type of processing instruction needed and execute the relevant functions
            if(newSmartDocRequest.Instructions.Contains(InstructionFlag.AnalyzeText.ToString()))
            {
                string funcUri = GlobalSettings.GetKeyValue("FunctionBaseUrl") + "/NewCognitiveOCR";
                var content = new StringContent(newSmartDocRequestJson, Encoding.UTF8, "application/json");
                executionResult = await FunctionExecuter.CallFunction(funcUri, content);
                if(executionResult is OkObjectResult)
                {
                    //TODO: Update the request processing step
                    return executionResult;
                }
                else
                {
                    //return (ActionResult)new BadRequestObjectResult(result);
                }
            }

            return (ActionResult)new BadRequestObjectResult(executionResult);
        }
    }
    catch(Exception ex)
    {
        return new BadRequestObjectResult($"{ex.Message}");
    }
}
```

### 4. Backend Background Services - Async/Sync

1. Cognitive Pipeline orchestration will be either triggered by a queue message or http request (based on the original IsAsync flag)
    1. In case of ***Async***, Cognitive Pipeline durable function will be triggered via the new queue message
    2. In case of ***Sync***, the New Request function will make the calls directly and wait for the result to return them back to the API directly without going throw the orchestration.

### 5. Backend Background Services - Cognitive Pipeline Orchestrations Durable Function

1. Fan-out/fan-in pattern was selected as these instructions can be executed in parallel.

```csharp
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
```

> **NOTE:** If your tasks must be executed in sequence, you can select ***Function Chaining Pattern*** to execute in sequence with the ability to get the output of one function into the next. Refer back to [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview) documentations for further details about orchestration patterns.

### 6. Backend Background Services - Result Update & Callback (Async)

1. A final ResultCapture function will be called to update the CosmosDB with all the cognitive results.
2. If required, a Logic App based callback workflow can be triggered by adding a new queue message in ***callback*** queue to send push notification, email or even SMS to the concerned party about the completion of the processing

# Next Steps

[Authentication - Digital ID OCR](../03-CognitiveServices-OCR)