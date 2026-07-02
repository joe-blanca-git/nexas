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
export interface PendenciaDTO {
  temPendencia: boolean;
  status: string;
  metodoPagamento: string;
  pixCopiaECola?: string;
  qrCodeBase64?: string;
  mensagem?: string;
  jaPago?: boolean;
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

  verificarPendencias(cursoId: number | undefined, tipoCompra: string): Observable<PendenciaDTO> {
    let url = `${this.urlApiNexas}financeiro/checkout/pendencias?tipoCompra=${tipoCompra}`;
    if (cursoId) {
      url += `&cursoId=${cursoId}`;
    }
    return this.http.get<PendenciaDTO>(url, this.GetAuthHeaderJson());
  }

  getApiUrl(): string {
    return this.urlApiNexas;
  }
}
