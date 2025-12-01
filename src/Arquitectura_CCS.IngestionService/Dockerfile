FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Arquitectura_CCS.IngestionService/Arquitectura_CCS.IngestionService.csproj", "src/Arquitectura_CCS.IngestionService/"]
COPY ["src/Arquitectura_CCS.Common/Arquitectura_CCS.Common.csproj", "src/Arquitectura_CCS.Common/"]
RUN dotnet restore "src/Arquitectura_CCS.IngestionService/Arquitectura_CCS.IngestionService.csproj"

COPY . .
WORKDIR "/src/src/Arquitectura_CCS.IngestionService"
RUN dotnet build "Arquitectura_CCS.IngestionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Arquitectura_CCS.IngestionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Arquitectura_CCS.IngestionService.dll"]