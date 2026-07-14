import { Routes } from '@angular/router';
import { HomeComponent } from './features/modules/home/index/home.component';
import { HomeDashboardComponent } from './features/modules/home/components/home-dashboard/home-dashboard.component';
import { AuthGuardService } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'auth',
        loadChildren: () =>
            import('./features/auth/auth.routes').then((r) => r.AUTH_ROUTES),
    },
    {
        path: '',
        component: HomeComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                redirectTo: 'home',
                pathMatch: 'full'
            },
            {
                path: 'home',
                component: HomeDashboardComponent
            },
            {
                path: 'news-detail',
                redirectTo: 'home',
                pathMatch: 'full'
            },
            {
                path: 'news-detail/:id',
                loadComponent: () =>
                    import('./features/modules/home/components/news-detail/news-detail.component').then(c => c.NewsDetailComponent)
            },
            {
                path: 'profile',
                title: 'Dados Cadastrais',
                loadChildren: () =>
                    import('./features/modules/profile/profile.routes').then((r) => r.profileRoutes),
            }
        ]
    }
];
