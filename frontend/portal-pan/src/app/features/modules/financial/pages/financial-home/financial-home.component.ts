import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// ─── Interfaces ────────────────────────────────────────────────────────────────

export type TransactionStatus = 'Pago' | 'Pendente' | 'Cancelado' | 'Reembolsado';
export type TransactionType = 'Curso' | 'Assinatura';
export type PaymentMethod = 'Cartão de Crédito' | 'PIX' | 'Boleto' | 'Cartão de Débito';

export interface ITransaction {
  id: string;
  name: string;
  type: TransactionType;
  value: number;
  paymentMethod: PaymentMethod;
  status: TransactionStatus;
  chargeDate: string;
  nextRenewal: string | null;
  transactionCode: string;
  icon: string;
  color: string;
  relatedCourseId?: number;
}

export interface IFinancialSummary {
  totalInvested: number;
  activeSubscription: string;
  nextCharge: string;
  coursesAcquired: number;
}

// ─── Component ─────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-financial-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './financial-home.component.html',
  styleUrl: './financial-home.component.scss'
})
export class FinancialHomeComponent implements OnInit {

  // ─── State ────────────────────────────────────────────────────────────────
  isLoading = true;
  searchTerm = '';
  selectedType: string = 'Todos';
  selectedStatus: string = 'Todos';
  sortField: keyof ITransaction = 'chargeDate';
  sortDirection: 'asc' | 'desc' = 'desc';
  currentPage = 1;
  itemsPerPage = 6;

  // ─── Modal ────────────────────────────────────────────────────────────────
  showDetailModal = false;
  selectedTransaction: ITransaction | null = null;
  toastMessage = '';
  showToast = false;

  // ─── Filter Options ───────────────────────────────────────────────────────
  typeOptions: string[] = ['Todos', 'Curso', 'Assinatura'];
  statusOptions: string[] = ['Todos', 'Pago', 'Pendente', 'Cancelado', 'Reembolsado'];

  // ─── Summary ─────────────────────────────────────────────────────────────
  summary: IFinancialSummary = {
    totalInvested: 0,
    activeSubscription: '',
    nextCharge: '',
    coursesAcquired: 0
  };

  // ─── Mock Data ────────────────────────────────────────────────────────────
  transactions: ITransaction[] = [];

  ngOnInit(): void {
    // Simula carregamento assíncrono dos dados mockados
    setTimeout(() => {
      this.transactions = [
        {
          id: 'TXN-2026-0001',
          name: 'Plano Nexas Premium — Assinatura Mensal',
          type: 'Assinatura',
          value: 89.90,
          paymentMethod: 'Cartão de Crédito',
          status: 'Pago',
          chargeDate: '05/05/2026',
          nextRenewal: '05/06/2026',
          transactionCode: 'NXS-SUB-2026-MAI-A9F2',
          icon: 'fa-crown',
          color: '#6366f1'
        },
        {
          id: 'TXN-2026-0002',
          name: 'Desenvolvimento Web com Angular',
          type: 'Curso',
          value: 297.00,
          paymentMethod: 'PIX',
          status: 'Pago',
          chargeDate: '15/03/2026',
          nextRenewal: null,
          transactionCode: 'NXS-CRS-2026-MAR-C1D8',
          icon: 'fa-laptop-code',
          color: '#6366f1',
          relatedCourseId: 1
        },
        {
          id: 'TXN-2026-0003',
          name: 'Banco de Dados NoSQL',
          type: 'Curso',
          value: 247.00,
          paymentMethod: 'Cartão de Crédito',
          status: 'Pago',
          chargeDate: '02/04/2026',
          nextRenewal: null,
          transactionCode: 'NXS-CRS-2026-ABR-E3F5',
          icon: 'fa-database',
          color: '#06b6d4',
          relatedCourseId: 2
        },
        {
          id: 'TXN-2026-0004',
          name: 'Plano Nexas Premium — Assinatura Mensal',
          type: 'Assinatura',
          value: 89.90,
          paymentMethod: 'Cartão de Crédito',
          status: 'Pago',
          chargeDate: '05/04/2026',
          nextRenewal: '05/05/2026',
          transactionCode: 'NXS-SUB-2026-ABR-B7G4',
          icon: 'fa-crown',
          color: '#6366f1'
        },
        {
          id: 'TXN-2026-0005',
          name: 'Estrutura de Dados em C#',
          type: 'Curso',
          value: 267.00,
          paymentMethod: 'Boleto',
          status: 'Pago',
          chargeDate: '10/02/2026',
          nextRenewal: null,
          transactionCode: 'NXS-CRS-2026-FEV-H2K9',
          icon: 'fa-code',
          color: '#10b981',
          relatedCourseId: 3
        },
        {
          id: 'TXN-2026-0006',
          name: 'Plano Nexas Premium — Assinatura Mensal',
          type: 'Assinatura',
          value: 89.90,
          paymentMethod: 'Cartão de Crédito',
          status: 'Pendente',
          chargeDate: '05/06/2026',
          nextRenewal: '05/07/2026',
          transactionCode: 'NXS-SUB-2026-JUN-P4L1',
          icon: 'fa-crown',
          color: '#6366f1'
        },
        {
          id: 'TXN-2026-0007',
          name: 'Arquitetura de Softwares Cloud',
          type: 'Curso',
          value: 317.00,
          paymentMethod: 'PIX',
          status: 'Reembolsado',
          chargeDate: '20/01/2026',
          nextRenewal: null,
          transactionCode: 'NXS-CRS-2026-JAN-R6M3',
          icon: 'fa-cloud',
          color: '#f59e0b',
          relatedCourseId: 4
        },
        {
          id: 'TXN-2026-0008',
          name: 'Introdução à Inteligência Artificial',
          type: 'Curso',
          value: 347.00,
          paymentMethod: 'Cartão de Débito',
          status: 'Cancelado',
          chargeDate: '18/04/2026',
          nextRenewal: null,
          transactionCode: 'NXS-CRS-2026-ABR-C9N7',
          icon: 'fa-robot',
          color: '#8b5cf6',
          relatedCourseId: 5
        }
      ];

      // Calcula o resumo financeiro a partir dos dados mockados
      this.calculateSummary();
      this.isLoading = false;
    }, 1200);
  }

