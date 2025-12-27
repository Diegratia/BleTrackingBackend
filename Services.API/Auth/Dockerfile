
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/Auth/Auth.csproj Services.API/Auth/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
  COPY Entities.Models/Entities.Models.csproj Entities.Models/
  COPY Helpers.Consumer/Helpers.Consumer.csproj Helpers.Consumer/
  COPY Repositories/Repositories.csproj Repositories/
  COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

WORKDIR /app/Services.API/Auth
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/Auth
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/Auth/out ./

EXPOSE 5001
ENTRYPOINT ["dotnet", "Auth.dll"]











# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/Auth
# RUN dotnet restore

# # COPY Services.API/Auth/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/Auth/out ./

# EXPOSE 5001

# ENTRYPOINT ["dotnet", "Auth.dll"]