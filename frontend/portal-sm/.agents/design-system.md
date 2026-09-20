# Design system

Não existe um framework de UI (nem Material, nem Tailwind). O visual inteiro é SCSS +
CSS custom properties definidas em `src/styles.scss`. Antes de criar uma tela nova, siga os
padrões abaixo em vez de inventar um novo.

## Tokens de cor (CSS custom properties)

Definidos em `:root` (tema dark, o padrão) e sobrescritos em `.theme-light` (aplicada no
`<body>` pelo `ThemeService`). **Sempre usar as variáveis, nunca cor fixa direto no CSS**
(exceção: cores de estado pontuais como erro de validação, ex. `#F87171`, que já aparecem
hardcoded em alguns lugares — não é o ideal, mas é o padrão atual).

```scss
--bg-primary       // fundo da página
--bg-secondary      // fundo de seções/inputs
--bg-card           // fundo de cards/painéis
--bg-card-hover     // hover de cards
--bg-nav            // fundo da navbar (com transparência)

--border-color      // borda padrão
--border-hover      // borda em hover/focus
--border-light      // borda sutil

--text-primary      // texto principal
--text-secondary    // texto secundário
--text-muted        // texto apagado/placeholder

--accent-cyan       // cor de destaque principal (links, botões primários, aba ativa)
--accent-green
--accent-blue
--accent-yellow
--accent-purple     // usado em conjunto com --accent-cyan em gradientes (ex. avatar)

--shadow-sm / --shadow-md / --shadow-lg
```

Tema é alternado trocando a classe `theme-light`/`theme-dark` no `<body>` — ver
`ThemeService.setTheme()`. Componente novo não precisa saber disso, só usar as variáveis.

## Tipografia

Fonte: `'Outfit'` (ver `styles.scss`). Tamanhos usados nas telas do dashboard:
- Título de página (`h1`): `2rem`, `font-weight: 600`
- Título de card/seção (`h2`/similar): `1.5rem`, `font-weight: 600`
- Corpo: `0.9375rem`
- Legenda/hint: `0.75rem`–`0.8125rem`, cor `--text-muted` ou `--text-secondary`

## Padrões de componente já estabelecidos

### Card genérico

`border-radius: 16px`, `background: var(--bg-card)`, `border: 1px solid var(--border-color)`,
`box-shadow: var(--shadow-sm)` (ou `--shadow-md`/`--shadow-lg` em hover/destaque). Ver
`portal-card.component.scss` e `account.component.scss` (`.account-content`,
`.account-tabs`).

### Ícones

Sempre SVG inline no template (não *icon font*, não biblioteca de ícones importada), estilo
"feather/lucide": `viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"`.
Copiar um ícone existente do mesmo estilo em vez de importar algo novo.

### Botão primário

```scss
background: var(--accent-cyan);
color: #04141F;          // texto escuro sobre o ciano, não var(--text-primary)
border-radius: 10px;
font-weight: 600;
&:hover:not(:disabled) { filter: brightness(1.08); }
&:disabled { opacity: 0.7; cursor: not-allowed; }
```
Ver `.profile-save-btn` em `profile-tab.component.scss`.

### Formulário (campo + label + erro)

```html
<div class="profile-field">
  <label for="x">Rótulo</label>
  <input id="x" formControlName="x" placeholder="..." />
  @if (x?.invalid && x?.touched) {
    <span class="profile-field__error">Mensagem de erro.</span>
  }
</div>
```
Input: `background: var(--bg-secondary)`, `border: 1px solid var(--border-color)`,
`border-radius: 10px`, foco com `border-color: var(--accent-cyan)` +
`box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.15)`. Ver `profile-tab.component.scss` como
referência completa (inclui estado `:disabled` para campos somente-leitura).

Em telas mais largas, campos relacionados (ex. Nome + Telefone) vão lado a lado num
`display: grid; grid-template-columns: 1fr 1fr` com um breakpoint pra colapsar em 1 coluna
(`profile-fields__row`) — evita formulário "grudado" numa coluna estreita dentro de um card
largo. **Não deixe um formulário com `max-width` pequeno dentro de um card largo** — isso já
foi um problema real (vazio enorme à direita) e foi corrigido assim.

### Abas verticais (painel de configurações)

Padrão estabelecido em `AccountComponent` (`account.component.html`/`.scss`):
- Desktop: coluna vertical fixa (`width: 240px`) à esquerda, itens `flex` com ícone + label.
- Mobile (`≤768px`): **grid 2 colunas** (`grid-template-columns: repeat(2, 1fr)`), item vira
  coluna (ícone em cima, label embaixo, centralizado). **Não usar um scroll horizontal de
  abas no mobile** — já foi tentado, ficava com cara de quebrado (texto cortado no meio,
  sem indicação visual de que dava pra rolar). Grid 2x2 resolveu isso de vez.
- Item ativo: `background: var(--bg-secondary); color: var(--accent-cyan);`.
- Conteúdo da aba ativa (`@switch`) **não repete o título da aba** dentro do painel de
  conteúdo (seria redundante com o botão da aba já selecionado/destacado).

### Avatar com iniciais

