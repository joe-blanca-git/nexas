# VPS Infrastructure — Contexto para IA (srv1898382)

Documento de referência técnica para qualquer IA que precise subir, alterar ou mexer em containers/nginx nesta VPS. Escrito após um incidente real (recreate acidental do banco de produção do Theos) — leia a seção "Regra de Ouro" antes de rodar qualquer `docker compose up`.

## Acesso

- VPS Ubuntu 24.04.4 LTS, hostname `srv1898382`.
- Não há acesso direto (SSH) da IA à VPS. O fluxo é sempre: a IA lê o repo local (`nexas/backend`, clone espelhado) para entender o `docker-compose.yml`/`Dockerfile`/`appsettings.json`, monta os comandos, e o usuário (Joeder) copia/cola e executa manualmente na VPS, colando a saída de volta.
- Usuário roda como `root` na VPS.

## ⚠️ Regra de ouro — colisão de nome de projeto no Docker Compose

**Incidente real (2026-09-10):** `/projects/theos/backend` e `/projects/nexas/backend` são pastas diferentes mas **ambas se chamam `backend`**. O Docker Compose usa o nome da pasta como "nome do projeto" por padrão (labels `com.docker.compose.project`). Rodar `docker compose up -d mysql_banco` dentro de `/projects/nexas/backend` **recriou o container de produção `theos-mysql-db`** (mesmo nome de serviço `mysql_banco` nos dois compose files + mesmo project name "backend" = Compose achou que era o mesmo container). O portal Theos caiu. Os dados sobreviveram porque estavam em volume nomeado (`backend_theos_db_data`), mas o susto foi real.

**Correção aplicada:** cada projeto agora define `COMPOSE_PROJECT_NAME` explícito no `.env` da sua pasta:
- `/projects/theos/backend/.env` → `COMPOSE_PROJECT_NAME=theos-backend`
- `/projects/nexas/backend/.env` → `COMPOSE_PROJECT_NAME=nexas-backend`

**REGRA PARA QUALQUER PROJETO NOVO:** antes do primeiro `docker compose up` em qualquer pasta, SEMPRE:
1. Rodar `docker ps -a`, `docker network ls`, `docker volume ls` e comparar com o que já existe.
2. Garantir que o `.env` da pasta tem `COMPOSE_PROJECT_NAME=<algo-unico>` (nunca depender do nome default da pasta).
3. Nunca rodar `docker compose up` "às cegas" num diretório que pode ter nome repetido (`backend`, `frontend`, `api`, etc. são nomes genéricos usados em vários projetos aqui).

**⚠️ Pendência conhecida:** o Theos (`/projects/theos/backend`) só teve o `.env` ajustado — os containers atuais dele (`theos-mysql-db`, `theos-admin-api-container`, etc.) ainda estão rodando sob o project name antigo `backend`, e o volume real de dados é `backend_theos_db_data`. **Se algum dia alguém rodar `docker compose up` de novo em `/projects/theos/backend` (agora com `COMPOSE_PROJECT_NAME=theos-backend` ativo), o Compose vai procurar um volume chamado `theos-backend_theos_db_data` — que NÃO existe — e criar um vazio, dando a impressão de perda de dados de novo.** Antes de redeployar o Theos, é preciso migrar/renomear o volume (`docker run --rm -v backend_theos_db_data:/from -v theos-backend_theos_db_data:/to alpine sh -c "cp -a /from/. /to/"`, criar o volume novo antes) ou ajustar o `docker-compose.yml` do Theos para apontar explicitamente pro volume antigo via `external: true`.

## Layout de diretórios

```
/projects/
├── nexas/backend/        # este repo (Nexas.sln, docker-compose.yml, .env)
├── theos/backend/        # Theos.sln equivalente (mesma estrutura, "clonado" do Nexas ou vice-versa)
├── theos/frontend/        # frontends do Theos, docker-compose.yml próprio
├── agivys/                # projeto Agivys (backend + frontend juntos), docker-compose.yml único
├── portfolio/             # site pessoal (nginx:alpine), docker-compose.yml
└── shared/nginx/          # docker-compose.yml de um nginx-proxy DOCKERIZADO — NÃO ESTÁ EM USO (ver seção Nginx)
```

