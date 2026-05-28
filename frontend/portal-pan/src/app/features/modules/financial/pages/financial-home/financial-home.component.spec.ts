import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinancialHomeComponent } from './financial-home.component';

describe('FinancialHomeComponent', () => {
  let component: FinancialHomeComponent;
  let fixture: ComponentFixture<FinancialHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinancialHomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FinancialHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
