FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CorporateTravel.API/CorporateTravel.API.csproj", "src/CorporateTravel.API/"]
COPY ["src/CorporateTravel.Application/CorporateTravel.Application.csproj", "src/CorporateTravel.Application/"]
COPY ["src/CorporateTravel.Domain/CorporateTravel.Domain.csproj", "src/CorporateTravel.Domain/"]
COPY ["src/CorporateTravel.Infrastructure/CorporateTravel.Infrastructure.csproj", "src/CorporateTravel.Infrastructure/"]
RUN dotnet restore "src/CorporateTravel.API/CorporateTravel.API.csproj"
COPY . .
WORKDIR "/src/src/CorporateTravel.API"
RUN dotnet build "./CorporateTravel.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CorporateTravel.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateTravel.API.dll"]