![Banner](Assets/Banner.png)

# Architecture Choices

There are 2 major decisions needed to successfully deliver Contoso Shop Manager platform, AI platform and the backend architecture to support scalable and cost efficient services.

## AI Platform

### Required Services

Contoso Shop Manager application scope require the following AI powered services:

1. **OCR** for reading information out of the Employee Id cards scanned by Contoso Shop Manager app.
2. **Face Authentication** for verifying that captured face is identical to a pre-trained faces store and belong to the user being authenticated.
3. **Image Classification** to predict of a particular shelves image is compliant/non-compliant with the pre-trained policy (Soda, Water and Juice ordered stocking).

Microsoft AI democratization initiatives are enabling developers to take advantage of pre-trained models to bring intelligent to their applications without the need to have an advance degree in machine learning or AI.

For the image classification requirement, this workshop will focus on using [Custom Vision](https://azure.microsoft.com/en-us/services/cognitive-services/custom-vision-service/) as it provides sufficient accuracy for the required scope. This is not to underestimate other approaches to the problem, but to make it simpler and efficient.

>***NOTE:*** You can fund further information about [Advanced Computer Vision - Bonus Track](../Bonus/Advanced-ComputerVision/) where couple more Machine Learning approaches where discussed. Just make sure to bring your Python skills with [Jupyter Notebook](http://jupyter.org/) along the ride :)

As for other needed AI services like OCR and face authentication, we will be leveraging Azure Cognitive Services for Face and OCR to added the required functionalities.

>***NOTE:*** We will cover all Cognitive Services provisioning later in this workshop guides.

### Cognitive Services: SDK vs APIs

It was tough choice to select Cognitive Services SDKs vs. APIs to write the access code to Azure Cognitive Services.

As the matter of fact, all Cognitive Services offer APIs that support any platform that can access REST services but not all offers SDKs for every platform.

As a rule of thumb, the choice to go with SDK, when possible, is recommended as it produces cleaner code and in some cases enhanced performance (like in Speech SDKs).

For this workshop purposes, API access was implemented to leverage the different cognitive services.

> ***NOTE:*** All Cognitive Services have API-first architecture. So if you didn't find an SDK that is optimized for the platform you target, you can always fall back to using the service APIs.

[Microsoft Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/) are continuously improved by adding new services or enhance the existing ones.

## Backend Cognitive Pipeline

### Serverless Cognitive Pipeline

As for the Azure architecture for Contoso Shop Manager backend, a ***Serverless-based Cognitive Pipeline*** was selected to bring modular and flexible backend options to the table. The solution was designed to allow future addition of new AI-skills to the standardized cognitive pipeline.

That is why having a ***Cognitive Pipeline*** will allow adding new AI services to be part of the processing pipeline easily and without any significant changes to the client code (or the server).

### API BFF

Also the backend architecture is leveraging a API app to consolidate both the client access to the cognitive pipeline and apply common business logic.

This API layer can be considered as part of [Backend for Frontend](https://samnewman.io/patterns/architectural/bff/) design pattern to optimize the client access to the general purpose cognitive pipeline.

## Serverless

Event driven pipeline processing was the option selected to implement the primary component of the backend.

[Azure Functions](https://azure.microsoft.com/en-us/services/functions/) offers great capability to execute complex server less scenario by combining triggers, input/output binding and [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview).

I always like to think of Azure Functions like a Windows Service, it is available on the background listening to potential events to respond.

# Architecture Overview

Below is how the proposed system architecture that is leveraging several Azure services to deliver reliable and scalable backend.

![ArchitectureOverview](Assets/architecture-guided.png)

Mixing multiple implementations for Azure Functions was used to optimum execution orchestration.

1. Regular Azure Functions will be used to execute the specific cognitive tasks and in central processing new requests.
2. Durable Azure Functions will be used for orchestrating the asynchronous execution of the different individual cognitive functions.

## Request Workflow

### 1. A client (web or mobile) will submit new request to **Contoso.CognitivePipeline.API**

Contoso.CognitivePipeline.API contains 4 APIs (aka Controllers).

#### API Management

All APIs are exposed via [API Management Service](https://azure.microsoft.com/en-us/services/api-management/) which act as our turnkey API gateway solution.

All APIs will be controlled through enforced policy and subscription access.

>***NOTE:*** All APIs should not exposed directly to consumers. You should always plan to place and API Gateway in the middle.

API Management service is very easy to setup as you will see in later guides of this workshop.

API Management also offers other great features like:

- Manage all APIs in one place: with self-service developer portal and auto generated API catalog, documentation and code samples.
- APIs insights: from performance telemetry to real-time analytics with provide better understanding of how the APIs are being used.
- Add versioning and provision control to your APIs without changing the backend code.

#### (Deprecated) Classification API

Initial idea around having a single API that can process different types of documents (ID, Face, Shelves,...).

This turned out not to be a good practice as I wanted to have different return type for each document.

Classification/SubmitDoc API takes the following parameters:

1. OwnerId: currently being fixed to a static values but in the future it will be based on the authenticated user
2. Document Type: which is basically what type of image are being submitted with the following initial values:
    - Passport, DriverLicense, ID, BirthCertificate, Receipt, Check, Generic, Face, VoiceID, Unidentified
3. Document Image: the actual image that will be processed by the backend
4. IsAsync bool flag to tell the back end how this request should be processed

#### Refactor the Classification API into 3 APIs

As the implementation move forward, I decided to abandon the single controller approach and replace it with 3 controllers for each document type currently supported (ID, Face and Shelves Compliance)

#### IDAuth API

Like the Classification API, it takes the same parameters but without the document type (as it only process Employee Ids).

It returns a strongly typed and fully processed EmployeeId json:

>SAMPLE HERE

#### FaceAuth API

Like the Classification API, it takes the same parameters but without the document type (as it only process employee face authentications).

It returns a strongly typed and fully processed FaceCard json:

>SAMPLE HERE

#### ShelvesCompliance API

Like the Classification API, it takes the same parameters but without the document type (as it only process shelves images).

It returns a strongly typed and fully processed ShelfCompliance json:

>SAMPLE HERE

### 2. New Document Processing Request

Each API from above have a POST method that preform the following actions:

1. Validate the request:
   - **Document File:** File were uploaded
   - **Owner Exist:**ownerId passed exists in CosmosDB ***users*** collection
2. **Upload to Storage:** upload the supplied document to storage
3. CosmosDB update:
   - **SmartDoc:** create new SmartDoc object and assign the properties.
   - **Save SmartDoc to CosmosDB:** save the new document details to CosmosDB ***smartdoc*** collection
4. Invoke the Cognitive Pipeline processing:
   - **NewReq:** create a [NewRequest<SmartDoc>](../../Src/Backend/Contoso.CognitivePipeline.SharedModels/Models/NewRequest.cs) object that will be passed for cognitive pipeline for processing including a reference to the [SmartDoc](../../Src/Backend/Contoso.CognitivePipeline.SharedModels/Models/SmartDoc.cs)
   - **Generate list of cognitive actions:** based on the document type, a list of cognitive actions will be added to [NewRequest.Instructions](../../Src/Backend/Contoso.CognitivePipeline.SharedModels/Models/NewRequest.cs) by calling a helper method [DocumentInstructionsProcessor.GetInstructions(docType)](../../Src/Backend/Contoso.CognitivePipeline.API/BusinessLogic/DocumentInstructionsProcessor.cs)
   - **Call NewReq Function:** kick of the cognitive processing by invoking HTTP NewReq Function (which is the entry point for the serverless cognitive pipeline).

### 3. Cognitive Pipeline - **NewSmartDocReq** Function

1. Once the HTTP request was received from the API to [NewSmartDocReq](../../Src/Backend/Contoso.CognitivePipeline.BackgroundServices/NewSmartDocReq.cs) function will collaborate to finalize the required instructions.
2. Based on IsAsync flag, this function will do one of the of the following:
    1. **Async:** (NOT IMPLEMENTED FULLY YET) will create a new queue item to be picked up by Cognitive Pipeline Durable Function. The request object will be returned immediately to the caller while the function process this document.
    2. **Sync:** will post an HTTP request(s) directly to the relevant function(s) and wait for the result to come back. Final cognitive processing result will be updated in CosmosDB and returned back to the caller.

### 4. Backend Background Services - Async/Sync

Cognitive Pipeline orchestration will be either triggered by a queue message or http request (based on the original IsAsync flag).

#### Async Cognitive Pipeline

In case of ***Async***, Cognitive Pipeline durable function will be triggered via the new queue message.

In this case, Step 5 in architecture diagram will be starting.

#### Sync Cognitive Pipeline

In case of ***Sync***, the New Request function will make the calls directly and wait for the result to return them back to the API directly without going throw the orchestration.

You can check [CognitivePipelineSyncProcessing(NewRequest<SmartDoc> newSmartDocRequest)](../../Src/Backend/Contoso.CognitivePipeline.BackgroundServices/NewSmartDocReq.cs) implementation.

When execution finishes, this will return results immediately to the NewSmartDoc function so it will in turn return the it to the calling API.

Execution finishes here.

### 5. Backend Background Services - Cognitive Pipeline Orchestrations Durable Function

>***NOTE:*** This feature still under development

>***NOTE:*** In case of the **Async** scenario, this durable function orchestrator will be triggered by the new message in the queue.

1. Fan-out/fan-in pattern will be selected as these instructions can be executed in parallel.

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

>***NOTE:*** This feature still under development

>***NOTE:*** In case of the **Async** scenario, this durable function orchestrator will be triggered by the new message in the queue.

1. A final ResultCapture function will be called to update the CosmosDB with all the cognitive results.
2. If required, a Logic App based callback workflow can be triggered by adding a new queue message in ***callback*** queue to send push notification, email or even SMS to the concerned party about the completion of the processing

# Next Steps

[Authentication - Digital ID OCR](../03-CognitiveServices-OCR)