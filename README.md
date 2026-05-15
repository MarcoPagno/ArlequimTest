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
| Docker | 29.x | Containerização |

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

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
    "invoiceNumber": "NF-2026-001"
  }'
```

**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "productName": "Notebook",
  "quantity": 50,
  "invoiceNumber": "NF-2026-001",
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
| POST | `/api/users` | Nenhuma | Cadastrar usuário |
| POST | `/api/users/login` | Nenhuma | Login e obtenção do token |
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
| GET | `/api/stock/{productName}` | Qualquer | Consultar estoque disponível |

### Pedidos
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| POST | `/api/orders` | Qualquer | Emitir pedido |

---

## Regras de Negócio

- E-mail de usuário deve ser único
- Senha deve ter no mínimo 6 caracteres
- Nome de produto deve ser único
- Estoque só pode ser adicionado por Administradores, com número de nota fiscal obrigatório
- Pedidos com quantidade superior ao estoque disponível são rejeitados
- O preço unitário do produto é registrado no momento da venda
- Apenas Administradores podem cadastrar, editar e excluir produtos