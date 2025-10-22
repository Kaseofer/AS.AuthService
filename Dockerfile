FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia archivos de proyecto primero
COPY ["AuthService.API/*.csproj", "AuthService.API/"]
COPY ["AuthService.Application/*.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/*.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/*.csproj", "AuthService.Infrastructure/"]

# Configurar NuGet para usar solo rutas del contenedor
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Restore (con configuración de red host)
RUN dotnet restore "AuthService.API/AuthService.API.csproj"

# Copia el resto del código
COPY . .

# Build
RUN dotnet build "AuthService.API/AuthService.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "AuthService.API/AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]