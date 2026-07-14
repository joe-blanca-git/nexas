# Portal Pan (Frontend) - Documentação Oficial

Bem-vindo à documentação oficial do frontend do **Portal Pan**. Este documento contém a visão geral completa da arquitetura, fluxo funcional, organização estrutural e guia de implementação para desenvolvedores e agentes de IA, cobrindo todo o ciclo de vida do projeto.

---

## Índice
1. [Visão Geral](#1-visão-geral)
2. [Arquitetura](#2-arquitetura)
3. [Stack Técnica](#3-stack-técnica)
4. [Estrutura de Pastas](#4-estrutura-de-pastas)
5. [Páginas da Aplicação](#5-páginas-da-aplicação)
6. [Componentes Compartilhados](#6-componentes-compartilhados)
7. [Fluxos Funcionais](#7-fluxos-funcionais)
8. [Camada de Dados](#8-camada-de-dados)
9. [Regras de Negócio](#9-regras-de-negócio)
10. [Dependências entre Módulos](#10-dependências-entre-módulos)
11. [Configuração do Ambiente](#11-configuração-do-ambiente)
12. [Convenções do Projeto](#12-convenções-do-projeto)
13. [Pontos de Atenção](#13-pontos-de-atenção)
14. [Contexto para Agentes de IA](#14-contexto-para-agentes-de-ia)

---

## 1. Visão Geral

- **Objetivo do Projeto:** O Portal Pan é uma plataforma interativa para gerenciar e disponibilizar trilhas de conhecimento, cursos, certificações, módulos financeiros, fóruns de discussão e suporte, unificando a experiência educacional e comunitária em um só ambiente.
- **Problema que Resolve:** Centraliza o consumo de conteúdo EaD, gerenciamento financeiro do aluno e interação social (fórum), evitando que o usuário precise acessar múltiplas plataformas.
- **Público-alvo:** Estudantes, assinantes e usuários em busca de educação, certificação e troca de conhecimento através da comunidade (fórum).
- **Principais Funcionalidades:**
  - Login e recuperação de senha.
  - Consumo de cursos e visualização de aulas (`lesson-viewer`).
  - Emissão e listagem de certificados.
  - Gestão de faturas, pagamentos e carrinho (módulo financeiro).
  - Fórum de discussões interativo.
  - Central de suporte.
- **Tecnologias Base:** Angular 18 com Server-Side Rendering (SSR) suportado por Node.js/Express.

---

## 2. Arquitetura

A aplicação foi projetada no padrão modularizado (Feature-based Modular Architecture) com forte separação de responsabilidades (SoC).
- **Fluxo Geral:** Usuário acessa o app $\rightarrow$ Guardião de Rotas (AuthGuard) verifica autenticação $\rightarrow$ Carregamento da Rota Lazy (Ex: Courses) $\rightarrow$ Consumo de APIs via interceptors e BaseService $\rightarrow$ Exibição na UI (Bootstrap 5).
- **SSR (Server-Side Rendering):** Utilizado via `@angular/ssr` e Express para otimização de SEO e tempo de carregamento (First Contentful Paint).
- **Estratégias de Reutilização:** Módulos independentes orquestram seus próprios componentes locais, mas dependem do módulo genérico `shared/` para pão-de-forma (breadcrumbs), menus, alertas e UI.
- **Padrões:**
  - *Smart/Dumb Components:* A maioria das páginas em `features/modules/.../pages` atua como "Smart", lidando com serviços, estados e fluxos de dados, enquanto itens de `shared/components` são orientados puramente por `@Input()` e `@Output()`.

### Arquitetura em Diagrama
```mermaid
graph TD
  User((Usuário)) --> AppRoute[App.Routes]
  AppRoute -- "/auth" --> AuthModule[Auth Module]
  AppRoute -- "/" --> CoreGuard[AuthGuard]
  CoreGuard -- Aprovado --> Layout[App Component Layout]
  Layout --> Home[Home Module]
  Layout --> Courses[Courses Module]
  Layout --> Forum[Forum Module]
  Layout --> Financial[Financial Module]
  
  Courses --> BaseService[BaseService/Interceptors]
  Forum --> BaseService
  
  BaseService --> API_NEXAS[API Nexas]
  BaseService --> API_AGIVYS[API Agivys]
```

---

## 3. Stack Técnica

- **Framework Core:** Angular v18.2.0 (SPA)
- **SSR (Server-Side Rendering):** `@angular/ssr`, Express e Node.js
- **UI Framework & Estilização:** 
  - Bootstrap v5.3.8
  - Bootstrap Icons e FontAwesome 7
  - SCSS/SASS para estilizadores modulares e customização (`custom-theme/`).
- **Gerenciamento de Estado:** `@ngrx/store` (v18.1.1)
- **Roteamento:** Angular Router (Lazy Loading)
- **Websockets / Realtime:** `@microsoft/signalr` v10.0 (Serviços em tempo real e fórum/notificações)
- **Utilitários Adicionais:** `jspdf` (para exportação/certificados), `rxjs` (reatividade).
- **Build e Ferramentas:** Angular CLI, Node 18+, Karma/Jasmine para testes (Configuração base gerada).

---

## 4. Estrutura de Pastas

A organização segue a divisão lógica recomendada pela equipe do Angular:

- **`src/app/core/`**: O "coração" da aplicação. Possui tudo o que é carregado uma única vez na inicialização (Singleton services).
  - **`auth/`**: Gerencia tokens e sessões.
  - **`guards/`**: Regras de proteção de rotas (ex: `auth.guard.ts`).
  - **`interceptors/`**: Tratamento genérico de HTTP (erros, injeção de tokens).
  - **`services/`**: Serviços transversais (ex: `base.service.ts`, `signalr.service.ts`).
- **`src/app/features/`**: Funcionalidades agrupadas por contexto de negócios (Domain Driven).
  - **`auth/`**: Fluxo de autenticação, login, registro, troca de senha.
  - **`modules/`**:
    - `certificates`: Exibição e listagem de certificações do aluno.
    - `courses`: Catálogo de cursos, visualizador de aulas.
    - `financial`: Carrinho, histórico de pagamento, faturas.
    - `forum`: Discussões, tópicos, postagens e interações.
    - `home`: Dashboards do usuário e consumo de notícias.
    - `support`: Tickets e ajuda técnica.
- **`src/app/shared/`**: Recursos compartilhados por toda a aplicação (sem injeção de estado complexo).
  - `components`: `toast`, `breadcrumb`, menu lateral.
- **`custom-theme/`**: Configurações globais de tokens SCSS (`theme.scss`).

---

## 5. Páginas da Aplicação

### Módulo de Autenticação (`/auth`)
- **Login (`/auth/login`):** Tela inicial de acesso. Chama a API de login e delega o token ao `AuthUtil`.
- **Register (`/auth/register`):** Formulário de inscrição.
- **Recovery Password (`/auth/recovery-password`):** Recuperação via email.
- **Update Password (`/auth/update-password`):** Tela ativada via deep link para reset de credencial.

### Módulo Home (`/home`)
- **Dashboard (`/home`):** Página principal pós-login. Exibe métricas, progresso e notícias. Usa o `HomeDashboardComponent`.
- **News Detail (`/news-detail/:id`):** Componente para exibir conteúdo detalhado da plataforma de notícias ou avisos.

### Módulo de Fórum (`/forum`)
- **Fórum Home (`/forum`):** Exibe métricas (Total de Tópicos, Respostas, Sem respostas), categorias e filtro em abas (Todos, Recentes, Meus Tópicos). Clicar no card "Sem Resp." aplica um filtro imediato nos tópicos que possuem 0 respostas.
- **Fórum Topic (`/forum/topic/:id`):** Visualização detalhada de um tópico. O usuário pode ler respostas, enviar respostas (via SignalR opcionalmente) e, se dono, marcar como resolvido ou reabrir.

### Módulo de Cursos (`/courses`)
- **Home de Cursos (`/courses`):** Catálogo geral com progresso.
- **Detalhes do Curso (`/courses/:id`):** Descritivo de ementa.
- **Lesson Viewer (`/courses/lesson/:id`):** Player de aula, marcação de concluído.

### Módulo Financeiro (`/financial`)
- **Home Financeiro (`/financial`):** Faturas em aberto e histórico pago.
- **Pagamento e Carrinho:** Checkout de produtos/planos, conexão com gateway de pagamento.

---

## 6. Componentes Compartilhados

Componentes mantidos na pasta `src/app/shared/components/`:
- **`BreadcrumbComponent`:** Lê as rotas ativas ou recebe um array via `@Input()` para montar a navegação de retorno.
- **`ToastComponent`:** Mostra mensagens flutuantes e efémeras para sucesso/erro. Operado centralmente via `ToastService` em `core/services/toast.service.ts`.
- **`MenuSideComponent`:** Barra de navegação responsiva esquerda. Construída recebendo os estados de permissão.

---

## 7. Fluxos Funcionais

### Fluxo de Autenticação e Sessão
1. O usuário entra em `/auth/login`.
2. Envia credenciais. A API valida e devolve um token JWT.
3. O `AuthUtil` salva o JWT em **cookies/localStorage**.
4. O usuário é redirecionado para `/`.
5. O `AuthGuardService` avalia se existe token válido; se não, bloqueia e redireciona ao `/auth`.
6. O `BaseService` injeta o token (`GetAuthHeaderJson`) automaticamente em todas as chamadas.

### Fluxo de Interação no Fórum
1. Acesso à home do Fórum. O serviço carrega categorias e topicos (`IForumTopicSummaryDto`).
2. Cálculos de UI mapeiam os *status* ("Resolvido", "Sem resposta", "Em andamento") dependendo da resposta da API (que checa `status` do tópico e quantidade `replyCount`).
3. Ao visualizar um tópico, o dono do tópico visualiza regras de negócio exclusivas (ex: botões de Encerrar/Reabrir tópico).

---

## 8. Camada de Dados

- **Base Service (`core/services/base.service.ts`):** Classe abstrata fundamental. Todos os serviços assíncronos (`ForumService`, `HomeService`, etc.) herdam dela. Ela lida com a montagem padronizada de *Headers* (JSON, URL Encoded, Tokens) através das URLs globais (Nexas, Agivys, Auth).
- **Interceptors:** `error-interceptor.ts` escuta requisições falhas de modo global. Falhas de Token (`401`) limpam o cookie e despacham o usuário para o login.
- **Gerenciamento State Local:** Uso primário do paradigma de Promessas (`async/await`) somado a `firstValueFrom` encapsulando fluxos RxJS.
- **Gerenciamento State Global:** A infraestrutura prevê o NGRX Store (`@ngrx/store`), ideal para cenários altamente reativos (como dados de notificação recebidos via SignalR).

---

## 9. Regras de Negócio

- **Autenticação:** O sistema desloga automaticamente em retornos `401 Unauthorized`.
- **Fórum (Status e Propriedade):**
  - Botão "Marcar como Resolvido" só surge se `topic.isOwn == true` e se não estiver "Resolved".
  - Botão "Reabrir Tópico" só surge se `topic.isOwn == true` e o status for "Resolved".
  - O cálculo do card "Sem Resposta" filtra tópicos cuja propriedade `replyCount === 0`.
- **Navegação (Lazy Loading):** Partes do sistema só são carregadas em RAM e via rede se o usuário efetivamente clicar no link do menu, economizando tráfego de dados.

---

## 10. Dependências entre Módulos

O projeto adota uma arquitetura acoplada ao Core.
- O Módulo `Auth` não depende de nenhuma feature.
- Todos os módulos (`courses`, `forum`, `financial`) dependem estritamente do `CoreModule` e de `Shared`. Não há dependência cruzada (*circular dependency*) entre módulos (ex: `courses` não importa componentes de `forum`).

### Diagrama de Dependências
```mermaid
graph BT
  Shared[Shared Components]
  Core[Core / Services / Guards]
  
  Forum[Feature: Forum] --> Shared
  Forum --> Core
  
  Courses[Feature: Courses] --> Shared
  Courses --> Core
  
  Financial[Feature: Financial] --> Shared
  Financial --> Core
```

---

## 11. Configuração do Ambiente

O projeto Angular 18 é iniciado primariamente via CLI.
- **Arquivos de Ambiente (`core/environments/`):**
  - `environment.ts`: Contém configurações locais de APIs (`apiAgivysUrl`, `apiNexasUrl`).
  - `environment.prod.ts`: URLs produtivas substituídas no momento de build.
- **Scripts do `package.json`:**
  - `npm start`: Inicia o servidor local CSR (Client-Side) (`ng serve`).
  - `npm run watch`: Modo contínuo de build para desenvolvimento SSR.
  - `npm run serve:ssr:portal-pan`: Executa a compilação universal no lado do servidor via Node.
- **Processo de Deploy:** Envolve o build estático somado ao bundle de servidor SSR (`dist/portal-pan/server/server.mjs`), que deve ser invocado via host PM2 ou contêiner (Dockerfile incluído no root).

---

## 12. Convenções do Projeto

- **Nomenclatura (Arquivos):** `[nome-do-componente].component.ts`, `[contexto].service.ts`.
- **SCSS Modular:** Todo componente possui seu próprio arquivo SCSS encapsulado (ViewEncapsulation.Emulated). Estilos globais ficam restritos ao `custom-theme`.
- **Promessas em Serviços:** Embora Angular utilize muito RxJS de fábrica, o padrão do projeto em serviços (ex: `ForumService`) é o envelopamento de Observables em Promises: `await firstValueFrom(...)`. O objetivo é criar código imperativo mais legível em handlers de UI.
- **Componentes Standalone:** O projeto adere ao uso de componentes Standalone (`standalone: true`), descartando na maioria das pastas o uso dos antigos `NgModules`.

---

## 13. Pontos de Atenção

- **Tratamento de Tokens DTO:** Na conversão de DTOs nas respostas (como visto no `forum.service`), a flexibilidade nos nomes dos JSON properties da API (ex: `replyCount` vs `repliesCount`) requer validações defensivas constantes no frontend para evitar erros de renderização nula (`|| 0`).
- **SignalR:** Certifique-se de fechar conexões `HubConnection` no `ngOnDestroy` de componentes que dependam do `SignalrService`, a fim de evitar vazamento de memória.
- **Server Side Rendering (SSR):** Não utilizar artefatos que dependem puramente da API do navegador (ex: `window`, `document`) fora do block condicional `isPlatformBrowser`. Isso causará falhas severas de construção pelo SSR Node.js.

---

## 14. Contexto para Agentes de IA

Este guia é feito exclusivamente para Assistentes Autônomos (LLMs) operarem nesta base de código sem a necessidade de varredura prévia de toda a árvore de diretórios.

### Checklist Antes de Alterar Código
1. **Analise o Diretório Correto:** As páginas sempre residem em `src/app/features/modules/[nome-do-modulo]/pages`. Os serviços relacionados ficam em `.../[nome-do-modulo]/services`.
2. **Entenda o Serviço Base:** Todos os chamados de API devem passar por um Service do módulo correspondente. Este service **deve** estender a classe `BaseService` do `core`. Nunca injete `HttpClient` puro num componente de visualização.
3. **Respeite o CSS:** Evite classes utilitárias *ad-hoc* excessivas se não houver no Bootstrap. Respeite os blocos encapsulados SCSS do componente.

### Regras de Ouro da Implementação
- **Chamadas de API:** Devem utilizar `async / await` convertendo RxJS (`firstValueFrom`) dentro do serviço, devolvendo DTOs ou Models puros para os componentes.
- **Autorização/Status:** Checagens de regras de negócio de visualização (botões que devem sumir, cards filtrados) devem ser resolvidas primariamente por *getters* ou métodos públicos no arquivo `.ts` do componente. Não inclua muita lógica na interpolação `.html` (`*ngIf="longa regra && blabla"`).
- **Standalone:** O projeto usa o Angular moderno. Todo novo componente criado (`ng g c ...`) deve ser standalone e gerenciar seus próprios *imports* de `CommonModule`, `FormsModule`, e `RouterModule`.

### Onde implementar Novas Features?
- **Novas páginas de contexto existente:** Crie dentro da sub-pasta `pages/` do módulo respectivo e cadastre na variável `routes` (`.routes.ts`) do próprio módulo.
- **Novos módulos isolados:** Crie a pasta em `src/app/features/modules/`, prepare um `.routes.ts` com loadChildren sendo invocado do `src/app/app.routes.ts`.

---
*Documentação mantida e atualizada para o ecossistema Portal Pan.*
