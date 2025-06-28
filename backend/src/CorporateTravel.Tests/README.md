# CorporateTravel.Tests

Este projeto contém os testes unitários para o backend da aplicação CorporateTravel, utilizando xUnit, Moq e FluentAssertions.

## Estrutura do Projeto

```
CorporateTravel.Tests/
├── Features/
│   ├── Authentication/
│   │   └── Commands/
│   │       ├── LoginUser/
│   │       │   └── LoginUserCommandHandlerTests.cs
│   │       └── RegisterUser/
│   │           └── RegisterUserCommandHandlerTests.cs
│   └── TravelRequests/
│       ├── Commands/
│       │   ├── ApproveTravelRequest/
│       │   │   └── ApproveTravelRequestCommandHandlerTests.cs
│       │   └── CreateTravelRequest/
│       │       └── CreateTravelRequestCommandHandlerTests.cs
│       └── Queries/
│           └── GetAllTravelRequests/
│               └── GetAllTravelRequestsQueryHandlerTests.cs
└── Services/
    └── TokenServiceTests.cs
```

## Tecnologias Utilizadas

- **xUnit**: Framework de testes unitários
- **Moq**: Framework para criação de mocks
- **FluentAssertions**: Biblioteca para asserções mais legíveis
- **Microsoft.AspNetCore.Identity**: Para testes relacionados à autenticação
- **AutoMapper**: Para testes de mapeamento
- **MediatR**: Para testes de handlers de comandos e queries

## Testes Implementados

### 1. LoginUserCommandHandlerTests
Testa o handler responsável pelo login de usuários:
- ✅ Login com credenciais válidas
- ✅ Login com email inválido
- ✅ Login com senha inválida
- ✅ Login com múltiplas roles
- ✅ Tratamento de email nulo

### 2. RegisterUserCommandHandlerTests
Testa o handler responsável pelo registro de usuários:
- ✅ Registro com dados válidos
- ✅ Registro com role vazia (deve usar "User" como padrão)
- ✅ Registro com role nula (deve usar "User" como padrão)
- ✅ Registro com role customizada
- ✅ Falha na criação do usuário
- ✅ Falha na atribuição de role
- ✅ Email duplicado
- ✅ Diferentes roles (Manager, Admin, User)

### 3. CreateTravelRequestCommandHandlerTests
Testa o handler responsável pela criação de requisições de viagem:
- ✅ Criação com comando válido
- ✅ Configuração correta de status e data de criação
- ✅ Envio de notificação com parâmetros corretos
- ✅ Falha no repositório
- ✅ Falha no serviço de notificação

### 4. ApproveTravelRequestCommandHandlerTests
Testa o handler responsável pela aprovação de requisições de viagem:
- ✅ Aprovação de requisição pendente
- ✅ Requisição não encontrada
- ✅ Requisição já aprovada
- ✅ Requisição rejeitada
- ✅ Requisição cancelada
- ✅ Falha no repositório
- ✅ Falha na atualização
- ✅ Diferentes status (teoria)

### 5. GetAllTravelRequestsQueryHandlerTests
Testa o handler responsável pela listagem de requisições de viagem:
- ✅ Usuário Admin (todas as requisições)
- ✅ Usuário Manager (todas as requisições)
- ✅ Usuário regular (apenas suas requisições)
- ✅ Usuário com múltiplas roles
- ✅ Filtros aplicados corretamente
- ✅ Resultado vazio
- ✅ Diferentes roles (teoria)

### 6. TokenServiceTests
Testa o serviço de geração e validação de tokens JWT:
- ✅ Geração de token com usuário válido
- ✅ Geração de token com múltiplas roles
- ✅ Inclusão de claims do usuário
- ✅ Tratamento de email nulo
- ✅ Validação de token válido
- ✅ Validação de token inválido
- ✅ Validação de token expirado
- ✅ Validação com issuer incorreto
- ✅ Validação com audience incorreto
- ✅ Geração de token sem roles

## Como Executar os Testes

### Via Visual Studio
1. Abra a solution `CorporateTravel.sln`
2. No Test Explorer, clique em "Run All Tests"

### Via Linha de Comando
```bash
# Navegar para o diretório do backend
cd backend

# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "FullyQualifiedName~LoginUserCommandHandlerTests"
```

### Via Docker
```bash
# Executar testes no container
docker-compose exec backend dotnet test
```

## Padrões de Teste Utilizados

### 1. Arrange-Act-Assert (AAA)
Todos os testes seguem o padrão AAA:
- **Arrange**: Preparação dos dados e mocks
- **Act**: Execução da ação sendo testada
- **Assert**: Verificação dos resultados

### 2. Nomenclatura de Testes
Os testes seguem o padrão: `[MethodName]_[Scenario]_[ExpectedResult]`

Exemplo: `Handle_WithValidCredentials_ShouldReturnAuthResponse`

### 3. Mocks
- Uso extensivo do Moq para simular dependências
- Verificação de chamadas de métodos mockados
- Setup de retornos específicos para diferentes cenários

### 4. Asserções
- Uso do FluentAssertions para asserções mais legíveis
- Verificação de propriedades específicas
- Verificação de exceções quando apropriado

## Cobertura de Testes

Os testes cobrem:
- ✅ Handlers de comandos (CQRS)
- ✅ Handlers de queries (CQRS)
- ✅ Serviços de aplicação
- ✅ Validação de regras de negócio
- ✅ Tratamento de erros
- ✅ Cenários de sucesso e falha

## Próximos Passos

Para expandir a cobertura de testes, considere adicionar:

1. **Testes de Integração**
   - Testes de controllers
   - Testes de repositórios com banco de dados em memória
   - Testes de pipeline de validação

2. **Testes de Serviços de Infraestrutura**
   - NotificationService
   - RequestCodeService
   - NotificationHub

3. **Testes de Validação**
   - Validators de comandos
   - Validação de DTOs

4. **Testes de Configuração**
   - DependencyInjection
   - Configuração de AutoMapper

## Contribuindo

Ao adicionar novos testes:
1. Siga os padrões estabelecidos
2. Use nomes descritivos para os testes
3. Cubra cenários de sucesso e falha
4. Documente casos especiais ou complexos
5. Execute todos os testes antes de fazer commit 