Círculo com gradiente `linear-gradient(135deg, var(--accent-cyan), var(--accent-purple))`,
texto branco, iniciais = primeira letra do nome. Quando há foto (preview local ou, no futuro,
URL persistida), troca pra `<img>` com `object-fit: cover`, mesmo tamanho/`border-radius: 50%`.
Ver `.profile-avatar__circle` em `profile-tab.component.scss`.

Botão de editar/trocar foto: badge circular pequeno (`26px`) posicionado
`position: absolute; bottom/right: -2px` sobre o canto do avatar, com borda da cor do card por
trás (`border: 2px solid var(--bg-card)`) pra "recortar" visualmente do círculo do avatar.
Abre um menu simples (posição absoluta, com backdrop fixo transparente atrás pra fechar ao
clicar fora) — ver `.profile-avatar__menu`/`.profile-avatar__backdrop`.

### Modal simples (overlay central)

Padrão usado pelo modal de câmera (`.camera-modal` em `profile-tab.component.scss`): backdrop
fixo cobrindo a tela inteira (`position: fixed; inset: 0`) com fundo escuro semi-transparente
(`rgba(3, 7, 18, 0.75)`, não usa nenhuma variável de tema — é sempre escuro, funciona igual em
ambos os temas) atrás de um painel centralizado (`display: flex; align-items/justify-content: center`
no container pai) usando os mesmos tokens de card (`--bg-card`, `--border-color`,
`border-radius: 16px`, `--shadow-lg`). Dois botões de ação lado a lado (`flex: 1` cada),
primário ciano + secundário neutro — mesmo par de classes (`--primary`/`--secondary`) que
outros lugares já usam para ações destrutivas/neutras. Reutilize essa estrutura para qualquer
modal futuro em vez de inventar outra.

### Lista de cards horizontais (não confundir com grid)

Padrão em `.app-card` (`applications.component.scss`) pra listar recursos que o usuário
cadastrou (aplicações, e futuramente organizações etc.): card **largo, full-width, empilhado
verticalmente** (`.app-list { flex-direction: column }`), não um grid de tiles quadrados como
o `portal-card` da home. Estrutura interna: logo/emblema quadrado (56px, `border-radius: 14px`,
gradiente das cores da entidade ou iniciais) à esquerda, corpo (título + badge de status +
descrição + metadados com ícone) no meio ocupando o espaço, ações (ícones de editar/excluir)
à direita. Badge de status usa fundo translúcido da cor + ponto sólido
(`background: rgba(cor, 0.12); color: var(--accent-X)`), não uma pílula sólida. Use esse
padrão sempre que for listar "coisas que o usuário cadastrou" — é mais elegante que um grid
denso e escala melhor pra descrições longas.

### Seletor de cor

`<input type="color" formControlName="...">` puro (nativo do navegador), 40x40px,
`border-radius: 8px`, com o valor hex exibido ao lado em texto monoespaçado — ver
`.app-form-color` (`application-form-modal.component.scss`). Não usa nenhum color-picker
customizado; o valor já vem como string `#rrggbb`, compatível direto com reactive forms.

### Modal com estado de sucesso (mostrar segredo uma vez)

Padrão em `ApplicationFormModalComponent`: o mesmo modal tem dois estados renderizados via
`@if (createdApp(); as app) { ... } @else { ... }` — formulário normal, e depois de salvar
com sucesso, troca pro estado de sucesso (ícone de check verde, a informação sensível/importante
em destaque num `<code>` com botão de copiar, e um botão único "Concluir" fechando o modal).
Use esse padrão sempre que uma ação gerar algo que o usuário precisa copiar/guardar na hora
(chaves, tokens, senhas geradas) — não force ele a caçar essa informação depois de fechar o
modal.

### Toggle switch (boolean)

Padrão em `.toggle` (`privacy-tab.component.scss`): `<input type="checkbox">` visualmente
escondido (`opacity: 0`, mas ocupando o espaço todo pra continuar clicável/acessível) +
2 `<span>` irmãos (`__track` e `__thumb` dentro dele) estilizados via seletor `input:checked + .toggle__track`.
Não usa nenhum componente de UI kit — é só isso, ~40 linhas de CSS. Reutilize esse padrão pra
qualquer boolean on/off; não introduza uma lib de UI só pra isso.

### Responsivo — breakpoints usados

Não há um sistema formal de breakpoints (nem mixins), mas os valores usados consistentemente
são `768px` (tablet/mobile) e `560px`/`480px` (mobile menor, pra colapsar grids de 2 colunas
em 1). Usar media queries inline dentro de cada regra SCSS (aninhadas via `&`), não um arquivo
de breakpoints separado — é o padrão do projeto até agora.

### Toast

Global, renderizado por `<app-toast>` (incluído só uma vez, em `app.component.html`).
Chamar via `ToastService.success()/error()/info()/warning()`. Fica fixo no canto
inferior-direito da tela.

**Cuidado conhecido**: o toast pode colidir visualmente com o texto do footer (canto
inferior-direito também) em viewports onde os dois calham de aparecer juntos — ver
[known-issues.md](known-issues.md). Não é bloqueante, mas fique atento se mexer no z-index ou
posicionamento de qualquer um dos dois.
