FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstBleReader
RUN dotnet restore

# COPY Services.API/Auth/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstBleReader/out ./

EXPOSE 5008

ENTRYPOINT ["dotnet", "MstBleReader.dll"]