# Etapa 1: Usar o SDK do .NET 8 para compilar o código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar os arquivos de projeto (.csproj) para restaurar as dependências primeiro
COPY ["Nexas.Lemon.Admin.Api/Nexas.Lemon.Admin.Api.csproj", "Nexas.Lemon.Admin.Api/"]
COPY ["Nexas.Application/Nexas.Application.csproj", "Nexas.Application/"]
COPY ["Nexas.Domain/Nexas.Domain.csproj", "Nexas.Domain/"]
COPY ["Nexas.Infrastructure/Nexas.Infrastructure.csproj", "Nexas.Infrastructure/"]
RUN dotnet restore "Nexas.Lemon.Admin.Api/Nexas.Lemon.Admin.Api.csproj"

# Copiar o resto do código e compilar a aplicação
COPY . .
WORKDIR "/src/Nexas.Lemon.Admin.Api"
RUN dotnet build "Nexas.Lemon.Admin.Api.csproj" -c Release -o /app/build

# Publicar a versão otimizada
FROM build AS publish
RUN dotnet publish "Nexas.Lemon.Admin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Criar a imagem final super leve apenas com o ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexas.Lemon.Admin.Api.dll"]
