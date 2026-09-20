# Inventário de páginas e componentes

Status real de cada parte — "funcional" significa que fala com o backend de verdade e
persiste dado; "UI apenas" significa que só simula (sem chamada real de API).

## Páginas

| Página | Rota | Status | Arquivo principal |
|---|---|---|---|
| Home | `/` | ✅ Funcional (cards de navegação + hero com nome do usuário) | `feature/dashboard/pages/home/` |
| Login | `/login` | ✅ Funcional (e-mail/senha + Google) | `feature/auth/pages/login/` |
| Registro | `/register` | ✅ Funcional (e-mail/senha + Google) | `feature/auth/pages/register/` |
| Esqueci a senha | `/forgot-password` | ⚠️ UI apenas (`setTimeout` simulando) | `feature/auth/pages/forgot-password/` |
| Redefinir senha | `/reset-password` | ⚠️ UI apenas (`setTimeout` simulando) | `feature/auth/pages/reset-password/` |
| Organizações | `/organizations` | 🚧 Stub "em construção", sem conteúdo, **sem navbar/footer** | `feature/dashboard/pages/organizations/` |
| Aplicações | `/applications` | ⚠️ Listar/criar/excluir/regenerar chave são reais; só editar falta — ver abaixo | `feature/dashboard/pages/applications/` |
| Detalhes da aplicação | `/applications/:id` | ⚠️ Vê dados + regenera chave + lista usuários finais; sem edição ainda — ver abaixo | `feature/dashboard/pages/application-detail/` |
| Minha Conta | `/account` | Parcial — ver abaixo | `feature/dashboard/pages/account/` |

### Aplicações (`/applications`) em detalhe

