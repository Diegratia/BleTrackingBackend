FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MonitoringConfig
# COPY Services.API/FloorplanMaskedArea/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/FloorplanMaskedArea/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MonitoringConfig/out ./

EXPOSE 5029

ENTRYPOINT ["dotnet", "MonitoringConfig.dll"]



