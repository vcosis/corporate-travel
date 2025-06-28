# Frontend - Corporate Travel

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.0.3.

## Desenvolvimento Local

Para rodar o frontend localmente com proxy para o backend:

```bash
# Certifique-se de que o backend está rodando na porta 5178
npm run start:proxy
```

O frontend estará disponível em `http://localhost:4200` e fará proxy das requisições `/api/*` para `http://localhost:5178`.

## Docker

Para rodar no Docker, use:

```bash
docker-compose up frontend
```

O Dockerfile automaticamente usa a configuração `proxy.docker.conf.json` que aponta para o backend container.

## Configurações de Proxy

- **Desenvolvimento Local**: `proxy.conf.json` → `http://localhost:5178`
- **Docker**: `proxy.docker.conf.json` → `http://backend:8080`

## Estrutura de Arquivos

- `proxy.conf.json` - Configuração para desenvolvimento local
- `proxy.docker.conf.json` - Configuração para Docker
- `nginx.conf` - Configuração do nginx para produção no Docker

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
