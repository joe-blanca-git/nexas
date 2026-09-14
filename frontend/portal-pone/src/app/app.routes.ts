import { Routes } from '@angular/router';
import { HomeComponent } from './features/modules/home/index/home.component';
import { HomeDashboardComponent } from './features/modules/home/components/home-dashboard/home-dashboard.component';
import { AuthGuardService } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'auth',
        canActivate: [AuthGuardService],
        canActivateChild: [AuthGuardService],
        loadChildren: () =>
            import('./features/auth/auth.routes').then((r) => r.AUTH_ROUTES),
    },
    {
        path: '',
        component: HomeComponent,
        canActivate: [AuthGuardService],
        canActivateChild: [AuthGuardService],
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
                path: 'courses',
                title: 'Cursos',
                loadChildren: () =>
                    import('./features/modules/courses/courses.routes').then((r) => r.coursesRoutes),
            },
            {
                path: 'profile',
                title: 'Dados Cadastrais',
                loadChildren: () =>
                    import('./features/modules/profile/profile.routes').then((r) => r.profileRoutes),
            },
            {
                path: 'blog',
                title: 'Blog',
                loadChildren: () =>
                    import('./features/modules/blog/blog.routes').then((r) => r.blogRoutes),
            },
            {
                path: 'settings',
                title: 'Configurações',
                loadChildren: () =>
                    import('./features/modules/settings/settings.routes').then((r) => r.routes),
            },
            {
                path: 'support',
                title: 'Suporte',
                loadChildren: () =>
                    import('./features/modules/support/support.routes').then((r) => r.routes),
            },
            {
                path: 'financial',
                title: 'Financeiro',
                loadChildren: () =>
                    import('./features/modules/financial/financial.routes').then((r) => r.routes),
            }
        ]
    }
];
