FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Services.API/Visitor/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

COPY Services.API/Visitor/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 5019
ENTRYPOINT ["dotnet", "Visitor.dll"] 