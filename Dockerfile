# syntax=docker/dockerfile:1

# ---- Angular storefront -------------------------------------------------
FROM node:22-bookworm-slim AS web
WORKDIR /src/frontend/noor-web
COPY frontend/noor-web/package.json frontend/noor-web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY frontend/noor-web/ ./
RUN npm run build -- --configuration production

# ---- .NET API -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/Noor.Core/Noor.Core.csproj backend/Noor.Core/
COPY backend/Noor.Infrastructure/Noor.Infrastructure.csproj backend/Noor.Infrastructure/
COPY backend/Noor.Api/Noor.Api.csproj backend/Noor.Api/
RUN dotnet restore backend/Noor.Api/Noor.Api.csproj
COPY backend/ backend/
RUN dotnet publish backend/Noor.Api/Noor.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=web /src/frontend/noor-web/dist/noor-web/browser/ ./wwwroot/

ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} exec dotnet Noor.Api.dll"]
