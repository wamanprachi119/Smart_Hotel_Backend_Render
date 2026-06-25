# ─────────────────────────────────────────────────────────────
# SmartHotelBackend — Dockerfile for Render (env: docker)
# Multi-stage build: SDK image to build/publish, ASP.NET runtime to run.
# ─────────────────────────────────────────────────────────────

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first and restore (layer caching speeds up rebuilds)
COPY SmartHotelBackend.csproj ./
RUN dotnet restore "SmartHotelBackend.csproj"

# Copy the rest of the source and publish
COPY . .
RUN dotnet build "SmartHotelBackend.csproj" -c Release -o /app/build
RUN dotnet publish "SmartHotelBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render injects PORT at runtime; Program.cs reads it via Environment.GetEnvironmentVariable("PORT").
# ASPNETCORE_ENVIRONMENT defaults to Production unless overridden in Render's environment variables.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Render's free web service routes traffic to whatever PORT it assigns and injects via env var.
# No EXPOSE port is hardcoded since Program.cs binds dynamically to $PORT.

ENTRYPOINT ["dotnet", "SmartHotelBackend.dll"]
