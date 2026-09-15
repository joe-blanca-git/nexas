FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and restore as distinct layers
COPY ["Nexas.sln", "./"]
COPY ["Nexas.Domain/Nexas.Domain.csproj", "Nexas.Domain/"]
COPY ["Nexas.Application/Nexas.Application.csproj", "Nexas.Application/"]
COPY ["Nexas.Infrastructure/Nexas.Infrastructure.csproj", "Nexas.Infrastructure/"]
COPY ["Nexas.Api/Nexas.Api.csproj", "Nexas.Api/"]
COPY ["Nexas.Lemon.Admin.Api/Nexas.Lemon.Admin.Api.csproj", "Nexas.Lemon.Admin.Api/"]
COPY ["Nexas.Lemon.Landing.Api/Nexas.Lemon.Landing.Api.csproj", "Nexas.Lemon.Landing.Api/"]
COPY ["Nexas.SystemManager/Nexas.SystemManager.csproj", "Nexas.SystemManager/"]

RUN dotnet restore "Nexas.sln"

# Copy everything else and build
COPY . ./
RUN dotnet publish "Nexas.SystemManager/Nexas.SystemManager.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexas.SystemManager.dll"]
