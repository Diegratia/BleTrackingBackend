# build with cache
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# copy project file dulu
COPY *.sln ./
COPY Directory.Packages.props ./
COPY global.json ./


COPY Services.API/MstBrand/MstBrand.csproj Services.API/MstBrand/
  COPY Shared/BusinessLogic.Services/BusinessLogic.Services.csproj Shared/BusinessLogic.Services/
  COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared/Shared.Contracts/
  COPY Shared/Data.ViewModels/Data.ViewModels.csproj Shared/Data.ViewModels/
  COPY Shared/Entities.Models/Entities.Models.csproj Shared/Entities.Models/
  COPY Shared/Helpers.Consumer/Helpers.Consumer.csproj Shared/Helpers.Consumer/
  COPY Shared/Repositories/Repositories.csproj Shared/Repositories/
  COPY Shared/Web.API.Controllers/Web.API.Controllers.csproj Shared/Web.API.Controllers/

# # WORKDIR /app/Services.API/MstBrand
# RUN dotnet restore
RUN dotnet restore Services.API/MstBrand/MstBrand.csproj

# copy source code
WORKDIR /app
COPY . .

# output publish
WORKDIR /app/Services.API/MstBrand
RUN dotnet publish -c Release -o out

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# copy hasil build
COPY --from=build /app/Services.API/MstBrand/out ./

EXPOSE 5009
ENTRYPOINT ["dotnet", "MstBrand.dll"]







# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/MstBrand
# RUN dotnet restore

# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/MstBrand/out ./

# EXPOSE 5009
# ENTRYPOINT ["dotnet", "MstBrand.dll"] 



