# build with cache
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# copy project file dulu
  COPY *.sln ./
  COPY Services.API/MstBrand/MstBrand.csproj Services.API/MstBrand/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
  COPY Entities.Models/Entities.Models.csproj Entities.Models/
  COPY Helpers.Consumer/Helpers.Consumer.csproj Helpers.Consumer/
  COPY Repositories/Repositories.csproj Repositories/
  COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

WORKDIR /app/Services.API/MstBrand
RUN dotnet restore

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



