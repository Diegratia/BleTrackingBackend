FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/MstApplication
# COPY Services.API/MstApplication/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/MstApplication/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/MstApplication/out ./

EXPOSE 10007
ENTRYPOINT ["dotnet", "MstApplication.dll"]