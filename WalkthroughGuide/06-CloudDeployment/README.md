![Banner](Assets/Banner.png)

# Contoso Shop Manager Backend

As per the [architecture options discussed](../02-ArchitectureOptions), Contoso Shop Manager system consist of several components as shown in the below reference:

![Azure Architecture](Assets/architecture.png)

In this walkthrough we will start provisioning all services that were not provisioned as part of the AI services that are provisioned in the previous steps.

>***NOTE:*** On the next guide [Backend-Services-DevOps]() will explore how to continuously build (CI) and deploy (CD) the code leveraging Azure DevOps.

To have a quick overview, we will be provisioning the following services:

***Cognitive Services***

1. Computer Vision Service for OCR
2. Computer Vision Service for Face
3. Custom Vision Service

***Storage Services***

1. Azure Storage
2. CosmosDB

***App Services***

1. App Service
2. Function App
3. API Management

***Operations & Security***

1. Azure DevOps
2. App Center
3. App Insights
4. Azure Key Vault (Optional)
5. Azure ADB2C (Optional)

When everything is provisioned, you should end up with a resource group similar to this:

![azure-deployment-overview](Assets/azure-deployment-overview.png)

## Cognitive Services

Please refer back to the previous guided steps to provision and setup the needed AI services.

1. [Cognitive Services - OCR](../03-CognitiveServices-OCR)
2. [Cognitive Services - Face](../04-CognitiveServices-Face)
3. [Cognitive Services - Customer Vision](../05-CognitiveServices-CustomVision)

## Storage Services

