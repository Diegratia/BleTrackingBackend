
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/AlarmRecordTracking
# COPY /app/Services.API/AlarmRecordTracking/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore

# COPY Services.API/AlarmRecordTracking/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/AlarmRecordTracking/out ./

EXPOSE 10002

ENTRYPOINT ["dotnet", "AlarmRecordTracking.dll"]