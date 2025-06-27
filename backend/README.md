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