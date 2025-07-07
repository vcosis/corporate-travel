# Corporate Travel Backend

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

---

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

---

## Proxy Configuration

O sistema oferece duas opções de proxy:

- **proxy.conf.json**: Usado para desenvolvimento local com o Angular (`ng serve`). Redireciona chamadas da interface web para o backend, evitando problemas de CORS e facilitando o desenvolvimento.
- **proxy.docker.conf.json**: Usado quando o frontend está rodando em container Docker/Nginx, redirecionando as chamadas para o backend no ambiente de containers.

**Por que foi criado este proxy?**  
O proxy foi criado para permitir que o frontend (Angular) se comunique com o backend sem esbarrar em restrições de CORS, além de simplificar as URLs das chamadas HTTP no código do frontend. Isso garante que, tanto em desenvolvimento local quanto em ambiente Docker, as requisições sejam roteadas corretamente para a API, sem necessidade de alterar o código-fonte entre ambientes.

## Executando a aplicação

### Execução normal (sem seed)
```bash
dotnet run
```