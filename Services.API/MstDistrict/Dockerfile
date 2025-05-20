FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Salin file proyek dan Common dari root context
COPY src/12.MstDistrict/*.csproj ./
COPY src/Common/ ./Common/
RUN dotnet restore

# Salin kode layanan
COPY src/12.MstDistrict/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5012
ENTRYPOINT ["dotnet", "12.MstDistrict.dll"] 