Cada projeto de backend .NET segue o mesmo padrão de solution: `<Nome>.Domain`, `<Nome>.Application`, `<Nome>.Infrastructure`, `<Nome>.Api`, `<Nome>.Admin.Api`, `<Nome>.Landing.Api`, Clean Architecture + CQRS (MediatR) + EF Core (Pomelo MySQL).

## Inventário de containers (snapshot 2026-09-10)

| Container | Projeto | Imagem | Porta host | Rede(s) |
|---|---|---|---|---|
| `nexas-mysql-db` | Nexas | mysql:8.0 | 3317→3306 | `nexas-backend_nexas-network` |
| `nexas-api-container` | Nexas | nexas-backend-nexas-api | 6011→8080 | `nexas-backend_nexas-network`, `agivys_default` |
| `nexas-admin-api-container` | Nexas | nexas-backend-nexas-admin-api | 6012→8080 | idem |
| `nexas-landing-api-container` | Nexas | nexas-backend-nexas-landing-api | 6013→8080 | idem |
| `theos-mysql-db` | Theos | mysql:8.0 | 3307→3306 | `backend_theos-network` (project name antigo, ver pendência acima) |
| `theos-api-container` | Theos | backend-theos-api | 5011→8080 | `backend_theos-network`, `agivys_default` |
| `theos-admin-api-container` | Theos | backend-theos-admin-api | 5012→8080 | idem |
| `theos-landing-api-container` | Theos | backend-theos-landing-api | 5013→8080 | idem |
| `theos-landing-container` | Theos (frontend) | frontend-theos-landing | 4210→80 | — |
| `theos-portal-pat-container` | Theos (frontend, alunos) | frontend-theos-portal-pat | 4211→80 | — |
| `theos-portal-popt-container` | Theos (frontend, professores) | frontend-theos-portal-popt | 4212→80 | — |
| `agivys-db` | Agivys | mysql:8.0 | 3306→3306 | `agivys_default` |
| `agivys-api-container` | Agivys | agivys-agivys-api | 5000→5000 | `agivys_default` |
| `agivys-landing-container` | Agivys | agivys-agivys-landing | 4213→80 | `agivys_default` |
| `agivys-portal-container` | Agivys | agivys-agivys-portal | 4214→80 | `agivys_default` |
| `joeder-portfolio` | Portfolio | nginx:alpine | 127.0.0.1:8081→80 | — |

Mapa de portas ocupadas (pra não colidir num projeto novo): **3306, 3307, 3317** (MySQL) · **4210-4214** (frontends) · **5000, 5011-5013** (Theos/Agivys APIs) · **6011-6013** (Nexas APIs) · **80, 443** (nginx do host) · **8081** (portfolio, só localhost).

## Nginx — como o tráfego HTTPS realmente chega nos containers

**Importante:** existe um `docker-compose.yml` em `/projects/shared/nginx/` que sobe um container `nginx-proxy` (imagem `nginx:latest`, portas 80/443, lê `theos.conf`) — **esse container NÃO está rodando e não é usado**. Não perca tempo editando `theos.conf` nem `/projects/shared/nginx/conf.d/`.

O proxy real é o **Nginx nativo do host, rodando como serviço systemd** (`systemctl status nginx`), instalado fora do Docker, padrão Debian/Ubuntu:

