# Lacunas conhecidas e próximos passos

Coisas que já foram notadas, discutidas ou deliberadamente deixadas pra depois — não são
"bugs escondidos" que você precisa descobrir, é só o backlog real do projeto.

## Pendências de produto (esperadas, não urgentes)

- **Abas Segurança, Privacidade e Pagamento** (`/account`) — só placeholders "em construção",
  sem nenhuma funcionalidade.
- **`/forgot-password` e `/reset-password`** (páginas de login) são só UI (`setTimeout`
  simulando sucesso) — ainda não chamam a API real. A aba **Segurança** de "Minha Conta"
  (`SecurityTabComponent`) já chama o endpoint de verdade (`AuthService.forgotPassword()` →
  `POST /api/v1/auth/forgotPassword`, mesmo endpoint do `MapIdentityApi`), mas com uma limitação
  conhecida: **nenhum e-mail é enviado de verdade ainda**, ver item abaixo.
- **Envio de e-mail não está configurado no `Nexas.SystemManager`** — confirmado: não existe
  nenhum `IEmailSender`/`IEmailSender<NexasUser>` registrado, então o Identity usa o
  `NoOpEmailSender` padrão (não faz nada, só loga um aviso uma vez). Isso significa que
  `/forgotPassword` e `/resendConfirmationEmail` **sempre respondem 200** (por design de
  segurança do Identity — não revela se o e-mail existe), mas nenhum e-mail chega de verdade.
  Plano combinado com o usuário: integrar **Resend** — e já existe a peça reaproveitável:
  `backend/Nexas.Infrastructure/Services/ResendEmailService.cs` implementa `IEmailService`
  (interface própria do domínio, não a do Identity) usando a API do Resend, e já está
  registrado via `AddHttpClient<IEmailService, ResendEmailService>()` em
  `Nexas.Infrastructure/DependencyInjection.cs` — `Nexas.SystemManager` já referencia esse
  projeto e chama `AddInfrastructure(...)`, então `IEmailService` já está disponível no DI.
  Falta: (1) configurar as chaves reais (`EmailSettings:apikey`/`EmailSettings:senderemail`,
  ausentes no `appsettings` do SystemManager) e (2) escrever um adapter que implemente
  `IEmailSender<NexasUser>` delegando pro `IEmailService` já existente, e registrá-lo antes do
  `AddIdentityApiEndpoints<NexasUser>()`. **Isso foi decidido mas explicitamente adiado — não
  implementar sem o usuário pedir.**
- **Alterar e-mail** — o campo de e-mail em "Minha Conta" é somente leitura de propósito.
  Mudar e-mail no Identity exige reconfirmação (token por e-mail); não foi implementado ainda.
- **Organizações e Aplicações** (`/organizations`, `/applications`) — rotas existem, guard
  aplicado, mas as páginas em si ainda não têm conteúdo nenhum.

## Configuração

- **Sem `environment.prod.ts`** — só existe `src/environments/environment.ts`, usado tanto em
  dev quanto (por enquanto) em qualquer build de produção, apontando pra
  `http://localhost:5179`. Não há `fileReplacements` no `angular.json`. Precisa resolver antes
  de um deploy real apontando pra um backend não-local.
- **Google Identity Services (OAuth) — erro 403 de origem** — o client ID hardcoded no
  `login.component.ts` e `register.component.ts` (`889143178707-...apps.googleusercontent.com`)
  não está autorizado para todas as origens/portas usadas em dev — já foi visto um erro
  `403 — The given origin is not allowed for the given client ID` no console ao rodar em
  `localhost:4200`. É configuração do Google Cloud Console (origens autorizadas do OAuth
  client), não é bug de código. Não gastar tempo tentando "consertar" isso no frontend.
- **Google Identity Services — botão "sumindo" (já corrigido)** — o botão de login/cadastro
  com Google usava `if (typeof google !== 'undefined')` **uma única vez** no `ngOnInit`,
  torcendo pra que o script `accounts.google.com/gsi/client` (carregado via `<script async defer>`
  no `index.html`) já tivesse terminado de carregar nesse exato instante. Se não tivesse
  (rede lenta, cache frio, hidratação do SSR), o botão nunca aparecia — sem erro nenhum,
  parecia só "sumir" de forma intermitente. Corrigido: `core/utils/google-identity.util.ts`
  injeta o script sob demanda e devolve uma Promise que só resolve quando ele carrega de
  verdade; `login.component.ts`/`register.component.ts` chamam
  `loadGoogleIdentityScript().then(...)` em vez de checar uma vez só. A tag `<script>` estática
  foi removida do `index.html` — se precisar reintroduzir carregamento estático por algum
  motivo, lembre de não duplicar com o loader dinâmico.

## Gerando/aplicando migrations com o backend já rodando

Se você (ou o usuário) já tem uma instância do `Nexas.SystemManager` rodando localmente
(via `dotnet run`, F5 na IDE, etc.), tanto `dotnet build` quanto `dotnet ef migrations add`/
`dotnet ef database update` falham com `MSB3027`/`MSB3021` (arquivo `.dll`/`.exe` em uso) —
porque esses comandos escrevem no mesmo `bin/obj` do projeto que o processo já tem carregado.

