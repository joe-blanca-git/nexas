import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-financial',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet/>',
  styleUrl: './financial.component.scss'
})
export class FinancialComponent {

}
