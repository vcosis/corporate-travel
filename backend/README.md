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

Este projeto utiliza o Serilog para logging estruturado com as seguintes funcionalidades:

### Configuração

- **Console**: Logs formatados no console para desenvolvimento
- **Arquivo**: Logs salvos em `logs/corporate-travel-{data}.txt` com rotação diária
- **Seq**: Logs enviados para Seq (http://localhost:5341) para análise estruturada

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
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Sinks.Seq
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Thread 