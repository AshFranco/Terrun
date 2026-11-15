FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Terrun.Api/Terrun.Api.csproj src/Terrun.Api/
COPY src/Terrun.Domain/Terrun.Domain.csproj src/Terrun.Domain/
COPY src/Terrun.Engine/Terrun.Engine.csproj src/Terrun.Engine/
COPY src/Terrun.Infrastructure/Terrun.Infrastructure.csproj src/Terrun.Infrastructure/
RUN dotnet restore src/Terrun.Api/Terrun.Api.csproj

COPY src/ src/
RUN dotnet publish src/Terrun.Api/Terrun.Api.csproj --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Terrun.Api.dll"]
