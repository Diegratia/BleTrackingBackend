FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Services.API/MstOrganization/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

COPY Services.API/MstOrganization/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5017
ENTRYPOINT ["dotnet", "MstOrganization.dll"] 