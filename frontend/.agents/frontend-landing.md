# Frontend Nexas — Contexto para IA (site institucional / landing)

Documento de referência para qualquer IA que precise mexer no frontend estático do Nexas (`frontend/landing/`). Espelha o `vps/.agents/vps-infrastructure.md` (mesmo espírito: fatos verificados, pendências reais, e uma seção de "regra de ouro" com bugs que já aconteceram nesta base de código — leia antes de editar qualquer página).

## O que é isto

Um site estático **sem build step**: HTML puro + um único `style.css` + `<script>` inline por página (vanilla JS, sem framework, sem bundler, sem `package.json`). Cada página é um arquivo `.html` autocontido que só depende de `style.css` (compartilhado) e dos arquivos em `assets/`. Não existe servidor de desenvolvimento — abre-se o `.html` direto no navegador ou serve-se a pasta com qualquer servidor estático.

**Não confundir com o `frontend/` da VPS**: `vps/.agents/vps-infrastructure.md` (seção "Frontend do Nexas — pendente") ainda descreve a busca por um frontend a ser deployado; este aqui É esse frontend, construído neste repo em `frontend/landing/`. Ainda não foi deployado na VPS — ver seção "Deploy" abaixo.

## Layout de diretórios

```
frontend/
├── .agents/
│   └── frontend-landing.md   # este arquivo
└── landing/                  # o site em si — tudo relativo a esta pasta
    ├── index.html            # home (hero, cursos, preços, trilhas, metodologia, projeto, depoimentos, FAQ)
    ├── course-detail.html    # template único de curso — dados via query string (?curso=<slug>)
    ├── nexpace.html          # portal de conteúdo (notícias/artigos/podcasts/vídeos/aulas grátis)
    ├── space-detail.html     # template único de publicação do Nexpace — dados via ?post=<slug>
    ├── sobre.html
    ├── for-enterprises.html  # formulário de proposta B2B
    ├── contato.html          # formulário de contato geral
    ├── central-de-ajuda.html # FAQ por categoria (pagamentos/reembolso/acesso/conta) + formulário de suporte
    ├── certificados.html     # explicação do sistema de certificados + verificador (código ou câmera/QR)
    ├── termos-de-uso.html
    ├── politica-de-privacidade.html
    ├── style.css             # ÚNICO arquivo de estilo — todas as páginas importam este mesmo arquivo
    └── assets/
        ├── logo.png, hero.jpeg, background-dark.jpeg, background-light.jpeg, favicon.ico
        └── slides-instuticional/1.jpeg … 8.jpeg   # slideshow do hero da home (1.jpeg e 3.jpeg são o MESMO arquivo, byte a byte — confirmado via md5sum)
```

11 páginas HTML, ~6.400 linhas somadas, 1 CSS de ~1.320 linhas. Sem `.js` separado — cada página termina com seu próprio `<script>` inline.

## Sistema de design (definido em `style.css`, `:root`)

- **Paleta**: fundo `--bg #0B1120` (+ `--bg-secondary`, `--bg-card`, `--bg-card-elevated` para profundidade), texto `--text-primary` a `--text-disabled`, acento `--accent #2563EB`/`--accent-hover #3B82F6`, mais `--green #10B981`, `--cyan #06B6D4`, `--purple #A78BFA` e `--amber #FB923C` (as duas últimas foram adicionadas só para o Nexpace, ver abaixo).
- **Tipografia**: Inter (corpo) + JetBrains Mono (usada como assinatura visual em "eyebrows" tipo comentário de código: `// como funciona`, números-fantasma, chaves de certificado etc.) — carregadas via Google Fonts CDN em cada `<head>`.
- **Espaçamento**: escala `--s-1` (4px) até `--s-24` (96px), todos os componentes usam essas variáveis.
- **Identidade "code/editorial"**: eyebrows como comentário mono (`// seção`), números grandes translúcidos atrás de linhas de conteúdo (`.ghost-num`), e uma moldura de cantos reaproveitada em cards importantes — classe `.bracket-frame` + 4 `<span class="corner corner-tl|tr|bl|br">` filhos. **Sempre que copiar um card com `.bracket-frame`, copiar os 4 spans de canto junto** — sem eles a classe não desenha nada (o CSS mira nos `.corner`, não no container).
- **Breakpoints usados de fato**: `1180px` (esconde a rail-nav lateral), `1024px`, `900px` (menu mobile via checkbox hack, sem JS pra abrir/fechar), `768px`, `640px`, `480px`.
- **Menu mobile**: checkbox (`#nav-toggle`) + `<label>` + seletor `:checked ~ .main-nav` — não usa JS pra abrir. JS só é usado pra **fechar** o menu ao clicar num link (senão o dropdown fica cobrindo a seção pra qual acabou de rolar).
- **Scroll reveal**: qualquer elemento com classe `.reveal` começa `opacity:0` e ganha `.is-in` via `IntersectionObserver` (padrão repetido em toda página, ver "Padrões de JS" abaixo).
- **Rail nav lateral** (pontinhos fixos à direita, um por seção da página): usada em várias páginas, cada uma com seu próprio conjunto de seções — **não é a mesma em todas as páginas**, então não copiar cegamente de uma página pra outra.

