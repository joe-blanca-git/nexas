import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FinancialService, CheckoutSummary, PixResponse } from '../../services/financial.service';
import { CoursesService } from '../../../courses/services/courses.service';

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
  cursoId: number = 0;
  checkoutSummary?: CheckoutSummary;
  tipoCompra: 'AVULSO' | 'ANUAL' = 'ANUAL';
  valorTotal = 0;
  isLoadingCourse = true;

  // Mock data for installments
  installments: { value: number, label: string }[] = [];

  constructor(
    private fb: FormBuilder, 
    private router: Router, 
    private route: ActivatedRoute,
    private financialService: FinancialService,
    private coursesService: CoursesService
  ) {}

  ngOnInit() {
    this.initForm();
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam === 'subscription') {
      this.tipoCompra = 'ANUAL';
      this.isLoadingCourse = false;
      this.checkoutSummary = {
        id: 0,
        title: 'Assinatura Portal Pan (Acesso Total)',
        imgCoverLink: 'https://cdn-icons-png.flaticon.com/512/4169/4169165.png', // Pode trocar pela logo oficial
        priceSingle: 0,
        priceSubscription: 79.90
      };
      this.calcularParcelas();
    } else if (idParam) {
      this.cursoId = Number(idParam);
      this.loadCourseData();
    } else {
      this.router.navigate(['/courses']);
    }
  }

  async loadCourseData() {
    this.isLoadingCourse = true;
    try {
      const data = await this.coursesService.getCourseCheckoutSummary(this.cursoId);
      this.checkoutSummary = data as CheckoutSummary;
      this.isLoadingCourse = false;
      
      // Mantemos a leitura do plan via query param para decidir o fluxo
      this.route.queryParams.subscribe(params => {
        this.tipoCompra = params['plan'] === 'single' ? 'AVULSO' : 'ANUAL';
        this.calcularParcelas();
      });
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

    if (this.tipoCompra === 'ANUAL') {
      this.valorTotal = this.checkoutSummary.priceSubscription * 12;
      for (let i = 1; i <= maxParcelas; i++) {
        const parcela = this.valorTotal / i;
        this.installments.push({
          value: i,
          label: `${i}x de ${this.formatPrice(parcela)} sem juros`
        });
      }
    } else {
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
    this.paymentMethod = method;
    this.paymentError = false;
    this.paymentSuccess = false;
    this.qrCodeGenerated = false;
    this.errorMessage = '';
  }

  generatePix() {
    if (this.pixCpf && this.pixCpf.length >= 11) {
      this.isLoadingPix = true;
      this.errorMessage = '';
      
      this.financialService.gerarPixAsaas({
        cursoId: this.cursoId,
        tipoCompra: this.tipoCompra,
        cpf: this.pixCpf,
        valor: this.valorTotal
      }).subscribe({
        next: (res: PixResponse) => {
          this.isLoadingPix = false;
          if (res.sucesso) {
            this.qrCodeGenerated = true;
            this.qrCodeUrl = res.qrCode;
            this.pixCopiaECola = res.pixCopiaECola;
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
