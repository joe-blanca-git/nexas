import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinancialPaymentComponent } from './financial-payment.component';

describe('FinancialPaymentComponent', () => {
  let component: FinancialPaymentComponent;
  let fixture: ComponentFixture<FinancialPaymentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinancialPaymentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FinancialPaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
