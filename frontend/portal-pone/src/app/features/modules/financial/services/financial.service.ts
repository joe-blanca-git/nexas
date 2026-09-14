import { HttpClient } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';
import { FinancialDashboardSummaryDto, FinancialClosingSimulationDto, RefundDashboardSummaryDto, RefundRequestDto } from '../models/financial.model';

@Injectable({
  providedIn: 'root'
})
export class FinancialService extends BaseService {

  constructor(injector: Injector, private httpClient: HttpClient) {
    super(injector);
  }

  getDashboardSummary(): Observable<FinancialDashboardSummaryDto> {
    const url = `${this.urlApiNexas}FinancialClosings/Summary`;
    return this.httpClient.get<FinancialDashboardSummaryDto>(url, this.GetAuthHeaderJson());
  }

  simulateClosing(): Observable<FinancialClosingSimulationDto> {
    const url = `${this.urlApiNexas}FinancialClosings/Simulate`;
    return this.httpClient.get<FinancialClosingSimulationDto>(url, this.GetAuthHeaderJson());
  }

  processClosing(teacherId: number): Observable<any> {
    const url = `${this.urlApiNexas}FinancialClosings`;
    return this.httpClient.post(url, { teacherId }, this.GetAuthHeaderJson());
  }

  getTeachers(): Observable<any[]> {
    return this.httpClient.get<any[]>(`${this.urlApiNexas}Teachers`, this.GetAuthHeaderJson());
  }

  // --- Refunds & Authorizations ---

  getRefundSummary(startDate?: string, endDate?: string): Observable<RefundDashboardSummaryDto> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    const url = `${this.urlApiNexas}Refunds/Summary?${params.toString()}`;
    // Using dummy mock if not implemented yet, but keeping the correct signature.
    return this.httpClient.get<RefundDashboardSummaryDto>(url, this.GetAuthHeaderJson());
  }

  getRefunds(filters: any): Observable<RefundRequestDto[]> {
    const url = `${this.urlApiNexas}Refunds`;
    // For GET with body or complex filters, adjust as needed. Often it's query string for GET or a POST for complex filters.
    // We'll use POST /Refunds/search as a common pattern or pass params. Let's pass via params for simple GET.
    return this.httpClient.get<RefundRequestDto[]>(url, { headers: this.GetAuthHeaderJson().headers, params: filters });
  }

  approveRefund(id: number): Observable<any> {
    const url = `${this.urlApiNexas}Refunds/${id}/approve`;
    return this.httpClient.post(url, {}, this.GetAuthHeaderJson());
  }

  rejectRefund(id: number, reason: string): Observable<any> {
    const url = `${this.urlApiNexas}Refunds/${id}/reject`;
    return this.httpClient.post(url, { reason }, this.GetAuthHeaderJson());
  }

  executeRefund(id: number): Observable<any> {
    const url = `${this.urlApiNexas}Refunds/${id}/execute`;
    return this.httpClient.post(url, {}, this.GetAuthHeaderJson());
  }

  getSalesForRefund(searchTerm?: string): Observable<import('../models/financial.model').RefundableSaleDto[]> {
    const params: any = {};
    if (searchTerm) params.searchTerm = searchTerm;
    const url = `${this.urlApiNexas}Refunds/sales`;
    return this.httpClient.get<import('../models/financial.model').RefundableSaleDto[]>(url, { headers: this.GetAuthHeaderJson().headers, params });
  }

  requestRefund(purchaseId: number): Observable<any> {
    const url = `${this.urlApiNexas}Refunds/request`;
    return this.httpClient.post(url, { purchaseId }, this.GetAuthHeaderJson());
  }

  // --- Taxes (FinancialTaxes) ---

  getFinancialTaxes(): Observable<import('../models/financial.model').FinancialTaxDto[]> {
    const url = `${this.urlApiNexas}FinancialTaxes`;
    return this.httpClient.get<import('../models/financial.model').FinancialTaxDto[]>(url, this.GetAuthHeaderJson());
  }

  createFinancialTax(data: import('../models/financial.model').CreateFinancialTaxCommand): Observable<any> {
    const url = `${this.urlApiNexas}FinancialTaxes`;
    return this.httpClient.post(url, data, this.GetAuthHeaderJson());
  }

  toggleFinancialTaxStatus(id: number): Observable<any> {
    const url = `${this.urlApiNexas}FinancialTaxes/${id}/toggle-status`;
    return this.httpClient.patch(url, {}, this.GetAuthHeaderJson());
  }
}
