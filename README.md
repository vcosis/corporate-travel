# Corporate Travel Management System

## Visão geral da aplicação

O **Corporate Travel** é um sistema para gestão de solicitações de viagens corporativas, voltado para empresas que desejam centralizar, aprovar e acompanhar pedidos de viagem de seus colaboradores.  
Principais funcionalidades:
- Cadastro e autenticação de usuários com diferentes papéis (Admin, Manager, User)
- Solicitação, aprovação, reprovação e acompanhamento de pedidos de viagem
- Notificações em tempo real para aprovações e atualizações de status (via SignalR)
- Dashboard com estatísticas e indicadores de viagens
- Gerenciamento de usuários e permissões
- Histórico e filtros avançados de solicitações
- Interface web moderna e responsiva (Angular)

## Principais decisões arquiteturais

- **CQRS com MediatR:**  
  O padrão CQRS (Command Query Responsibility Segregation) foi adotado para separar operações de leitura e escrita, facilitando a manutenção, testes e escalabilidade.

- **SignalR para notificações em tempo real:**  
  SignalR foi escolhido para prover comunicação em tempo real entre backend e frontend, permitindo que usuários recebam atualizações instantâneas sobre o status de suas solicitações de viagem.  
  [Veja aqui a notificação funcionando.](docs/Funcionamento%20notifica%C3%A7%C3%A3o.gif)

- **ASP.NET Core Identity:**  
  Utilizado para autenticação e gerenciamento de usuários, garantindo segurança e flexibilidade na definição de papéis e permissões.

- **Entity Framework Core + PostgreSQL:**  
  O EF Core foi adotado para facilitar o mapeamento objeto-relacional e a manutenção do banco de dados, enquanto o PostgreSQL foi escolhido por sua robustez e compatibilidade com ambientes Docker.

- **Angular no frontend:**  
  O Angular foi escolhido para o frontend por sua maturidade, suporte a SPA, integração facilitada com APIs REST e ecossistema rico de componentes.

- **Docker e Docker Compose:**  
  Toda a stack é containerizada, facilitando o setup, testes e deploy em diferentes ambientes.

- **Outras decisões relevantes:**  
  - Uso de AutoMapper para simplificar o mapeamento entre entidades e DTOs.
  - Serviços e injeção de dependência para promover baixo acoplamento e testabilidade.
  - Padrão de repositório para abstração do acesso a dados.

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


- **proxy.conf.json**: Usado para desenvolvimento local com o Angular (`ng serve`). Redireciona chamadas da interface web para o backend, evitando problemas de CORS e facilitando o desenvolvimento.
- **proxy.docker.conf.json**: Usado quando o frontend está rodando em container Docker/Nginx, redirecionando as chamadas para o backend no ambiente de containers.

**Por que foi criado este proxy?**  
O proxy foi criado para permitir que o frontend (Angular) se comunique com o backend sem esbarrar em restrições de CORS, além de simplificar as URLs das chamadas HTTP no código do frontend. Isso garante que, tanto em desenvolvimento local quanto em ambiente Docker, as requisições sejam roteadas corretamente para a API, sem necessidade de alterar o código-fonte entre ambientes.

## 👥 Usuários Padrão

Após executar o seed do banco de dados:

- **Admin**: admin@corporatetravel.com / Admin123!
- **Manager**: manager@corporatetravel.com / Manager123!  
- **User**: user@corporatetravel.com / User123!

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