import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService } from '../../services/financial.service';
import { SignalRService, PaymentNotification } from '../../../../../core/services/signalr.service';
import { forkJoin, Subscription } from 'rxjs';

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
export class FinancialHomeComponent implements OnInit, OnDestroy {

  // ─── State ────────────────────────────────────────────────────────────────
  isLoading = true;
  searchTerm = '';
  private broadcastChannel: BroadcastChannel;
  private signalRSub?: Subscription;
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

  constructor(
    private financialService: FinancialService,
    private signalRService: SignalRService
  ) {
    this.broadcastChannel = new BroadcastChannel('payment_sync_channel');
  }

  ngOnInit(): void {
    this.loadTransactions();

    this.signalRSub = this.signalRService.paymentConfirmed$.subscribe((notification: PaymentNotification) => {
      if (notification.sucesso) {
        this.triggerToast('Pagamento confirmado em tempo real!');
        this.loadTransactions();
      }
    });

    this.broadcastChannel.onmessage = (event) => {
      if (event.data === 'payment_confirmed') {
        this.loadTransactions();
      }
    };
  }

  ngOnDestroy(): void {
    this.broadcastChannel.close();
    if (this.signalRSub) {
      this.signalRSub.unsubscribe();
    }
  }

  mapStatus(backendStatus: string): TransactionStatus {
    const s = backendStatus?.toUpperCase();
    if (s === 'APPROVED' || s === 'PAID' || s === 'ACTIVE') return 'Pago';
    if (s === 'PENDING') return 'Pendente';
    if (s === 'CANCELED' || s === 'EXPIRED') return 'Cancelado';
    if (s === 'REFUNDED') return 'Reembolsado';
    return 'Pendente';
  }

  loadTransactions(): void {
    this.isLoading = true;
    forkJoin({
      purchases: this.financialService.getMyPurchases(),
      subscription: this.financialService.getMySubscription()
    }).subscribe({
      next: (res) => {
        const txs: ITransaction[] = [];

        // Mapear Purchases
        if (res.purchases && Array.isArray(res.purchases)) {
          res.purchases.forEach(p => {
            txs.push({
              id: `PUR-${p.purchaseId}`,
              name: p.courseTitle,
              type: 'Curso',
              value: p.amount,
              paymentMethod: (p.paymentMethod === 'PIX' ? 'PIX' : 'Cartão de Crédito') as PaymentMethod,
              status: this.mapStatus(p.status),
              chargeDate: new Date(p.purchasedAt).toLocaleDateString(),
              nextRenewal: null,
              transactionCode: `NXS-PUR-${p.purchaseId}`,
              icon: 'fa-book',
              color: '#06b6d4',
              relatedCourseId: p.courseId
            });
          });
        }

        // Mapear Subscription
        if (res.subscription && res.subscription.subscriptionId) {
          const sub = res.subscription;
          
          if (sub.lastCharges && Array.isArray(sub.lastCharges)) {
            sub.lastCharges.forEach((c: any) => {
              txs.push({
                id: `SUB-CHG-${c.chargeId}`,
                name: sub.planName,
                type: 'Assinatura',
                value: c.amount,
                paymentMethod: 'PIX', // ou c.paymentMethod se existir no backend
                status: this.mapStatus(c.status),
                chargeDate: c.paymentDate ? new Date(c.paymentDate).toLocaleDateString() : 'N/A',
                nextRenewal: sub.nextDueDate ? new Date(sub.nextDueDate).toLocaleDateString() : null,
                transactionCode: `NXS-CHG-${c.chargeId}`,
                icon: 'fa-crown',
                color: '#6366f1'
              });
            });
          } else {
             // Caso não tenha cobranças listadas mas tenha assinatura
             txs.push({
              id: `SUB-${sub.subscriptionId}`,
              name: sub.planName,
              type: 'Assinatura',
              value: 0,
              paymentMethod: 'PIX',
              status: this.mapStatus(sub.status),
              chargeDate: sub.startDate ? new Date(sub.startDate).toLocaleDateString() : 'N/A',
              nextRenewal: sub.nextDueDate ? new Date(sub.nextDueDate).toLocaleDateString() : null,
              transactionCode: `NXS-SUB-${sub.subscriptionId}`,
              icon: 'fa-crown',
              color: '#6366f1'
            });
          }
        }

        this.transactions = txs;
        this.calculateSummary();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar compras', err);
        this.isLoading = false;
      }
    });
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
