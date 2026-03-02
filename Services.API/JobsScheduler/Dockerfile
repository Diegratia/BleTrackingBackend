
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Directory.Packages.props ./
  COPY global.json ./
  COPY Services.API/JobsScheduler/JobsScheduler.csproj Services.API/JobsScheduler/
  COPY Shared/BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared.Contracts/
  COPY Shared/Repositories/Repositories.csproj Repositories/

RUN dotnet restore Services.API/JobsScheduler/JobsScheduler.csproj

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/JobsScheduler
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/JobsScheduler/out ./

ENTRYPOINT ["dotnet", "JobsScheduler.dll"]






