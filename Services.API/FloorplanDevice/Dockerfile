FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/FloorplanDevice

# COPY Services.API/FloorplanDevice/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API//FloorplanDevice/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/FloorplanDevice/out ./

EXPOSE 5003

ENTRYPOINT ["dotnet", "FloorplanDevice.dll"]