**Não mate o processo do usuário sem perguntar** (pode estar com debugger anexado). Alternativas
que não exigem isso:
- `dotnet build -o <pasta-temporária>` — só pra checar se o código compila, sem gerar migration.
- Pra gerar uma migration nova sem rodar `dotnet ef`: escreva os arquivos de migration à mão
  (o `.cs` com `Up()`/`Down()` e o `.Designer.cs`), copiando a estrutura exata da migration
  mais recente já existente em `Infrastructure/Migrations/` e só adicionando a mudança nova —
  depois atualize também o `SystemManagerDbContextModelSnapshot.cs` (mesma entidade, mesmo
  formato). Valide compilando (`dotnet build -o <temp>`).
- Pra testar de ponta a ponta sem tocar na instância do usuário: suba uma instância isolada
  numa porta diferente (`ASPNETCORE_URLS=http://localhost:5199 dotnet <pasta-temp>/Nexas.SystemManager.dll`,
  com `ASPNETCORE_ENVIRONMENT=Development`) apontando pro mesmo banco de dev — o `Program.cs`
  já roda `Database.Migrate()` automaticamente no startup, então essa instância isolada aplica
  a migration nova sozinha, e dá pra testar os endpoints com `curl` sem mexer no processo real.
- Pra testar endpoints autenticados sem passar pela UI: gere um JWT válido manualmente (mesmo
  algoritmo HS256, mesma `Jwt:Key` do `appsettings.Development.json`) com um script Node curto
  (`crypto.createHmac('sha256', key)`) — mais rápido que logar pela UI toda vez, e não deixa
  usuários de teste no banco.
- **Cuidado**: se você usar um `ExternalId`/usuário REAL (não um de teste) pra validar
  persistência, qualquer PUT de teste sobrescreve os dados reais dele. Sempre leia o valor
  atual antes (`GET /profile` ou uma query direta no banco) e restaure explicitamente depois
  dos testes — já aconteceu de sobrescrever nome/telefone reais do usuário nesse projeto
  durante um teste de avatar.

## Backend real do usuário precisa reiniciar pra pegar código novo

O processo `Nexas.SystemManager` que o usuário roda localmente (porta 5179, via `dotnet run`/F5)
só reflete o código-fonte no momento em que foi iniciado — **não há hot reload** aplicado a
mudanças de controllers/endpoints/migrations feitas depois que ele já estava no ar. Isso já foi
confirmado na prática: depois de implementar o módulo `AppEndUser` (registro/login/etc. dos
usuários finais das aplicações, ver pages-and-components.md), testar `POST /api/v1/apps/auth/register`
contra a instância real na porta 5179 devolveu **404** (rota não existia ali), enquanto a mesma
chamada contra uma instância isolada recém-compilada (porta 5199, protocolo de teste já
documentado acima) funcionou normalmente — a migration já tinha sido aplicada no banco
compartilhado (o `Database.Migrate()` de qualquer instância aplica pra todo mundo), só o
processo em si que estava com o binário antigo.

**Sempre que uma sessão implementar um endpoint/controller/migration novo**, ele só vai
funcionar de verdade pro usuário depois que ele parar e reiniciar sua própria instância
(`dotnet run` de novo) — isso não é algo pra "consertar", é só avisar o usuário explicitamente
no fim da tarefa, já que a validação via porta 5199 confirma que o *código* funciona, não que
o ambiente dele já está servindo esse código.

## Detalhes visuais menores

- **Toast x Footer** — o toast (canto inferior-direito) pode colidir visualmente com o texto
  do footer, dependendo do viewport. Visto durante teste manual, não corrigido ainda (baixa
  prioridade, mas anotar se for mexer em qualquer um dos dois).

## Coisas que PARECEM bug mas não são (já investigado)

- **`AuthService.logOut()` "não funciona"** — testado ponta-a-ponta várias vezes (com token
  real e sintético), sempre funcionou corretamente (limpa cookie, limpa `StateUtil`, navega
  pro `/login`). Se alguém relatar isso de novo, a causa mais provável é outra (ex.: uma rota
  quebrada lançando erro não tratado no Router *antes* do clique em "Sair", deixando a
  aplicação num estado estranho) — não é o método de logout em si.
- **Botão da navbar (dropdown de perfil) "parou de responder" depois de um tempo de uso, mas um
  F5 resolve — e volta a acontecer depois** — investigado a fundo (Playwright, várias páginas,
  antes/depois de abrir e fechar cada modal novo do dia: gerenciar papéis, novo usuário, editar
  papéis, login com Google, regenerar chave) e o clique **sempre funcionou** nesses cenários, sem
  nenhum erro no console. O padrão relatado ("quebra depois de um tempo, F5 resolve, volta a
  quebrar") não é sintoma de bug de código — é a assinatura clássica de **drift do hot-reload do
  `ng serve`/Vite** depois de muitas edições de arquivo em sequência na mesma sessão de dev
  (exatamente o que acontece numa sessão longa de trabalho como as deste projeto, com o `ng serve`
  do usuário ficando aberto o dia todo enquanto dezenas de arquivos são salvos). Se isso for
  relatado de novo: primeiro perguntar se um F5 resolve temporariamente — se sim, a solução é
  reiniciar o `ng serve` (não investigar o componente), não perder tempo caçando um bug que não
  existe no código.
- **Nome do usuário sumindo/mostrando e-mail** — pode ser (a) falta de rehydratação no boot
  (já corrigida via `APP_INITIALIZER`, ver [architecture.md](architecture.md)), (b) token
  emitido *antes* de uma correção no backend relacionada a claims — nesse caso, só reiniciar o
  backend não resolve, o usuário precisa deslogar e logar de novo pra ganhar um token novo
  (tokens antigos continuam com as claims antigas até expirar).
