# Contexto do Portal SM para agentes (Claude)

Esta pasta existe para uma coisa só: fazer uma nova conversa/sessão do Claude entender o
`portal-sm` rapidamente, sem precisar re-explorar tudo do zero. Não é documentação para
humanos (isso é o `README.md` na raiz do projeto) — é contexto de trabalho: decisões,
padrões, armadilhas já pisadas, e o estado real (não o aspiracional) de cada parte.

Leia nesta ordem, conforme a tarefa:

- **[architecture.md](architecture.md)** — stack, estrutura de pastas, roteamento, padrões de
  código (standalone components, signals, reactive forms). Comece por aqui se for a primeira
  vez tocando no projeto nesta sessão.
- **[auth.md](auth.md)** — o sistema de autenticação inteiro: Identity + JWT customizado,
  formato das claims, guards, e principalmente o bug crítico já encontrado e corrigido (não
  reintroduza). Leia antes de mexer em qualquer coisa de login/token/perfil/roles.
- **[design-system.md](design-system.md)** — tokens de cor/spacing, padrões visuais (cards,
  abas, formulários, botões) já estabelecidos. Leia antes de criar qualquer tela/componente
  novo, pra manter consistência visual.
- **[pages-and-components.md](pages-and-components.md)** — inventário de páginas/componentes
  existentes, o que é funcional de verdade vs. placeholder "em construção".
- **[known-issues.md](known-issues.md)** — lacunas conhecidas e próximos passos já
  identificados. Confira antes de assumir que algo "ainda não foi feito" é surpresa.

## Regra de ouro desta pasta

Sempre que você (agente) descobrir algo não-óbvio sobre este projeto — um bug sutil, uma
decisão de design com um motivo específico, uma convenção que não é derivável só lendo o
código — atualize o arquivo relevante aqui. Isso é memória de trabalho para as próximas
sessões, não só para esta.

## Resumo de uma linha

Portal SM é o hub central de login/conta do ecossistema Nexas (Angular 18 + SSR no front,
ASP.NET Core Identity + JWT customizado no back, projeto `Nexas.SystemManager`).
