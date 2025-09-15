# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# copy seluruh source
COPY ./ ./  

# pindah ke project LicenseChecker
WORKDIR /app/LicenseChecker
RUN dotnet restore
RUN dotnet publish -c Release -o out

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/LicenseChecker/out ./

# license file bisa dimount dari host
VOLUME ["/app/license"]

ENTRYPOINT ["dotnet", "LicenseChecker.dll"]
