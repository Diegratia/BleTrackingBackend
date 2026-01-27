
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/CardRecord/CardRecord.csproj Services.API/CardRecord/

  COPY Shared/BusinessLogic.Services/BusinessLogic.Services.csproj Shared/BusinessLogic.Services/
  COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared/Shared.Contracts/
  COPY Shared/Data.ViewModels/Data.ViewModels.csproj Shared/Data.ViewModels/
  COPY Shared/Entities.Models/Entities.Models.csproj Shared/Entities.Models/
  COPY Shared/Helpers.Consumer/Helpers.Consumer.csproj Shared/Helpers.Consumer/
  COPY Shared/Repositories/Repositories.csproj Shared/Repositories/
  COPY Shared/Web.API.Controllers/Web.API.Controllers.csproj Shared/Web.API.Controllers/

# WORKDIR /app/Services.API/CardRecord
# RUN dotnet restore
RUN dotnet restore Services.API/CardRecord/CardRecord.csproj

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/CardRecord
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/CardRecord/out ./

EXPOSE 5024
ENTRYPOINT ["dotnet", "CardRecord.dll"]








# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/CardRecord
# # COPY Services.API/20.BlacklistArea/*.csproj ./
# # COPY Services.API/Common/ ./Common/
# RUN dotnet restore


# # COPY Services.API/20.BlacklistArea/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/CardRecord/out ./

# EXPOSE 5024
# ENTRYPOINT ["dotnet", "CardRecord.dll"] 