## Padrões de JS repetidos (copiados manualmente em cada página — não há módulo compartilhado)

Todo `<script>` de página segue mais ou menos esta ordem, cada bloco independente e sempre com guarda de existência do elemento antes de usar:

1. Fechar o menu mobile ao clicar num link (`navToggle.checked = false`).
2. Rail-nav: `IntersectionObserver` marcando `.is-active` no ponto cuja seção está visível (`rootMargin: "-45% 0px -45% 0px"`).
3. `.reveal` → `.is-in` via outro `IntersectionObserver` (`threshold: 0.15`), com fallback pra adicionar `.is-in` direto se `IntersectionObserver` não existir.
4. Específico da página: filtro de categoria (Nexpace), carrossel de cursos (`index.html`), formulário com confirmação client-side (`contato`, `for-enterprises`, `central-de-ajuda`), verificador de certificado + câmera (`certificados.html`), slideshow do hero + textos rotativos dos badges (`index.html`), renderização via `?query=` (`course-detail.html`, `space-detail.html`).

**Como isso é copiado manualmente em 11 arquivos, qualquer correção de bug num desses blocos (ex.: o fix do menu mobile, feito numa sessão anterior) precisa ser replicada arquivo por arquivo — sempre rodar `grep -rn "<trecho a corrigir>" *.html` antes de achar que "já corrigiu".**

## ⚠️ Regra de ouro — bugs reais que já aconteceram aqui

**1. `display` fixo numa regra de overlay ignora o atributo `hidden` (incidente real, `certificados.html`).**
O overlay da câmera (`.qr-scanner-overlay`) tinha `display: flex;` direto na regra base. Como estilo de autor sempre vence o estilo padrão do navegador (`[hidden]{display:none}`) mesmo com especificidade igual, a câmera aparecia **desde o carregamento da página**, sem clique nenhum — o `hidden` no HTML virava letra morta. Correção: nunca colocar `display` fixo em algo que alterna por `[hidden]`; sempre usar `.minha-classe:not([hidden]){ display: flex; }` e deixar a regra base sem `display`. Ver `.modal-overlay`, `.form-success` e `.qr-scanner-overlay` em `style.css` — os três já seguem esse padrão corrigido; qualquer overlay/modal novo **tem que seguir o mesmo molde**.

**2. Dois `transform` no mesmo elemento colidem — animação "come" o posicionamento (incidente real, badges flutuantes do hero).**
`.hero-badge--cert` usa `transform: translate(-50%, 50%)` pra se centralizar/descer na borda da foto. Ao tentar animar o mesmo elemento com uma flutuação (`transform: translate(...)` num `@keyframes`), a animação simplesmente substitui o transform de posição — o elemento perde a centralização. Solução aplicada: quando positioning E animação precisam de `transform` no mesmo elemento, **separar em dois elementos** — um wrapper só com a posição (`.hero-badge-cert-pos`, sem animação) e o card visual dentro dele livre pra animar (`.hero-badge`, com `animation:`). Reaplicar esse padrão sempre que for animar algo que já é centralizado/posicionado via `transform`.

**3. Nomes de classe "genéricos demais pra reaproveitar depois".**
O formulário de `for-enterprises.html` nasceu como `.enterprise-form`; quando o mesmo layout de formulário foi reaproveitado em `contato.html` e `central-de-ajuda.html`, a classe foi renomeada pra `.form-grid` (nome sem contexto de página) em todos os lugares. Se for criar um componente que só existe numa página, considerar de antemão se ele vai ser reaproveitado — economiza um rename depois.

**4. Nenhum template compartilhado ⇒ header/menu/rodapé só ficam consistentes se forem conferidos manualmente.**
Depois de várias sessões de edição incremental, o rodapé ("Plataforma") e o menu principal ficaram com itens faltando/fora de ordem em algumas páginas (`Preços` sumiu de 3 páginas, `Certificados` sumiu de 2, `Como funciona` sumiu de 5) — nada quebrado, só inconsistente. Corrigido nesta sessão; hoje as 11 páginas têm exatamente os mesmos 6 links na mesma ordem em "Plataforma", e os mesmos 6 no menu principal (`Cursos, Nexpace, Como funciona, Trilhas, Para empresas, Sobre`). **Ao adicionar/remover um link do menu ou rodapé, rodar o mesmo `grep`/`sed` nas 11 páginas, nunca só numa.** Comando útil pra auditar antes de mexer:
```bash
for f in *.html; do echo "== $f =="; awk '/<h4>Plataforma<\/h4>/{f=1} f{print} /<\/ul>/{if(f)exit}' "$f"; done
```

