
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Services.API/FloorplanDevice/FloorplanDevice.csproj Services.API/FloorplanDevice/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
  COPY Entities.Models/Entities.Models.csproj Entities.Models/
  COPY Helpers.Consumer/Helpers.Consumer.csproj Helpers.Consumer/
  COPY Repositories/Repositories.csproj Repositories/
  COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

WORKDIR /app/Services.API/FloorplanDevice
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/FloorplanDevice
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/FloorplanDevice/out ./

EXPOSE 5003
ENTRYPOINT ["dotnet", "FloorplanDevice.dll"]









# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/FloorplanDevice

# # COPY Services.API/FloorplanDevice/*.csproj ./
# # COPY Services.API/Common/ ./Common/
# RUN dotnet restore

# # COPY Services.API//FloorplanDevice/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/FloorplanDevice/out ./

# EXPOSE 5003

# ENTRYPOINT ["dotnet", "FloorplanDevice.dll"]
