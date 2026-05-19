FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy NuGet.Config so restore uses /root/nuget-feed
COPY NuGet.Config .

# Copy private NuGet feed into the container
COPY nuget-feed /root/nuget-feed

# Copy the rest of the source
COPY . .

# Publish
RUN dotnet publish "ZypryxAPI/ZypryxAPI.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ZypryxAPI.dll"]
