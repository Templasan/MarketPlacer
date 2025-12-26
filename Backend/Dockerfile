# Estágio de Build - MUDADO PARA 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copia todos os arquivos
COPY . .

# 2. Restaura as dependências (ajuste para o caminho exato da sua pasta)
RUN dotnet restore "Backend/MarketPlacer.API/MarketPlacer.API.csproj"

# 3. Publica a aplicação
RUN dotnet publish "Backend/MarketPlacer.API/MarketPlacer.API.csproj" -c Release -o /app

# Estágio de Execução - MUDADO PARA 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Ajuste conforme o nome exato da sua DLL
ENTRYPOINT ["dotnet", "MarketPlacer.API.dll"]