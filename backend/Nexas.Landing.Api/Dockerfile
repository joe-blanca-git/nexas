# Etapa 1: Usar o SDK do .NET 8 para compilar o código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar os arquivos de projeto (.csproj) para restaurar as dependências primeiro
COPY ["Nexas.Landing.Api/Nexas.Landing.Api.csproj", "Nexas.Landing.Api/"]
COPY ["Nexas.Application/Nexas.Application.csproj", "Nexas.Application/"]
COPY ["Nexas.Domain/Nexas.Domain.csproj", "Nexas.Domain/"]
COPY ["Nexas.Infrastructure/Nexas.Infrastructure.csproj", "Nexas.Infrastructure/"]
RUN dotnet restore "Nexas.Landing.Api/Nexas.Landing.Api.csproj"

# Copiar o resto do código e compilar a aplicação
COPY . .
WORKDIR "/src/Nexas.Landing.Api"
RUN dotnet build "Nexas.Landing.Api.csproj" -c Release -o /app/build

# Publicar a versão otimizada
FROM build AS publish
RUN dotnet publish "Nexas.Landing.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Criar a imagem final super leve apenas com o ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexas.Landing.Api.dll"]