- Configs em `/etc/nginx/sites-available/<dominio>`, symlink em `/etc/nginx/sites-enabled/<dominio>`.
- Cada domínio tem um `server{}` com `proxy_pass http://127.0.0.1:<porta-publicada-do-container>` — ou seja, o Nginx do host fala com os containers via **portas publicadas no host** (`-p host:container`), não via rede Docker interna.
- SSL gerenciado pelo **Certbot** (`/etc/letsencrypt/live/<dominio>/`), blocos marcados `# managed by Certbot` — não editar manualmente essas linhas, e não rodar certbot de novo sem necessidade.
- Domínios ativos hoje:
  - `portaltheos.com.br` (+ www) → frontend Theos na raiz (`/` → 4210), `/portal-pat/` → 4211, `/portal-pop/` → 4212, `/api/` → 5011, `/admin-api/` → 5012, `/landing-api/` → 5013.
  - `joederblanca.com.br` (+ www) → portfólio na raiz (`/` → 8081), `/agivys/` → 4213, `/agivys/portal/` → 4214, `/agivys-api/` → 5000, `/nexas-api/` → 6011, `/nexas-admin-api/` → 6012, `/nexas-landing-api/` → 6013.

**Fluxo pra adicionar uma rota nova:**
1. `cp /etc/nginx/sites-available/<dominio> /etc/nginx/sites-available/<dominio>.bak-$(date +%Y%m%d%H%M%S)` (sempre backup antes de editar arquivo de produção).
2. Editar com `nano`, adicionar bloco `location /algo/ { proxy_pass http://127.0.0.1:<porta>/; ... headers padrão ... }` dentro do `server{}` HTTPS (o que tem `listen 443 ssl`), antes das linhas do Certbot.
3. `nginx -t` (só recarrega se der "successful").
4. `systemctl reload nginx`.
5. Testar com `curl -I https://<dominio>/algo/...`.

Se for domínio **novo** (nunca teve certificado), o fluxo muda: precisa apontar o DNS (registro A) pro IP da VPS antes, criar o server block em `sites-available` sem SSL primeiro, e rodar `certbot --nginx -d <dominio>` pra emitir o certificado (não documentado em detalhe aqui porque ainda não foi feito nesta sessão).

## Padrão dos backends .NET (Nexas/Theos)

- Cada API (`<Nome>.Api`, `<Nome>.Admin.Api`, `<Nome>.Landing.Api`) tem seu próprio `Dockerfile` multi-stage (SDK 8.0 pra build, `aspnet:8.0` runtime), expõe porta **8080** interna sempre.
- `docker-compose.yml` na raiz do backend sobe: 1 MySQL (`mysql_banco`, imagem `mysql:8.0`, auth `mysql_native_password`) + as 3 APIs, todas com `env_file: .env` e `depends_on: mysql_banco: condition: service_healthy`.
- **`.env` nunca é commitado** — só existe um `.env.example` no repo. Precisa ser criado manualmente em cada deploy com: `ConnectionStrings__DefaultConnection`, `Jwt__Key`/`Issuer`/`Audience`, credenciais de integrações externas (ex.: `Agivys__Email`/`Password`), e `COMPOSE_PROJECT_NAME` (ver regra de ouro acima).
- **Migration é automática**: a `Admin.Api` (só ela) roda `context.Database.Migrate()` no `Program.cs` no startup — não precisa (e não tem) `dotnet ef database update` manual. Basta a Admin.Api subir com sucesso.
- Swagger **desabilitado em produção** (comentado no `Program.cs`) — pra testar que uma API está viva, usar um endpoint público real (ex. `GET /api/v1/Courses` em `Nexas.Landing.Api`, `[AllowAnonymous]`) em vez de `/swagger/index.html` (sempre dá 404).
- Rede `agivys_default` é uma dependência **externa** obrigatória (`networks: agivys_default: external: true`) — as APIs do Theos e do Nexas se conectam nela pra falar com o Agivys (integração via `Agivys:Email`/`Agivris:Password` na config, injeta config extra via `AddAgivysConfiguration`). Precisa existir antes do `docker compose up` (normalmente já existe, criada pelo compose do próprio Agivys).
- CORS: o `Program.cs` do `Nexas.Api` libera só origens terminadas em `portalnexas.com.br`, `localhost` e `127.0.0.1` — se o frontend do Nexas subir em outro domínio, precisa editar essa policy.

