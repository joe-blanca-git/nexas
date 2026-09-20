# Autenticação — leia isto antes de mexer em login/token/roles/perfil

Este é o assunto mais enganoso do projeto porque o histórico do backend tem duas tentativas
anteriores conflitantes (uma delas revertida) antes de chegar no estado atual. Se você não ler
isto e tentar "consertar" a autenticação do zero, é bem provável que reintroduza um bug já
corrigido.

## Como funciona hoje (backend `Nexas.SystemManager`)

O backend usa **ASP.NET Core Identity** (`AddIdentityApiEndpoints<NexasUser>()` +
`MapIdentityApi<NexasUser>()`) — isso dá de graça os endpoints `/login`, `/refresh`,
`/register` (customizado, ver abaixo), `/forgotPassword`, `/resetPassword`, `/confirmEmail`,
`/manage/info`, etc., todos sob `/api/v1/auth`.

**Por padrão**, o esquema `Identity.Bearer` do Identity emite tokens **opacos** (criptografados
via Data Protection, não-decodificáveis no cliente) — incompatível com o que o frontend
(`AuthUtil`) sempre esperou (um JWT de verdade).

A solução usada (**não mude isso sem entender o motivo**): em vez de registrar um segundo
esquema `JwtBearer` ao lado do `Identity.Bearer` (isso **já foi tentado e revertido** — ver
comentário histórico ainda presente perto da configuração do Identity em
`backend/Nexas.SystemManager/Program.cs` —, porque registrar os dois esquemas juntos faz o
esquema padrão virar `JwtBearer`, e o token opaco do `/login` do Identity para de autenticar
qualquer coisa), o projeto **substitui só o `BearerTokenProtector`** do esquema
`Identity.Bearer` por um formatador próprio:

- `backend/Nexas.SystemManager/Services/JwtTicketDataFormat.cs` — implementa
  `ISecureDataFormat<AuthenticationTicket>`. `Protect()` gera um JWT de verdade (HS256, chave
  em `Jwt:Key`/`Jwt:Issuer`/`Jwt:Audience` no `appsettings`, mesma seção usada pelas outras
  APIs Nexas como `Nexas.Lemon.Admin.Api`). `Unprotect()` valida e decodifica esse JWT de
  volta pra um `ClaimsPrincipal`.
- Registrado em `Program.cs` via
  `services.PostConfigure<BearerTokenOptions>(IdentityConstants.BearerScheme, opts => opts.BearerTokenProtector = new JwtTicketDataFormat(config))`
  — **`PostConfigure`, não `Configure`**: o Identity já registra o protector padrão via
  `Configure`, e `PostConfigure` roda depois, garantindo que o nosso sempre vence,
  independente da ordem de registro no `Program.cs`.

Resultado: `/login`, `/refresh` e `/manage/info` (endpoints prontos do `MapIdentityApi`)
continuam funcionando exatamente como documentados no Swagger, só que emitindo/validando JWT
de verdade em vez de token opaco. `/google-login` (endpoint customizado em
`AuthEndpoints.cs`) usa o mesmo protector (pega ele via `IOptionsMonitor<BearerTokenOptions>`),
então também ganhou JWT de graça, sem precisar mudar o código dele.

### 🔴 Bug crítico já corrigido — não reintroduzir

