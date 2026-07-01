import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { CoursesService } from '../../../courses/services/courses.service';

export interface ICartCourse {
  id: number;
  title: string;
  description: string;
  imgCoverLink: string;
  priceSingle: number;
  priceSubscription: number;
  totalLessons: number;
  totalHours: number;
  category: string;
}

type PurchaseOption = 'single' | 'subscription';

@Component({
  selector: 'app-financial-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, CurrencyPipe],
  templateUrl: './financial-cart.component.html',
  styleUrl: './financial-cart.component.scss'
})
export class FinancialCartComponent implements OnInit {

  course: ICartCourse | null = null;
  selectedOption: PurchaseOption = 'single';
  isLoading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private coursesService: CoursesService
  ) {}

  async ngOnInit(): Promise<void> {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (!idParam) {
      this.router.navigate(['/courses']);
      return;
    }

    const courseId = Number(idParam);
    this.isLoading = true;

    try {
      this.course = await this.coursesService.getCourseCheckoutSummary(courseId);
    } catch (error) {
      console.error('Erro ao carregar os dados do curso', error);
      this.router.navigate(['/courses']);
    } finally {
      this.isLoading = false;
    }
  }

  selectOption(option: PurchaseOption): void {
    this.selectedOption = option;
  }

  get selectedPrice(): number {
    if (!this.course) return 0;
    return this.selectedOption === 'single' ? this.course.priceSingle : this.course.priceSubscription;
  }

  get selectedLabel(): string {
    return this.selectedOption === 'single' ? 'Compra Avulsa' : 'Assinatura Mensal';
  }

  // TODO: Integrar com gateway de pagamento (ex: Asaas, Stripe, etc.)
  onCheckout(): void {
    console.log('Checkout:', {
      courseId: this.course?.id,
      option: this.selectedOption,
      price: this.selectedPrice
    });
    // this.router.navigate(['financial/payment'], { queryParams: { courseId: this.course?.id, plan: this.selectedOption } });
  }

  goBack(): void {
    this.router.navigate(['/courses']);
  }
}
