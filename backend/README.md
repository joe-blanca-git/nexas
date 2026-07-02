# Nexas - Plataforma de Cursos Online

**Nexas** é uma plataforma de gerenciamento de cursos online construída com **ASP.NET Core 8.0** utilizando **Clean Architecture**, **CQRS (Command Query Responsibility Segregation)** e **MediatR** para orquestração de requisições.

O sistema foi projetado com foco em **escalabilidade**, **manutenibilidade** e **padrões SOLID**, facilitando a adição de novas funcionalidades e integrações (como Bunny.net para armazenamento de vídeos).

---

## 📋 Índice

- [Visão Geral da Arquitetura](#visão-geral-da-arquitetura)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Entidades e Relacionamentos](#entidades-e-relacionamentos)
- [Campos Bunny.net](#campos-bunnynet)
- [Fluxo Completo de Uma Requisição](#fluxo-completo-de-uma-requisição)
- [Padrões Obrigatórios](#padrões-obrigatórios)
- [Autenticação e Autorização](#autenticação-e-autorização)
- [Banco de Dados e Entity Framework Core](#banco-de-dados-e-entity-framework-core)
- [API e Swagger](#api-e-swagger)
- [Exemplos Reais](#exemplos-reais)
- [Guia para IA](#guia-para-ia)
- [Adicionando Novas Features](#adicionando-novas-features)

---

## Visão Geral da Arquitetura

### Clean Architecture

Nexas utiliza **Clean Architecture** com separação clara de responsabilidades entre 4 projetos:

```
┌─────────────────────────────────────────────────────────┐
│                  Nexas.Api (Presentation)               │
│  Controllers | Middlewares | Extensions | Services      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│           Nexas.Application (Application Services)      │
│  Commands | Queries | Validators | Behaviors | DTOs    │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              Nexas.Domain (Business Logic)              │
│  Entities | Value Objects | Specifications | Interfaces│
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│          Nexas.Infrastructure (Data Access)            │
│  DbContext | Entity Configurations | Migrations        │
└─────────────────────────────────────────────────────────┘
```

**Regras de Dependência:**
- ✅ Camadas superiores podem depender de camadas inferiores
- ❌ Camadas inferiores NUNCA podem depender de camadas superiores
- ✅ Comunicação entre camadas ocorre via **Interfaces** (inversão de controle)
- ✅ Injeção de dependência centralizada no `Program.cs`

### CQRS (Command Query Responsibility Segregation)

O padrão **CQRS** separa **operações de escrita** (Commands) de **operações de leitura** (Queries):

- **Commands**: Alteram o estado da aplicação (Create, Update, Delete)
  - Retornam `IRequest<int>` (novo ID) ou `IRequest<Unit>` (sem retorno)
  - Possuem **Validators** associados
  - Executados através do **MediatR**
  
- **Queries**: Apenas leem dados sem alterar estado
  - Retornam `IRequest<List<...>>` ou `IRequest<SomeDto>`
  - Otimizadas para projeção de dados
  - Utilizadas para relatórios e consultas

**Exemplo:**
```csharp
// Command: Cria um novo curso (escrita)
var command = new CreateCourseCommand { Name = "C# 101", ... };
var courseId = await mediator.Send(command);

// Query: Obtém todos os cursos (leitura)
var courses = await mediator.Send(new GetCoursesQuery());
```

### MediatR - Pipeline e Behaviors

O **MediatR** atua como um **mediador central** que orquestra requisições através de um **pipeline de behaviors**:

```
Request
  ↓
[ValidationBehavior] ← Valida a requisição usando FluentValidation
  ↓
[LoggingBehavior] ← Registra entrada/saída (logging)
  ↓
[Handler] ← Executa a lógica de negócio
  ↓
Response
```

**Behaviors Implementados:**
1. **ValidationBehavior**: Valida Commands/Queries antes de executar
2. **LoggingBehavior**: Registra todas as requisições e respostas

---

## Estrutura do Projeto

### Nexas.Api (Camada de Apresentação)

Responsável por expor os endpoints HTTP e gerenciar o contexto da requisição.

```
Nexas.Api/
├── Controllers/
│   ├── CoursesController.cs    ← Endpoints para cursos, módulos, aulas
│   └── MeController.cs          ← Endpoint do usuário atual
├── Middlewares/
│   └── GlobalExceptionMiddleware.cs  ← Tratamento global de erros
├── Extensions/
│   ├── AuthenticationExtensions.cs   ← Configuração JWT e autenticação
│   └── SwaggerExtensions.cs         ← Configuração Swagger/OpenAPI
├── Services/
│   └── CurrentUserService.cs    ← Extrai dados do usuário do JWT
├── Properties/
│   └── launchSettings.json      ← Configurações de launch
├── appsettings.json             ← Configurações gerais
├── appsettings.Development.json ← Configurações de desenvolvimento
├── Program.cs                   ← Configuração de DI e middleware
└── Nexas.Api.csproj
```

**Controllers Pattern:**
- Todos herdam de `ControllerBase`
- Decorador `[ApiController]` ativa comportamento automático
- Decorador `[Authorize]` no nível do controller para exigir autenticação
- Decorador `[AllowAnonymous]` em métodos específicos para permitir acesso público
- Injetam `IMediator` no construtor

**Exemplo de Controller:**
```csharp
[Authorize]
[ApiController]
[Route("[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCoursesQuery());
        return Ok(result);
    }
}
```

### Nexas.Application (Camada de Lógica de Aplicação)

Contém **toda a lógica de negócio**, separada em **Commands** (escrita) e **Queries** (leitura).

```
Nexas.Application/
├── Courses/
│   ├── Commands/
│   │   ├── CreateCourse/
│   │   │   ├── CreateCourseCommand.cs      ← Definição + Handler
│   │   │   └── CreateCourseCommandValidator.cs
│   │   ├── UpdateCourse/
│   │   │   ├── UpdateCourseCommand.cs
│   │   │   └── UpdateCourseCommandValidator.cs
│   │   ├── DeactivateCourse/
│   │   │   ├── DeactivateCourseCommand.cs
│   │   │   └── DeactivateCourseCommandValidator.cs
│   │   ├── CreateModule/ ├── CreateModuleCommand.cs + Validator
│   │   ├── UpdateModule/ ├── UpdateModuleCommand.cs + Validator
│   │   ├── DeleteModule/ ├── DeleteModuleCommand.cs + Validator
│   │   ├── DeactivateModule/ ├── DeactivateModuleCommand.cs + Validator
│   │   ├── CreateLesson/ ├── CreateLessonCommand.cs + Validator
│   │   ├── UpdateLesson/ ├── UpdateLessonCommand.cs + Validator
│   │   ├── DeleteLesson/ ├── DeleteLessonCommand.cs + Validator
│   │   └── DeactivateLesson/ ├── DeactivateLessonCommand.cs + Validator
│   │
│   └── Queries/
│       └── GetCourses/
│           └── GetCoursesQuery.cs  ← Definição + Handler + DTOs
│
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs   ← Pipeline: valida todas as requisições
│   │   └── LoggingBehavior.cs      ← Pipeline: registra requisições
│   ├── Interfaces/
│   │   ├── INexasDbContext.cs      ← Abstração do DbContext
│   │   ├── ICurrentUserService.cs  ← Acesso ao usuário do JWT
│   │   └── IUserContextService.cs  ← Recupera User do banco via ExternalId
│   └── Services/
│       └── UserContextService.cs   ← Implementação de IUserContextService
│
├── DependencyInjection.cs          ← Registra MediatR, Validators, Services
└── Nexas.Application.csproj
```

**Estrutura de um Command:**
```csharp
// Cada pasta em Commands/ contém:
// 1. CommandName.cs (Record + Handler)
// 2. CommandNameValidator.cs (Validações)

public record CreateCourseCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    // ... outras propriedades
}

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // Lógica de negócio aqui
    }
}
```

**Estrutura de uma Query:**
```csharp
// Queries/ contêm:
// 1. QueryName.cs (Record)
// 2. Dtos (inline no mesmo arquivo)
// 3. QueryNameHandler (implementa IRequestHandler)

public record GetCoursesQuery : IRequest<List<CourseDto>>;

public record CourseDto { /* ... */ }

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{
    public async Task<List<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        // Query com projeção otimizada
    }
}
```

### Nexas.Domain (Camada de Domínio)

Contém as **entidades** que representam os conceitos de negócio.

```
Nexas.Domain/
├── Common/
│   └── BaseEntity.cs        ← Base para todas as entidades (Id, CreatedAt, UpdatedAt)
├── Constants/
│   └── AuthConstants.cs     ← Constantes de autenticação (claim names, etc)
├── Entities/
│   ├── User.cs              ← Usuário do sistema
│   ├── Course.cs            ← Curso com métodos factory
│   ├── Module.cs            ← Módulo (parte de um curso)
│   ├── Lesson.cs            ← Aula (parte de um módulo)
│   ├── Enrollment.cs        ← Matrícula de usuário em curso
│   └── Subscription.cs      ← Assinatura/Plano do usuário
└── Nexas.Domain.csproj
```

**Características das Entidades:**

1. **Factory Methods**: Lógica de criação centralizada
   ```csharp
   public static Course Create(string name, string? description, ...)
   {
       return new Course 
       { 
           Name = name,
           Description = description,
           Active = true,
           // ...
       };
   }
   ```

2. **BaseEntity**: Propriedades comuns
   ```csharp
   public abstract class BaseEntity
   {
       public int Id { get; set; }
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       public DateTime? UpdatedAt { get; set; }
   }
   ```

3. **Propriedades de Auditoria**: `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`

### Nexas.Infrastructure (Camada de Persistência)

Implementa acesso ao banco de dados via **Entity Framework Core**.

```
Nexas.Infrastructure/
├── Persistence/
│   ├── NexasDbContext.cs    ← DbContext principal
│   └── Configurations/
│       └── EntityConfigurations.cs  ← Fluent API configs
├── DependencyInjection.cs   ← Registra DbContext e conexão
└── Nexas.Infrastructure.csproj
```

**NexasDbContext:**
```csharp
public class NexasDbContext : DbContext, INexasDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## Entidades e Relacionamentos

### User (Usuário)

```csharp
public class User : BaseEntity
{
    public string ExternalId { get; private set; } = string.Empty;  // ID externo (do JWT)
    public string Email { get; private set; } = string.Empty;
    
    // Relacionamentos
    public virtual ICollection<Enrollment> Enrollments { get; private set; }
    public virtual ICollection<Subscription> Subscriptions { get; private set; }
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Users (
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    external_id VARCHAR(100) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);
```

**Responsabilidades:**
- Representa usuários do sistema (alunos e professores)
- Vinculado a `ExternalId` do JWT (vem do sistema de autenticação externo)
- Auto-criado quando usuário autenticado acessa pela primeira vez

---

### Course (Curso)

```csharp
public class Course : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DescriptionSub { get; set; }
    public string? Level { get; set; }                  // Ex: "Iniciante", "Avançado"
    public decimal? PriceSingle { get; set; }           // Preço unitário
    public string? ImgCoverLink { get; set; }           // URL da imagem de capa
    public string? BunnyLibraryId { get; set; }         // ID da biblioteca Bunny.net
    public bool Active { get; set; } = true;            // Soft delete
    public int? CreatedBy { get; set; }                 // User ID do criador
    public int? UpdatedBy { get; set; }                 // User ID do atualizador
    
    // Relacionamentos
    public virtual ICollection<Module> Modules { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; }
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Courses (
    course_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(255) NOT NULL,
    description LONGTEXT NULL,
    description_sub LONGTEXT NULL,
    level VARCHAR(50) NULL,
    price_single DECIMAL(10,2) NULL,
    img_cover_link VARCHAR(2000) NULL,
    bunny_library_id VARCHAR(100) NULL,
    active BOOLEAN DEFAULT TRUE,
    created_by INT NULL,
    updated_by INT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);
```

**Relacionamentos:**
- 1 Curso → N Módulos
- 1 Curso → N Inscrições (Enrollments)

**Factory:**
```csharp
public static Course Create(
    string name, string? description, string? descriptionSub,
    string? level, decimal? priceSingle, string? imgCoverLink,
    string? bunnyLibraryId, int? createdBy)
{
    return new Course 
    { 
        Name = name,
        Description = description,
        DescriptionSub = descriptionSub,
        Level = level,
        PriceSingle = priceSingle,
        ImgCoverLink = imgCoverLink,
        BunnyLibraryId = bunnyLibraryId,
        CreatedBy = createdBy,
        Active = true
    };
}
```

---

### Module (Módulo)

```csharp
public class Module : BaseEntity
{
    public int CourseId { get; set; }                   // Foreign Key
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DescriptionSub { get; set; }
    public string? ImgCoverLink { get; set; }           // URL da imagem de capa
    public string? BunnyCollectionId { get; set; }      // ID da coleção Bunny.net
    public bool Active { get; set; } = true;            // Soft delete
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Relacionamentos
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<Lesson> Lessons { get; set; }
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Modules (
    module_id INT PRIMARY KEY AUTO_INCREMENT,
    course_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description LONGTEXT NULL,
    description_sub LONGTEXT NULL,
    img_cover_link VARCHAR(2000) NULL,
    bunny_collection_id VARCHAR(100) NULL,
    active BOOLEAN DEFAULT TRUE,
    created_by INT NULL,
    updated_by INT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    FOREIGN KEY (course_id) REFERENCES Courses(course_id)
);
```

**Relacionamentos:**
- N Módulos → 1 Curso (foreign key: CourseId)
- 1 Módulo → N Aulas (Lessons)

---

### Lesson (Aula)

```csharp
public class Lesson : BaseEntity
{
    public int ModuleId { get; set; }                   // Foreign Key
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationSeconds { get; set; }           // Duração em segundos
    public string? BunnyVideoId { get; set; }           // ID do vídeo Bunny.net
    public bool Active { get; set; } = true;            // Soft delete
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Relacionamentos
    public virtual Module Module { get; set; } = null!;
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Lessons (
    lesson_id INT PRIMARY KEY AUTO_INCREMENT,
    module_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description LONGTEXT NULL,
    duration_seconds INT NULL,
    bunny_video_id VARCHAR(100) NULL,
    active BOOLEAN DEFAULT TRUE,
    created_by INT NULL,
    updated_by INT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    FOREIGN KEY (module_id) REFERENCES Modules(module_id)
);
```

**Relacionamentos:**
- N Aulas → 1 Módulo (foreign key: ModuleId)

---

### Enrollment (Matrícula)

```csharp
public class Enrollment : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public int CourseId { get; set; }
    public virtual Course Course { get; set; } = null!;
    
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Enrollments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    course_id INT NOT NULL,
    enrolled_at DATETIME NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (course_id) REFERENCES Courses(course_id)
);
```

**Responsabilidades:**
- Tabela de junção (many-to-many) entre User e Course
- Registra quando um usuário se matriculou em um curso

---

### Subscription (Assinatura)

```csharp
public class Subscription : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public string PlanName { get; set; } = string.Empty;  // Ex: "Premium", "Basic"
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}
```

**Estrutura no BD:**
```sql
CREATE TABLE Subscriptions (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    plan_name VARCHAR(50) NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NULL,
    is_active BOOLEAN NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);
```

**Responsabilidades:**
- Gerencia assinatura/plano do usuário
- Futuro: integração com sistema de pagamento

---

## Campos Bunny.net

O sistema foi estendido para suportar integração com **Bunny.net** (CDN e armazenamento de mídia).

### BunnyLibraryId (Course)

- **Propósito**: Identifica a **biblioteca** de vídeos no Bunny.net para um curso
- **Tipo**: `string? (nvarchar(100))`
- **Nullable**: Sim (não todos os cursos necessitam vídeos)
- **Exemplo**: `"library_abc123def456"`
- **Uso**: Determina o local no Bunny.net onde os vídeos do curso são armazenados

### BunnyCollectionId (Module)

- **Propósito**: Identifica a **coleção** dentro da biblioteca para um módulo
- **Tipo**: `string? (nvarchar(100))`
- **Nullable**: Sim
- **Exemplo**: `"collection_module_1"`
- **Uso**: Agrupa os vídeos de um módulo dentro da biblioteca

### BunnyVideoId (Lesson)

- **Propósito**: Identifica o **vídeo específico** no Bunny.net para uma aula
- **Tipo**: `string? (nvarchar(100))`
- **Nullable**: Sim
- **Exemplo**: `"video_lesson_1_intro"`
- **Uso**: Link direto para o vídeo da aula

### ImgCoverLink (Course, Module)

- **Propósito**: URL pública para a **imagem de capa**
- **Tipo**: `string? (nvarchar(2000))`
- **Usado para**: Thumbnail nos listings de cursos/módulos
- **Exemplo**: `"https://cdn.example.com/courses/course1/cover.jpg"`

---

## Fluxo Completo de Uma Requisição

### Exemplo: Criando um Novo Curso

Visualizar o que acontece internamente quando um cliente faz uma requisição para criar um curso:

#### 1. Request HTTP

```http
POST /courses HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "name": "ASP.NET Core 8 - Clean Architecture",
  "description": "Aprenda a construir aplicações escaláveis...",
  "descriptionSub": "Do zero ao avançado",
  "level": "Avançado",
  "priceSingle": 299.90,
  "imgCoverLink": "https://cdn.example.com/asp-core.jpg",
  "bunnyLibraryId": "library_aspcore_2024",
  "modules": [
    {
      "name": "Módulo 1: Fundamentos",
      "description": "Conceitos básicos de ASP.NET Core...",
      "descriptionSub": "Primeiros passos",
      "imgCoverLink": "https://cdn.example.com/mod1.jpg",
      "bunnyCollectionId": "collection_mod1",
      "lessons": [
        {
          "name": "Aula 1: Introducción",
          "description": "Bem-vindo ao curso",
          "durationSeconds": 600,
          "bunnyVideoId": "video_intro"
        }
      ]
    }
  ]
}
```

#### 2. AuthenticationMiddleware (JWT Validation)

```
┌─────────────────────────────────┐
│  JWT Token from Authorization   │
│  Header: Bearer eyJhbGc...      │
└────────────────┬────────────────┘
                 ↓
     ╔═══════════════════════════╗
     ║  JWT Parser & Validation  ║
     ║  ✓ Signature valid        ║
     ║  ✓ Not expired           ║
     ║  ✓ Claims extracted      ║
     ╚════════════┬══════════════╝
                  ↓
        Claims: {
          nameid: "user_123",
          email: "teacher@nexas.com",
          role: ["Teacher"]
        }
```

#### 3. CoursesController.Create()

```csharp
[Authorize(Roles = "Teacher")]
[HttpPost]
public async Task<IActionResult> Create(CreateCourseCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result }, command);
}
```

#### 4. MediatR Pipeline - ValidationBehavior

```
┌────────────────────────────────┐
│  CreateCourseCommand received  │
└────────────┬───────────────────┘
             ↓
  ╔══════════════════════════════╗
  ║  ValidationBehavior<,>       ║
  ║  (IPipelineBehavior)         ║
  ╚────────────┬─────────────────╝
               ↓
     FluentValidation Validators:
     ✓ Name not empty
     ✓ Name max 255 chars
     ✓ ImgCoverLink max 2000 chars
     ✓ BunnyLibraryId max 100 chars
     ✓ Modules not empty
     ✓ Each module validated
     ✓ Each lesson validated
               ↓
        All validations passed ✓
```

#### 5. MediatR Pipeline - LoggingBehavior

```
┌──────────────────────────────────┐
│  LoggingBehavior<,>              │
│  (IPipelineBehavior)             │
└────────────┬─────────────────────┘
             ↓
   _logger.LogInformation(
     "Nexas Request: CreateCourseCommand {@Request}",
     command
   )
```

#### 6. CreateCourseCommandHandler.Handle()

```csharp
public async Task<int> Handle(CreateCourseCommand request, CancellationToken ct)
{
    // 1. Obter usuário autenticado
    var currentUser = await _userContextService.GetCurrentUserAsync();
    // → Busca User no BD usando ExternalId do JWT
    // → Se não existe, cria novo User
    // → Retorna User(Id=1, ExternalId="user_123", Email="teacher@nexas.com")

    // 2. Criar entidade Course com Factory Method
    var course = Course.Create(
        request.Name,
        request.Description,
        request.DescriptionSub,
        request.Level,
        request.PriceSingle,
        request.ImgCoverLink,
        request.BunnyLibraryId,
        currentUser.Id  // ← ID do professor que criou
    );

    // 3. Criar Modules com Factory
    foreach (var moduleDto in request.Modules)
    {
        var module = Module.Create(
            moduleDto.Name,
            moduleDto.Description,
            moduleDto.DescriptionSub,
            moduleDto.ImgCoverLink,
            moduleDto.BunnyCollectionId,
            currentUser.Id
        );

        // 4. Criar Lessons com Factory
        foreach (var lessonDto in moduleDto.Lessons)
        {
            var lesson = Lesson.Create(
                lessonDto.Name,
                lessonDto.Description,
                lessonDto.DurationSeconds,
                lessonDto.BunnyVideoId,
                currentUser.Id
            );
            module.Lessons.Add(lesson);
        }

        course.Modules.Add(module);
    }

    // 5. Persistir no BD (transação única)
    _context.Courses.Add(course);
    await _context.SaveChangesAsync(cancellationToken);

    // 6. Retornar ID do novo curso
    return course.Id;  // Ex: 42
}
```

#### 7. Entity Framework Core - Persistência

```
┌──────────────────────────────────┐
│  SaveChangesAsync() executado    │
└────────────┬─────────────────────┘
             ↓
  EF Core gera INSERT SQL:
  ┌────────────────────────────────┐
  │ INSERT INTO Courses(            │
  │   name, description, level,     │
  │   price_single, img_cover_link, │
  │   bunny_library_id, active,     │
  │   created_by, created_at        │
  │ ) VALUES(...)                   │
  │ → course_id = 42                │
  │                                 │
  │ INSERT INTO Modules(...)        │
  │ → module_id = 1                 │
  │                                 │
  │ INSERT INTO Lessons(...)        │
  │ → lesson_id = 1                 │
  └────────────┬───────────────────┘
               ↓
         Transaction commited
```

#### 8. Resposta HTTP

```http
HTTP/1.1 201 Created
Location: /courses/42
Content-Type: application/json

{
  "id": 42,
  "name": "ASP.NET Core 8 - Clean Architecture",
  "description": "Aprenda a construir aplicações escaláveis...",
  "descriptionSub": "Do zero ao avançado",
  "level": "Avançado",
  "priceSingle": 299.90,
  "imgCoverLink": "https://cdn.example.com/asp-core.jpg",
  "bunnyLibraryId": "library_aspcore_2024",
  "modules": [...]
}
```

#### 9. LoggingBehavior Resposta

```
_logger.LogInformation(
  "Nexas Response: CreateCourseCommand {@Response}",
  courseId: 42
)
```

---

## Padrões Obrigatórios

### 1. Nomenclatura

#### Controllers
- **Padrão**: Plural + "Controller"
- **Localização**: `Nexas.Api/Controllers/`
- **Exemplo**: `CoursesController`, `UsersController`, `ModulesController`

#### Commands/Queries
- **Commands**: `Create{Entity}Command`, `Update{Entity}Command`, `Delete{Entity}Command`, `Deactivate{Entity}Command`
- **Queries**: `Get{Entities}Query`, `Get{Entity}ByIdQuery`
- **Localização**: `Nexas.Application/{Module}/Commands/` ou `Nexas.Application/{Module}/Queries/`

#### Validators
- **Padrão**: `{CommandName}Validator` ou `{QueryName}Validator`
- **Localização**: Mesma pasta do Command/Query

#### Handlers
- **Padrão**: `{CommandName}Handler` ou `{QueryName}Handler`
- **Implementa**: `IRequestHandler<TRequest, TResponse>`

#### DTOs
- **Padrão**: `{EntityName}Dto`, `{EntityName}CreateDto`, `{EntityName}UpdateDto`
- **Localização**: Inline no Query ou em arquivo separado se reutilizado

### 2. Padrão de Endpoints REST

```
GET     /courses              ← Listar todos os cursos
GET     /courses/{id}         ← Obter um curso específico
POST    /courses              ← Criar novo curso
PUT     /courses/{id}         ← Atualizar curso (substitui)
DELETE  /courses/{id}         ← Deletar curso
PATCH   /courses/{id}/status  ← Atualizar status/Deactivate
```

**Status Codes Padronizados:**
- `200 OK` - Sucesso na leitura/atualização
- `201 Created` - Recurso criado com sucesso
- `204 No Content` - Operação bem-sucedida sem retorno
- `400 Bad Request` - Validação falhou
- `401 Unauthorized` - Não autenticado
- `403 Forbidden` - Autenticado mas sem permissão
- `404 Not Found` - Recurso não encontrado
- `409 Conflict` - Conflito (duplicado, estado inválido)
- `500 Internal Server Error` - Erro interno

### 3. Padrão de DTOs

```csharp
// DTO para resposta (leitura)
public record CourseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImgCoverLink { get; init; }
    public string? BunnyLibraryId { get; init; }
    public List<ModuleDto> Modules { get; init; } = new();
}

// DTO para entrada (escrita) - normalmente parte do Command
public record CreateCourseCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImgCoverLink { get; init; }
    public string? BunnyLibraryId { get; init; }
}
```

**Regras:**
- ✅ Use `record` para DTOs (imutável, ótimo para mapear)
- ✅ Use `init` para propriedades (apenas atribuição na construção)
- ✅ Inclua XML comments para documentação Swagger
- ✅ DTOs nunca contêm lógica de negócio
- ✅ Mapear entidades para DTOs no Handler

### 4. Padrão de Responses

```csharp
// Sucesso - retornar objeto
return Ok(new CourseDto { ... });

// Criado
return CreatedAtAction(nameof(GetById), new { id = courseId }, dto);

// Erro de validação (lançar ValidationException)
throw new ValidationException(failures);

// Erro não encontrado
throw new InvalidOperationException("Course not found");

// Unauthorized
throw new UnauthorizedAccessException("User not authenticated");
```

### 5. Padrão de Validações

```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        // Campo obrigatório
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(255).WithMessage("Course name must not exceed 255 characters.");

        // Campo opcional com validação
        RuleFor(v => v.ImgCoverLink)
            .MaximumLength(2000).WithMessage("Course cover image URL must not exceed 2000 characters.")
            .When(v => !string.IsNullOrEmpty(v.ImgCoverLink));

        // Validação de coleção aninhada
        RuleForEach(v => v.Modules).ChildRules(modules =>
        {
            modules.RuleFor(m => m.Name)
                .NotEmpty().WithMessage("Module name is required.");
        });
    }
}
```

**Regras:**
- ✅ Herde de `AbstractValidator<TRequest>`
- ✅ Uma classe por Command/Query
- ✅ Validações são executadas automaticamente via ValidationBehavior
- ✅ Lançam `ValidationException` se falharem
- ✅ Use `When()` para validações condicionais
- ✅ Use `RuleForEach()` para coleções

### 6. Padrão de Exceptions

```csharp
// Validação (automático via ValidationBehavior)
throw new ValidationException(failures);  // 400 Bad Request

// Recurso não encontrado
throw new InvalidOperationException("Course not found");  // 500 (será capturado)

// Não autenticado
throw new UnauthorizedAccessException("User not authenticated");  // 401

// Negócio
throw new InvalidOperationException("Cannot deactivate course with active enrollments");
```

**GlobalExceptionMiddleware trata:**
- `ValidationException` → 400 Bad Request
- `UnauthorizedAccessException` → 401 Unauthorized
- Outras → 500 Internal Server Error (+ StackTrace em dev)

### 7. Padrão de Migrations

```bash
# Criar migration (após alterar entidades)
dotnet ef migrations add AddBunnyFieldsToCourses --project Nexas.Infrastructure

# Aplicar
dotnet ef database update --project Nexas.Infrastructure
```

**Arquivo de migration:**
```csharp
public partial class AddBunnyFieldsToCourses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "bunny_library_id",
            table: "Courses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "bunny_library_id",
            table: "Courses");
    }
}
```

### 8. Padrão de Comentários

```csharp
public record CreateCourseCommand : IRequest<int>
{
    /// <summary>
    /// Nome do curso.
    /// </summary>
    /// <example>Desenvolvimento Web com .NET 8</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// ID da biblioteca Bunny para o curso.
    /// </summary>
    /// <example>library_12345</example>
    public string? BunnyLibraryId { get; init; }
}
```

**Regras:**
- ✅ Use XML comments (`///`) em classes públicas
- ✅ Inclua `<summary>` descrevendo o propósito
- ✅ Inclua `<example>` para valores esperados
- ✅ Gera documentação Swagger automaticamente

### 9. Padrão de Organização de Arquivos

```
Feature/
├── Commands/
│   └── CreateFeature/
│       ├── CreateFeatureCommand.cs        (Command + Handler)
│       └── CreateFeatureCommandValidator.cs
├── Queries/
│   └── GetFeatures/
│       └── GetFeaturesQuery.cs            (Query + Handler + DTOs)
└── [DTOs/]  (opcional, se muito reutilizadas)
    └── FeatureDto.cs
```

**Regras:**
- ✅ Uma pasta por use case (CreateCourse, UpdateCourse, etc)
- ✅ Command + Handler no mesmo arquivo
- ✅ Validator em arquivo separado
- ✅ DTOs inline no Query ou em pasta separada

### 10. Padrão de Injeção de Dependência

```csharp
// Em Nexas.Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    var assembly = Assembly.GetExecutingAssembly();

    // Registra MediatR com behaviors
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    });

    // Registra validators
    services.AddValidatorsFromAssembly(assembly);

    // Registra services
    services.AddScoped<IUserContextService, UserContextService>();

    return services;
}

// Em Nexas.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<NexasDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    services.AddScoped<INexasDbContext>(provider => 
        provider.GetRequiredService<NexasDbContext>());

    return services;
}

// Em Nexas.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationSetup(builder.Configuration);
```

### 11. Regras de Arquitetura

✅ **PERMITIDO:**
- Camadas superiores dependem de camadas inferiores
- Domain conhecer apenas Domain (sem dependências externas)
- Application depender de Domain
- Api depender de Application
- Usar interfaces (inversão de controle)
- Injeção de dependência

❌ **PROIBIDO:**
- Domain depender de Application/Infrastructure/Api
- Application depender de Api
- Lógica de negócio fora do domain
- DbContext em controllers
- Entity navigation properties em DTOs sem cuidado
- N+1 queries (sempre usar Include/Select projetado)
- Lógica de validação fora dos Validators

### 12. Anti-patterns a Evitar

❌ **Anemic Domain Model** (entidades sem lógica)
```csharp
// ❌ ERRADO
public class Course
{
    public string Name { get; set; }
    // Sem métodos, apenas propriedades
}

// ✅ CORRETO
public class Course
{
    public string Name { get; set; }
    public static Course Create(string name, ...) { ... }
    public void Deactivate() { ... }
}
```

❌ **God Objects** (classes muito grandes)
```csharp
// ❌ ERRADO: Tudo em uma classe
public class CourseService
{
    public void Create() { ... }
    public void Update() { ... }
    public void Delete() { ... }
    public void SendEmail() { ... }
    public void LogMetrics() { ... }
}

// ✅ CORRETO: Separado em Commands/Queries
public class CreateCourseCommandHandler { ... }
public class UpdateCourseCommandHandler { ... }
```

❌ **Circular Dependencies**
```csharp
// ❌ ERRADO
// A → B → C → A

// ✅ CORRETO: Usar interfaces, inversão de controle
```

❌ **Duplicação de Código**
```csharp
// ❌ ERRADO: Mesmo validation em vários places
// CreateCourseCommandValidator
// UpdateCourseCommandValidator
// (Ambos validam Name)

// ✅ CORRETO: Reutilizar validators ou criar base
public abstract class CourseValidatorBase<T>
{
    protected void ValidateCourseName(IRuleBuilder<T, string> rule) { ... }
}
```

---

## Autenticação e Autorização

### Fluxo de Autenticação JWT

```
┌────────────────────────────────────────┐
│  Client (Frontend/Mobile/API)          │
└──────────────────┬─────────────────────┘
                   │
         1. Faz login em outro sistema
         2. Recebe JWT Token
                   │
                   ↓
┌────────────────────────────────────────┐
│  Cliente envia: GET /me                │
│  Header: Authorization: Bearer <token> │
└──────────────────┬─────────────────────┘
                   │
                   ↓
┌────────────────────────────────────────────┐
│  Nexas.Api - AuthenticationMiddleware      │
│  1. Extrai token do header                 │
│  2. Valida assinatura (secret key)         │
│  3. Valida expiração                       │
│  4. Extrai claims (nameid, email, role)    │
└──────────────────┬──────────────────────────┘
                   │
                   ↓
       ✓ Token válido - Claims extraídos
       {
         "nameid": "user_123",
         "email": "teacher@nexas.com",
         "role": ["Teacher"],
         "exp": 1742000000
       }
                   │
                   ↓
┌────────────────────────────────────────┐
│  HttpContext.User.Identity populated   │
│  com os claims extraídos               │
└──────────────────┬─────────────────────┘
                   │
                   ↓
    3. CurrentUserService lê os claims
       e extrai ExternalId e Email
                   │
                   ↓
    4. UserContextService busca/cria User
       usando ExternalId
```

### Configuração JWT (AuthenticationExtensions.cs)

```csharp
public static IServiceCollection AddAuthenticationSetup(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("Jwt");
    var secretKey = jwtSettings.GetValue<string>("Key");

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;  // ← IMPORTANTE!
        // Sem isso, "role" no JWT não é reconhecido por [Authorize(Roles = ...)]

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,           // Não valida issuer
            ValidateAudience = false,         // Não valida audience
            ValidateLifetime = true,          // ✓ Valida expiração
            ValidateIssuerSigningKey = true,  // ✓ Valida assinatura
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey ?? string.Empty)
            ),
            RoleClaimType = "role"  // ← Define qual claim é a role
        };
    });

    return services;
}
```

### Autorização - Atributos

```csharp
// Qualquer usuário autenticado
[Authorize]
public async Task<IActionResult> GetAll() { ... }

// Qualquer usuário não autenticado (público)
[AllowAnonymous]
public async Task<IActionResult> GetCourses() { ... }

// Apenas professores
[Authorize(Roles = "Teacher")]
public async Task<IActionResult> CreateCourse(CreateCourseCommand cmd) { ... }

// Apenas administradores
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteCourse(int id) { ... }

// Múltiplos roles (qualquer um dos dois)
[Authorize(Roles = "Teacher,Admin")]
public async Task<IActionResult> UpdateCourse(UpdateCourseCommand cmd) { ... }
```

### CurrentUserService - Extração de Claims

```csharp
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string? ExternalId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            
            if (user == null || user.Identity?.IsAuthenticated == false)
                return null;

            // Tenta múltiplos nomes de claim (compatibilidade)
            return user.FindFirst("nameid")?.Value ??        // JWT original
                   user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??  // Mapeado
                   user.FindFirst("sub")?.Value ??
                   user.FindFirst("id")?.Value ??
                   user.FindFirst("external_id")?.Value;
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated == false)
                return null;

            return user.FindFirst("email")?.Value ??
                   user.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
```

### UserContextService - Resolução de User

```csharp
public class UserContextService : IUserContextService
{
    private readonly INexasDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public async Task<User> GetCurrentUserAsync()
    {
        var externalId = _currentUserService.ExternalId;

        if (string.IsNullOrEmpty(externalId))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated or external ID is missing in token.");
        }

        // Tenta buscar usuário existente
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId);

        if (user == null)
        {
            // Auto-criar novo usuário na primeira autenticação
            var email = _currentUserService.Email ?? "unknown@nexas.com";
            user = User.Create(externalId, email);
            _context.Users.Add(user);
            await _context.SaveChangesAsync(default);
        }

        return user;  // Agora com Id preenchido
    }
}
```

### Appsettings - JWT Configuration

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-at-least-256-bits-minimum-for-security",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers"
  }
}
```

---

## Banco de Dados e Entity Framework Core

### DbContext Interface

```csharp
public interface INexasDbContext
{
    DbSet<User> Users { get; }
    DbSet<Course> Courses { get; }
    DbSet<Module> Modules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Subscription> Subscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

### DbContext Implementation

```csharp
public class NexasDbContext : DbContext, INexasDbContext
{
    public NexasDbContext(DbContextOptions<NexasDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações (EntityConfigurations.cs)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### Fluent API - Entity Configurations

**Padrão: IEntityTypeConfiguration<T>**

```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("course_id");

