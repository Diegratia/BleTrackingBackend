FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./ ./

WORKDIR /app/Services.API/CardAccess
# COPY Services.API/20.VisitorBlacklistArea/*.csproj ./
# COPY Services.API/Common/ ./Common/
RUN dotnet restore


# COPY Services.API/20.VisitorBlacklistArea/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Services.API/CardAccess/out ./

EXPOSE 5028
ENTRYPOINT ["dotnet", "CardAccess.dll"] 