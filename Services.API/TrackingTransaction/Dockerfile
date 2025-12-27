
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/TrackingTransaction/TrackingTransaction.csproj Services.API/TrackingTransaction/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
  COPY Entities.Models/Entities.Models.csproj Entities.Models/
  COPY Helpers.Consumer/Helpers.Consumer.csproj Helpers.Consumer/
  COPY Repositories/Repositories.csproj Repositories/
  COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

WORKDIR /app/Services.API/TrackingTransaction
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/TrackingTransaction
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/TrackingTransaction/out ./

EXPOSE 5018
ENTRYPOINT ["dotnet", "TrackingTransaction.dll"]


# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/TrackingTransaction
# # COPY Services.API/TrackingTransaction/*.csproj ./
# # COPY Services.API/Common/ ./Common/
# RUN dotnet restore


# # COPY Services.API/TrackingTransaction/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/TrackingTransaction/out ./

# EXPOSE 5018
# ENTRYPOINT ["dotnet", "TrackingTransaction.dll"] 



