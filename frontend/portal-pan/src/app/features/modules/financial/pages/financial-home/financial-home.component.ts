import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService } from '../../services/financial.service';
import { SignalRService, PaymentNotification } from '../../../../../core/services/signalr.service';
import { forkJoin, Subscription } from 'rxjs';
import { jsPDF } from 'jspdf';

// ─── Interfaces ────────────────────────────────────────────────────────────────

export type TransactionStatus = 'Pago' | 'Pendente' | 'Cancelado' | 'Reembolsado';
export type TransactionType = 'Curso';
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
  isRefundable?: boolean;
}

export interface IFinancialSummary {
  totalInvested: number;

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
  statusOptions: string[] = ['Todos', 'Pago', 'Pendente', 'Cancelado', 'Reembolsado'];

  // ─── Summary ─────────────────────────────────────────────────────────────
  summary: IFinancialSummary = {
    totalInvested: 0,
    coursesAcquired: 0
  };

  // ─── Transactions List ─────────────────────────────────────────────────────
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
    this.financialService.getMyPortalTransactions().subscribe({
      next: (res) => {
        const txs: ITransaction[] = [];

        if (res && Array.isArray(res)) {
          const now = new Date();
          res.forEach(p => {
            const payDate = new Date(p.paymentDate);
            const diffTime = now.getTime() - payDate.getTime();
            const diffDays = diffTime / (1000 * 60 * 60 * 24);
            const statusStr = p.status?.toUpperCase() || '';
            const isPaid = statusStr === 'APPROVED' || statusStr === 'PAID' || statusStr === 'ACTIVE';
            const isRefundable = isPaid && diffDays <= 7;

            txs.push({
              id: p.id.toString(),
              name: p.name,
              type: 'Curso',
              value: p.value,
              paymentMethod: (p.paymentMethod === 'PIX' ? 'PIX' : 'Cartão de Crédito') as PaymentMethod,
              status: this.mapStatus(p.status),
              chargeDate: payDate.toLocaleDateString(),
              nextRenewal: null,
              transactionCode: p.transactionCode,
              icon: 'fa-book',
              color: '#06b6d4',
              isRefundable,
              relatedCourseId: p.courseId
            });
          });
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
  }

  // ─── Filtering & Sorting ──────────────────────────────────────────────────

  get filteredTransactions(): ITransaction[] {
    let result = this.transactions.filter(t => {
      const matchSearch =
        t.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        t.transactionCode.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchStatus = this.selectedStatus === 'Todos' || t.status === this.selectedStatus;
      return matchSearch && matchStatus;
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
    this.triggerToast(`Gerando comprovante de "${tx.name}"...`);

    const renderPdf = (logoImg?: HTMLImageElement) => {
      const doc = new jsPDF();
      
      const primaryColor = '#0f172a'; // Slate-900 (premium dark)
      const accentColor = '#6366f1'; // Indigo-500
      const textColor = '#334155';
      const lightGray = '#f8fafc';
      
      // Header Background
      doc.setFillColor(primaryColor);
      doc.rect(0, 0, 210, 45, 'F');
      
      if (logoImg) {
        // Render image maintaining aspect ratio
        const aspectRatio = logoImg.width / logoImg.height;
        const targetHeight = 14;
        const targetWidth = targetHeight * aspectRatio;
        doc.addImage(logoImg, 'PNG', 15, 15, targetWidth, targetHeight);
      } else {
        doc.setTextColor('#ffffff');
        doc.setFontSize(24);
        doc.setFont('helvetica', 'bold');
        doc.text('NEXAS', 15, 28);
      }
      
      doc.setTextColor('#94a3b8');
      doc.setFontSize(8);
      doc.setFont('helvetica', 'normal');
      doc.text('COMPROVANTE DE PAGAMENTO', 195, 22, { align: 'right' });
      
      doc.setTextColor('#ffffff');
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text(tx.transactionCode, 195, 29, { align: 'right' });

      // Title Section
      doc.setTextColor(primaryColor);
      doc.setFontSize(15);
      doc.setFont('helvetica', 'bold');
      doc.text('Detalhes da Transação', 15, 65);

      doc.setDrawColor('#e2e8f0');
      doc.setLineWidth(0.5);
      doc.line(15, 70, 195, 70);

      let startY = 82;
      const drawRow = (label: string, value: string) => {
        doc.setTextColor('#64748b');
        doc.setFontSize(10);
        doc.setFont('helvetica', 'normal');
        doc.text(label, 15, startY);
        
        doc.setTextColor(primaryColor);
        doc.setFontSize(10);
        doc.setFont('helvetica', 'bold');
        doc.text(value, 195, startY, { align: 'right' });
        
        doc.setDrawColor('#f1f5f9');
        doc.line(15, startY + 4, 195, startY + 4);
        
        startY += 12;
      };

      drawRow('Data do Pagamento:', tx.chargeDate);
      drawRow('Descrição:', tx.name);
      drawRow('Tipo de Produto:', tx.type);
      drawRow('Método de Pagamento:', tx.paymentMethod);
      drawRow('Status do Pagamento:', tx.status);
      
      if (tx.nextRenewal) {
        drawRow('Próxima Renovação:', tx.nextRenewal);
      }

      // Total Box (Rounded Rect)
      startY += 10;
      doc.setFillColor(lightGray);
      doc.setDrawColor(accentColor);
      doc.setLineWidth(0.5);
      doc.roundedRect(15, startY, 180, 28, 3, 3, 'FD'); // Fill and border
      
      doc.setTextColor('#64748b');
      doc.setFontSize(10);
      doc.setFont('helvetica', 'bold');
      doc.text('VALOR TOTAL PAGO', 25, startY + 16);
      
      doc.setTextColor(accentColor);
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text(this.formatCurrency(tx.value), 185, startY + 18, { align: 'right' });

      // Footer
      doc.setTextColor('#94a3b8');
      doc.setFontSize(8);
      doc.setFont('helvetica', 'normal');
      doc.text('Este documento é um comprovante de pagamento válido e gerado eletronicamente.', 105, 275, { align: 'center' });
      doc.text('Plataforma Educacional Nexas - CNPJ: 00.000.000/0001-00', 105, 280, { align: 'center' });
      doc.text(window.location.origin, 105, 285, { align: 'center' });

      doc.save(`Comprovante_${tx.transactionCode}.pdf`);
      this.closeDetail();
    };

    // Carrega a logo dinamicamente para renderizar no PDF
    const img = new Image();
    img.src = '/logos/logo-nobg.png';
    img.onload = () => renderPdf(img);
    img.onerror = () => renderPdf(); // Falha graciosa se não encontrar a imagem
  }

  requestRefund(tx: ITransaction): void {
    if (!tx.isRefundable) return;
    
    if (confirm(`Tem certeza que deseja solicitar o reembolso de "${tx.name}"? Isso cancelará seu acesso ao curso imediatamente e a reversão não será possível.`)) {
      // Futura chamada à API de reembolso. Por enquanto mock visual
      this.triggerToast(`Solicitação de reembolso enviada para ${tx.transactionCode}! Em breve você receberá um email.`);
      this.closeDetail();
    }
  }

  continuePayment(tx: ITransaction): void {
    if (tx.relatedCourseId) {
      this.closeDetail();
      window.location.href = `http://localhost:4200/portal-pan/financial/payment/${tx.relatedCourseId}?plan=single`;
    } else {
      this.triggerToast('ID do curso não encontrado para continuar o pagamento.');
    }
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