        // Propriedades
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.ImgCoverLink)
            .HasColumnName("img_cover_link")
            .HasMaxLength(2000);

        builder.Property(c => c.BunnyLibraryId)
            .HasColumnName("bunny_library_id")
            .HasMaxLength(100);

        builder.Property(c => c.Active)
            .HasColumnName("active");

        // Audit fields
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.UpdatedBy).HasColumnName("updated_by");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        // Relacionamentos
        builder.HasMany(c => c.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId);
    }
}

// Aplicadas automaticamente em OnModelCreating:
modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexasDbContext).Assembly);
```

### Migrations - Padrão

```bash
# Adicionar migration após alterar modelos
dotnet ef migrations add <MigrationName> \
    --project Nexas.Infrastructure \
    --startup-project Nexas.Api

# Exemplos:
dotnet ef migrations add AddBunnyFields --project Nexas.Infrastructure
dotnet ef migrations add AddImgCoverLinkToModules --project Nexas.Infrastructure

# Aplicar ao banco
dotnet ef database update --project Nexas.Infrastructure

# Ver histórico
dotnet ef migrations list --project Nexas.Infrastructure

# Remover migration não aplicada
dotnet ef migrations remove --project Nexas.Infrastructure
```

### Exemplo de Migration

```csharp
public partial class AddBunnyFieldsToCourses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "bunny_library_id",
            table: "Courses",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "img_cover_link",
            table: "Courses",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "bunny_library_id",
            table: "Courses");

        migrationBuilder.DropColumn(
            name: "img_cover_link",
            table: "Courses");
    }
}
```

### Relacionamentos - Padrões

**One-to-Many (1:N):**
```csharp
// Modelagem
public class Course
{
    public virtual ICollection<Module> Modules { get; set; }
}

