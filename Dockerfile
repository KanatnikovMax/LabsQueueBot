FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["*.sln", "."]
COPY ["LabsQueueBot.BusinessLogic/LabsQueueBot.BusinessLogic.csproj", "LabsQueueBot.BusinessLogic/"]
COPY ["LabsQueueBot.Core/LabsQueueBot.Core.csproj", "LabsQueueBot.Core/"]
COPY ["LabsQueueBot.Repository/LabsQueueBot.Repository.csproj", "LabsQueueBot.Repository/"]
COPY ["LabsQueueBot.DataAccess/LabsQueueBot.DataAccess.csproj", "LabsQueueBot.DataAccess/"]
COPY ["LabsQueueBot.Web/LabsQueueBot.Web.csproj", "LabsQueueBot.Web/"]

RUN dotnet restore "LabsQueueBot.sln"

COPY . .
WORKDIR "/src"
RUN dotnet build "LabsQueueBot.sln" -c Release -o /app/build

FROM build AS publisher
WORKDIR "/src/LabsQueueBot.Web"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publisher /app/publish .
ENTRYPOINT ["dotnet", "LabsQueueBot.Web.dll"]

#COPY . .
#
#WORKDIR "/src/LabsQueueBot.Web"
#RUN dotnet publish -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "LabsQueueBot.Web.dll"] 