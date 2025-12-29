# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copia todos os arquivos da raiz (Contexto: .)
COPY . .

# 2. Ajustado para o caminho real: BACKEND/MarketPlacer.API.csproj
RUN dotnet restore "BACKEND/MarketPlacer.API/MarketPlacer.API.csproj"

# 3. Publica a aplicação
RUN dotnet publish "BACKEND/MarketPlacer.API/MarketPlacer.API.csproj" -c Release -o /app

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# O nome da DLL deve ser exatamente o nome do projeto
ENTRYPOINT ["dotnet", "MarketPlacer.API.dll"]
