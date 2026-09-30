# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything first (no layer caching issues)
COPY . .

# Verify csproj content
RUN cat FmhyPlugin/FmhyPlugin.csproj

# Restore with no cache
RUN dotnet restore FmhyPlugin/FmhyPlugin.csproj --no-cache -v n

# Build and publish the plugin assembly.
WORKDIR /src/FmhyPlugin
RUN dotnet publish -c Release -o /app/publish --no-restore -v n

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev && rm -rf /var/lib/apt/lists/*

# Create plugin directory
RUN mkdir -p /plugins/FmhyPlugin/Web

# Copy published files
COPY --from=build /app/publish/FmhyPlugin.dll /plugins/FmhyPlugin/
COPY FmhyPlugin/plugin.json /plugins/FmhyPlugin/
COPY FmhyPlugin/Web/configuration.html /plugins/FmhyPlugin/Web/
COPY FmhyPlugin/Web/browse.html /plugins/FmhyPlugin/Web/

VOLUME ["/plugins"]
CMD ["echo", "Plugin built at /plugins/FmhyPlugin"]