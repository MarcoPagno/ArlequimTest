# ArlequimTest API

REST API desenvolvida em **ASP.NET Core 9** com autenticação JWT, testes unitários e de integração, e suporte a Docker.

---

## Tecnologias e Bibliotecas

| Tecnologia | Versão | Descrição |
|---|---|---|
| .NET SDK | 9.0 | Framework principal |
| ASP.NET Core | 9.0 | Framework HTTP |
| xUnit | 2.x | Testes unitários e de integração |
| Microsoft.AspNetCore.Mvc.Testing | 9.0 | Testes de integração com WebApplicationFactory |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0 | Autenticação via JWT |
| BCrypt.Net-Next | 4.x | Hash de senhas |
| OpenTelemetry | 1.x | Tracing e APM |
| Jaeger | 1.65 | Dashboard de visualização de traces |
| Docker | 29.x | Containerização |

---

## Decisões Técnicas

### Persistência em memória

Optou-se por persistência em memória (`List<T>` estática) em vez de um banco de dados relacional. O desafio não especifica banco de dados, e a adição de PostgreSQL ou SQL Server introduziria configuração de connection string, migrations e setup adicional no Docker sem agregar valor à avaliação das funcionalidades em si. Em um ambiente de produção, a substituição seria direta — os Services são a única camada que precisaria ser alterada, mantendo Controllers e DTOs intactos. A escolha natural seria PostgreSQL com Entity Framework Core.

### Testes de integração além dos unitários

O desafio solicita testes unitários. Optou-se por adicionar também testes de integração usando `WebApplicationFactory`, que sobem a API em memória e realizam requisições HTTP reais contra ela. Isso garante que o fluxo completo — autenticação, autorização, validação e resposta — funciona de ponta a ponta, não apenas a lógica isolada de cada Service.

### Exceptions de domínio personalizadas

Em vez de lançar `Exception` genérica ou retornar objetos de erro manualmente, foi criada uma hierarquia de exceptions com HTTP status code embutido. Cada tipo de erro tem sua própria classe:

| Exception | Status HTTP | Uso |
|---|---|---|
| `ValidationError` | 400 | Dados inválidos na requisição |
| `UnauthorizedError` | 401 | Usuário não autenticado |
| `ConflictError` | 409 | Recurso já existe (e-mail, nome de produto) |
| `NotFoundError` | 404 | Recurso não encontrado |
| `MethodNotAllowedError` | 405 | Método HTTP não permitido |
| `InternalServerError` | 500 | Erro inesperado interno |

Os Controllers capturam apenas `AppException` (classe base) e delegam o status code para a própria exception, mantendo o código de tratamento de erro centralizado e sem repetição.

### Status de pedido não implementado

O H5 não especifica um fluxo de aprovação ou cancelamento de pedidos. Adicionar status (`Pending`, `Confirmed`, `Cancelled`) sem uma regra de negócio clara seria over-engineering — o pedido é confirmado diretamente na criação, com baixa de estoque imediata. Caso o negócio evolua para um fluxo de aprovação, o campo `Status` pode ser adicionado ao model `Order` sem impacto nas outras entidades.

### Tracing com OpenTelemetry e Jaeger

Implementado como diferencial do desafio. O OpenTelemetry instrumenta automaticamente todas as requisições HTTP, registrando tempo de resposta e metadados de cada endpoint. O Jaeger coleta e exibe esses dados em um dashboard visual acessível em `http://localhost:16686`. Isso permite identificar gargalos de performance — por exemplo, o endpoint de login é visivelmente mais lento por conta do BCrypt, que é intencionalmente custoso para dificultar ataques de força bruta.

---

## Pré-requisitos

