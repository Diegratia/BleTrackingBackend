FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstEngine
# COPY Services.API/MstDistrict/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore


# COPY Services.API/MstDistrict/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstEngine/out ./

EXPOSE 5022
ENTRYPOINT ["dotnet", "MstEngine.dll"] 