public class Module
{
    public int CourseId { get; set; }
    public virtual Course Course { get; set; } = null!;
}

// Configuração
builder.HasMany(c => c.Modules)
    .WithOne(m => m.Course)
    .HasForeignKey(m => m.CourseId);
```

**Many-to-Many (N:N) - via tabela de junção:**
```csharp
// Modelagem
public class User
{
    public virtual ICollection<Enrollment> Enrollments { get; set; }
}

public class Course
{
    public virtual ICollection<Enrollment> Enrollments { get; set; }
}

public class Enrollment
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public int CourseId { get; set; }
    public virtual Course Course { get; set; } = null!;
}

// Configuração
builder.HasOne(e => e.User)
    .WithMany(u => u.Enrollments)
    .HasForeignKey(e => e.UserId);

builder.HasOne(e => e.Course)
    .WithMany(c => c.Enrollments)
    .HasForeignKey(e => e.CourseId);
```

### Queries Otimizadas - Projeção

❌ **Problema: N+1 Query**
```csharp
// ❌ ERRADO: Gera múltiplas queries
var courses = await _context.Courses.ToListAsync();
foreach (var course in courses)
{
    var modules = course.Modules;  // ← Query adicional para cada curso!
}
```

✅ **Solução: Include + Select**
```csharp
// ✅ CORRETO: Carrega tudo numa única query
var courses = await _context.Courses
    .Include(c => c.Modules.Where(m => m.Active))
        .ThenInclude(m => m.Lessons.Where(l => l.Active))
    .ToListAsync();
