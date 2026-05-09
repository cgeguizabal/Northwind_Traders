# ── Stage 1: Build ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and all project files first (layer caching)
COPY NorthwindTraders.slnx ./
COPY src/NorthwindTraders.API/NorthwindTraders.API.csproj             ./src/NorthwindTraders.API/
COPY src/NorthwindTraders.Application/NorthwindTraders.Application.csproj ./src/NorthwindTraders.Application/
COPY src/NorthwindTraders.Domain/NorthwindTraders.Domain.csproj       ./src/NorthwindTraders.Domain/
COPY src/NorthwindTraders.Infrastructure/NorthwindTraders.Infrastructure.csproj ./src/NorthwindTraders.Infrastructure/

RUN dotnet restore src/NorthwindTraders.API/NorthwindTraders.API.csproj

# Copy the rest and publish
COPY src/ ./src/
RUN dotnet publish src/NorthwindTraders.API/NorthwindTraders.API.csproj \
    -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5272
ENV ASPNETCORE_URLS=http://+:5272

ENTRYPOINT ["dotnet", "NorthwindTraders.API.dll"]