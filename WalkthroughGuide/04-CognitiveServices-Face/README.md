![Banner](Assets/Banner.png)

# Face Services Overview

Azure Computer Vision for Face offers comprehensive capabilities to deal with photos with faces.

You can visit [Face Portal](https://azure.microsoft.com/en-us/services/cognitive-services/face/) to test and evaluate the different capabilities.

## Face Authentication

Part of Cognitive Services Face APIs is [Face Verify](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523a), which verify whether two faces belong to a same person or whether one face belongs to a person.

Contoso Biometric Authentication uses Face Verify to authenticate a live capture face image against a predefined list of employee faces.

In order to achieve this scenario, some preparation work needs to be done. Face APIs have capabilities to store faces in a secure data store.

Faces data store is hosted under what is called a [Person Group]() or [Large Person Group](). A person group is the container of the uploaded person data, including face images and face recognition features. After creation, use [PersonGroup Person - Create](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523c) to add persons into the group, and then call [PersonGroup - Train](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395249) to get this group ready for [Face - Identify](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395239).

## Face APIs - Postman

To try out the different Face APIs preparation work, this workshop includes a ***Postman*** collection extract that can be imported. Collection is organized in folders that each include the relevant operations for each part of the Face APIs usage process.

![Postman Face APIs](Assests/face-postman.png)

Also you need to import the [Dev](../../Src/Postman-APIs/Dev.postman_environment.json) environment variables that are being used through out the APIs.

>**NOTE:** All APIs Postman collections used through out this workshop can be found under [Src/Postman-APIs](../../Src/Postman-APIs)

Steps to leverage Face Authentication scenario include:

1. Create a Person Group (consider this as the organization or department, default ID used is 1)
2. Create a Person (inside the created Person Group (represent an individual inside the organization)
3. Create a Person Face (with a URL to the image that hold face of the person. You can add multiple faces to a person for better recognition)
4. Train the system on the newly formed Person Group (you must do this every time you add or update Person in the Person Group)
5. Verify the training status to ensure that Face APIs are using up to date model.
6. Now you can start verifying faces using Face Detect and Verify

## Face Explorer

Included with this workshop a nice Angular web application that provides GUI to interact with the Face APIs from setup to verification.

You can access the source code for [Face Explrer here](../../Src/FaceExplorer-App).

![Face Explorer App](Assets/face-explorer-app)

If you wish to run the Angular app locally on you machine, you need to have [Angular CLI](https://cli.angular.io/) installed. Below are the steps you should perform:

1. [NodeJs](https://nodejs.org/en/) must be installed.
2. You can then use NPM to install Angular CLI using the following command:

```js
npm install -g @angular/cli
```

>***NOTE:*** You can now open the project inside Visual Studio Code to perform the next steps. Once it is opened, you can launch a new terminal window like the screen shot below:
![Visual Studio Code](Assets/face-explorer-terminal.png)

3. Next step will be installing all project dependencies using NPM (must be run inside the [FaceExplorer-App](../../Src/FaceExplorer-App) folder):

```js
npm install
```

4. You can then use command line or Visual Studio Code to build and run the Angular project (command must be executed in folder [FaceExplorer-App](../../Src/FaceExplorer-App)).

```js
ng serve
```