```

✅ **Melhor: Projeção**
```csharp
// ✅ ÓTIMO: Apenas os campos necessários
var courses = await _context.Courses
    .Where(c => c.Active)
    .Include(c => c.Modules.Where(m => m.Active))
        .ThenInclude(m => m.Lessons.Where(l => l.Active))
    .Select(c => new CourseDto
    {
        Id = c.Id,
        Name = c.Name,
        Modules = c.Modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            Name = m.Name,
            Lessons = m.Lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                Name = l.Name,
                BunnyVideoId = l.BunnyVideoId
            }).ToList()
        }).ToList()
    })
    .ToListAsync();
```

---

## API e Swagger

### Swagger Configuration

```csharp
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();  // ← Ativa atributos [SwaggerOperation]
            
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Nexas API",
                Version = "v1",
                Description = "API do sistema Nexas para gerenciamento de cursos.",
                Contact = new OpenApiContact
                {
                    Name = "Nexas Support",
                    Email = "suporte@nexas.com.br"
                }
            });

            // Incluir XML comments de documentação
            var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            c.IncludeXmlComments(apiXmlPath);

            // Incluir XML comments da Application layer
            var appXmlFile = "Nexas.Application.xml";
            var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
            if (File.Exists(appXmlPath))
                c.IncludeXmlComments(appXmlPath);

            // Configurar segurança JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }
}
```

### XML Documentation Comments

Adicionar no `.csproj` para gerar XML docs:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### Swagger Annotations

```csharp
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    /// <summary>
    /// Obtém a lista de todos os cursos ativos.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todos os cursos com seus módulos e aulas.
    /// Público - não requer autenticação.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista cursos",
        Description = "Retorna uma lista de cursos ativos com módulos e aulas")]
    [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }

    /// <summary>
    /// Cria um novo curso.
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [SwaggerOperation(Summary = "Cria novo curso")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        // ...
    }
}
```

### Responses HTTP Padronizadas

**200 OK (Leitura bem-sucedida):**
```json
{
  "id": 1,
  "name": "ASP.NET Core 8",
  "description": "...",
  "modules": [...]
}
```

**201 Created (Criação bem-sucedida):**
```
Status: 201 Created
Location: /courses/42