[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
[Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Executando a aplicação

### Localmente

```bash
# Clone o repositório
git clone https://github.com/MarcoPagno/ArlequimTest.git
cd ArlequimTest

# Restaura as dependências
dotnet restore

# Executa a API
cd ArlequimTest.Api
dotnet run
```

A API estará disponível em `http://localhost:5000`.

### Via Docker

```bash
# Na raiz do projeto (onde está o docker-compose.yml)
docker compose up --build
```

A API estará disponível em `http://localhost:8080`.

O dashboard de tracing Jaeger estará disponível em `http://localhost:16686`.

---

## Executando os testes

### Localmente

```bash
# Na raiz do projeto
dotnet test
```

### Via Docker

O container da API usa apenas o runtime do .NET, sem o SDK necessário para compilar e rodar testes. Para rodar os testes via Docker, utilize o serviço dedicado que usa a imagem com SDK completo:

```bash
docker compose run --rm tests
```

Os testes unitários e de integração rodam juntos. O resultado mostra cada teste com passou ou falhou.

---

## Fluxo completo da aplicação

A seguir, os passos para percorrer o fluxo completo: cadastro de usuários, produtos, estoque e pedido.

> Todos os exemplos usam `curl`. Substitua o `TOKEN` pelo token retornado no login.

---

### 1. Cadastrar um Administrador

```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Admin",
    "email": "admin@email.com",
    "password": "123456",
    "role": "Admin"
  }'
```

**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "name": "Admin",
  "email": "admin@email.com",
  "role": "Admin"
}
```

---

### 2. Cadastrar um Vendedor

```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Vendedor",
    "email": "vendedor@email.com",
    "password": "123456",
    "role": "Seller"
  }'
```

**Resposta esperada:** `201 Created`

---

### 3. Login como Administrador

```bash
curl -X POST http://localhost:8080/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@email.com",
    "password": "123456"
  }'
```

**Resposta esperada:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

> Guarde o token — ele será necessário nas próximas requisições como Admin.

---

### 4. Cadastrar um Produto (requer Admin)

```bash
curl -X POST http://localhost:8080/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "name": "Notebook",
    "description": "Notebook 16GB RAM",
    "price": 4999.99
  }'
```

**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "name": "Notebook",
  "description": "Notebook 16GB RAM",
  "price": 4999.99
}
```

---

### 5. Adicionar Estoque ao Produto (requer Admin)

```bash
curl -X POST http://localhost:8080/api/stock \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "productName": "Notebook",
    "quantity": 50,
    "invoiceNumber": "2026000000123"
  }'
```

**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "productName": "Notebook",
  "quantity": 50,
  "invoiceNumber": "2026000000123",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

---

### 6. Consultar Estoque Disponível

```bash
curl http://localhost:8080/api/stock/Notebook
```

**Resposta esperada:** `200 OK`
```json
{
  "productName": "Notebook",
  "availableStock": 50
}
```

---

### 7. Login como Vendedor

```bash
curl -X POST http://localhost:8080/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "vendedor@email.com",
    "password": "123456"
  }'
```

> Guarde o token do Vendedor para o próximo passo.

---

### 8. Realizar um Pedido (requer autenticação)

```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN_VENDEDOR" \
  -d '{
    "customerDocument": "123.456.789-00",
    "sellerName": "Vendedor",
    "items": [
      {
        "productName": "Notebook",
        "quantity": 2
      }
    ]
  }'
```

**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "customerDocument": "123.456.789-00",
  "sellerName": "Vendedor",
  "createdAt": "2026-01-01T00:00:00Z",
  "items": [
    {
      "productName": "Notebook",
      "quantity": 2,
      "unitPrice": 4999.99
    }
  ]
}
```

> Após o pedido, o estoque do produto é atualizado automaticamente. Uma nova consulta em `GET /api/stock/Notebook` retornará `availableStock: 48`.

---

## Endpoints

### Usuários
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/api/users` | Pública | Cadastrar usuário |
| POST | `/api/users/login` | Pública | Login e obtenção do token |
| GET | `/api/users/me` | Qualquer | Retorna o usuário autenticado atual |

### Produtos
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/api/products` | Admin | Cadastrar produto |
| GET | `/api/products` | Pública | Listar produtos |
| GET | `/api/products/{name}` | Pública | Consultar produto por nome |
| PATCH | `/api/products/{name}` | Admin | Atualizar produto |
| DELETE | `/api/products/{name}` | Admin | Excluir produto |

### Estoque
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/api/stock` | Admin | Adicionar estoque |
| GET | `/api/stock/{productName}` | Autenticado | Consultar estoque disponível |

### Pedidos
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/api/orders` | Autenticado | Emitir pedido |
| GET | `/api/orders` | Autenticado | Listar pedidos |