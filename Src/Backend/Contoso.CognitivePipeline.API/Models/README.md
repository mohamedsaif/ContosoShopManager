# The Use of Shared Project for Models

Using shared project to share the Models accross both the API (ASP .NET Core Project) and Background Services (Azure Functions).

You may prefere to add these objects to a .NET Standard library project instead for better portability beyond Visual Sutdio (like to be used in a Visual Studio Code)

> ***NOTE:*** Make sure to include these files in the API and Backgnroud Servvices Directories if you are using Visual Studio Code