{
  "id": 42,
  "name": "ASP.NET Core 8",
  ...
}
```

**400 Bad Request (Validação falhou):**
```json
{
  "statusCode": 400,
  "message": "One or more validation failures have occurred.",
  "errors": [
    {
      "propertyName": "Name",
      "errorMessage": "Course name is required."
    }
  ]
}
```

**401 Unauthorized (Sem autenticação):**
```json
{
  "statusCode": 401,
  "message": "User is not authenticated."
}
```

**404 Not Found:**
```json
{
  "statusCode": 404,
  "message": "Course with ID 999 not found."
}
```

**500 Internal Server Error:**
```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred.",
  "stackTrace": "[em development apenas]"
}
```

---

## Exemplos Reais

### 1. Criar um Curso Completo com Módulos e Aulas

**Request:**
```http
POST /courses HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "C# Advanced",
  "description": "Masterclass de C# avançado",
  "descriptionSub": "Para desenvolvedores intermediários",
  "level": "Avançado",
  "priceSingle": 199.90,
  "imgCoverLink": "https://cdn.example.com/csharp.jpg",
  "bunnyLibraryId": "lib_csharp_2024",
  "modules": [
    {
      "name": "Generics e Collections",
      "description": "Entendendo genéricos em C#",
      "descriptionSub": "Parte 1",
      "imgCoverLink": "https://cdn.example.com/generics.jpg",
      "bunnyCollectionId": "col_generics",
      "lessons": [
        {
          "name": "O que são Generics?",
          "description": "Introdução a genéricos",
          "durationSeconds": 900,
          "bunnyVideoId": "vid_generics_intro"
        },
        {
          "name": "Constraints em Generics",
          "description": "Limitando tipos genéricos",
          "durationSeconds": 1200,
          "bunnyVideoId": "vid_generics_constraints"
        }
      ]
    }
  ]
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /courses/5
Content-Type: application/json

