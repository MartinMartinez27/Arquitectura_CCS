FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Arquitectura_CCS.EmergencyService/Arquitectura_CCS.EmergencyService.csproj", "src/Arquitectura_CCS.EmergencyService/"]
COPY ["src/Arquitectura_CCS.Common/Arquitectura_CCS.Common.csproj", "src/Arquitectura_CCS.Common/"]
RUN dotnet restore "src/Arquitectura_CCS.EmergencyService/Arquitectura_CCS.EmergencyService.csproj"

COPY . .
WORKDIR "/src/src/Arquitectura_CCS.EmergencyService"
RUN dotnet build "Arquitectura_CCS.EmergencyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Arquitectura_CCS.EmergencyService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Arquitectura_CCS.EmergencyService.dll"]