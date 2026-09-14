import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/financial-home/financial-home.component').then((c) => c.FinancialHomeComponent),
  },
];
