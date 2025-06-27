FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/CorporateTravel.API/CorporateTravel.API.csproj", "src/CorporateTravel.API/"]
COPY ["src/CorporateTravel.Application/CorporateTravel.Application.csproj", "src/CorporateTravel.Application/"]
COPY ["src/CorporateTravel.Domain/CorporateTravel.Domain.csproj", "src/CorporateTravel.Domain/"]
COPY ["src/CorporateTravel.Infrastructure/CorporateTravel.Infrastructure.csproj", "src/CorporateTravel.Infrastructure/"]

RUN dotnet restore "src/CorporateTravel.API/CorporateTravel.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/CorporateTravel.API"
RUN dotnet build "CorporateTravel.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CorporateTravel.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorporateTravel.API.dll"] 