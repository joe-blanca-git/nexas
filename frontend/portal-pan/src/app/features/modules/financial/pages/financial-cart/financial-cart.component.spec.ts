import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinancialCartComponent } from './financial-cart.component';

describe('FinancialCartComponent', () => {
  let component: FinancialCartComponent;
  let fixture: ComponentFixture<FinancialCartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinancialCartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FinancialCartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
