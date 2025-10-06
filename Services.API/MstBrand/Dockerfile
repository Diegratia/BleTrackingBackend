FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY Services.API/MstBrand/MstBrand.csproj Services.API/MstBrand/
COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
COPY Repositories/Repositories.csproj Repositories/
COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

RUN dotnet restore Services.API/MstBrand/MstBrand.csproj

COPY . .
RUN dotnet build Services.API/MstBrand/MstBrand.csproj -c Release -o /app/build




# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app
# COPY ./ ./

# WORKDIR /app/Services.API/MstBrand
# # COPY Services.API/MstBrand/*.csproj ./
# # COPY Services.API/Common/ ./Common/
# RUN dotnet restore

# # COPY Services.API/MstBrand/ ./
# RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/Services.API/MstBrand/out ./

# EXPOSE 5009
# ENTRYPOINT ["dotnet", "MstBrand.dll"] 



