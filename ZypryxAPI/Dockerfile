# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (including Utility submodule)
COPY . .

# Restore dependencies
RUN dotnet restore "./ZypryxAPI.csproj"

# Publish trimmed, ready for runtime
RUN dotnet publish "./ZypryxAPI.csproj" -c Release -o /app/publish

# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose API port
EXPOSE 8080

# Run the API
ENTRYPOINT ["dotnet", "ZypryxAPI.dll"]