## Estado atual — o que é real vs. o que é fachada (demo)

Isto é **só frontend**, sem nenhuma integração com o backend Nexas (`vps/.agents/vps-infrastructure.md` documenta as APIs `.NET` reais). Confirmado por grep: **nenhum arquivo tem `fetch(`, `XMLHttpRequest` ou `axios`.** Tudo que parece "enviar dados" é fachada:

- **Formulários** (`contato.html`, `for-enterprises.html`, `central-de-ajuda.html`, newsletter no rodapé): `preventDefault()` no submit, troca a UI por uma mensagem de sucesso, e não manda nada a lugar nenhum.
- **Verificador de certificado** (`certificados.html`): compara o código digitado contra um objeto JS `DEMO_CERTIFICATES` com **2 certificados fixos** (`NEXAS-7F3K-2Q9R`, `NEXAS-4M8T-1X5V`). Qualquer outro código cai no estado "não encontrado". A leitura de QR usa a lib real `jsQR` (via CDN `cdn.jsdelivr.net/npm/jsqr@1.4.0`) + `getUserMedia` — a câmera funciona de verdade, mas o que ela decodifica ainda é validado contra os mesmos 2 códigos fixos.
- **Cursos e publicações do Nexpace**: conteúdo 100% hard-coded em objetos JS (`COURSES` em `course-detail.html`, `POSTS` em `space-detail.html`) — 4 cursos, 12 publicações. Não há CMS nem API.
- **Contato de verdade**: telefone/e-mail em `contato.html` são placeholders explícitos (`(00) 00000-0000`, `contato@nexas.com.br`) — combinado com o usuário, ainda não substituídos por dados reais.
- **CDN externo único**: `jsqr@1.4.0` via jsDelivr, carregado só em `certificados.html`. Todas as outras dependências (fontes) vêm do Google Fonts. Se o site for pra um ambiente sem internet/CDN, a leitura de QR quebra silenciosamente (a lib some, o `try/catch` já cobre isso e cai no fallback de digitar o código manualmente).

## Pendências conhecidas

- **`favicon.ico` existe em `assets/` mas nenhuma página tem `<link rel="icon">`** — confirmado por grep, nenhuma referência em nenhum `<head>`. Fácil de corrigir, só não foi pedido ainda.
- **`assets/background-light.jpeg` não é referenciado em lugar nenhum** — sobrou de quando se cogitou um tema claro; ou remover ou usar.
- **Sem dados reais de contato** (telefone/e-mail placeholders, ver acima).
- **Sem qualquer chamada de rede** — todos os "envios" são só front-end. Quando o backend (`Nexas.Api`/`Nexas.Landing.Api`, ver `vps/.agents/vps-infrastructure.md`) tiver endpoints prontos pra cursos/matrícula/contato, esses pontos (`COURSES`, `POSTS`, os 3 formulários, o verificador de certificado) são exatamente onde plugar `fetch()`.
- **CORS do backend só libera `portalnexas.com.br`, `localhost`, `127.0.0.1`** (ver `Nexas.Api/Program.cs`, citado no doc da VPS) — se este frontend for servido de outro domínio, a policy de CORS do backend precisa ser atualizada antes de qualquer integração funcionar.

## Deploy

Ainda **não foi deployado**. O doc da VPS (`vps/.agents/vps-infrastructure.md`, seção "Frontend do Nexas — pendente") descreve o checklist genérico (Dockerfile, domínio, porta, Nginx) mas foi escrito antes deste código existir — está desatualizado quanto ao stack (não é React/Vue/Next/Angular, é HTML/CSS/JS estático puro, então o Dockerfile mais simples é algo como `nginx:alpine` servindo `frontend/landing/` direto, sem etapa de build). Atualizar aquele documento quando o deploy for feito de verdade.

## Checklist para criar uma página nova

1. Copiar o `<head>` + `<header>` + `<footer>` + `<script>` base de uma página existente (ex. `sobre.html`, que não tem rail-nav nem lógica extra) — não reinventar a estrutura do zero.
2. Ajustar `<title>`, `<meta name="description">`, e o link "ativo" (se aplicável) no menu.
3. Se a página tiver seções própria, decidir se ganha rail-nav (`<nav class="rail-nav">` + `data-target` batendo com `id` de cada `<section>`) — nem toda página tem, ver quais IDs cada uma usa antes de copiar o JS do `IntersectionObserver`.
4. **Adicionar o link da página nova no menu principal E no rodapé ("Plataforma" ou a coluna que fizer sentido) das outras 10 páginas** — não existe include/partial, é copy-paste manual. Auditar com o comando `awk` da seção "Regra de ouro" acima antes de considerar terminado.
5. Validar antes de considerar pronto: balanceamento de tags HTML e sintaxe do `<script>` inline (não há linter automático no projeto — rodar um script Node ad-hoc que percorre as tags e verifica abre/fecha, como foi feito em todas as páginas desta sessão).
