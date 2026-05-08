# Nexas API - Guia Completo do Projeto

Este documento serve como um mapa completo do projeto Nexas API para desenvolvedores e Assistentes de Inteligência Artificial. Ele descreve a arquitetura, as tecnologias utilizadas e as regras de como o código está organizado. O objetivo é evitar a leitura completa do código para entender o contexto do projeto, servindo como uma base de conhecimento rápida e eficiente.

## Resumo do Projeto

O **Nexas API** é uma aplicação backend desenvolvida em **.NET** (C#) voltada para o gerenciamento de cursos (Learning Management System - LMS) ou uma plataforma similar. O sistema gerencia entidades como **Usuários**, **Cursos**, **Módulos**, **Aulas**, **Matrículas (Enrollments)** e **Assinaturas (Subscriptions)**.

O projeto utiliza a **Clean Architecture (Arquitetura Limpa)**, separando as responsabilidades em camadas bem definidas e independentes. Também adota o padrão **CQRS** (Command Query Responsibility Segregation) utilizando a biblioteca **MediatR**.

### Stack Tecnológica e Padrões:
* **Linguagem / Framework:** C# / .NET (ASP.NET Core Web API)
* **Arquitetura:** Clean Architecture
* **Padrões:** CQRS (MediatR), Injeção de Dependência
* **Banco de Dados:** MySQL (utilizando Entity Framework Core / Pomelo)
* **Validação:** FluentValidation (Integrado via Pipeline Behavior do MediatR)
* **Autenticação:** JWT Bearer (Integração com provedor externo, como o *Agivys*)
* **Documentação:** Swagger / OpenAPI

---

## Estrutura de Diretórios e Camadas

A solução `Nexas.sln` está dividida em 4 projetos (camadas), do mais externo (depende de todos) para o mais interno (não depende de ninguém):

### 1. `Nexas.Api` (Camada de Apresentação)
É o ponto de entrada da aplicação (Entrypoint). Esta camada é responsável por expor a API RESTful e lidar com as requisições HTTP. Não deve conter regras de negócio, apenas orquestrar as chamadas para a camada de Application.

* **`/Controllers/`**: Controladores REST que recebem as requisições, enviam Comandos/Queries para o MediatR e retornam as respostas (Ex: `CoursesController`, `MeController`).
* **`/Extensions/`**: Métodos de extensão para configuração de serviços no `Program.cs` (Ex: `AuthenticationExtensions`, `SwaggerExtensions`).
* **`/Middlewares/`**: Middlewares customizados para o pipeline HTTP (Ex: Tratamento global de exceções - `GlobalExceptionHandler`).
* **`/Services/`**: Implementação de serviços da API, como o `CurrentUserService` (que extrai informações do usuário logado via `HttpContext`).
* **`Program.cs`**: Arquivo de inicialização, configuração de CORS, injeção de dependências (`AddApplication`, `AddInfrastructure`), setup do Swagger e do pipeline HTTP.
* **`appsettings.json`**: Arquivos de configuração de ambiente (Connection Strings, chaves JWT, etc).

### 2. `Nexas.Application` (Camada de Aplicação / Casos de Uso)
Contém a lógica de aplicação e os casos de uso do sistema. Orquestra a execução de tarefas utilizando as entidades do domínio, mas sem depender de tecnologias de banco de dados ou frameworks web.

* **`/Common/`**: Interfaces comuns (`INexasDbContext`, `ICurrentUserService`), Behaviors do MediatR (`ValidationBehavior`, `LoggingBehavior`) e serviços utilitários da aplicação.
* **`/{Funcionalidade}/` (Ex: `/Courses/`)**: O código é organizado por *Features* (Funcionalidades).
    * **`/Commands/`**: Ações que alteram o estado do sistema (Criar, Atualizar, Deletar). Cada comando geralmente tem a classe do Comando em si e o seu *Handler* (quem executa a ação).
    * **`/Queries/`**: Ações que apenas consultam o estado do sistema (Buscar, Listar).
* **Validações**: Classes de validação utilizando `FluentValidation` devem ser colocadas próximas aos Commands/Queries que validam. O `ValidationBehavior` intercepta as chamadas e roda a validação automaticamente.
* **`DependencyInjection.cs`**: Configuração do MediatR e FluentValidation para o projeto de Application.

### 3. `Nexas.Domain` (Camada de Domínio)
O coração do sistema. Contém as regras de negócio puras e as entidades. Não tem dependência de *nenhum* outro projeto ou framework (exceto as bibliotecas base do .NET).

* **`/Entities/`**: As classes que representam o modelo de negócio: `User`, `Course`, `Module`, `Lesson`, `Enrollment`, `Subscription`.
* **`/Constants/`**: Constantes usadas em todo o sistema.
* **`/Common/`**: Classes base para entidades, value objects, exceções de domínio.

### 4. `Nexas.Infrastructure` (Camada de Infraestrutura)
Responsável pelas implementações técnicas (Banco de dados, envio de e-mails, integrações externas). Implementa as interfaces definidas na camada de Application.

* **`/Persistence/`**: Configuração do Entity Framework Core.
    * **`NexasDbContext.cs`**: O contexto principal do banco de dados MySQL, expondo as tabelas (DbSets).
    * **`/Configurations/`**: Mapeamentos fluentes (Fluent API) para configurar as tabelas no banco (tipos de coluna, chaves primárias, relacionamentos).
* **`DependencyInjection.cs`**: Configura o EF Core (`UseMySql`) e registra os repositórios ou o `DbContext` na injeção de dependência.

---

## 🤖 Guia Prático para a Inteligência Artificial

Quando for solicitado para criar ou alterar algo neste projeto, siga estas regras:

1. **Adicionar uma Nova Funcionalidade (Endpoint):**
   * **Domain:** Se for uma entidade nova, crie-a em `Nexas.Domain/Entities`.
   * **Application:** Crie uma nova pasta em `Nexas.Application` com o nome da feature (ex: `/Subscriptions`). Dentro, crie subpastas `/Commands/` ou `/Queries/`.
   * Crie o `Command`/`Query` correspondente, seu `Handler` e a classe do `Validator` (FluentValidation) no mesmo arquivo ou na mesma pasta.
   * **Api:** Crie um Controller em `Nexas.Api/Controllers` que injeta o `IMediator` e envia o Command/Query, mapeando a rota REST (ex: `[HttpPost]`).

2. **Alterar Banco de Dados (Entity Framework):**
   * Modifique a entidade correspondente em `Nexas.Domain/Entities`.
   * Atualize ou crie o arquivo de configuração da entidade (Fluent API) em `Nexas.Infrastructure/Persistence/Configurations`.
   * **ATENÇÃO:** O Contexto (`NexasDbContext`) fica no projeto de Infraestrutura. Caso precise rodar `Add-Migration`, certifique-se de referenciar o projeto de `Nexas.Infrastructure` mas com a inicialização no `Nexas.Api`.

3. **Validações:**
   * Nunca faça validações manuais nos Controllers. Crie uma classe herdando de `AbstractValidator<TCommand>` na camada `Application`. O pipeline behavior do MediatR configurado em `DependencyInjection.cs` da Application cuidará de interceptar e lançar exceções automaticamente.

4. **Tratamento de Exceções:**
   * Não use blocos `try-catch` nos Controllers. A API possui um Middleware Global (`UseGlobalExceptionHandler`) que cuida disso e converte as exceções para o formato padrão do problema HTTP (Problem Details). Exceções de negócio devem ser lançadas na camada Domain ou Application.

5. **Usuário Logado:**
   * Para pegar dados do usuário autenticado no JWT (ex: ID do usuário), **NÃO** leia o `HttpContext` diretamente na camada `Application`.
   * Em vez disso, injete a interface `IUserContextService` (ou `ICurrentUserService`) que já está mapeada para fornecer esses dados com segurança para qualquer camada.

Este README deve ser o ponto de partida para qualquer alteração no sistema, poupando a leitura de múltiplos arquivos para entender a estrutura.