Contoso Shop Manager will be using [Azure Storage](https://docs.microsoft.com/en-us/azure/storage/) to store images and for queue triggers to the cognitive pipeline asynchronous requests. Also storage is used by Azure Functions runtime to persist state and settings.

The system also leverage no-sql data store [ComsoDB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) to store all information related to system operations and documents processing.

### Azure Storage

Azure Storage is a Microsoft-managed service providing cloud storage that is highly available, secure, durable, scalable, and redundant. Azure Storage includes Azure Blobs (objects), Azure Data Lake Storage Gen2, Azure Files, Azure Queues, and Azure Tables.

Head to [portal.azure.com](https://portal.azure.com) to start provision a new storage account.

If you didn't already done so, create a ***Resource Group*** to be used to provision all services related to this workshop for easy management.

#### Provisioning - Storage Account

Next you can click on ***Create a resource*** and search for [Storage Account]. Fill in the information needed take into consideration that storage account name must be unique across Azure with all small letters (no special characters allowed).

![Azure Storage Creation](Assets/azure-storage-create.png)

#### Configuration - Storage

Azure Storage required blobs will be created by the Contoso APIs up on initializing it for the first time as you will see later. All just what we need for now to take note of the ***Storage Connection String*** that will be updated in the settings for Azure Functions and App Service.

>***NOTE:*** for more information about the different storage capabilities and options, you can review [Azure Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/).

#### Azure Storage Explorer

A good way to manage your storage accounts (and more) is using the cross platform [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/).

Through this tool you can easily manage the contents of your storage account. Upload, download, and manage blobs, files, queues, tables, and Cosmos DB entities. Gain easy access to manage your virtual machine disks. Work with either Azure Resource Manager or classic storage accounts, plus manage and configure cross-origin resource sharing (CORS) rules.

![Azure Storage Explorer](Assets/azure-storage-explorer.png)

### CosmosDB

[Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) is Microsoft's globally distributed, multi-model database. With the click of a button, Azure Cosmos DB enables you to elastically and independently scale throughput and storage across any number of Azure's geographic regions. It offers throughput, latency, availability, and consistency guarantees with comprehensive service level agreements (SLAs), something no other database service can offer.

Although Cosmos DB has multiple data models and popular APIs for accessing and querying data (like SQL API, MongoDB API, Cassandara API, Gremilin API,...), we will be using SQL API, a schema-less JSON database engine with rich SQL querying capabilities.

#### Provisioning - Cosmos DB

Again, with the amazing experience in Azure Portal, you can prevision a new CosmosDB:

![Azure Comsos DB Creationg](Assets/azure-cosmosdb-create.png)

#### Configuration - Cosmos DB Collections

Collections in the Cosmos DB will be created automatically by the Contoso APIs app and will be populated by sample documents.

Impartant thing is to make sure that the data seeded in the CosmosDB is what you want from here [MockDataSeeder.cs](../../Src/Backend/Contoso.CognitivePipeline.API/Mocks/MockDataSeeder.cs)

Please take note of ***Connection String*** that will be used later in configuration access to both API and Function services later.

>***NOTE:*** for more information about the different Comoso DB capabilities and options, you can review [Cosmos DB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction).

#### Azure Storage Explorer for Cosmos DB

A good way to manage your Cosmos DB (like Storage) is using the cross platform [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/).

![Azure Storage Explorer](Assets/azure-storage-explorer.png)

## App Services

Azure cloud offers many AppDev-PaaS offerings that make building cloud native backend an easy task which will free you to focus more on the actual value rather than on the infrastructure.

In this section, we will be provisioning 1 API App for hosting our CognitivePipeline.API and 1 Web App for hosting Face Explorer (Optional).

### API Web App

[Azure Web Apps](https://docs.microsoft.com/en-us/azure/app-service/) enables you to build and host web applications in the programming language of your choice without managing infrastructure. It offers auto-scaling and high availability, supports both Windows and Linux, and enables automated deployments from GitHub, Azure DevOps, or any Git repo.

>***NOTE:** C# and .NET Core was selected to be the language used in developing Contoso Shop Manager backend (C# Functions, ASP .NET Core APIs). Keep in mind that all discussed services below support several other languages as well (like Java and JavaScript for example).

#### Provisioning new Web App

Click ***Create a resource*** to add a new Web App:

![Azure Web App Creation](Assets/azure-webapp-create.png)

>***NOTE:*** Optionally you can also provision a second web app for hosting Face Explorer.

#### Configuration - API Web App

After successful provisioning of the web app, you will see you have access to URL and various settings of your web app.

As we don't maintain any secrets or connection strings in the source code, you need to configure the required App Settings to include the appropriate values:

![app-settings](Assets/azure-app-service-settings.png)

You can also review and update these settings in the source code at [appsettings.json](../../Src/Backend/Contoso.CognitivePipeline.API/appsettings.json) so you can run the code locally if you wish.

>***NOTE:*** Notice the Azure Function related settings need to be updated after provisioning the Azure Function App later.

It is very good practice to provision at least 1 [Staging Slot](https://docs.microsoft.com/en-us/azure/app-service/deploy-staging-slots). You can use this slot to release your updates first before moving it to production slot.

>***NOTE:*** The app must be running in the Standard, Premium, or Isolated tier in order for you to enable multiple deployment slots.

Start creating the Dev deployment slot after fulling configuring the production app service settings first so you can copy all these settings to the new slot.

![deployment-slot](Assets/azure-app-service-slots.png)

### Function App

For simplicity, all Cognitive Pipeline services are deployed to a single function app.

#### Provisioning New Function App

You can go ahead with provisioning a new consumption based function app on Azure Portal:

![function-creation](Assets/azure-function-creation.png)

When you complete your function app provisioning and deploying the function code later in the DevOps guide, you will have something like this:

![function-overview](Assets/azure-function-provisioned.png)

#### Configuration - Function App

When provisioning completes, open the Application Settings in order to include all needed secrets as we did with API App Service before:

![function-settings](Assets/azure-function-settings.png)

You can also create a copy of [host.json](../../Src/Backend/Contoso.CognitivePipeline.BackgroundServices/host.json) and rename it to [local.settings.json](../../Src/Backend/Contoso.CognitivePipeline.BackgroundServices/local.settings.json). Then replace all relative values with the ones that reflect your provisioned environment.

This will allow you also to run the functions locally on your machine using Azure Functions CLI in Visual Studio Code or Visual Studio.

>***NOTE:*** Some of the settings will be created by the Function runtime, you don't need to change anything there.

Deployment of Contoso Cognitive Pipeline functions will be done via DevOps CI/CD pipelines that we will be discussing in the next guide.

### API Management

All APIs are exposed via [API Management Service](https://azure.microsoft.com/en-us/services/api-management/) which act as our turnkey API gateway solution.

All APIs will be accessed through enforced policy and subscription access.

>***NOTE:*** All APIs should not exposed directly to consumers. You should always plan to place and API Gateway in the middle.

#### Provisioning New API Management Service

API Management service is very easy to setup, just click Create new service and select or search for API Management:

![apim-creation](Assets/azure-apim-creation.png)

>***NOTE:*** API Management provisioning takes a little bit more time compared to other services.

#### Configuration - API Management

We will tackle this later after we deploy the actual API source code to our App Service.

This is what you would have after fully importing the cognitive APIs swagger definitions:

![apim-configuration](Assets/azure-apim-configuration.png)

#### API Management Access Information

You will need the base Url of the API Management service with a valid subscription key.

You can access the base Url in the **Overview** tab, it will be named **Gateway URL**.

You can access the key under **Users** -> **Administrator**.

![azure-apim-users](Assets/azure-apim-subs.png)

Keys by default are hidden. In order to copy any key you need first to show them. Show the keys associated with Unlimited subscription and take note of them:

![azure-apim-users](Assets/azure-apim-keys.png)

You will need the keys in Azure Pipelines variables (configured later) and in any Postman testing of the API Management APIs :)

## Operations (DevOps)

Now it is time to setup the initial environments for DevOps.

### Azure DevOps

If you don't have already an account with Azure DevOps, you can go ahead and provision a new one here [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/?nav=min).

![Azure-DevOps](Assets/azure-devops-docs.png)

Azure DevOps offers unlimited free private repos, 5 basic free licenses and free build minutes. So go ahead and get started.

As Microsoft is committed to Open Source, [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=vsts) are free for public projects offering 10 pipeline with unlimited hours :).

>***NOTE:*** This workshop is leveraging Azure Pipelines for GitHub to have integrated build and release services for our projects.
>![azure-devops](Assets/azure-devops-github.png)

#### Create New Project

After successfully signing in to Azure DevOps, you can start by creating your first project:

![azure-devops-newproject](Assets/azure-devops-createproject.png)

#### Get Source Code Repository Setup

Now it is up to you, either import the workshop repo to [Azure Repos](https://azure.microsoft.com/en-us/services/devops/repos/) or fork it to your GitHub account to start using your own Azure Pipelines.

##### Using Azure Repos

![azure-devops-repos](Assets/azure-devops-importgit.png)

##### Using GitHub

![azure-devops-github](Assets/azure-devops-gitfork.png)

#### Enabling Azure Pipelines for GitHub

Azure Pipelines as I said before, works with both GitHub and Azure Repos (and several others source control). Now that you have your own copy setup, it is time to activate the automated build pipelines.

Just go to [GitHub Marketplace](https://github.com/marketplace) and search for **Azure Pipelines** and install it on your account.

![github-pipelines](Assets/azure-devops-github-pipelines.png)

Select the Free plan :)

![github-pipelines-pricing](Assets/azure-devops-github-pipelines-pricing.png)

Azure Pipelines can be enabled on specific repos if you opt to.

![github-pipelines-configuration](Assets/azure-devops-github-pipelines-config.png)

The final step is to select your Azure DevOps project that will host the pipelines:

![github-pipelines-configuration](Assets/azure-devops-github-pipelines-project.png)

#### Azure Pipelines Build Tasks

I opted to use the newest approach to define pipelines which is YAML files.

You will find all the Azure Pipelines YAML files in the root of this repo. Azure DevOps look for a file called [azure-pipelines.yml](../../azure-pipelines.yml).

```yaml
# Azure Pipelines for GitHub :)
# More info: https://docs.microsoft.com/en-us/azure/devops/pipelines/?view=vsts
trigger:
  branches:
    include:
      - master
  paths:
    include:
      - 'Src/FaceExplorer-App'
      - 'Src/Backend'
      - 'Src/Mobile'
      - 'azure-pipelines.yml'
      - 'azure-pipelines-angular.yml'
      - 'azure-pipelines-function.yml'
      - 'azure-pipelines-webapp.yml'
      - 'azure-pipelines-nuget.yml'

variables:
  buildConfiguration: 'Release'

jobs:
- template: azure-pipelines-webapp.yml
  parameters:
    Name: ContosoBackendAPIs
    RestoreBuildProjects: '**/*.API.csproj'
    BuildConfiguration: 'Debug'
    WorkingDirectory: 'Src/Backend/Contoso.CognitivePipeline.API'
    ArtifactName: 'ContosoBackendAPIs'
    PublishWebApp: True

- template: azure-pipelines-webapp.yml
  parameters:
    Name: ContosoBackendFunction
    RestoreBuildProjects: '**/*.BackgroundServices.csproj'
    BuildConfiguration: 'Release'
    WorkingDirectory: 'Src/Backend/Contoso.CognitivePipeline.BackgroundServices'
    ArtifactName: 'ContosoBackendFunctions'
    PublishWebApp: False
    ZipAfterPublish: False

- template: azure-pipelines-angular.yml
  parameters:
    Name: FaceExplorerApp

- template: azure-pipelines-nuget.yml
  parameters:
    Name: 'ClientSDK'
    WorkingDirectory: 'Src/Backend/CognitivePipeline.ClientSDK'

- template: azure-pipelines-tests.yml
  parameters:
    Name: 'Tests'
```

I've used one useful feature of YAML definitions, Templates, which you will find also in the root of the repo.

- [azure-pipelines-angular.yml](../../azure-pipelines-angular.yml)
- [azure-pipelines-function.yml](../../azure-pipelines-function.yml)
- [azure-pipelines-nuget.yml](../../azure-pipelines-nuget.yml)
- [azure-pipelines-tests.yml](../../azure-pipelines-tests.yml)
- [azure-pipelines-webapp.yml](../../azure-pipelines-webapp.yml)

>***NOTE:*** You need to disable the Tests pipeline in the root Azure Pipelines definition in order to release the project for the first time. Tests are using the API Management endpoints to test the ClientSDK functionalities.

A successful run will look like this:

![azure-pipelines-results](Assets/azure-devops-pipelines-results.png)

#### Azure Pipeline Variables

I mentioned before that secrets must not be part of source control. 

Also you may need to inject some dynamic information as part of building your project.

A good way to accomplish the above points is to use Pipelines Variables.

Open the Pipelines -> Build in Azure DevOps and then click Edit:

![azure-pipelines-results](Assets/azure-devops-pipelines-overview.png)

In the Pipelines editor, select **Edit in the visual design**:

![azure-pipelines-results](Assets/azure-devops-pipelines-edit.png)

In the designer, navigate to the **Variables** tab:

![azure-pipelines-results](Assets/azure-devops-pipelines-edit-var.png)

Add the new variables shown above with the needed values for the build pipeline to function correctly.

In this case, a successful build pipeline, will include the following artifact that we will need to deploy via the **Release Pipeline**:

![pipeline-artifacts](Assets/azure-devops-pipelines-artifacts.png)

1. ClientSDK-Tests
2. CognitivePipeline-ClientSDK
3. ContosoBackendAPIs
4. ContosoBackendFunctions
5. FaceExplorer-App-Dist

>***NOTE:*** Please refer back to Azure DevOps documentation for [YAML](https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=vsts&tabs=schema) and [Build Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/?view=vsts) to get further information about this subject.

#### Azure Release Pipelines

Now once you have a successful build pipeline setup, you need to deploy the resources to Azure services via Azure Release pipeline.

To create one, head to Pipelines -> Releases in Azure DevOps and click New -> Import release pipeline:

![azure-pipeline-release](Assets/azure-devops-release.png)

In this repo, [Contoso-Backend-Services-CD.json](../../Src/Azure-Pipelines/Contoso-Backend-Services-CD.json), I've included a JSON file that include sample release definition that you can start with:

![azure-pipeline-release-run](Assets/azure-devops-release-run.png)

Notice that I connected this release pipeline to the output of the Build Pipeline.

You can see here, I'm releasing all backend components here:

1. API is deployed to Dev slot then swapped with production
2. Backend Functions is deployed to Function App
3. Face Explorer is deployed to App Service dev slot
4. ClientSDK NuGet is deployed to Azure Artifacts

>***NOTE:*** Please refer back to [Azure DevOps Release](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/?view=vsts) documentation to get further information about this subject.