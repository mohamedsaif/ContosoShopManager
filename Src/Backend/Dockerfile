FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Contoso.CognitivePipeline.API/Contoso.CognitivePipeline.API.csproj", "Contoso.CognitivePipeline.API/"]
RUN dotnet restore "Contoso.CognitivePipeline.API/Contoso.CognitivePipeline.API.csproj"
COPY ./Contoso.CognitivePipeline.API ./Contoso.CognitivePipeline.API/
COPY ./Contoso.CognitivePipeline.SharedModels ./Contoso.CognitivePipeline.SharedModels/
RUN ls
WORKDIR "/src/Contoso.CognitivePipeline.API"
RUN dotnet build "Contoso.CognitivePipeline.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Contoso.CognitivePipeline.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Contoso.CognitivePipeline.API.dll"]