FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY archlens-contracts/Directory.Build.props ./archlens-contracts/
COPY archlens-contracts/src/ArchLens.SharedKernel/*.csproj ./archlens-contracts/src/ArchLens.SharedKernel/
COPY archlens-contracts/src/ArchLens.Contracts/*.csproj ./archlens-contracts/src/ArchLens.Contracts/

COPY archlens-notification-service/*.sln ./archlens-notification-service/
COPY archlens-notification-service/Directory.Build.props ./archlens-notification-service/
COPY archlens-notification-service/src/ArchLens.Notification.Api/*.csproj ./archlens-notification-service/src/ArchLens.Notification.Api/
COPY archlens-notification-service/src/ArchLens.Notification.Application/*.csproj ./archlens-notification-service/src/ArchLens.Notification.Application/
COPY archlens-notification-service/src/ArchLens.Notification.Application.Contracts/*.csproj ./archlens-notification-service/src/ArchLens.Notification.Application.Contracts/
COPY archlens-notification-service/src/ArchLens.Notification.Domain/*.csproj ./archlens-notification-service/src/ArchLens.Notification.Domain/
COPY archlens-notification-service/src/ArchLens.Notification.Infrastructure/*.csproj ./archlens-notification-service/src/ArchLens.Notification.Infrastructure/

WORKDIR /src/archlens-notification-service
RUN dotnet restore src/ArchLens.Notification.Api/ArchLens.Notification.Api.csproj

WORKDIR /src
COPY archlens-contracts/ ./archlens-contracts/
COPY archlens-notification-service/ ./archlens-notification-service/

WORKDIR /src/archlens-notification-service
RUN dotnet publish src/ArchLens.Notification.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL org.opencontainers.image.source="https://github.com/archlens-platform/archlens-notification-service"
LABEL org.opencontainers.image.title="ArchLens Notification Service"
LABEL org.opencontainers.image.version="1.0.0"
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ArchLens.Notification.Api.dll"]
