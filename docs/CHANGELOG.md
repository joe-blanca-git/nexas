# Changelog

Registro humano do que foi construído, por dia de trabalho. Complementa (não substitui) a
documentação de contexto para agentes em `frontend/portal-sm/.agents/`, que é mais detalhada
sobre decisões técnicas e armadilhas — este arquivo é o resumo do que mudou e por quê.

## 2026-09-20 — Autenticação real (JWT), Minha Conta, Aplicações e módulo multi-tenant de usuários finais

Dia de trabalho longo no `portal-sm` (frontend Angular) e no `Nexas.SystemManager` (backend).
Resumo por área:

### Autenticação do portal
- Diagnosticado e corrigido o mismatch entre o token opaco do ASP.NET Core Identity e o JWT que
  o frontend esperava — causa raiz de vários bugs que pareciam não relacionados (logout
  "quebrado", nome do usuário sumindo, guards se comportando mal). Backend agora emite JWT de
  verdade via um `BearerTokenProtector` customizado (`JwtTicketDataFormat`), mantendo o Identity
  como único esquema de autenticação (sem duplicar esquemas, erro já cometido e revertido antes).
- Login com Google agora captura e exibe o nome real da pessoa (não o e-mail) — claim de nome
  ajustada via `NexasUserClaimsPrincipalFactory`.

### "Minha Conta" (`/account`)
- Painel de abas verticais: Perfil, Segurança, Privacidade, Pagamento (este último ainda
  placeholder).
- **Perfil**: formulário completo (nome, telefone, e-mail somente leitura) com avatar — upload de
  foto ou captura via câmera (`getUserMedia`, não o atributo `capture` do input, que é
  inconsistente), redimensionado e comprimido no cliente antes de enviar, persistido como base64
  no banco. Botão de salvar só habilita quando algo realmente mudou (comparação contra o valor
  original carregado, não só `form.dirty`).
- **Segurança**: solicitação de troca de senha reaproveita o mesmo endpoint de "esqueci minha
  senha" do login (decisão deliberada: mais seguro que expor um endpoint de troca direta).
- **Privacidade**: preferências de comunicação (toggles que salvam sozinhos), exportar dados
  (download client-side de tudo que o sistema guarda) e excluir conta (exige digitar o próprio
  e-mail para confirmar).

### "Aplicações" (`/applications`) — cadastro de sistemas dos clientes Nexas
- Tela onde um cliente Nexas cadastra os próprios sistemas que consomem os serviços da
  plataforma. Lista em cards (não grid), criar/editar campos básicos, excluir, e gerar/regenerar
  uma chave de API única por aplicação.
- Página de detalhes (`/applications/:id`) com informações, chave de API (mascarada, copiável) e,
  a partir de hoje, mais três seções (ver abaixo).
- Navbar reordenada: Início, Organizações, Aplicações.

### Módulo de autenticação para os usuários finais das aplicações (novo)
Até hoje, só o dono de uma aplicação (o cliente Nexas) tinha login. A partir de agora, cada
aplicação cadastrada pode ter os **seus próprios usuários finais** — logo, os clientes do
cliente — com um sistema de autenticação completo e isolado por aplicação:
- Tabela própria (`AppEndUser`), sem reaproveitar o Identity do portal — mesmo e-mail pode se
  cadastrar em duas aplicações diferentes como contas totalmente independentes.
- Endpoints (autenticados por `X-Api-Key`, a chave da própria aplicação): registro, login,
  esqueci/redefinir senha, e introspecção (`/me`).
- Token JWT próprio, isolado do token dos administradores do portal — inclui verificação cruzada
  entre o token e a chave de API usada na requisição, impedindo que um token de uma aplicação
  seja usado contra outra.
- No portal, card "Usuários cadastrados" na página de detalhes da aplicação, com criação de
  usuário diretamente pelo admin (e-mail + senha gerada automaticamente, mostrada uma única vez).

### Papéis (roles) por aplicação (novo)
- Cada aplicação pode criar/remover seus próprios papéis livres (ex.: Admin, Professor, Aluno) —
  sem lista fixa — e um usuário final pode acumular **múltiplos papéis** ao mesmo tempo.
- Papéis viajam como claims no JWT do usuário final, para o backend de cada aplicação já poder
  fazer controle de acesso sem chamar a Nexas de novo.
- No portal: modal "Gerenciar papéis" (criar/remover) e edição dos papéis de cada usuário já
  cadastrado (chips na lista + modal de checkboxes).

### Login com Google para usuários finais (novo)
- Cada aplicação pode configurar o **seu próprio** Client ID OAuth do Google (não o da Nexas) —
  necessário porque um Client ID só autoriza origens fixas no Google Cloud, então cada cliente
  precisa do seu para poder autorizar o próprio domínio.
- Novo endpoint reaproveita a mesma validação de token já usada no login Google do portal
  (`Google.Apis.Auth`), mas verificando a audience contra o Client ID daquela aplicação
  específica.
- Configuração pelo card "Login com Google" na página de detalhes da aplicação.
- **Fora do escopo construído hoje**: o botão "Entrar com Google" que o usuário final efetivamente
  clica vive no frontend que cada cliente Nexas constrói para o próprio produto — isso é do lado
  do cliente, não deste repositório. Ver [docs/integration-guide.html](integration-guide.html),
  que documenta exatamente como um cliente Nexas integra isso (abrir esse arquivo direto no
  navegador para ler formatado).

### Bugs encontrados e corrigidos ao longo do dia
- `MapDelete` do ASP.NET Minimal API não infere corpo JSON automaticamente — um parâmetro
  complexo aí derrubava o app inteiro no startup (endpoint de excluir conta ajustado para receber
  o parâmetro via query string).
- Drift entre a entidade `SystemApp` e o schema real do banco (`ApiKey` sem as anotações que
  batiam com a coluna já existente) — gerar uma migration nova quase incluiu uma alteração
  destrutiva não relacionada; corrigido anotando a entidade corretamente.
- Validação de token do Google podia devolver erro 500 em vez de 400 quando o token recebido nem
  tinha formato de JWT (exceção não coberta pelo catch original).

### Adiado deliberadamente (não esquecido)
- Envio de e-mail de verdade (Resend) — `/forgot-password` já funciona, mas nenhum e-mail sai
  ainda; combinado com o usuário para integrar depois.
- Edição completa dos campos de uma aplicação já criada (hoje só chave de API e Google Client ID
  são editáveis depois da criação).
- Conteúdo real da página "Organizações" e da aba "Pagamento" de Minha Conta.
