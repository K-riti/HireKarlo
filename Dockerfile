# Multi-stage build for HireKarlo API
# Alpine-based, minimal size, production-ready

# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /src

# Copy solution and projects
COPY ["HireKarlo.slnx", "./"]
COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

# Restore and build
RUN dotnet restore "HireKarlo.slnx"
RUN dotnet build -c Release --no-restore -o /app/build

# ===== PUBLISH STAGE =====
FROM build AS publish

RUN dotnet publish "src/Presentation/HireKarlo.Api/HireKarlo.Api.csproj" \
	-c Release --no-build -o /app/publish

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

# Install required packages
RUN apk add --no-cache ca-certificates tzdata

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser
USER appuser

# Expose port
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
	CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

# Environment variables (to be overridden)
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Run application
ENTRYPOINT ["dotnet", "HireKarlo.Api.dll"]