  // ─── Summary Calculation ──────────────────────────────────────────────────

  calculateSummary(): void {
    const paid = this.transactions.filter(t => t.status === 'Pago');
    this.summary.totalInvested = paid.reduce((sum, t) => sum + t.value, 0);
    this.summary.coursesAcquired = this.transactions.filter(
      t => t.type === 'Curso' && (t.status === 'Pago' || t.status === 'Pendente')
    ).length;
    const activeSub = this.transactions.find(t => t.type === 'Assinatura' && t.status === 'Pago');
    this.summary.activeSubscription = activeSub ? activeSub.name.split('—')[0].trim() : 'Nenhuma';
    const pendingSub = this.transactions.find(t => t.type === 'Assinatura' && t.status === 'Pendente');
    const nextSub = pendingSub || activeSub;
    this.summary.nextCharge = nextSub?.chargeDate || '—';
  }

  // ─── Filtering & Sorting ──────────────────────────────────────────────────

  get filteredTransactions(): ITransaction[] {
    let result = this.transactions.filter(t => {
      const matchSearch =
        t.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        t.transactionCode.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchType = this.selectedType === 'Todos' || t.type === this.selectedType;
      const matchStatus = this.selectedStatus === 'Todos' || t.status === this.selectedStatus;
      return matchSearch && matchType && matchStatus;
    });

    // Ordenação
    result = result.sort((a, b) => {
      const valA = a[this.sortField] ?? '';
      const valB = b[this.sortField] ?? '';
      if (typeof valA === 'number' && typeof valB === 'number') {
        return this.sortDirection === 'asc' ? valA - valB : valB - valA;
      }
      return this.sortDirection === 'asc'
        ? String(valA).localeCompare(String(valB))
        : String(valB).localeCompare(String(valA));
    });

    return result;
  }

  get paginatedTransactions(): ITransaction[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredTransactions.slice(start, start + this.itemsPerPage);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredTransactions.length / this.itemsPerPage);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  sortBy(field: keyof ITransaction): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.currentPage = 1;
  }

  setPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedType = 'Todos';
    this.selectedStatus = 'Todos';
    this.currentPage = 1;
  }

  // ─── Modal ────────────────────────────────────────────────────────────────

  openDetail(tx: ITransaction): void {
    this.selectedTransaction = tx;
    this.showDetailModal = true;
  }

  closeDetail(): void {
    this.showDetailModal = false;
    this.selectedTransaction = null;
  }

  downloadReceipt(tx: ITransaction): void {
    this.triggerToast(`Comprovante de "${tx.name}" sendo gerado...`);
    this.closeDetail();
  }

  renewSubscription(tx: ITransaction): void {
    this.triggerToast(`Renovação de "${tx.name}" iniciada com sucesso!`);
    this.closeDetail();
  }

  // ─── Toast ────────────────────────────────────────────────────────────────

  triggerToast(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => (this.showToast = false), 3000);
  }

  // ─── Status Helpers ───────────────────────────────────────────────────────

  getStatusBadgeClass(status: TransactionStatus): string {
    const map: Record<TransactionStatus, string> = {
      'Pago': 'badge-status-paid',
      'Pendente': 'badge-status-pending',
      'Cancelado': 'badge-status-cancelled',
      'Reembolsado': 'badge-status-refunded'
    };
    return map[status] ?? 'badge-secondary';
  }

  getStatusIcon(status: TransactionStatus): string {
    const map: Record<TransactionStatus, string> = {
      'Pago': 'fa-check-circle',
      'Pendente': 'fa-clock',
      'Cancelado': 'fa-times-circle',
      'Reembolsado': 'fa-undo-alt'
    };
    return map[status] ?? 'fa-circle';
  }

  getPaymentMethodIcon(method: PaymentMethod): string {
    const map: Record<PaymentMethod, string> = {
      'Cartão de Crédito': 'fa-credit-card',
      'Cartão de Débito': 'fa-credit-card',
      'PIX': 'fa-qrcode',
      'Boleto': 'fa-barcode'
    };
    return map[method] ?? 'fa-money-bill';
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
