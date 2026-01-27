
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/Analytics/Analytics.csproj Services.API/Analytics/

  COPY Shared/BusinessLogic.Services/BusinessLogic.Services.csproj shared/BusinessLogic.Services/
  COPY Shared/Data.ViewModels/Data.ViewModels.csproj shared/Data.ViewModels/
  COPY Shared/Entities.Models/Entities.Models.csproj shared/Entities.Models/
  COPY Shared/Helpers.Consumer/Helpers.Consumer.csproj shared/Helpers.Consumer/
  COPY Shared/Repositories/Repositories.csproj shared/Repositories/
  COPY Shared/Web.API.Controllers/Web.API.Controllers.csproj shared/Web.API.Controllers/

# WORKDIR /app/Services.API/Analytics
# RUN dotnet restore
RUN dotnet restore Services.API/Analytics/Analytics.csproj


WORKDIR /app
COPY . .

WORKDIR /app/Services.API/Analytics
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/Analytics/out ./

EXPOSE 5031
ENTRYPOINT ["dotnet", "Analytics.dll"]







# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/Analytics
# RUN dotnet restore

# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/Analytics/out ./

# EXPOSE 5027

# ENTRYPOINT ["dotnet", "Analytics.dll"]