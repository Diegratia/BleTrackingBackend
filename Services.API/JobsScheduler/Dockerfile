
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

  COPY *.sln ./
  COPY Services.API/JobsScheduler/JobsScheduler.csproj Services.API/JobsScheduler/
  COPY BusinessLogic.Services/BusinessLogic.Services.csproj BusinessLogic.Services/
  COPY Repositories/Repositories.csproj Repositories/

WORKDIR /app/Services.API/JobsScheduler
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/Services.API/JobsScheduler
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

COPY --from=build /app/Services.API/JobsScheduler/out ./

ENTRYPOINT ["dotnet", "JobsScheduler.dll"]