## ⏳ Frontend do Nexas — pendente, ainda não subiu (próxima sessão)

Ainda não existe pasta `frontend` neste repo (`nexas`) nem em `/projects/nexas/` na VPS — o front deve vir de outro repositório/clone, igual o padrão do Theos (`/projects/theos/frontend`, separado do `/projects/theos/backend`). Antes de subir, resolver:

1. **Descobrir o stack e o Dockerfile do front** (React/Vue/Next/Angular?) — repetir o mesmo processo de leitura que fizemos com o backend (ler `Dockerfile`, ver porta interna exposta, ver se precisa de `.env`/build args com URL da API).
2. **Decidir o domínio antes de subir**, porque muda o fluxo:
   - Se for **`portalnexas.com.br`** (é o que o código já assume, ver CORS acima) → é domínio **novo** pro Nginx desta VPS. Precisa: DNS (registro A/AAAA) apontando pro IP da VPS → criar `server{}` em `/etc/nginx/sites-available/portalnexas.com.br` sem SSL → `certbot --nginx -d portalnexas.com.br -d www.portalnexas.com.br` pra emitir certificado → só depois adicionar os `location` dos containers.
   - Se for sob **`joederblanca.com.br`** (reaproveitando o domínio já configurado, como fizemos com as APIs) → **precisa editar a CORS do `Nexas.Api`/`Nexas.Admin.Api`** (`Program.cs`, política `DevelopmentCors`) pra liberar esse domínio, senão o front quebra com erro de CORS no navegador ao chamar a API.
3. **Porta livre para o(s) container(s) de front**: a faixa 4210-4214 já está ocupada (Theos + Agivys). Próxima livre: **4215** em diante (conferir com `docker ps` antes de fixar, caso algo tenha mudado).
4. Depois de decidido o domínio/porta, seguir o mesmo checklist genérico da seção "Checklist para subir um projeto novo" acima (`.env`, `COMPOSE_PROJECT_NAME` único, `docker compose up -d --build`, rota no Nginx, teste com `curl`).

## Checklist para subir um projeto novo (ou o Nexas de novo do zero)

1. `git clone`/`git pull` na pasta certa dentro de `/projects/<projeto>/`.
2. Conferir que a pasta local (`backend`, `frontend`, etc.) não colide de nome com outro projeto já existente — se colidir, planejar `COMPOSE_PROJECT_NAME` único desde o início.
3. `docker network ls` — confirmar que redes externas referenciadas no compose (ex. `agivys_default`) já existem.
4. Copiar `.env.example` → `.env`, preencher segredos reais (senha do banco deve bater com `MYSQL_ROOT_PASSWORD` do `docker-compose.yml`), adicionar `COMPOSE_PROJECT_NAME=<projeto>-<pasta>`.
5. `docker compose up -d mysql_banco` (ou nome do serviço de banco) — esperar `docker inspect --format='{{.State.Health.Status}}' <container>` retornar `healthy`.
6. `docker compose up -d --build <serviços-das-apis>` — a Admin API aplica as migrations sozinha no boot; conferir `docker logs <admin-api-container>` procurando `Applying migration '...'`.
7. Testar as APIs localmente na VPS via `curl http://localhost:<porta-publicada>/<endpoint-publico>`.
8. Configurar rota no Nginx do host (ver seção acima) e testar via `https://<dominio>/<path>/...`.
9. Documentar aqui (`vps/.agents/`) qualquer porta nova ocupada, volume novo, ou domínio novo, pra manter este arquivo atualizado.

## Segredos e credenciais

- Senha root do MySQL é a mesma em todos os stacks (`Likeaboos1970`) — está em texto puro nos `docker-compose.yml` e `.env` de cada projeto (risco conhecido, citado inclusive no `README.md` do Nexas como item de prioridade 1 a corrigir).
- Não reescrever senhas/chaves aqui: sempre ler o `.env` real na VPS (`cat /projects/<projeto>/backend/.env`) antes de assumir um valor — pode ter sido rotacionado.
