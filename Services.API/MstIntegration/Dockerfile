FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstIntegration
# COPY Services.API/MstIntegration/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/MstIntegration/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstIntegration/out ./

EXPOSE 5015
ENTRYPOINT ["dotnet", "MstIntegration.dll"] 