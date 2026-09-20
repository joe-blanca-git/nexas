# Arquitetura

## Stack

- **Angular 18.2**, standalone components (sem NgModules), lazy loading via `loadComponent`
  em todas as rotas.
- **Signals** para estado reativo (`signal()`, não `BehaviorSubject`) — ver `StateUtil`,
  `ThemeService`, `ToastService`, e os componentes novos (`AccountComponent`,
  `ProfileTabComponent`).
- Sintaxe de template moderna: `@if`/`@else`, `@switch`/`@case`, `@for` — não usar
  `*ngIf`/`*ngFor` em código novo (o código legado do login ainda usa `*ngIf` em alguns
  lugares, não precisa migrar, só não replicar em código novo).
- **SSR** via `@angular/ssr` + Express (`server.ts`). Todo código que toca `document`,
  `window`, `sessionStorage`, `localStorage` precisa checar `isPlatformBrowser(platformId)`
  primeiro — ver `AuthService`, `AuthUtil`, `ThemeService` como referência.
- **RxJS** só nas chamadas HTTP (services que estendem `BaseService`); estado de UI é sempre
  Signal, não Observable.
- **Reactive Forms** (`FormBuilder`/`FormGroup`) em todo formulário — não usar template-driven
  forms.
- SCSS puro por componente (`styleUrl`), sem framework de CSS (não é Tailwind, não é Material).

## Backend

O portal fala com **`Nexas.SystemManager`** (`backend/Nexas.SystemManager` no monorepo), uma
API ASP.NET Core 8 com Identity. Ver [auth.md](auth.md) para os detalhes de autenticação —
é a parte mais não-óbvia do projeto.

- `environment.ts` define `apiNexasUrl` e `apiNexasAuthUrl`, ambos apontando para
  `http://localhost:5179` em dev. Não existe `environment.prod.ts` nem `fileReplacements`
  configurado ainda (ver [known-issues.md](known-issues.md)).
- `BaseService` (em `core/services/base.service.ts`) é a classe-base de todo service HTTP:
  expõe `urlApiNexas`/`urlApiNexasAuth`, `GetHeaderJson()` (sem auth) e `GetAuthHeaderJson()`
  (com `Authorization: Bearer <token>`, lendo o cookie via `AuthUtil`).

## Estrutura de pastas

```
src/app/
├── core/
│   ├── auth/
│   │   ├── auth.service.ts        # login, register, google-login, logout, rehydrate
│   │   ├── auth.util.ts           # cookie do token + decode JWT (client-side, sem chamar API)
│   │   └── guards/
│   │       ├── auth.guard.ts      # exige token válido, senão desloga e manda pro /login
│   │       └── guest.guard.ts     # bloqueia acesso logado a telas de visitante
│   ├── models/
│   │   └── userLoged.model.ts     # UserLogedModel: email, name, username, id, roles
│   ├── services/
│   │   ├── base.service.ts        # classe-base com URLs e headers
│   │   ├── profile.service.ts     # GET/PUT /api/v1/auth/profile
│   │   ├── theme.service.ts       # dark/light, persistido em localStorage
│   │   └── toast.service.ts       # toasts globais via signal
│   └── utils/
│       └── UserState.util.ts      # StateUtil: signal com o usuário logado (em memória)
├── feature/
│   ├── auth/pages/                # login, register, forgot-password, reset-password
│   └── dashboard/pages/
│       ├── home/                  # cards de navegação (Organizações, Aplicações)
│       ├── organizations/         # stub "em construção"
│       ├── applications/          # stub "em construção"
│       └── account/               # painel de abas — ver pages-and-components.md
│           └── components/profile-tab/  # única aba com conteúdo real
└── shared/components/
    ├── navbar/                    # topo: logo, nav links, tema, dropdown de perfil/logout
    ├── footer/                    # rodapé simples
    └── toast/                     # renderiza o toast atual do ToastService
```

**Importante**: `navbar` e `footer` **não são globais** — cada página que precisa deles os
inclui explicitamente no próprio template (`<app-navbar>`/`<app-footer>`). Não existe um
"app-shell" chapado no `app.component.html` (que só tem `<router-outlet>` + `<app-toast>`).
Ao criar uma página nova do dashboard, lembre de incluir navbar/footer manualmente, senão
a página fica "solta" sem navegação.

## Roteamento

Todas as rotas em `app.routes.ts`, com lazy loading (`loadComponent`). Duas guards:

- `authGuard` — nas rotas que exigem login (`/organizations`, `/applications`, `/account`).
- `guestGuard` — nas rotas só-de-visitante (`/login`, `/register`, `/forgot-password`,
  `/reset-password`) — redireciona pra `/` se já houver cookie.

A rota `/` (home) **não tem guard nenhum** — é acessível deslogado (mostra "Usuário" como
placeholder de nome). Isso é proposital, não esquecimento — a home funciona como landing
tanto pra quem já tem conta quanto pra quem não tem.

Não existe rota coringa (`**`)/página 404. Uma rota inexistente lança
`NG04002: Cannot match any routes` (erro não tratado do Router) — se for adicionar um link
novo em qualquer lugar (navbar, card, botão), **crie a rota correspondente antes**, mesmo que
a página seja só um stub "em construção" (padrão: copiar `organizations.component.*`).

## Estado do usuário logado

`StateUtil` (signal) guarda `UserLogedModel | null` **em memória** — some em qualquer reload
de página. Por isso existe rehydratação:

1. No boot do app, um `APP_INITIALIZER` (`app.config.ts`) chama
   `AuthService.rehydrateUserState()`.
2. Esse método lê o cookie, decodifica o JWT (client-side, via `AuthUtil.decodeToken`) e
   popula o `StateUtil` com nome/e-mail/roles vindos das claims.
3. Como otimização, ele também guarda uma cópia em `sessionStorage['nexas_user']` e prefere
   ler de lá se existir — **cuidado**: se você atualizar algo no perfil (ex.: nome), precisa
   atualizar esse cache manualmente também (ver `ProfileTabComponent.onSubmit`), senão um F5
   reverte a exibição pro valor antigo.

Se for adicionar um campo novo ao usuário exibido na UI (ex.: avatar), o padrão é:
`StateUtil.updateUser({ campo: valor })` (merge parcial) + atualizar o cache do
`sessionStorage` do mesmo jeito.
