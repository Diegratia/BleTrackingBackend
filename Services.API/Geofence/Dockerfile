
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/Geofence/Geofence.csproj Services.API/Geofence/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
  COPY Entities.Models/Entities.Models.csproj Entities.Models/
  COPY Helpers.Consumer/Helpers.Consumer.csproj Helpers.Consumer/
  COPY Repositories/Repositories.csproj Repositories/
  COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

WORKDIR /app/Services.API/Geofence
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/Geofence
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/Geofence/out ./

EXPOSE 5030
ENTRYPOINT ["dotnet", "Geofence.dll"]







# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/Geofence
# # COPY Services.API/Geofence/*.csproj ./
# # COPY Services.API/Common/ ./Common/
# RUN dotnet restore

# # COPY Services.API/Geofence/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/Geofence/out ./

# EXPOSE 5030

# ENTRYPOINT ["dotnet", "Geofence.dll"]



