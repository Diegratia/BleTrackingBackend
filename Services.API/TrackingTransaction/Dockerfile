FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/TrackingTransaction
# COPY Services.API/TrackingTransaction/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore


# COPY Services.API/TrackingTransaction/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/TrackingTransaction/out ./

EXPOSE 10018
ENTRYPOINT ["dotnet", "TrackingTransaction.dll"] 



