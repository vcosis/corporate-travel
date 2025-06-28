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

## Parâmetros disponíveis

- `--seed` ou `-s`: Executa o seed do banco de dados, criando usuários padrão e dados de exemplo
  - Admin: admin@corporatetravel.com / Admin@123
  - Manager: manager@corporatetravel.com / Manager@123
  - User: user@corporatetravel.com / User@123

## Configuração

Certifique-se de que o banco de dados está configurado e acessível antes de executar o seed.

O seed irá:
1. Criar os roles: Admin, Manager, User
2. Criar usuários padrão com suas respectivas roles
3. Criar solicitações de viagem de exemplo para cada usuário 

## Logging com Serilog

Este projeto utiliza o Serilog para logging estruturado com configuração baseada em `appsettings.json`.

### Configuração

O Serilog é configurado através dos arquivos `appsettings.json`:

#### appsettings.json (Configuração Base)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/corporate-travel-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithEnvironmentName", "WithThreadId"]
  }
}
```

#### appsettings.Development.json
- **Nível de log**: Information/Debug para desenvolvimento
- **Console**: Habilitado com formatação detalhada
- **Arquivo**: Logs salvos em `logs/corporate-travel-{data}.txt`
- **Seq**: Logs enviados para análise estruturada

#### appsettings.Production.json
- **Nível de log**: Warning para reduzir volume
- **Console**: Desabilitado
- **Arquivo**: Com limite de tamanho e rotação
- **Seq**: Para monitoramento em produção

### Middleware de Logging

O projeto inclui um middleware personalizado (`RequestLoggingMiddleware`) que captura:
- Todas as requisições HTTP
- Tempo de resposta
- Status codes
- IP do cliente
- Método HTTP e path

### Níveis de Log

- **Information**: Eventos gerais da aplicação
- **Warning**: Avisos e situações que merecem atenção
- **Error**: Erros que não impedem a execução
- **Fatal**: Erros críticos que causam falha na aplicação
- **Debug**: Informações detalhadas para desenvolvimento

### Uso

```csharp
// Injeção do logger
private readonly ILogger _logger = Log.ForContext<MinhaClasse>();

// Exemplos de uso
_logger.Information("Aplicação iniciada");
_logger.Warning("Token expirado para usuário {UserId}", userId);
_logger.Error(ex, "Erro ao processar requisição");
_logger.Debug("Dados de debug: {Dados}", dados);
```

### Configuração de Ambiente

- **Development**: Logs detalhados no console e arquivo
- **Production**: Logs estruturados para Seq e arquivo

### Dependências

- Serilog.AspNetCore
- Serilog.Settings.Configuration
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Sinks.Seq
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Thread 