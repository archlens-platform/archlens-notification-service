FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .

COPY src/SharedKernel/ArchLens.SharedKernel/ArchLens.SharedKernel.csproj src/SharedKernel/ArchLens.SharedKernel/
COPY src/Contracts/ArchLens.Contracts/ArchLens.Contracts.csproj src/Contracts/ArchLens.Contracts/
COPY src/Services/Notification/ArchLens.Notification.Domain/ArchLens.Notification.Domain.csproj src/Services/Notification/ArchLens.Notification.Domain/
COPY src/Services/Notification/ArchLens.Notification.Application.Contracts/ArchLens.Notification.Application.Contracts.csproj src/Services/Notification/ArchLens.Notification.Application.Contracts/
COPY src/Services/Notification/ArchLens.Notification.Application/ArchLens.Notification.Application.csproj src/Services/Notification/ArchLens.Notification.Application/
COPY src/Services/Notification/ArchLens.Notification.Infrastructure/ArchLens.Notification.Infrastructure.csproj src/Services/Notification/ArchLens.Notification.Infrastructure/
COPY src/Services/Notification/ArchLens.Notification.Api/ArchLens.Notification.Api.csproj src/Services/Notification/ArchLens.Notification.Api/

RUN dotnet restore src/Services/Notification/ArchLens.Notification.Api/ArchLens.Notification.Api.csproj

COPY src/SharedKernel/ src/SharedKernel/
COPY src/Contracts/ src/Contracts/
COPY src/Services/Notification/ src/Services/Notification/

RUN dotnet publish src/Services/Notification/ArchLens.Notification.Api/ArchLens.Notification.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ArchLens.Notification.Api.dll"]
