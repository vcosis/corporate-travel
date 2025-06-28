# Corporate Travel Management System

Sistema completo de gerenciamento de viagens corporativas com backend .NET Core e frontend Angular.

## 🚀 Quick Start

### Pré-requisitos
- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento local)
- Node.js 18+ (para desenvolvimento local)

### Opção 1: Execução com Docker Compose (Nginx Proxy)

```bash
# Clone o repositório
git clone <repository-url>
cd corporate-travel

# Inicie todos os serviços
docker-compose up -d
```

Isso irá iniciar:
- **PostgreSQL** (porta 5432) - Banco de dados
- **Seq** (porta 5341) - Log aggregation e análise
- **Backend API** (porta 5178) - API .NET Core
- **Frontend com Nginx** (porta 4200) - Aplicação Angular com proxy reverso

### Opção 2: Desenvolvimento Local (Angular CLI Proxy)

```bash
# Inicie apenas o backend e banco
docker-compose up -d postgres backend seq

# Em outro terminal, execute o frontend localmente
cd frontend
npm install
npm run start:proxy
```

### Acessos

#### Com Docker Compose (Nginx):
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:4200/api
- **Seq Logs**: http://localhost:5341
- **PostgreSQL**: localhost:5432

#### Com Desenvolvimento Local (Angular CLI):
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:4200/api (via proxy)
- **Seq Logs**: http://localhost:5341
- **PostgreSQL**: localhost:5432

## 📁 Estrutura do Projeto

```
corporate-travel/
├── backend/                 # API .NET Core
│   ├── src/
│   │   ├── CorporateTravel.API/      # Web API
│   │   ├── CorporateTravel.Application/ # Lógica de negócio
│   │   ├── CorporateTravel.Domain/   # Entidades e domínio
│   │   ├── CorporateTravel.Infrastructure/ # Infraestrutura
│   │   └── CorporateTravel.Tests/    # Testes unitários
│   └── Dockerfile
├── frontend/               # Aplicação Angular
│   ├── src/
│   │   ├── app/           # Componentes e serviços
│   │   ├── environments/  # Configurações de ambiente
│   │   └── theme/         # Temas e estilos
│   ├── Dockerfile         # Build do frontend
│   ├── nginx.conf         # Configuração do proxy reverso
│   ├── proxy.conf.json    # Configuração do proxy Angular CLI
│   └── package.json
├── docker-compose.yml     # Orquestração dos serviços
└── README.md
```

## 🔧 Desenvolvimento Local

### Backend (.NET Core)

```bash
cd backend/src/CorporateTravel.API

# Instalar dependências
dotnet restore

# Executar com seed do banco
dotnet run --seed

# Executar testes
dotnet test
```

### Frontend (Angular)

```bash
cd frontend

# Instalar dependências
npm install

# Executar em modo desenvolvimento (com proxy)
npm run start:proxy

# Executar testes
ng test
```

## 🌐 Proxy Configuration

O sistema oferece duas opções de proxy:

### Opção 1: Nginx Proxy (Docker)
- **Arquivo**: `frontend/nginx.conf`
- **Uso**: `docker-compose up -d`
- **Vantagens**: Produção-ready, rate limiting, cache, segurança

### Opção 2: Angular CLI Proxy (Desenvolvimento)
- **Arquivo**: `frontend/proxy.conf.json`
- **Uso**: `npm run start:proxy`
- **Vantagens**: Simples, hot reload, debug fácil

### Configuração do Proxy Angular CLI
```json
{
  "/api": {
    "target": "http://localhost:5178",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  },
  "/notificationhub": {
    "target": "http://localhost:5178",
    "secure": false,
    "changeOrigin": true,
    "ws": true,
    "logLevel": "debug"
  }
}
```

## 👥 Usuários Padrão

Após executar o seed do banco de dados:

- **Admin**: admin@corporatetravel.com / Admin@123
- **Manager**: manager@corporatetravel.com / Manager@123  
- **User**: user@corporatetravel.com / User@123

## 📊 Logging e Monitoramento

### Serilog Configuration

O sistema utiliza Serilog para logging estruturado com:

- **Console**: Logs formatados para desenvolvimento
- **File**: Logs persistentes com rotação diária
- **Seq**: Análise estruturada e dashboards

### Seq Dashboard

Acesse http://localhost:5341 para:
- Visualizar logs em tempo real
- Criar filtros e dashboards
- Configurar alertas
- Analisar performance

## 🛠️ Tecnologias

### Backend
- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **Serilog** - Logging estruturado
- **SignalR** - Comunicação em tempo real
- **JWT** - Autenticação

### Frontend
- **Angular 17** - Framework SPA
- **Angular Material** - Componentes UI
- **RxJS** - Programação reativa
- **SignalR Client** - Comunicação em tempo real

### DevOps
- **Docker** - Containerização
- **Docker Compose** - Orquestração
- **Nginx** - Proxy reverso
- **Seq** - Log aggregation

## 🔒 Segurança

- Autenticação JWT
- Autorização baseada em roles
- CORS configurado
- Validação de entrada
- Logging de auditoria
- Rate limiting (Nginx)
- Headers de segurança

## 📝 API Documentation

A API inclui endpoints para:
- Autenticação e autorização
- Gerenciamento de usuários
- Solicitações de viagem
- Notificações em tempo real
- Dashboard e relatórios

## 🧪 Testes

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
ng test
```

## 📦 Deploy

### Com Docker Compose
```bash
# Build e deploy com Docker
docker-compose up -d
```

### Desenvolvimento Local
```bash
# Backend e banco
docker-compose up -d postgres backend seq

# Frontend local
cd frontend && npm run start:proxy
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes. 