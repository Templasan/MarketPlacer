# Estágio 1: Build com Node 20
FROM node:20 AS build
WORKDIR /app

# 1. Instala as dependências (Caminho relativo ao context ./FRONTEND)
COPY package*.json ./
RUN npm install

# 2. Copia os arquivos e faz o build
COPY . .
RUN npm run build -- --configuration=production

# Estágio 2: Servir com Nginx
FROM nginx:alpine

# IMPORTANTE: No Angular 18, o caminho costuma ser dist/[nome-projeto]/browser
# Verifique se o nome no seu package.json é 'market-placer'
COPY --from=build /app/dist/market-placer/browser /usr/share/nginx/html

# Configuração do Nginx para Single Page Application (SPA)
RUN echo "server { listen 80; location / { root /usr/share/nginx/html; index index.html; try_files \$uri \$uri/ /index.html; } }" > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]