FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything from the solution root
COPY . .

# Restore only the current service
RUN dotnet restore CloudTaskManager.Identity/CloudTaskManager.Identity.csproj

# Publish only the current service
RUN dotnet publish CloudTaskManager.Identity/CloudTaskManager.Identity.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CloudTaskManager.Identity.dll"]
