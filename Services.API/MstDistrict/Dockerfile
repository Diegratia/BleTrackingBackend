FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstDistrict
# COPY Services.API/MstDistrict/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore


# COPY Services.API/MstDistrict/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstDistrict/out ./

EXPOSE 5012
ENTRYPOINT ["dotnet", "MstDistrict.dll"] 