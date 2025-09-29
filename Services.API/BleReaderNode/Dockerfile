FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/BleReaderNode
# COPY Services.API/FloorplanDevice/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API//FloorplanDevice/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/BleReaderNode/out ./

EXPOSE 5021

ENTRYPOINT ["dotnet", "BleReaderNode.dll"]





