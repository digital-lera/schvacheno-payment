# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Notifications.csproj", "Notifications.csproj"]
RUN dotnet restore "Notifications.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Notifications.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Notifications.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (Alpine = 50MB image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Notifications.dll"]
