# Corporate Travel Management System

Sistema completo de gerenciamento de viagens corporativas com backend .NET Core e frontend Angular.

## 🚀 Quick Start

### Pré-requisitos
- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento local)
- Node.js 18+ (para desenvolvimento local)

### Execução com Docker Compose (Recomendado)

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
- **Frontend** (porta 4200) - Aplicação Angular

### Acessos

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5178
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
│   └── package.json
└── docker-compose.yml     # Orquestração dos serviços
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

# Executar em modo desenvolvimento
ng serve

# Executar testes
ng test
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
- **Seq** - Log aggregation

## 🔒 Segurança

- Autenticação JWT
- Autorização baseada em roles
- CORS configurado
- Validação de entrada
- Logging de auditoria

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

### Produção
```bash
# Build e deploy com Docker
docker-compose -f docker-compose.prod.yml up -d
```

### Desenvolvimento
```bash
# Execução local
docker-compose up -d
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes. 