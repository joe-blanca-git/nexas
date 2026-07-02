# PortalPan

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 18.2.21.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

---

## 🚀 Novas Funcionalidades Implementadas (Asaas + SignalR)

### 1. Comunicação em Tempo Real (WebSockets / SignalR)
Foi adicionado o pacote `@microsoft/signalr` e configurado o `SignalRService` (`src/app/core/services/signalr.service.ts`).
O frontend agora é capaz de se conectar via JWT Token e escutar eventos emitidos pelo servidor. 
- **Eventos escutados**: `PaymentConfirmed` e `PaymentRefunded`.
- Quando um pagamento é processado via Webhook no servidor, a tela reage em tempo real sem precisar de refresh.

### 2. Integração de Roteamento Dinâmico (Checkout / Pagamento)
A tela `financial-payment.component.ts` foi atualizada com as seguintes lógicas:
- **Proteção de Rota Imediata**: Ao carregar o componente, a API `/api/v1/financeiro/checkout/pendencias` retorna a flag `jaPago`. Se for verdadeiro (usuário já é assinante ativo ou já comprou o curso), o fluxo de checkout é bloqueado e ele é redirecionado (`/courses/details/:id` ou `/courses`).
- **Bloqueio Visual de Pendências**: Caso um PIX esteja "PENDING" ou um Cartão "REJECTED", os botões e abas de outras opções de pagamento ficam bloqueados e banners informativos do Bootstrap alertam o cliente para impedir pagamentos em duplicidade.
- **Auto-Redirecionamento**: Em conjunto com o SignalR, assim que um pagamento via celular é concluído (PIX, etc), o WebSocket força o redirecionamento automático da página no frontend para liberar a tela de consumo de aulas.
