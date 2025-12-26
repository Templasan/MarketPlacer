# Estágio 1: Build com Node 20
FROM node:20 AS build
WORKDIR /app

# Instala as dependências
COPY package*.json ./
RUN npm install

# Copia os arquivos e faz o build
COPY . .
RUN npm run build -- --configuration=production

# Estágio 2: Servir com Nginx
FROM nginx:alpine
# Ajuste 'market-placer' para o nome real do seu projeto se for diferente
COPY --from=build /app/dist/market-placer/browser /usr/share/nginx/html

RUN echo "server { listen 80; location / { root /usr/share/nginx/html; index index.html; try_files \$uri \$uri/ /index.html; } }" > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]