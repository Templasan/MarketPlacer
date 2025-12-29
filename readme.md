![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)
![Auth](https://img.shields.io/badge/Security-JWT-black)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)

# MarketPlacer

MarketPlacer é uma plataforma de marketplace completa, composta por um ecossistema robusto com **Back-end em .NET 8**, **Front-end em Angular 18** e base de dados **SQL Server**.

## 🚀 Tecnologias Utilizadas

* **Back-end:** .NET 8 API (C#) com Entity Framework Core.
* **Front-end:** Angular 18, Angular Material e Tailwind CSS.
* **Base de Dados:** Microsoft SQL Server 2022.
* **Infraestrutura:** Docker e Docker Compose.
* **Segurança:** Autenticação e Autorização via JWT (JSON Web Tokens).

---

## 🛠️ Como Executar o Projeto

### Opção 1: Via Docker (Recomendado)

Certifique-se de ter o Docker e o Docker Compose instalados. Na raiz do projeto, execute:

```bash
docker-compose up --build

```

Isso subirá três serviços:

1. **Banco de Dados (db):** SQL Server disponível na porta `1433`.
2. **API (api):** Back-end disponível em `http://localhost:5000`.
3. **Web (web):** Front-end disponível em `http://localhost:4200`.

### Opção 2: Execução Manual (CLI)

#### 1. Banco de Dados e Back-end

Para preparar o banco de dados (Wipe e Update):

```bash
dotnet ef database drop
dotnet ef database update

```

Para rodar a API:

```bash
dotnet run --project BACKEND/MarketPlacer.API

```

#### 2. Front-end

Navegue até a pasta `FRONTEND` e execute:

```bash
npm install
npm start

```

O portal estará disponível em `http://localhost:4200`.

---

## 📖 Documentação da API (Swagger)

A API possui documentação interativa via **Swagger**. Quando o projeto estiver em execução (ambiente de desenvolvimento), acesse:

* `http://localhost:5000/swagger`

A API exige um Token Bearer para rotas protegidas. Utilize a chave configurada para testes ou realize o login via `AuthController`.

---

## 📂 Estrutura de Pastas Relevante

* `/BACKEND`: Contém a lógica de negócio, repositórios e controladores da API.
* `/FRONTEND`: Aplicação Angular com os componentes de interface.
* `/logs`: Logs de execução da aplicação (mapeados via volume no Docker).
* `docker-compose.yml`: Orquestração dos containers.

---

## ⚙️ Configurações Importantes

* **JWT Key:** A chave mestra está definida no `Program.cs` para fins de desenvolvimento.
* **CORS:** A API está configurada com a política `AllowAll`, permitindo requisições de qualquer origem.
* **Imagens:** As imagens de produtos são servidas estaticamente através da pasta `wwwroot`.
