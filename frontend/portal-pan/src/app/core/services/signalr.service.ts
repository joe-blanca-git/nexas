import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface PaymentNotification {
  sucesso: boolean;
  tipoCompra: 'AVULSO' | 'ANUAL';
  cursoId: number;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | undefined;
  
  // Subject for components to subscribe to
  private paymentConfirmedSource = new Subject<PaymentNotification>();
  paymentConfirmed$ = this.paymentConfirmedSource.asObservable();

  private paymentRefundedSource = new Subject<PaymentNotification>();
  paymentRefunded$ = this.paymentRefundedSource.asObservable();

  constructor() { }

  public startConnection(token: string, baseUrl: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    // Assumindo que a API base contém "api/v1", formatamos para a raiz do hub
    const hubUrl = baseUrl.replace('api/v1/', '') + 'hubs/payment';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection started'))
      .catch(err => console.error('Error while starting SignalR connection: ' + err));

    this.addReceivePaymentListener();
  }

  private addReceivePaymentListener() {
    if (!this.hubConnection) return;

    this.hubConnection.on('PaymentConfirmed', (data: PaymentNotification) => {
      this.paymentConfirmedSource.next(data);
    });

    this.hubConnection.on('PaymentRefunded', (data: PaymentNotification) => {
      this.paymentRefundedSource.next(data);
    });
  }

  public stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
