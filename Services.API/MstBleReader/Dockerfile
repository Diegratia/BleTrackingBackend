FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/MstBleReader/*.csproj ./
COPY src/Common/ ./Common/
RUN dotnet restore

COPY src/8.MstBleReader/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5008
ENTRYPOINT ["dotnet", "8.MstBleReader.dll"] 