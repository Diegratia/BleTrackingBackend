FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/TrxVisitor
# COPY Services.API/TrxVisitor/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/TrxVisitor/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/TrxVisitor/out ./

EXPOSE 5025
ENTRYPOINT ["dotnet", "TrxVisitor.dll"] 