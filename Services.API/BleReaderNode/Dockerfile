FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app


COPY Services.API/BleReaderNode/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore


COPY Services.API/BleReaderNode/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5021
ENTRYPOINT ["dotnet", "BleReaderNode.dll"] 