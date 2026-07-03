import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FinancialService, CheckoutSummary, PixResponse, PendenciaDTO } from '../../services/financial.service';
import { CoursesService } from '../../../courses/services/courses.service';
import { SignalRService, PaymentNotification } from '../../../../../core/services/signalr.service';
import { AuthUtil } from '../../../../../core/auth/auth.util';

@Component({
  selector: 'app-financial-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './financial-payment.component.html',
  styleUrl: './financial-payment.component.scss'
})
export class FinancialPaymentComponent implements OnInit {
  paymentMethod: 'PIX' | 'CREDIT' | 'DEBIT' = 'PIX';
  
  // States
  isProcessing = false;
  paymentSuccess = false;
  paymentError = false;
  errorMessage = '';

  // Forms
  cardForm!: FormGroup;
  
  // PIX specifics
  pixCpf = '';
  qrCodeGenerated = false;
  pixCopiaECola = '';
  qrCodeUrl = '';
  isLoadingPix = false;

  // Business Rules States
  transacaoPendente: PendenciaDTO | null = null;
  bloquearOutrosMetodos = false;

  cursoId: number = 0;
  checkoutSummary?: CheckoutSummary;
  tipoCompra: 'AVULSO' = 'AVULSO';
  valorTotal = 0;
  isLoadingCourse = true;

  // Mock data for installments
  installments: { value: number, label: string }[] = [];

  private broadcastChannel: BroadcastChannel;

  constructor(
    private fb: FormBuilder, 
    private router: Router, 
    private route: ActivatedRoute,
    private financialService: FinancialService,
    private coursesService: CoursesService,
    private signalRService: SignalRService,
    private authUtil: AuthUtil
  ) {
    this.broadcastChannel = new BroadcastChannel('payment_sync_channel');
    this.broadcastChannel.onmessage = (event) => {
      if (event.data === 'payment_confirmed') {
        this.redirectOnSuccess(this.cursoId);
      }
    };
  }

  ngOnDestroy() {
    this.broadcastChannel.close();
  }

  ngOnInit() {
    this.initForm();
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      this.cursoId = Number(idParam);
      this.loadCourseData();
    } else {
      this.router.navigate(['/courses']);
    }

    // Inicializar o WebSocket (SignalR)
    const token = this.authUtil.getCookieAuth();
    if (token) {
      // Como SignalR precisa apenas da base URL
      this.signalRService.startConnection(token, this.financialService.getApiUrl());

      this.signalRService.paymentConfirmed$.subscribe((notification: PaymentNotification) => {
        if (notification.sucesso) {
          this.broadcastChannel.postMessage('payment_confirmed');
          alert('Pagamento processado e aprovado com sucesso!');
          this.redirectOnSuccess(notification.cursoId);
        }
      });

      this.signalRService.reconnected$.subscribe(() => {
        console.log('WebSocket reconectado, verificando pendências atualizadas...');
        this.verificarPendencias();
      });
    }