O `BearerTokenHandler` do ASP.NET **não confia só na claim `exp` do JWT** pra saber se o token
expirou — ele confere `AuthenticationTicket.Properties.ExpiresUtc` (um campo separado, de
metadado do ticket, não do payload do JWT). Se `Unprotect()` devolver um ticket construído sem
preencher esse campo (ex.: `new AuthenticationTicket(principal, scheme)`, sem
`AuthenticationProperties`), **todo request autenticado falha com 401** ("Unprotected token
failed"), mesmo com um JWT perfeitamente válido e assinado corretamente. É um erro silencioso
e enganoso — não aparece nenhuma exceção, só uma mensagem genérica de log.

A correção (já aplicada em `JwtTicketDataFormat.Unprotect`): usar o `validatedToken.ValidTo`
(devolvido por `JwtSecurityTokenHandler.ValidateToken`) pra popular
`new AuthenticationProperties { ExpiresUtc = validatedToken.ValidTo }` e passar isso pro
`AuthenticationTicket`. Se você reescrever esse método por qualquer motivo, **não esqueça
desse campo** — é a causa mais provável de "autenticação quebrada do nada" nesse projeto.

Se precisar depurar autenticação de novo: rode uma instância isolada do backend
(`dotnet Nexas.SystemManager.dll` numa porta diferente, `ASPNETCORE_ENVIRONMENT=Development`)
e teste com `curl` + um JWT assinado manualmente com a mesma chave do `appsettings.Development.json`
(`Jwt:Key`) — é mais rápido e mais confiável que testar pela UI.

## Formato das claims no JWT

`JwtTicketDataFormat.Protect` **limpa o `OutboundClaimTypeMap`** do
`JwtSecurityTokenHandler` antes de gerar o token — isso preserva os claim types exatamente como
o Identity os gera por padrão, que são os **URIs longos** do `System.Security.Claims.ClaimTypes`
(ex.: `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`, não `nameid`
curto). Isso importa porque:

- O `AuthUtil.decodeToken()` do frontend lê exatamente esses URIs longos
  (`ClaimTypes.NameIdentifier`, `ClaimTypes.Email`, `ClaimTypes.Name`,
  `ClaimTypes.Role`/`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) — não são
  "nameid"/"email" curtos.
- `CurrentUserService.cs` (backend, usado por `ICurrentUserService`) faz fallback pra ambos os
  formatos (curto primeiro, longo depois) — funciona hoje porque o Identity gera o formato
  longo por padrão, mas os comentários no arquivo mostram que ele foi escrito supondo um
  cenário `MapInboundClaims = false` que **não está ativo** nesse esquema (`Identity.Bearer`
  não usa `JwtBearerOptions`, então essa flag não se aplica aqui — só é usada de fato nas
  outras APIs Nexas, tipo `Nexas.Lemon.Admin.Api`).
- A claim de **Name** do Identity, por padrão, usa o `UserName` do usuário — que neste projeto
  é **sempre o e-mail** (definido assim tanto no `/register` quanto no `/google-login`). Ou
  seja, sem intervenção, a claim "Name" mostraria o e-mail, não o nome de verdade.
  `NexasUserClaimsPrincipalFactory.cs` (backend) resolve isso: sobrescreve a claim de Name com
  `user.FullName` sempre que ele existir. Está registrado em `Program.cs` como
  `IUserClaimsPrincipalFactory<NexasUser>`.

## Roles

`ApplicationsController` usa `[Authorize(Roles = "Admin,Owner")]`. Como os claims de role do
Identity já vêm no formato longo (`ClaimTypes.Role`) e não há remapeamento configurado nesse
esquema, isso funciona sem configuração extra — diferente de `Nexas.Lemon.Admin.Api`, que
precisa setar `RoleClaimType = ClaimTypes.Role` explicitamente porque usa JWT bearer "de
verdade" com `MapInboundClaims = false`.

## Fluxo no frontend

1. `LoginComponent`/`RegisterComponent` chamam `AuthService.logIn()`/`.register()`.
2. No `next` da resposta: `AuthUtil.saveCookieAuth(response)` — **atenção ao nome do campo**:
   o `/login` padrão do Identity devolve `{ tokenType, accessToken, expiresIn, refreshToken }`
   (sem campo `token`); só o `/google-login` customizado devolve `token` como alias. O código
   lê `response.accessToken || response.token` — não reverta pra só `response.token`.
3. `AuthService.rehydrateUserState()` é chamado logo em seguida — decodifica o token e popula
   `StateUtil` + `sessionStorage['nexas_user']`.
4. Logout (`AuthService.logOut()`): remove o cookie, limpa `StateUtil`, remove o
   `sessionStorage`, navega pro `/login`. Isso já foi testado ponta-a-ponta e funciona
   corretamente — se um usuário disser "o logout não funciona", desconfie primeiro de outra
   causa (ex.: uma rota quebrada no meio do caminho lançando erro não tratado no Router) antes
   de mexer em `logOut()`.

## O que NÃO fazer

- Não registrar um segundo esquema de autenticação (`AddJwtBearer`) ao lado do
  `Identity.Bearer` sem também trocar o que `/login` emite — é exatamente o erro já cometido e
  revertido antes.
- Não reescrever `JwtTicketDataFormat.Unprotect` sem preencher
  `AuthenticationProperties.ExpiresUtc` (ver bug crítico acima).
- Não assumir que testar via browser é suficiente pra validar mudanças de auth no backend —
  o bug do `ExpiresUtc` só apareceu claramente testando com `curl` direto contra o backend,
  porque o erro no browser era só "não desloga"/comportamento estranho, sem pista clara.