{
  "id": 5,
  "name": "C# Advanced",
  "description": "Masterclass de C# avançado",
  "descriptionSub": "Para desenvolvedores intermediários",
  "level": "Avançado",
  "priceSingle": 199.90,
  "imgCoverLink": "https://cdn.example.com/csharp.jpg",
  "bunnyLibraryId": "lib_csharp_2024",
  "modules": [
    {
      "id": 12,
      "name": "Generics e Collections",
      "description": "Entendendo genéricos em C#",
      "descriptionSub": "Parte 1",
      "imgCoverLink": "https://cdn.example.com/generics.jpg",
      "bunnyCollectionId": "col_generics",
      "lessons": [
        {
          "id": 42,
          "name": "O que são Generics?",
          "description": "Introdução a genéricos",
          "durationSeconds": 900,
          "bunnyVideoId": "vid_generics_intro"
        },
        {
          "id": 43,
          "name": "Constraints em Generics",
          "description": "Limitando tipos genéricos",
          "durationSeconds": 1200,
          "bunnyVideoId": "vid_generics_constraints"
        }
      ]
    }
  ]
}
```

Curl example (create course):
```bash
curl -X POST "http://localhost:5000/courses" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"name":"C# Advanced","description":"Masterclass de C# avançado","priceSingle":199.9}'
```

### 2. Listar Todos os Cursos

**Request:**
```http
GET /courses HTTP/1.1
Accept: application/json
```

Curl example (list courses):
```bash
curl "http://localhost:5000/courses" -H "Accept: application/json"
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": 1,
    "name": "ASP.NET Core 8",
    "description": "Construa APIs modernas com ASP.NET Core 8",
    "level": "Avançado",
    "priceSingle": 299.90,
    "imgCoverLink": "https://cdn.example.com/aspcore.jpg",
    "bunnyLibraryId": "lib_aspcore_2024",
    "modules": [
      {
        "id": 1,
        "name": "Fundamentos",
        "bunnyCollectionId": "col_fund",
        "lessons": [
          {
            "id": 1,
            "name": "Introdução",
            "bunnyVideoId": "vid_intro"
          }
        ]
      }
    ]
  }
]
```

Curl example (get course by id):
```bash
curl "http://localhost:5000/courses/1" -H "Accept: application/json"
```

### 3. Atualizar um Curso

**Request:**
```http
PUT /courses/1 HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "name": "ASP.NET Core 8 - Updated",
  "description": "Descrição atualizada",
  "level": "Avançado",
  "priceSingle": 349.90,
  "imgCoverLink": "https://cdn.example.com/aspcore-new.jpg",
  "bunnyLibraryId": "lib_aspcore_2024_v2"
}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Course updated successfully"
}
```

### 4. Deactivar um Curso

**Request:**
```http
PATCH /courses/1/deactivate HTTP/1.1
Authorization: Bearer <token>
```

**Response:**
```http
HTTP/1.1 200 OK
```

### 5. Criar um Módulo em um Curso

**Request:**
```http
POST /courses/1/modules HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{
  "courseId": 1,
  "name": "Módulo Avançado",
  "description": "Tópicos avançados",
  "imgCoverLink": "https://cdn.example.com/advanced.jpg",
  "bunnyCollectionId": "col_advanced",
  "lessons": [
    {
      "name": "Aula 1",
      "durationSeconds": 1500,
      "bunnyVideoId": "vid_adv_1"
    }
  ]
}
```

### 6. Obter Perfil do Usuário Autenticado

**Request:**
```http
GET /me HTTP/1.1
Authorization: Bearer <token>
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "user_id": 1,
  "external_id": "user_123_from_agivys",
  "email": "teacher@nexas.com",
  "created_at": "2024-05-08T10:30:00Z"
}
```

---

### 7. Criar Compra (Endpoint: POST /purchases)

Requer autenticação: `Authorization: Bearer <token>`

Request (JSON):
```http
POST /purchases HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{
    "courseId": 1,
    "amount": 197.90,
    "paymentMethod": "CREDIT_CARD",
    "card": {
        "holderName": "JOEDER BLANCA TESTE",
        "number": "4444444444444444",
        "expiryMonth": "12",
        "expiryYear": "2030",
        "ccv": "123",
        "holderCpfCnpj": "39395533870"
    }
}
```

Response (201 Created):
```http
HTTP/1.1 201 Created
Location: /purchases/3
Content-Type: application/json

{
    "purchaseId": 3,
    "status": "CONFIRMED",
    "pixQrCode": null,
    "pixCopyPaste": null,
    "asaasPaymentId": "pay_yiygnzqwo7syi2o3"
}
```

Curl example:
```bash
curl -X POST "http://localhost:5000/purchases" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"courseId":1,"amount":197.9,"paymentMethod":"CREDIT_CARD","card":{"holderName":"Test","number":"4444444444444444","expiryMonth":"12","expiryYear":"2030","ccv":"123","holderCpfCnpj":"39395533870"}}'
```

---

### 8. Criar Assinatura (Endpoint: POST /subscriptions)

Requer autenticação: `Authorization: Bearer <token>`

Request (JSON):
```http
POST /subscriptions HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{
    "planName": "Premium",
    "value": 49.90
}
```

Response (201 Created):
```http
HTTP/1.1 201 Created
Location: /subscriptions/7
Content-Type: application/json