    this.verificarPendencias();
  }

  redirectOnSuccess(cursoId: number) {
    this.router.navigate(['/courses/details', cursoId]);
  }

  verificarPendencias() {
    this.financialService.verificarPendencias(this.cursoId).subscribe({
      next: (res: PendenciaDTO) => {
        // Regra de Roteamento (Proteção de Rota)
        if (res && res.jaPago) {
          alert('Você já possui acesso ativo a este conteúdo!');
          this.router.navigate(['/courses/details', this.cursoId]);
          return;
        }

        if (res && res.temPendencia) {
          this.transacaoPendente = res;
          this.tratarPendencia(res);
        } else {
          // Se backend disser que não tem pendência (foi cancelado ou expirou), liberamos a UI
          this.transacaoPendente = null;
          this.bloquearOutrosMetodos = false;
          this.qrCodeGenerated = false;
          this.pixCopiaECola = '';
          this.qrCodeUrl = '';
        }
      },
      error: (err) => console.error('Erro ao verificar pendências', err)
    });
  }

  tratarPendencia(pendencia: PendenciaDTO) {
    if (pendencia.metodoPagamento === 'PIX' && pendencia.status === 'PENDING') {
      this.bloquearOutrosMetodos = true;
      this.setPaymentMethod('PIX');
      if (pendencia.pixCopiaECola && pendencia.qrCodeBase64) {
        this.qrCodeGenerated = true;
        this.pixCopiaECola = pendencia.pixCopiaECola;
        this.qrCodeUrl = pendencia.qrCodeBase64;
      }
    } else if (['CREDIT', 'DEBIT'].includes(pendencia.metodoPagamento) && pendencia.status === 'REJECTED') {
      this.bloquearOutrosMetodos = true;
      this.setPaymentMethod(pendencia.metodoPagamento as 'CREDIT' | 'DEBIT');
    }
  }

  async loadCourseData() {
    this.isLoadingCourse = true;
    try {
      const data = await this.coursesService.getCourseCheckoutSummary(this.cursoId);
      this.checkoutSummary = data as CheckoutSummary;
      this.isLoadingCourse = false;
      
      // Mantemos a leitura do plan via query param para decidir o fluxo
      this.tipoCompra = 'AVULSO';
      this.calcularParcelas();
    } catch (err) {
      console.error('Erro ao carregar curso', err);
      this.isLoadingCourse = false;
      // Backend failure -> show error on UI instead of mock
      this.errorMessage = 'Não foi possível carregar os dados do curso. Tente novamente mais tarde.';
    }
  }

  calcularParcelas() {
    if (!this.checkoutSummary) return;

    this.installments = [];
    const maxParcelas = 12;

    this.valorTotal = this.checkoutSummary.priceSingle;
    for (let i = 1; i <= maxParcelas; i++) {
      if (i === 1) {
        this.installments.push({
          value: i,
          label: `1x de ${this.formatPrice(this.valorTotal)} sem juros`
        });
      } else {
        // Juros mock de 2.5% simples pra avulso
        const juros = 0.025;
        const valorComJuros = this.valorTotal * (1 + (juros * i));
        const parcela = valorComJuros / i;
        this.installments.push({
          value: i,
          label: `${i}x de ${this.formatPrice(parcela)} (com juros)`
        });
      }
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price);
  }

  initForm() {
    this.cardForm = this.fb.group({
      cardNumber: ['', [Validators.required, Validators.minLength(16), Validators.maxLength(19)]],
      cardHolder: ['', [Validators.required, Validators.minLength(3)]],
      expiry: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/?([0-9]{2})$/)]],
      cvc: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(4)]],
      installments: [1]
    });
  }

  setPaymentMethod(method: 'PIX' | 'CREDIT' | 'DEBIT') {
    if (this.bloquearOutrosMetodos && this.transacaoPendente?.metodoPagamento !== method) {
      return;
    }
    this.paymentMethod = method;
    this.paymentError = false;
    this.paymentSuccess = false;
    this.errorMessage = '';
  }

  generatePix() {
    if (this.pixCpf && this.pixCpf.length >= 11) {
      this.isLoadingPix = true;
      this.errorMessage = '';
      
      this.financialService.gerarPixAsaas({
        cursoId: this.cursoId,
        cpf: this.pixCpf,
        valor: this.valorTotal
      }).subscribe({
        next: (res: PixResponse) => {
          this.isLoadingPix = false;
          if (res.sucesso) {
            this.qrCodeGenerated = true;
            this.qrCodeUrl = res.qrCode;
            this.pixCopiaECola = res.pixCopiaECola;
            
            // Regra: Bloquear outras abas
            this.transacaoPendente = {
              temPendencia: true,
              status: 'PENDING',
              metodoPagamento: 'PIX',
              pixCopiaECola: res.pixCopiaECola,
              qrCodeBase64: res.qrCode,
              mensagem: 'Você gerou um PIX e ele está aguardando pagamento.'
            };
            this.bloquearOutrosMetodos = true;

          } else {
            this.errorMessage = 'Ocorreu um erro ao gerar o PIX.';
          }
        },
        error: (err: any) => {
          this.isLoadingPix = false;
          this.errorMessage = err.error?.message || 'Falha na comunicação com o servidor. Verifique seu CPF e tente novamente.';
        }
      });
    }
  }

  copyPixCode() {
    // Mock copy to clipboard
    alert('Código PIX copiado para a área de transferência!');
  }

  processPayment() {
    // Basic validation
    if (this.paymentMethod !== 'PIX' && this.cardForm.invalid) {
      this.cardForm.markAllAsTouched();
      return;
    }

    this.isProcessing = true;
    this.paymentError = false;
    this.paymentSuccess = false;

    // Simulate API call and WebSocket delay
    setTimeout(() => {
      this.isProcessing = false;
      
      // Randomly simulate success or failure for demonstration (e.g., if CVV is '000', fail it)
      const cvc = this.cardForm.get('cvc')?.value;
      if (cvc === '000') {
        this.paymentError = true;
      } else {
        this.paymentSuccess = true;
        
        // Simulate redirect after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/courses']);
        }, 3000);
      }
    }, 2500);
  }
}
