# Portal SM (Nexas)

Portal de acesso unificado do ecossistema Nexas: hub central de login que reúne autenticação, gestão de conta e navegação para as demais aplicações da plataforma (Organizações, Aplicações, etc.).

Gerado com [Angular CLI](https://github.com/angular/angular-cli) 18.2.21, standalone components + Signals, com SSR (Angular Universal / Express).

## Stack

- **Angular 18** — standalone components, `signal()`, `@if`/`@switch` no template
- **SSR** — `@angular/ssr`, servidor Express (`server.ts`), hidratação de cliente
- **SCSS** com design tokens em `src/styles.scss` (temas dark/light via `--variáveis` CSS)
- **Backend**: `Nexas.SystemManager` (ASP.NET Core Identity + JWT), ver `backend/Nexas.SystemManager`

## Rodando localmente

```bash
npm install
npm start          # ng serve → http://localhost:4200
```

Precisa do backend `Nexas.SystemManager` rodando (por padrão em `http://localhost:5179`, ver `src/environments/environment.ts`). Sem o backend no ar, login/registro e as chamadas da página "Minha Conta" falham.

```bash
npm run build                # build de produção em dist/portal-sm
npm run watch                 # build em modo watch (development)
npm run serve:ssr:portal-sm   # roda o servidor Express com o build SSR
npm test                      # testes unitários (Karma/Jasmine)
```

## Estrutura de pastas

```
src/app/
├── core/
│   ├── auth/            # AuthService, AuthUtil (cookie/JWT), guards
│   ├── models/          # UserLogedModel
│   ├── services/        # BaseService, ProfileService, ThemeService, ToastService
│   └── utils/           # StateUtil (estado do usuário logado, via Signals)
├── feature/
│   ├── auth/pages/      # login, register, forgot-password, reset-password
│   └── dashboard/pages/ # home, organizations, applications, account
└── shared/components/   # navbar, footer, toast
```

## Autenticação

- O backend (`Nexas.SystemManager`) usa ASP.NET Core Identity, mas com o `BearerTokenProtector` padrão substituído por um formatador JWT customizado (`JwtTicketDataFormat`, no backend) — o token emitido em `/login`, `/google-login` e `/refresh` é um JWT de verdade, com as claims padrão do Identity (`ClaimTypes.*`, formato longo).
- `AuthUtil` salva o token num cookie (`nexas_accessToken`) e decodifica as claims direto no cliente (sem chamar o backend) para popular `StateUtil`.
- `StateUtil` guarda o usuário logado (nome, e-mail, roles) em um `signal`, consumido pela navbar e pela home.
- **Rehydratação no boot**: `AuthService.rehydrateUserState()` roda via `APP_INITIALIZER` (`app.config.ts`) toda vez que o app inicia — sem isso, um F5 na página apagaria o nome do usuário da tela mesmo com o cookie válido.
- **Guards**:
  - `authGuard` — bloqueia rotas que exigem login (verifica validade do token; se inválido, desloga e redireciona pro `/login`).
  - `guestGuard` — bloqueia rotas só-de-visitante (login, registro, etc.) quando já existe um cookie válido, redirecionando pra home.
- Login com **Google Identity Services** (client-side) além do login tradicional e-mail/senha.

### Status das telas de auth

| Tela | Rota | Integração com backend |
|---|---|---|
| Login | `/login` | ✅ completo (e-mail/senha + Google) |
| Registro | `/register` | ✅ completo (e-mail/senha + Google) |
| Esqueci a senha | `/forgot-password` | ⚠️ só UI — simula com `setTimeout`, não chama a API ainda |
| Redefinir senha | `/reset-password` | ⚠️ só UI — simula com `setTimeout`, não chama a API ainda |

## Rotas

| Rota | Guard | Componente | Observação |
|---|---|---|---|
| `/` | — | `HomeComponent` | Home com cards de navegação (Organizações, Aplicações) |
| `/login` | `guestGuard` | `LoginComponent` | |
| `/register` | `guestGuard` | `RegisterComponent` | |
| `/forgot-password` | `guestGuard` | `ForgotPasswordComponent` | UI apenas |
| `/reset-password` | `guestGuard` | `ResetPasswordComponent` | UI apenas |
| `/organizations` | `authGuard` | `OrganizationsComponent` | Página "em construção" |
| `/applications` | `authGuard` | `ApplicationsComponent` | Página "em construção" |
| `/account` | `authGuard` | `AccountComponent` | Ver seção abaixo |

## Minha Conta (`/account`)

Painel com abas verticais à esquerda (grid 2x2 no mobile) e conteúdo à direita:

- **Perfil** ✅ implementado — formulário (`ProfileTabComponent`) com avatar, nome completo, telefone e e-mail:
  - Nome e telefone são editáveis e persistidos via `GET/PUT /api/v1/auth/profile`.
  - E-mail é somente leitura por enquanto (alterar e-mail exige fluxo de confirmação, ainda não implementado).
  - O botão de câmera no avatar abre um menu (Enviar foto / Usar câmera) com **preview local da imagem** — o envio da foto para o servidor **ainda não está implementado** (não há coluna de avatar no `NexasUser` nem endpoint de upload; a UI já avisa isso via toast ao selecionar uma imagem).
- **Segurança**, **Privacidade**, **Pagamento** — placeholders "em construção", sem funcionalidade ainda.

## Configuração de ambiente

`src/environments/environment.ts`:

```ts
export const environment = {
  production: false,
  apiNexasUrl: 'http://localhost:5179/api/',
  apiNexasAuthUrl: 'http://localhost:5179/api/v1/auth/'
};
```

Ainda não existe `environment.prod.ts` nem `fileReplacements` configurados no `angular.json` — o build de produção usa as mesmas URLs de desenvolvimento. Ajustar antes de um deploy real.

## Próximos passos conhecidos

- Upload real do avatar (nova coluna `AvatarUrl` no backend + endpoint de upload, reaproveitando `CloudflareStorageService`/`BunnyNetService` já existentes)
- Implementar as abas Segurança, Privacidade e Pagamento
- Integrar `forgot-password`/`reset-password` com a API real (`/forgotPassword`, `/resetPassword` do `MapIdentityApi`)
- Configuração de ambiente de produção