Tela pra o usuário cadastrar/gerenciar seus próprios sistemas que consomem os serviços da
Nexas (não confundir com as "aplicações Nexas" do banner da home — lá é o ecossistema Nexas
mostrando pra si mesmo; aqui é o usuário cadastrando algo dele). `ApplicationsComponent`
renderiza navbar + header (título + botão "Nova aplicação") + footer, com uma **lista
vertical de cards largos** (de propósito, não um grid — pedido explícito do usuário: "não
quero uma grid comum").

- **Listar, criar, excluir e regenerar chave já são reais** — `ApplicationsService`
  (`core/services/applications.service.ts`) chama `GET`/`POST /api/v1/Applications`,
  `DELETE /api/v1/Applications/{id}` e `POST /api/v1/Applications/{id}/api-key/regenerate`.
  Exclusão e regeneração usam modal de confirmação simples — "Tem certeza?" + Cancelar/Excluir
  (ambos reaproveitam as mesmas classes `.app-delete-modal`, apesar do nome, sem precisar
  digitar nada), diferente do modal de excluir *conta* que exige digitar o e-mail —
  proporcional ao risco de cada ação.
- **Botão "Gerenciar" (`onManage()`) navega pra `/applications/:id`** — página própria
  (`ApplicationDetailComponent`, `feature/dashboard/pages/application-detail/`), carregada via
  `ApplicationsService.getById(id)` (usa o `id` da rota, `route.snapshot.paramMap.get('id')` —
  lido uma vez no `ngOnInit`, sem reagir a troca de `:id` sem recarregar o componente, o que é
  suficiente aqui já que a navegação sempre recria o componente ao trocar de app). Mostra
  informações (domínio, criado em, atualizado em) e a mesma seção de chave de API
  (mascarada + copiar + gerar nova, com modal de confirmação) que já existia na lista — ainda
  **não tem edição** dos campos, só visualização + gerenciamento da chave.
  `ApplicationFormModalComponent` é o candidato óbvio pra virar um modo de edição aqui quando
  isso for feito.
- **Helpers de exibição compartilhados**: `applicationStatusLabel`, `applicationInitials`,
  `applicationGradient`, `maskApiKey` moraram em `core/utils/application-display.util.ts` desde
  que passaram a ser usados tanto por `ApplicationsComponent` quanto por
  `ApplicationDetailComponent` — não duplique essa lógica de novo se criar mais uma tela que
  precise dela (ex.: um form de edição).
- **Gotcha do Angular control flow**: o binding `@if (expr; as alias)` só funcionou quando
  `@if` era o **primeiro** da cadeia `@if/@else if/@else` — colocar a mesma sintaxe num
  `@else if` do meio da cadeia falhou em compilação (`NG9: Property 'alias' does not exist`)
  nesse projeto (Angular 18.2). Se precisar de uma variável `as` num `@if`/`@else if`
  encadeado, coloque esse `@if` primeiro na cadeia (reordene as condições) em vez de tentar
  usar `as` num `@else if`.
- **"Nova aplicação" abre `ApplicationFormModalComponent`**
  (`applications/components/application-form-modal/`), um modal com 2 estados: formulário
  (nome, descrição, domínio, URL do logo, cor primária/secundária via `<input type="color">`,
  status) e, após criar com sucesso, uma tela mostrando a **chave de API gerada** com botão de
  copiar — porque essa é a única vez que faz sentido "empurrar" a chave pro usuário; depois
  disso ela só aparece mascarada no card (ver abaixo).
- **Chave de API (`ApiKey`)** — nova coluna no `SystemApp` (`varchar(255)`, índice único,
  migration `20260921120000_AddApiKeyToSystemApp`). Gerada automaticamente no `Create`
  (`nxs_` + 48 chars hex = 24 bytes de entropia, `RandomNumberGenerator.GetBytes` — ver
  `ApplicationsController.GenerateApiKey()`), guardada em **texto puro** (sem hash) por
  simplicidade — não é o ideal pra um cofre de segredos de produção, mas suficiente pro estágio
  atual. Endpoint dedicado pra regenerar: `POST /api/v1/Applications/{id}/api-key/regenerate`
  (não tem corpo — lição do bug do `MapDelete` já documentada aqui se aplica: cuidado com
  verbos HTTP que minimal APIs não esperam ter payload). No card, a chave aparece **mascarada**
  (`maskedApiKey()`: primeiros 8 + `••••••••••••` + últimos 4 chars) com botão "Copiar"
  (usa `navigator.clipboard.writeText`, não precisa revelar a chave completa na tela) e "Gerar
  nova chave" (abre confirmação, já que invalida a anterior na hora).
- **`status` agora existe de verdade no backend** — coluna `Status` (string) em `SystemApp`
  (migration `20260920180000_AddStatusToSystemApp`), exposta em `AppResponseDto`/
  `AppCreateDto`/`AppUpdateDto`. Valores usados pelo frontend: `"active"`/`"inactive"`/
  `"pending"` (badge verde/cinza/amarelo). O backend aceita qualquer string em `Status` (sem
  enum/validação de valores permitidos) — a responsabilidade de só mandar esses 3 valores é
  do frontend por enquanto.
- `primaryColor`/`secondaryColor` viram um gradiente CSS inline no quadrado do logo
  (`gradientFor()` no component) — quando `urlLogo` existir, mostra a imagem em vez do
  gradiente+inicial.
- `ApplicationsController` exige `[Authorize(Roles = "Admin,Owner")]` — um usuário sem uma
  dessas roles recebe 403 ao tentar listar (o primeiro usuário registrado no sistema ganha
  Admin+Dev automaticamente, os demais ganham Owner — ver `AuthEndpoints.MapCustomRegister`).

### Módulo de autenticação para usuários finais das aplicações (`AppEndUser`) — backend pronto, frontend pendente

Cada aplicação cadastrada por um cliente Nexas (ex.: "App A") tem os seus próprios usuários
finais (os clientes do cliente) — completamente isolados dos `NexasUser`/`AspNetUsers` do
portal e isolados entre aplicações diferentes, inclusive permitindo o mesmo e-mail cadastrado
em duas aplicações distintas como pessoas diferentes. Decisão tomada depois de perguntar ao
usuário "reaproveita Identity ou cria tabela própria?" — resposta: tabela própria
(`AppEndUser`), sem herdar do Identity.

- **Entidade `AppEndUser`** (`Domain/Entities/AppEndUser.cs`): `Id`, `ApplicationId` (FK pra
  `SystemApp`, cascade delete), `Email`/`NormalizedEmail`, `FullName`, `PasswordHash` (via
  `PasswordHasher<AppEndUser>` do Identity, usado standalone — não passa pelo pipeline completo
  do Identity, só a classe utilitária de hash), `PasswordResetToken` +
  `PasswordResetTokenExpiresAt`, `CreatedAt`. Índice único composto
  `(ApplicationId, NormalizedEmail)` — é isso que garante a isolação por app permitindo e-mail
  repetido entre apps diferentes.
- **Migration `20260921140000_AddAppEndUsers`** — gotcha de collation MySQL: as colunas GUID
  (`Id`, `ApplicationId`) precisam de `collation: "ascii_general_ci"` explícito (mesma
  collation de `Applications.Id`), senão a FK falha com "incompatible collation" — Pomelo não
  aplica isso por padrão em colunas `char(36)`, tem que copiar o padrão já usado em
  `20260914143618_AddApplicationsTable.cs`. Colunas string (`Email`, `FullName` etc.) levam
  `.Annotation("MySql:CharSet", "utf8mb4")`.
- **Autenticação por API Key**: todo o grupo de endpoints exige header `X-Api-Key` (a mesma
  chave mostrada em "Aplicações" no portal) — resolvido via `RequireApiKeyFilter`
  (`Filters/RequireApiKeyFilter.cs`, um `IEndpointFilter`) que busca o `SystemApp` pela chave e
  guarda em `HttpContext.Items["CurrentApp"]`. Sem header ou chave inválida → 401.
- **Token JWT próprio e isolado**: `AppEndUserTokenService` (singleton) emite/valida um JWT com
  claim custom `token_type: "app_end_user"` e `app_id: <ApplicationId>` — universo de token
  totalmente separado do JWT dos usuários do portal (`NexasUser`), mesma Issuer/Audience/Key do
  `appsettings` mas claims diferentes. Endpoints protegidos (`/me`) validam duas coisas: o JWT
  em si E que o claim `app_id` do token bate com o `SystemApp` resolvido pelo `X-Api-Key` da
  requisição atual — isso é o que impede um token emitido pra "App A" ser usado contra "App B"
  mesmo que o hacker tenha as duas chaves de API (testado e confirmado: retorna 403, não 401,
  quando o token é válido mas de outro app).
- **Endpoints** (`Endpoints/AppEndUserEndpoints.cs`, `MapAppAuth()`, grupo
  `/api/v1/apps/auth`, todos atrás do `RequireApiKeyFilter`):
  - `POST /register` — cria `AppEndUser` pro app da chave; 409 se e-mail já existe nesse app.
  - `POST /login` — 401 em senha errada, senão devolve `accessToken`/`expiresIn`/`user`.
  - `POST /forgot-password` — sempre 200 (não revela se e-mail existe, mesmo padrão do
    Identity), gera `PasswordResetToken` salvo no banco. Nenhum e-mail é enviado (mesma
    limitação do Resend não configurado, ver known-issues.md) — pra testar manualmente, o
    token precisa ser lido direto do banco (`SELECT PasswordResetToken FROM AppEndUsers ...`).
  - `POST /reset-password` — 400 se token errado/expirado, 200 e troca a senha se certo.
  - `GET /me` — introspecção do usuário logado a partir do JWT (com a checagem cruzada de
    `app_id` descrita acima).
- **`ApplicationsController.GetUsers(Guid id)`** (`GET /api/v1/Applications/{id}/users`,
  admin-only, mesma role `Admin,Owner` do resto do controller) — lista os `AppEndUser` de uma
  aplicação pra exibição no portal (não confundir com os endpoints de `/apps/auth`, que são do
  ponto de vista do usuário final, não do admin). Devolve `AppEndUserSummaryDto`
  (`Id`, `Email`, `FullName`, `CreatedAt`), ordenado por mais recente primeiro.
### Papéis (roles) por aplicação — backend pronto, frontend pendente

Extensão do módulo `AppEndUser`: o dono de uma aplicação pode criar/remover papéis livres
(ex.: "Admin", "Professor", "Aluno") e atribuir **múltiplos papéis por usuário** (decisão
explícita do usuário: "múltiplos papéis por usuário né" — um `AppEndUser` pode acumular
vários `AppRole` ao mesmo tempo, ex.: alguém ser "Professor" e "Admin" na mesma aplicação).

- **Entidades**: `AppRole` (`Id`, `ApplicationId` FK cascade, `Name`, `CreatedAt`, índice único
  composto `(ApplicationId, Name)` — nomes só precisam ser únicos dentro da mesma aplicação) e
  `AppEndUserRole` (join many-to-many, chave composta `(AppEndUserId, AppRoleId)` configurada
  via Fluent API em `SystemManagerDbContext.OnModelCreating`, já que Data Annotations não
  suportam chave composta). Migration `20260921150000_AddAppRoles`.
- **Gotcha real encontrado ao gerar essa migration**: rodar `dotnet ef migrations add` (mesmo
  numa cópia isolada, protocolo já documentado acima) expôs um **drift pré-existente** entre a
  entidade `SystemApp` e o `SystemManagerDbContextModelSnapshot.cs` — a propriedade `ApiKey` no
  banco é `varchar(255)` com índice único (migration `AddApiKeyToSystemApp`), mas a classe
  `SystemApp.cs` nunca tinha os atributos `[MaxLength(255)]`/`[Index]` correspondentes. Sem
  isso, o EF "recalculava" o modelo a partir da classe (que não conhece essas restrições) e
  gerava automaticamente, junto com a migration nova, um `AlterColumn` destrutivo que
  transformava `ApiKey` em `longtext` sem índice — teria corrompido o índice único da chave de
  API caso aplicado sem revisão. **Corrigido na raiz**: adicionados `[Required][MaxLength(255)]`
  na propriedade e `[Index(nameof(ApiKey), IsUnique = true)]` na classe `SystemApp`, o que já
  elimina esse falso diff pra sempre (verificado: nenhum `AlterColumn` de `ApiKey` aparece mais
  ao gerar uma migration nova). **Lição pra próximas migrations**: sempre revisar o `.cs` gerado
  por `dotnet ef migrations add` procurando por `AlterColumn`/`DropIndex` em tabelas que não são
  o alvo da mudança pretendida — se aparecer algo assim, é sinal de uma entidade sem anotação
  batendo com o banco real, não uma mudança que você quis fazer.
- **Endpoints** (todos em `ApplicationsController`, admin-only `[Authorize(Roles="Admin,Owner")]`,
  do ponto de vista do dono da aplicação usando o portal — não confundir com `/apps/auth`, que é
  do ponto de vista do usuário final):
  - `GET/POST /api/v1/Applications/{id}/roles` — listar/criar papel (nome único por aplicação,
    400 se duplicado).
  - `DELETE /api/v1/Applications/{id}/roles/{roleId}` — exclui o papel; cascade remove as
    atribuições (`AppEndUserRole`) mas não afeta os usuários em si (testado).
  - `POST/DELETE /api/v1/Applications/{id}/users/{userId}/roles/{roleId}` — atribuir/remover um
    papel de um usuário específico (idempotente: atribuir de novo o mesmo papel não duplica).
  - `GET /api/v1/Applications/{id}/users` (já existente) agora inclui `roles: AppRoleDto[]` por
    usuário.
- **Papéis também viajam no JWT do usuário final**: `AppEndUserTokenService.GenerateToken`
  ganhou um parâmetro opcional `roles` que vira múltiplas claims `ClaimTypes.Role` no token —
  assim o backend do próprio dono da aplicação pode fazer autorização por papel direto pelo JWT,
  sem chamar a Nexas de novo. `/apps/auth/login` e `/apps/auth/me` (`Endpoints/AppEndUserEndpoints.cs`)
  agora carregam `UserRoles` via `.Include().ThenInclude()` e devolvem `roles: string[]` na
  resposta. Testado: um usuário com Admin+Professor gera um token com
  `"...claims/role": ["Professor","Admin"]` (array), e login/me refletem a lista corretamente.
- **Frontend: implementado.** Dois botões novos no cabeçalho do card "Usuários cadastrados"
  (`.detail-users-card__header`): "Gerenciar papéis" e "Novo usuário", cada um abrindo seu
  próprio modal standalone (pasta `application-detail/components/`):
  - **`ManageRolesModalComponent`** — input + botão "Adicionar" pra criar papel (chama
    `ApplicationsService.createRole`), lista os papéis existentes com botão de lixeira que vira
    uma confirmação inline "Remover? Sim/Não" (sem modal aninhado) ao clicar — evita abrir modal
    dentro de modal. Emite `rolesChanged` pro pai, que atualiza `roles` E recarrega a lista de
    usuários (`loadUsers`), já que excluir um papel pode ter afetado os chips exibidos.
  - **`CreateEndUserModalComponent`** — cadastra um usuário final **diretamente pelo portal**,
    sem o dono da aplicação precisar implementar sua própria tela de registro ainda. Reaproveita
    o endpoint público `POST /api/v1/apps/auth/register` (o mesmo usado pelo app do cliente para
    auto-cadastro) chamando-o com a **própria chave de API da aplicação** (`current.apiKey`, já
    disponível na página) em vez do token Bearer do admin — ou seja, **nenhum endpoint novo foi
    necessário no backend** para esse recurso. Como quem cria o usuário é o admin (não a própria
    pessoa), o form gera uma **senha aleatória automaticamente** (`generatePassword()`, 12
    caracteres com maiúscula/minúscula/dígito/símbolo garantidos) — o admin pode revelar
    (`Mostrar`), copiar ou gerar outra (`Gerar nova`) antes de criar. Depois de criar, chama
    `assignRoleToUser` em paralelo (`forkJoin`) pra cada papel marcado nos checkboxes (múltiplos
    papéis desde a criação), e só então mostra a tela de sucesso com a senha pra copiar — **essa
    senha não é mostrada de novo depois**, o admin precisa repassar pra pessoa por fora (mesma
    limitação de sempre: sem Resend configurado ainda, não dá pra mandar por e-mail
    automaticamente).
  - **Gotcha do fluxo criar→atribuir papéis→mostrar sucesso**: o evento `created` (que dispara o
    refresh da lista de usuários no pai) é emitido **antes** de fechar o modal — ele só atualiza
    a lista em segundo plano enquanto o modal continua aberto mostrando a senha. Fechar o modal
    (`createUserModalOpen.set(false)`) só acontece quando o próprio modal emite `closed` (botão
    "Concluir" ou clique no backdrop). Se inverter essa ordem (fechar no `created`), o painel de
    senha nunca aparece pro admin ver/copiar.
  - Chips de papel (`.detail-users-list__role-chip`) foram adicionados a cada item da lista de
    usuários já existente, abaixo do nome/e-mail — só aparecem se o usuário tiver ao menos um
    papel.
  - **`EditUserRolesModalComponent`** (implementado) — botão de lápis (`.detail-users-list__edit-roles-btn`)
    ao final de cada item da lista de usuários abre um modal com checkboxes de todos os papéis
    da aplicação, **pré-marcados conforme os papéis atuais daquele usuário** (`user.roles`).
    Ao salvar, calcula o diff entre o estado inicial e o final (`toAssign`/`toRemove`) e dispara
    só as chamadas necessárias em paralelo (`forkJoin` de `assignRoleToUser`/`removeRoleFromUser`)
    — não reenvia papéis que não mudaram. Se nada mudou, fecha o modal sem chamar a API. Emite
    `saved` pro pai recarregar a lista de usuários (chips atualizados). Testado: papel inicial
    "Aluno" desmarcado + "Admin"/"Professor" marcados → chips passam a mostrar só os dois novos,
    e reabrir o modal depois confirma que o estado pré-marcado reflete exatamente o que foi
    salvo (persistência via GET /users a cada abertura, não um cache local).

- **Frontend: implementado.** `ApplicationDetailComponent` tem um terceiro card ("Usuários
  cadastrados", classe `.detail-users-card`), abaixo dos cards de "Informações" e "Chave de
  API" (fora do `.detail-grid`, ocupa a largura toda), carregado em paralelo com a aplicação no
  `ngOnInit` via `ApplicationsService.getUsers(id)` (novo método, chama o endpoint acima),
  com seu próprio signal de loading (`usersLoading`) independente do loading da aplicação —
  ou seja, o card de usuários tem seu próprio skeleton e não trava o resto da página. Cada
  usuário aparece com um avatar circular de iniciais (`applicationInitials`, reaproveitado —
  funciona com qualquer string, não só nome de aplicação), nome (ou e-mail, se `fullName` for
  nulo), e-mail (só aparece como linha secundária quando há nome) e data de criação
  formatada (`formatDate`, já existente no componente). Estado vazio ("Nenhum usuário
  cadastrado ainda nesta aplicação") e responsivo em mobile testados via Playwright.

### Login com Google para usuários finais das aplicações

Cada aplicação pode habilitar "Entrar com Google" para os seus próprios usuários finais,
independente do login com Google do portal (que já existia, é outro fluxo, outro Client ID,
outro find-or-create). Decisão de design (perguntada e confirmada com o usuário): **cada
aplicação usa o SEU PRÓPRIO Client ID OAuth do Google** — não o Client ID hardcoded que o
portal usa — porque um Client ID do Google só aceita origens JavaScript autorizadas fixas no
Google Cloud Console, e a Nexas não pode ficar autorizando manualmente o domínio de cada
cliente novo num Client ID compartilhado; deixando cada dono de aplicação criar o seu próprio
(no Google Cloud dele, autorizando o próprio domínio), o cadastro escala sozinho.

- **`SystemApp.GoogleClientId`** (novo campo, nullable, `varchar(255)`, migration
  `20260921160000_AddGoogleClientIdToSystemApp`) — não é segredo (é um identificador público,
  o mesmo padrão do Client ID que já aparece hardcoded no frontend do portal), então é
  armazenado e devolvido em texto puro pela API, sem mascarar. Quando nulo, login com Google
  fica desabilitado para essa aplicação.
- **Configuração pelo dono da aplicação**: card "Login com Google" em
  `ApplicationDetailComponent`, entre "Chave de API" e "Usuários cadastrados" — **reaproveita o
  `PATCH /api/v1/Applications/{id}` que já existia** (`AppUpdateDto.GoogleClientId`), sem
  precisar de endpoint novo. Três estados no template (`editingGoogleClientId()` /
  `current.googleClientId` / nenhum dos dois): não configurado (botão "Configurar"), configurado
  (mostra o Client ID + botão "Editar"), e editando (input + Salvar/Cancelar). **Gotcha do
  PATCH**: como o padrão existente do `Patch()` do controller só altera um campo se
  `!= null` no DTO (convenção de "campo omitido = não mexe"), mandar `null` nunca conseguiria
  *limpar* o valor — por isso o front sempre manda uma string (nunca `null`: usa
  `googleClientId ?? ''`) e o backend trata **string vazia como "limpar" e string com conteúdo
  como "definir"** (`ApplicationsController.Patch`, campo `GoogleClientId`) — different from
  todos os outros campos do Patch, que não têm como ser limpos hoje.
- **Backend: `POST /api/v1/apps/auth/google-login`** (`Endpoints/AppEndUserEndpoints.cs`, mesmo
  grupo `/api/v1/apps/auth` atrás do `RequireApiKeyFilter`, mesmo pacote `Google.Apis.Auth` já
  usado pelo login do portal em `AuthEndpoints.cs` — **copiado desse padrão de propósito**, pra
  manter os dois fluxos de Google consistentes):
  - 400 se a aplicação não tiver `GoogleClientId` configurado.
  - Valida o `idToken` via `GoogleJsonWebSignature.ValidateAsync` com `Audience = [app.GoogleClientId]`
    — a auditoria de segurança real está aqui: só um token emitido especificamente pro Client ID
    daquela aplicação passa.
  - **Gotcha descoberto e corrigido durante o teste**: `GoogleJsonWebSignature.ValidateAsync`
    lança `InvalidJwtException` só para um JWT bem-formado mas com assinatura/audience/expiração
    inválida — um valor que **nem tem formato de JWT** (ex.: uma string qualquer, erro de
    integração) lança `FormatException` (de `Convert.FromBase64String` por baixo dos panos) e
    **derrubava a request com 500** antes da correção. Catch ajustado pra
    `catch (Exception ex) when (ex is InvalidJwtException or FormatException)`. **O mesmo bug
    provavelmente existe no `/google-login` do portal** (`AuthEndpoints.cs`, só captura
    `InvalidJwtException`) — não foi corrigido lá porque não fazia parte do escopo pedido, mas
    vale considerar replicar o fix se for mexer nesse endpoint de novo.
  - Find-or-create de `AppEndUser` por `NormalizedEmail` escopado por `ApplicationId` (mesmo
    padrão de `/register`) — usuário novo ganha `PasswordHash` aleatório inutilizável (Guid +
    sufixo, só pra satisfazer a coluna `NOT NULL`, igual ao padrão já usado no Google login do
    portal), já que a autenticação dele nunca vai passar por senha.
  - Resposta igual ao `/login` normal (`AppLoginResponse` com token JWT + roles) — do ponto de
    vista de quem chama, logar com e-mail/senha ou com Google devolve exatamente a mesma forma.
- **Não testável de ponta a ponta neste ambiente**: validar um ID Token real do Google exige um
  fluxo OAuth de verdade (não dá pra forjar a assinatura). Testado tudo que dá pra testar sem
  isso: 400 quando não configurado, 400 com token malformado, 400 com JWT forjado (assinatura
  errada), e o ciclo completo de configurar/exibir/editar/limpar o Client ID pela UI (persistindo
  entre reloads). **Falta testar com um Client ID e login reais** quando o usuário tiver um
  Google Cloud project de teste configurado.
- **Frontend do lado do usuário final (o app do cliente) não é responsabilidade da Nexas** — só
  o backend (`/apps/auth/google-login`) e a configuração pelo dono (`ApplicationDetailComponent`)
  foram construídos aqui. O botão "Entrar com Google" que o usuário final vê vive no frontend
  que o **cliente da Nexas** constrói pro próprio produto dele (fora deste monorepo) — o padrão
  esperado é ele usar Google Identity Services com o próprio Client ID e mandar o `idToken`
  resultante pro endpoint acima, com o header `X-Api-Key` da aplicação.

### Minha Conta (`/account`) em detalhe

`AccountComponent` renderiza navbar + header + painel de abas verticais (Perfil, Segurança,
Privacidade, Pagamento) + footer. Estado da aba ativa é um `signal<AccountTab>`.

| Aba | Status |
|---|---|
| Perfil | ✅ Funcional — `ProfileTabComponent` (`components/profile-tab/`) |
| Segurança | ⚠️ Parcial — `SecurityTabComponent` (`components/security-tab/`), ver abaixo |
| Privacidade | ✅ Funcional — `PrivacyTabComponent` (`components/privacy-tab/`), ver abaixo |
| Pagamento | 🚧 Placeholder "em construção" |

**`ProfileTabComponent`** — o mais completo do projeto até agora:
- Carrega dados via `ProfileService.getProfile()` (`GET /api/v1/auth/profile`) no `ngOnInit`,
  com skeleton de loading.
- Formulário reativo: `fullName` (obrigatório, min. 2 chars), `phoneNumber` (opcional),
  `email` (campo desabilitado, só leitura).
- Salvar chama `ProfileService.updateProfile()` (`PUT /api/v1/auth/profile`) — persiste
  `FullName`, `PhoneNumber` e `AvatarBase64` de verdade no banco (via `UserManager.UpdateAsync`,
  backend em `Endpoints/ProfileEndpoints.cs`). **E-mail não é editável** — mudar e-mail exigiria
  fluxo de confirmação que não existe ainda.
- **Botão "Salvar alterações" só habilita se algo mudou** em relação ao que foi carregado do
  servidor (nome, telefone ou avatar) — não é só `form.dirty` do Angular (que fica `true`
  mesmo se o usuário editar e depois voltar pro valor original). A comparação é feita num
  `computed()` (`hasChanges`) contra um snapshot `originalValues` guardado como **signal**
  (não campo comum — um `computed()` só re-executa quando um *signal* que ele leu muda; se
  `originalValues` fosse um campo comum atualizado via `=`, o botão ficaria com o estado antigo
  depois de salvar. Isso já foi um bug real aqui, ver [known-issues.md](known-issues.md) se
  reaparecer algo parecido).
- Ao salvar com sucesso, atualiza `StateUtil` e o cache `sessionStorage['nexas_user']` na
  hora, pra navbar/home refletirem o nome novo sem precisar relogar.
- **Avatar — persistência real**: círculo com iniciais (ou foto) + badge de câmera que abre um
  menu com "Enviar foto" / "Usar câmera". Qualquer imagem escolhida (arquivo ou câmera) é
  **redimensionada no cliente** via `<canvas>` para um quadrado de 256px (cover-crop, sem
  distorcer) e comprimida como JPEG (`toDataURL('image/jpeg', 0.85)`) *antes* de virar preview
  — ou seja, o preview já mostra exatamente o que vai ser salvo. O resultado (data URL,
  tipicamente algumas dezenas de KB) é enviado como string no campo `avatar` do
  `UpdateProfileRequest` e persistido *como está* na coluna `AvatarBase64` (MEDIUMTEXT) do
  `NexasUser` — sem serviço de storage externo (`CloudflareStorageService`/`BunnyNetService`
  não são usados aqui de propósito; o pedido explícito foi base64 direto no banco, pela
  simplicidade e por já vir pré-comprimido a um tamanho pequeno).
  - **"Usar câmera" usa `getUserMedia`, não o atributo `capture` do `<input type="file">`** —
    esse atributo é só uma dica que o Chrome/desktop ignora (sempre abre o seletor de
    arquivos) e que é inconsistente até em mobile. A implementação atual abre um modal com
    `<video>` ao vivo (`[srcObject]="mediaStream()"`, espelhado via CSS pra parecer uma
    câmera frontal/selfie) e um botão "Capturar" que desenha o frame atual num `<canvas>`
    (também espelhado no draw, pra bater com o preview) e vira o mesmo fluxo de
    redimensionamento acima. **Sempre para as tracks do `MediaStream`**
    (`track.stop()`) ao cancelar, capturar ou destruir o componente — senão a luz da câmera
    fica acesa.
  - O avatar **não aparece em nenhum outro lugar** (navbar, home) — só na própria página de
    Perfil, porque o `StateUtil`/`UserLogedModel` (populado via claims do JWT) não carrega o
    avatar (base64 é grande demais pra colocar num JWT/cookie). Se for exibir o avatar em
    outro lugar, precisa buscar via `ProfileService.getProfile()` separadamente, não pelas
    claims do token.

**`SecurityTabComponent`** — só tem "Alterar senha" por enquanto (Privacidade/Pagamento da
aba Segurança não existem, é um card só):
- Botão "Enviar e-mail de redefinição" chama `AuthService.forgotPassword(email)` →
  `POST /api/v1/auth/forgotPassword` (mesmo endpoint do `MapIdentityApi` usado pelas páginas
  de login) com o e-mail do usuário logado (`StateUtil.user.email`). **De propósito, não existe
  um form de "senha atual + senha nova" aqui** — a decisão do usuário foi que trocar senha
  sempre passa por e-mail, por segurança (evita expor um endpoint de troca direta).
  - **Não implementar um form de troca direta de senha aqui sem confirmar de novo** — já foi
    decidido explicitamente que não é assim que deve funcionar.
- Cooldown de 60s no botão após enviar (evita spam de e-mails).
- ⚠️ **O endpoint responde 200, mas nenhum e-mail é enviado de verdade** — ver
  [known-issues.md](known-issues.md) pro que falta (integração com Resend, já combinada mas
  adiada).

**`PrivacyTabComponent`** — LGPD-style, 3 blocos:
- **Preferências de comunicação**: dois toggles (`ReceiveMarketingEmails`,
  `ReceiveProductNotifications`, colunas booleanas no `NexasUser`) que **salvam sozinhos ao
  trocar** (`PUT /api/v1/auth/privacy` a cada mudança, sem botão "Salvar" — diferente do
  Perfil de propósito, porque são só 2 booleans triviais, não vale a pena um form com dirty
  tracking pra isso).
- **Exportar meus dados**: `GET /api/v1/auth/export-data` devolve um JSON com tudo que o
  sistema guarda do usuário (dados do Identity, roles, preferências, avatar em base64,
  histórico de login) e o frontend gera um download client-side (`Blob` + `<a download>`,
  sem round-trip pelo servidor pra gerar arquivo).
- **Excluir conta**: botão abre um modal (mesmo padrão do `.camera-modal` — ver
  design-system.md) que só habilita "Excluir permanentemente" quando o usuário **digita o
  próprio e-mail exatamente igual** ao da conta (case-insensitive). Confirma →
  `DELETE /api/v1/auth/account?confirmEmail=...` → `UserManager.DeleteAsync` de verdade (hard
  delete, cascade nas tabelas do Identity relacionadas) → `AuthService.logOut()` no sucesso.
  **Isso é destrutivo e sem undo** — se for alterar essa lógica, teste sempre contra uma conta
  descartável, nunca contra uma conta real (já rolou um teste seguro assim: registrar um
  usuário de teste via `/register`, pegar o Id no banco, gerar um JWT manual pra ele, chamar
  DELETE, confirmar no banco que sumiu).
  - **Gotcha do ASP.NET Minimal API**: `MapDelete` **não infere corpo JSON automaticamente**
    (diferente de `MapPost`/`MapPut`) — um parâmetro de tipo complexo (`record`) num
    `MapDelete` derruba o app inteiro no startup com
    `InvalidOperationException: Body was inferred but the method does not allow inferred body parameters`.
    Por isso o `confirmEmail` vai via **query string**, não body, no endpoint de exclusão
    (também é mais correto pra semântica REST de DELETE). Se precisar de payload complexo
    num DELETE, tem que anotar o parâmetro com `[FromBody]` explicitamente.

## Componentes compartilhados (`shared/components/`)

- **`navbar`** — logo, links (Início/Aplicações/Organizações), toggle de tema, sino de
  notificação (decorativo, sem funcionalidade), dropdown de perfil (avatar com inicial, nome,
  e-mail, "Minha Conta" → navega pra `/account`, "Sair" → `AuthService.logOut()`). Incluído
  manualmente em cada página que precisa dele (não é global).
- **`footer`** — rodapé simples, texto + tagline. Também incluído manualmente por página.
- **`toast`** — renderiza o toast atual do `ToastService`. Esse sim é incluído uma única vez,
  em `app.component.html`, e funciona globalmente.

## Componentes de feature

- **`portal-card`** (`feature/dashboard/components/portal-card/`) — card grande usado na home
  para "Organizações" e "Aplicações". Recebe `title`, `category`, `description`, `variant`
  (`'applications' | 'organization'`, controla a cor de destaque) e `routeLink`, com conteúdo
  customizado via `<ng-content>` (o SVG ilustrativo de cada card é passado de fora, não é
  parte do componente).

## Ao criar uma página nova do dashboard

1. Copiar a estrutura de `organizations.component.*` (componente vazio "em construção") como
   ponto de partida se for só um placeholder, ou de `account.component.*` se precisar de
   navbar+footer+conteúdo real.
2. Adicionar a rota em `app.routes.ts` com `canActivate: [authGuard]` (a menos que deva ser
   acessível deslogado, como a home).
3. Incluir `<app-navbar>`/`<app-footer>` manualmente no template.
4. Seguir os tokens/padrões de [design-system.md](design-system.md).
