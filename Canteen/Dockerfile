FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Canteen.Api/Canteen.Api.csproj", "src/Canteen.Api/"]
COPY ["src/Canteen.DataAccess/Canteen.DataAccess.csproj", "src/Canteen.DataAccess/"]
COPY ["src/Canteen.Services/Canteen.Services.csproj", "src/Canteen.Services/"]

COPY . .
RUN dotnet restore "src/Canteen.Api/Canteen.Api.csproj"
RUN dotnet build "src/Canteen.Api/Canteen.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
# WORKDIR /src/Canteen.Api/
# RUN dotnet build "Canteen.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "Canteen.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet publish "src/Canteen.Api/Canteen.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN rm -rf /var/lib/apt/lists/* \
    && apt-get clean \
    && apt-get update -y \
    && apt-get install libldap-2.5-0 -y
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Canteen.Api.dll"]
