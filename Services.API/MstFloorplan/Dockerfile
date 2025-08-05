FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstFloorplan
# COPY Services.API/MstFloorplan/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/MstFloorplan/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstFloorplan/out ./

EXPOSE 5014
ENTRYPOINT ["dotnet", "MstFloorplan.dll"]