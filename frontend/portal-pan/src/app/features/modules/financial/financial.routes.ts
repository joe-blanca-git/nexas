import { Routes } from "@angular/router";
import { FinancialHomeComponent } from "./pages/financial-home/financial-home.component";
import { FinancialCartComponent } from "./pages/financial-cart/financial-cart.component";

export const financialRoutes: Routes = [
    {
        path: '',
        component: FinancialHomeComponent,
        title: 'Financeiro',
    },
    {
        path: 'cart/:id',
        component: FinancialCartComponent,
        title: 'Carrinho de Compras'
    }
];