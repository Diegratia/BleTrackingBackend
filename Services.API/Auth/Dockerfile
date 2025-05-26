FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Services.API/Auth/*.csproj ./
RUN dotnet restore

COPY Services.API/Auth/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5001

ENTRYPOINT ["dotnet", "Auth.dll"]

