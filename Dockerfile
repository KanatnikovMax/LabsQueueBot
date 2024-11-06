FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LabsQueueBot/LabsQueueBot.csproj", "LabsQueueBot/"]
RUN dotnet restore "LabsQueueBot/LabsQueueBot.csproj"
COPY . .
WORKDIR "/src/LabsQueueBot"
RUN dotnet build "LabsQueueBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LabsQueueBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LabsQueueBot.dll"]