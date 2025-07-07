# Corporate Travel Backend

## Executando a aplicação

### Execução normal (sem seed)
```bash
dotnet run
```

### Execução com seed do banco de dados
```bash
dotnet run --seed
```
ou
```bash
dotnet run -s
```

### Execução com Docker Compose (Recomendado)
```bash
docker-compose up -d
```

Isso irá iniciar:
- PostgreSQL (porta 5432)
- Seq - Log Aggregation (porta 5341)
- Backend API (porta 5178)

## Parâmetros disponíveis

- `--seed` ou `-s`: Executa o seed do banco de dados, criando usuários padrão e dados de exemplo
  - Admin: admin@corporatetravel.com / Admin123!
  - Manager: manager@corporatetravel.com / Manager123!
  - User: user@corporatetravel.com / User123!

## Configuração

### Variáveis de Ambiente

O projeto utiliza as seguintes variáveis de ambiente:

- `ASPNETCORE_ENVIRONMENT`: Define o ambiente (Development, Production, etc.)
- `ConnectionStrings__DefaultConnection`: String de conexão com o banco de dados PostgreSQL
- `Jwt__Key`: Chave secreta para assinatura dos tokens JWT
- `Jwt__Issuer`: Emissor do token JWT
- `Jwt__Audience`: Audiência do token JWT

### Configuração do Banco de Dados

O projeto utiliza PostgreSQL como banco de dados principal. A string de conexão deve seguir o formato:

```
Host=localhost;Port=5432;Database=corporatetravel;Username=postgres;Password=postgres
```

### Configuração JWT

As configurações JWT estão definidas no `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "a-very-secret-key-that-is-long-enough-and-should-be-in-secrets",
    "Issuer": "CorporateTravel.API",
    "Audience": "CorporateTravel.Users"
  }
}
```

## Estrutura do Projeto

### Camadas da Aplicação

- **API**: Controllers e configuração da aplicação
- **Application**: Casos de uso, DTOs e interfaces
- **Domain**: Entidades e regras de negócio
- **Infrastructure**: Implementação de repositórios e serviços externos

### Padrões Utilizados

- **CQRS**: Separação de comandos e consultas
- **MediatR**: Implementação do padrão mediator
- **Repository Pattern**: Abstração do acesso a dados
- **Identity**: Autenticação e autorização
- **JWT**: Tokens para autenticação stateless

## Logging

O projeto utiliza Serilog para logging estruturado com as seguintes configurações:

- Console logging para desenvolvimento
- File logging com rotação diária
- Seq para agregação de logs (opcional)
- Enriquecimento com contexto de ambiente e thread

### Pacotes Serilog Utilizados

- Serilog.AspNetCore
- Serilog.Settings.Configuration
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Sinks.Seq
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Thread 

## 👥 Usuários Padrão

Após executar o seed do banco de dados:

- **Admin**: admin@corporatetravel.com / Admin123!
- **Manager**: manager@corporatetravel.com / Manager123!
- **User**: user@corporatetravel.com / User123!