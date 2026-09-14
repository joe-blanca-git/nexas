export interface FinancialDashboardSummaryDto {
  totalAvailable: number;
  totalPending: number;
  totalWithdrawn: number;
}

export interface FinancialClosingItemSimulationDto {
  purchaseId: number;
  courseName: string;
  studentName: string;
  purchaseDate: string;
  paymentMethod: string;
  appliedTeacherPercentage: number;
  grossValue: number;
  bankFeeValue: number;
  nexasFeeValue: number;
  calculatedValue: number;
}

export interface FinancialClosingSimulationDto {
  grossRevenue: number;
  bankFeesTotal: number;
  nexasFeesTotal: number;
  netValue: number;
  totalToReceive: number;
  items: FinancialClosingItemSimulationDto[];
}

export type RefundStatus = 'Todos' | 'Pendente' | 'Aprovado' | 'Reprovado' | 'Processando' | 'Reembolsado' | 'Falha';

export interface RefundDashboardSummaryDto {
  totalPending: number;
  totalApproved: number;
  totalProcessing: number;
  totalRefunded: number;
  totalRefundedValue: number;
}

export interface RefundRequestDto {
  id: number;
  requestCode: string;
  requestDate: string;
  studentName: string;
  courseName: string;
  transactionCode: string;
  purchaseValue: number;
  courseProgress: number;
  status: RefundStatus;
  supportTicketCode: string;
  paymentMethod: string;
  isEligible: boolean;
}

export interface RefundableSaleDto {
  purchaseId: number;
  studentName: string;
  courseName: string;
  grossValue: number;
  paymentStatus: string;
  courseProgress: number;
  purchaseDate: string;
  paymentMethod: string;
  isEligible: boolean;
}

export enum TaxType {
  Pix = 1,
  CreditCard = 2,
  Nexas = 3
}

export interface FinancialTaxDto {
  id: number;
  type: TaxType;
  typeDescription: string;
  percentage: number;
  effectiveFrom: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateFinancialTaxCommand {
  type: TaxType;
  percentage: number;
  effectiveFrom: string;
}
