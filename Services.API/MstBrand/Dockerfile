# =============================
# 🧱 Step 1 - Build & Restore
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Salin file project & solution saja (agar restore bisa di-cache)
COPY *.sln ./
COPY Services.API/MstBrand/MstBrand.csproj Services.API/MstBrand/
COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
COPY Data.ViewModels/Data.ViewModels.csproj Data.ViewModels/
COPY Repositories/Repositories.csproj Repositories/
COPY Web.API.Controllers/Web.API.Controllers.csproj Web.API.Controllers/

# Restore hanya project target agar layer restore bisa di-cache
RUN dotnet restore Services.API/MstBrand/MstBrand.csproj

# Salin seluruh source code
COPY . .

# Build project
RUN dotnet build Services.API/MstBrand/MstBrand.csproj -c Release -o /app/build

# Publish ke lokasi sesuai struktur aslimu
RUN dotnet publish Services.API/MstBrand/MstBrand.csproj -c Release -o /app/Services.API/MstBrand/out


# =============================
# 🚀 Step 2 - Runtime
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Salin hasil publish dari stage build
COPY --from=build /app/Services.API/MstBrand/out ./

# Port sesuai API-mu
EXPOSE 5009

ENTRYPOINT ["dotnet", "MstBrand.dll"]





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