{
    "subscriptionId": 7
}
```

Curl example:
```bash
curl -X POST "http://localhost:5000/subscriptions" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"planName":"Premium","value":49.9}'
```

Curl example:
```bash
curl -X POST "http://localhost:5000/purchases" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"courseId":1,"amount":197.9,"paymentMethod":"CREDIT_CARD","card":{"holderName":"Test","number":"4444444444444444","expiryMonth":"12","expiryYear":"2030","ccv":"123","holderCpfCnpj":"39395533870"}}'
```


## Guia para IA

### Como Uma IA Deve Trabalhar Neste Projeto

#### 1. **Análise Inicial do Projeto**

Ao começar a trabalhar em uma tarefa, IA deve:

1. **Ler este README** (contexto geral)
2. **Entender a arquitetura** (Clean Architecture + CQRS)
3. **Mapear a estrutura real** (pastas, padrões, convenções)
4. **Identificar o padrão usado** em funcionalidades similares

**Exemplo:**
- "Preciso criar um novo endpoint para deletar um módulo"
- ✅ IA lê o projeto
- ✅ Vê que já existe `DeactivateCourse` e `DeleteLesson`
- ✅ Segue o padrão exato
- ✅ Cria `DeleteModuleCommand` + `DeleteModuleCommandValidator` + Handler

#### 2. **Quais Arquivos Ler Primeiro**

```
├── README.md                    ← Contexto geral (este arquivo)
├── Nexas.Api/Program.cs         ← DI + middleware setup
├── Nexas.Application/DependencyInjection.cs
├── Entidades relacionadas       ← Ex: Course.cs, Module.cs
├── Comando/Query semelhante      ← Ex: CreateCourseCommand (padrão)
├── Camada correspondente         ← Onde a IA precisa fazer mudança
```

**Ordem de leitura para nova feature:**
```
1. README.md (este arquivo)
2. Entidade relacionada (Nexas.Domain/Entities/*)
3. Command/Query similar em Nexas.Application/
4. Controller em Nexas.Api/Controllers/
5. EntityConfiguration em Nexas.Infrastructure/Persistence/
```

#### 3. **Implementando Uma Nova Feature**

**Checklist:**

```
□ 1. Entender requisito
□ 2. Ler este README
□ 3. Analisar feature similar
□ 4. Estender entidade (Domain)
   □ Adicionar propriedade
   □ Atualizar Factory Method
□ 5. Criar Command/Query (Application)
   □ Definir Command record com XML comments
   □ Criar Handler com IRequestHandler<,>
   □ Criar Validator com RuleFor()
   □ Adicionar DTOs se necessário
□ 6. Atualizar EntityConfiguration (Infrastructure)
   □ Adicionar mapping da coluna com HasColumnName()
□ 7. Criar/Atualizar migration (Infrastructure)
   □ dotnet ef migrations add <Name>
   □ Revisar Up/Down methods
□ 8. Criar/Atualizar Controller action (Api)
   □ Injetar IMediator
   □ Executar via mediator.Send()
   □ Retornar resposta correta (Ok, CreatedAtAction, etc)
□ 9. Adicionar testes de validação (opcional)
□ 10. Testar no Swagger
```

#### 4. **Alterar uma Entidade**

**Exemplo: Adicionar campo `Language` em `Course`**

```csharp
// 1. Nexas.Domain/Entities/Course.cs
public class Course : BaseEntity
{
    // ... propriedades existentes ...
    
    /// <summary>Idioma do curso (pt-BR, en-US, etc)</summary>
    public string? Language { get; set; }
    
    // Atualizar Factory Method
    public static Course Create(..., string? language = null)
    {
        return new Course 
        { 
            // ...
            Language = language,  // ← Adicionar
            Active = true
        };
    }
}

// 2. Nexas.Application/Courses/Commands/CreateCourse/CreateCourseCommand.cs
public record CreateCourseCommand : IRequest<int>
{
    // ... propriedades existentes ...
    
    /// <summary>Idioma do curso.</summary>
    /// <example>pt-BR</example>
    public string? Language { get; init; }
}

// 3. Handler - CreateCourseCommandHandler
Course.Create(
    // ... parâmetros ...
    request.Language,  // ← Adicionar
    currentUser.Id
);

// 4. Validator - CreateCourseCommandValidator
RuleFor(v => v.Language)
    .Must(lang => IsValidLanguage(lang))
    .When(v => !string.IsNullOrEmpty(v.Language));

private bool IsValidLanguage(string? language)
{
    if (string.IsNullOrEmpty(language)) return true;
    return language == "pt-BR" || language == "en-US";
}

// 5. EntityConfiguration
builder.Property(c => c.Language)
    .HasColumnName("language")
    .HasMaxLength(10);

// 6. Migration
dotnet ef migrations add AddLanguageToCourses --project Nexas.Infrastructure

// 7. Update/Query - GetCoursesQuery.cs
public record CourseDto
{
    // ...
    public string? Language { get; init; }
}

// Projeção
Modules = c.Modules.Select(m => new ModuleDto
{
    // ...
    Language = c.Language  // ← Adicionar
})

// 8. UpdateCourseCommand
public record UpdateCourseCommand : IRequest<Unit>
{
    public string? Language { get; init; }
}

// Handler
course.Language = request.Language;
```

#### 5. **Criando Uma Nova Query**

**Exemplo: `GetCourseById` query**

```csharp
// 1. Nexas.Application/Courses/Queries/GetCourseById/GetCourseByIdQuery.cs
public record GetCourseByIdQuery(int Id) : IRequest<CourseDetailDto>;

public record CourseDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    // ... mais campos ...
    public List<ModuleDto> Modules { get; init; } = new();
}

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDetailDto>
{
    private readonly INexasDbContext _context;

    public GetCourseByIdQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDetailDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Where(c => c.Id == request.Id && c.Active)
            .Include(c => c.Modules.Where(m => m.Active))
                .ThenInclude(m => m.Lessons.Where(l => l.Active))
            .Select(c => new CourseDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                // ... mapeamento ...
                Modules = c.Modules.Select(m => new ModuleDto { ... }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            throw new InvalidOperationException($"Course with ID {request.Id} not found.");

        return course;
    }
}

// 2. CoursesController.cs
[AllowAnonymous]
[HttpGet("{id}")]
[SwaggerOperation(Summary = "Obtém um curso por ID")]
[ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(int id)
{
    var result = await _mediator.Send(new GetCourseByIdQuery(id));
    return Ok(result);
}
```

#### 6. **Criando Um Novo Command**

**Exemplo: `UpdateModuleCommand`**

```csharp
// 1. Nexas.Application/Courses/Commands/UpdateModule/UpdateModuleCommand.cs
public record UpdateModuleCommand : IRequest<Unit>
{
    /// <summary>ID do módulo a atualizar</summary>
    public int Id { get; init; }
    
    /// <summary>Novo nome</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Descrição atualizada</summary>
    public string? Description { get; init; }
    
    /// <summary>Nova URL de capa</summary>
    public string? ImgCoverLink { get; init; }
    
    /// <summary>Novo ID de coleção Bunny</summary>
    public string? BunnyCollectionId { get; init; }
}

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, Unit>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public UpdateModuleCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Unit> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        var module = await _context.Modules.FindAsync(
            new object[] { request.Id }, 
            cancellationToken: cancellationToken);

        if (module == null)
            throw new InvalidOperationException($"Module with ID {request.Id} not found.");

        // Atualizar propriedades
        module.Name = request.Name;
        module.Description = request.Description;
        module.ImgCoverLink = request.ImgCoverLink;
        module.BunnyCollectionId = request.BunnyCollectionId;
        module.UpdatedBy = currentUser.Id;

        _context.Modules.Update(module);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

// 2. UpdateModuleCommandValidator.cs
public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Module ID must be greater than 0.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Module name is required.")
            .MaximumLength(255).WithMessage("Name max 255 chars.");

        RuleFor(v => v.ImgCoverLink)
            .MaximumLength(2000).WithMessage("Image URL max 2000 chars.")
            .When(v => !string.IsNullOrEmpty(v.ImgCoverLink));

        RuleFor(v => v.BunnyCollectionId)
            .MaximumLength(100).WithMessage("Bunny Collection ID max 100 chars.")
            .When(v => !string.IsNullOrEmpty(v.BunnyCollectionId));
    }
}

// 3. Controller action
[Authorize(Roles = "Teacher")]
[HttpPut("modules/{id}")]
public async Task<IActionResult> UpdateModule(int id, UpdateModuleCommand command)
{
    if (command.Id != id)
        return BadRequest("Module ID mismatch.");

    await _mediator.Send(command);
    return Ok(new { message = "Module updated successfully" });
}
```

#### 7. **Evitando Regressões**

Antes de fazer mudanças:

```
✅ LEIA:
  □ Este README completamente
  □ A funcionalidade similar
  □ Os testes existentes
  □ As validações relacionadas

✅ PERGUNTE A SI MESMO:
  □ Isto quebra alguma entidade?
  □ Isto quebra alguma relação?
  □ Isto quebra algum endpoint existente?
  □ O validator está completo?
  □ A migration é reversível?
  □ Os DTOs incluem novos campos?

✅ TESTE:
  □ Swagger /docs
  □ Validação de entrada
  □ Relacionamentos entidades
  □ Comportamento sem campos opcionais
```

#### 8. **Aseguindo as Convenções**

```
✅ FAZER:
  - Nomes em PascalCase (Commands, Classes)
  - Nomes de colunas em snake_case (BD)
  - XML comments em tudo público
  - Validators em arquivo separado
  - DTOs com `record` imutável
  - Lógica em Handlers, não em Controllers
  - Usar Factory Methods nas entidades
  - Audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
  - Soft delete via `Active` flag
  - Injetar interfaces, não implementações

❌ NÃO FAZER:
  - Lógica de negócio em Controllers
  - Queries N+1 (sem Include/Select)
  - DbContext em lógica de negócio
  - Entidades sem Factory Methods
  - Commands sem Validators
  - DTOs mutáveis
  - Hardcode valores/strings
  - Ignorar soft delete
  - Relação direta entre camadas
  - Múltiplos SaveChangesAsync na mesma transação
```

#### 9. **Entendendo o Pipeline MediatR**

```
Request entra no Controller
    ↓
MediatR recebe via _mediator.Send()
    ↓
ValidationBehavior (Primeira na pipeline)
    → Corre todos os Validators do command
    → Se há erro: lança ValidationException
    → Middleware captura e retorna 400
    ↓
LoggingBehavior
    → Registra entrada (nome do command + dados)
    ↓
Handler (IRequestHandler<,>)
    → Executa a lógica de negócio
    → Pode lançar exceções
    ↓
LoggingBehavior
    → Registra saída (resposta)
    ↓
GlobalExceptionMiddleware (se houver erro)
    → Captura e formata resposta de erro
    ↓
Response retorna ao cliente
```

#### 10. **Ordem Correta de Implementação**

Ao implementar uma nova feature, seguir esta ordem:

```
1️⃣  Entidade (Domain)
   └─ Adicionar propriedade
   └─ Atualizar Factory
   
2️⃣  Validator (Application)
   └─ Criar validações
   
3️⃣  Command/Query (Application)
   └─ Definir Command record
   └─ Criar Handler
   └─ Adicionar DTOs
   
4️⃣  Configuration (Infrastructure)
   └─ Mapear coluna no BD
   
5️⃣  Migration (Infrastructure)
   └─ Gerar migration
   └─ Revisar SQL
   
6️⃣  Controller (Api)
   └─ Criar action
   └─ Injetar IMediator
   └─ Executar via Send()
   └─ Adicionar Swagger docs
   
7️⃣  Teste (opcional)
   └─ Testar no Swagger
   └─ Validar request/response
```

---

## Adicionando Novas Features

### Template: Nova Feature Passo a Passo

**Objetivo: Adicionar campo `IsFeatured` em `Course`**

```bash
# 1. Atualizar Domain
# Arquivo: Nexas.Domain/Entities/Course.cs
# - Adicionar: public bool IsFeatured { get; set; } = false;
# - Atualizar Factory Method

# 2. Criar Command
# Arquivo: Nexas.Application/Courses/Commands/FeatureCourse/FeatureCourseCommand.cs
# - record FeatureCourseCommand : IRequest<Unit>
# - handler FeatureCourseCommandHandler
# - Lógica: marca curso como featured

# 3. Criar Validator
# Arquivo: Nexas.Application/Courses/Commands/FeatureCourse/FeatureCourseCommandValidator.cs
# - RuleFor(v => v.CourseId).GreaterThan(0);

# 4. Configurar EntityConfiguration
# Arquivo: Nexas.Infrastructure/Persistence/Configurations/EntityConfigurations.cs
# - builder.Property(c => c.IsFeatured).HasColumnName("is_featured");

# 5. Gerar Migration
dotnet ef migrations add AddIsFeaturedToCourses --project Nexas.Infrastructure

# 6. Revisar migration
# Verificar que adicionou coluna corretamente

# 7. Atualizar Query
# Arquivo: Nexas.Application/Courses/Queries/GetCourses/GetCoursesQuery.cs
# - Adicionar IsFeatured no CourseDto
# - Adicionar na projeção Select

# 8. Criar Controller Action
# Arquivo: Nexas.Api/Controllers/CoursesController.cs
# [Authorize(Roles = "Admin")]
# [HttpPatch("{id}/feature")]
# public async Task<IActionResult> FeatureCourse(int id, FeatureCourseCommand command)
# {
#     if (command.CourseId != id) return BadRequest();
#     await _mediator.Send(command);
#     return Ok();
# }

# 9. Testar no Swagger
# - Ir para /swagger/index.html
# - Fazer request POST /courses/{id}/feature
# - Verificar 200 OK
# - Verificar GET /courses retorna IsFeatured = true
```

---

## Resumo de Comandos Úteis

```bash
# Build
dotnet build

# Run API
dotnet run --project Nexas.Api

# Gerar migrations
dotnet ef migrations add <MigrationName> --project Nexas.Infrastructure

# Aplicar migrations
dotnet ef database update --project Nexas.Infrastructure

# Listar migrations
dotnet ef migrations list --project Nexas.Infrastructure

# Remover última migration
dotnet ef migrations remove --project Nexas.Infrastructure

# Executar testes
dotnet test

# Publicar
dotnet publish -c Release -o ./publish
```

---

## Configuração de Banco de Dados

### Appsettings - Conexão

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nexas_db;Uid=root;Pwd=password;"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-256-bits-for-hs256-algorithm",
    "Issuer": "YourAuthProvider",
    "Audience": "YourApiUsers"
  }
}
```

### Criar Banco MySQL

```sql
CREATE DATABASE nexas_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE nexas_db;
```

### Aplicar Migrations

```bash
dotnet ef database update --project Nexas.Infrastructure
```

---

## Conclusão

Este documento fornece documentação técnica profunda e completa do projeto Nexas. A IA ou desenvolvedor pode consultá-lo como referência para:

✅ **Entender a arquitetura** - Clean Architecture + CQRS  
✅ **Implementar novas features** - Seguindo padrões estabelecidos  
✅ **Alterar entidades** - Com migrations e validações  
✅ **Manter compatibilidade** - Sem quebrar endpoints existentes  
✅ **Evitar regressões** - Verificando dependências  
✅ **Documentar código** - XML comments e Swagger  

O projeto é escalável, mantível e segue Best Practices de .NET.




### 1) Estorno de Compra (Direito de Arrependimento - 7 dias)

Rota: `POST /api/purchases/{purchaseId}/refund`

Autenticação: Obrigatória (`Authorization: Bearer <token>`)

Request: Body vazio. O `purchaseId` é passado via rota e o usuário é identificado pelo claim do JWT.

Response (200 OK):

```json
{
    "success": true,
    "message": "Estorno processado e matrícula cancelada com sucesso."
}
```

Regras: o handler valida que a compra existe, pertence ao usuário autenticado e que a data de criação da compra é menor ou igual a 7 dias (direito de arrependimento). Em caso afirmativo o sistema chama o serviço Asaas para estornar o pagamento e remove a matrícula associada.

### 2) Assinatura com Trial de 7 Dias

Rota de criação: `POST /api/subscriptions`

Autenticação: Obrigatória

Request: JSON com `planName`, `amount` e dados de pagamento (se necessário).

Comportamento: ao criar a assinatura o sistema grava a `Subscription` com `IsActive = true` e `StartDate = now`. Na integração com Asaas a chamada de criação inclui `nextDueDate = now + 7 days` para que a primeira cobrança ocorra apenas no oitavo dia do trial. O ciclo subsequente continua mensalmente.

Response (201 Created): exemplo já documentado na seção "Criar Assinatura".

### 3) Cancelamento de Assinatura (dentro do trial)

Rota: `POST /api/subscriptions/{subscriptionId}/cancel`

Autenticação: Obrigatória

Request: Body vazio. O `subscriptionId` é passado via rota e o usuário é identificado pelo claim do JWT.

Response (200 OK):

```json
{
    "success": true,
    "message": "Assinatura cancelada com sucesso."
}
```

Regras: o handler valida que a assinatura pertence ao usuário e que o cancelamento ocorre dentro dos primeiros 7 dias a partir de `StartDate`. Se válido, a API chama o Asaas para cancelar a assinatura e inativa a assinatura no banco (`IsActive = false`, `EndDate` preenchida).

### 4) Listar Compras do Usuário Autenticado

Rota: `GET /api/purchases/my-purchases`

Autenticação: Obrigatória

Response (200 OK):

```json
[
    {
        "purchaseId": 152,
        "courseId": 42,
        "courseTitle": "Desenvolvimento Web Avançado com Angular",
        "amount": 197.90,
        "purchasedAt": "2026-05-20T14:30:00Z",
        "status": "APPROVED"
    }
]
```

O handler identifica o usuário pelo token e retorna compras/matrículas ativas associadas, incluindo dados básicos do curso.

### 5) Obter Detalhes da Assinatura do Usuário Autenticado

Rota: `GET /api/subscriptions/my-subscription`

Autenticação: Obrigatória

Response (200 OK):

```json
{
    "subscriptionId": "sub_83hd82jns8",
    "status": "ACTIVE",
    "startDate": "2026-05-01T10:00:00Z",
    "nextDueDate": "2026-06-01T10:00:00Z",
    "planName": "Plano Premium Mensal",
    "lastCharges": [
        {
            "chargeId": "pay_92jd73j",
            "amount": 89.90,
            "status": "CONFIRMED",
            "paymentDate": "2026-05-01T10:05:00Z"
        }
    ]
}
```

O handler retorna a assinatura ativa ou a mais recente do usuário, juntamente com o histórico de cobranças (se houver) e a estimativa de `nextDueDate` gerada a partir do último pagamento ou do `StartDate`.

### Observações Técnicas

O projeto utiliza o `CurrentUserService` e o `UserContextService` para extrair o `ExternalId`/ID do usuário a partir dos claims do JWT e materializar o `User` no banco, garantindo que todos os handlers identifiquem corretamente o usuário autenticado.

**Versão do .NET**: 8.0  
**Banco de Dados**: MySQL 8.0+  
**ORM**: Entity Framework Core 8.0

### Integração Asaas e SignalR (Novas Funcionalidades)

#### 1) Verificação de Pendências no Checkout
Rota: GET /api/v1/financeiro/checkout/pendencias
- **Função**: Retorna se o usuário possui pendências de pagamento (PIX ou cartão rejeitado) e trava o checkout no frontend, ou retorna se o usuário já possui o curso/assinatura ativo, protegendo a rota.

#### 2) Webhook do Asaas
Rota: POST /api/v1/webhooks/asaas
- **Função**: Recebe os payloads do Asaas (PAYMENT_RECEIVED, PAYMENT_CONFIRMED, PAYMENT_REFUNDED, PAYMENT_DELETED).
- **Regra de Negócio**: Utiliza CQRS (ProcessAsaasWebhookCommand) para atualizar o status das tabelas Purchases ou SubscriptionPayments.
- **Regra de Revogação**: Estornos de assinaturas apenas removem a flag IsActive do usuário na tabela Subscriptions, preservando cursos avulsos antigos.

#### 3) WebSockets em Tempo Real
Hub: /hubs/payment
- **Função**: Integrado nativamente no backend via **SignalR**.
- **Comportamento**: Assim que o Webhook do Asaas aprova/cancela um pagamento, o servidor dispara mensagens WebSocket (PaymentConfirmed ou PaymentRefunded) diretamente e de forma exclusiva para o UserId autenticado.
