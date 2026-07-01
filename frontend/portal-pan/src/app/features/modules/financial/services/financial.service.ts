import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';

export interface PixRequest {
  cursoId: number;
  tipoCompra: 'AVULSO' | 'ANUAL';
  cpf: string;
  valor: number;
}

export interface PixResponse {
  sucesso: boolean;
  cobrancaId: string;
  pixCopiaECola: string;
  qrCode: string;
}

// Essa interface pode ser mantida aqui para tipar o que vem do CoursesService
export interface CheckoutSummary {
  id: number;
  title: string;
  imgCoverLink: string;
  priceSingle: number;
  priceSubscription: number;
}

@Injectable({
  providedIn: 'root'
})
export class FinancialService extends BaseService {
  constructor(injector: Injector, private http: HttpClient) {
    super(injector);
  }

  gerarPixAsaas(payload: PixRequest): Observable<PixResponse> {
    return this.http.post<PixResponse>(`${this.urlApiNexas}financeiro/checkout/pix`, payload, this.GetAuthHeaderJson());
  }
}
