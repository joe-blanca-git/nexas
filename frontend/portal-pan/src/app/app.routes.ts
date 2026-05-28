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
        children: [
            {
                path: '',
                redirectTo: 'home',
                pathMatch: 'full'
            },
            {
                path: 'home',
                component: HomeDashboardComponent,
                canActivate: [AuthGuardService]
            },
            {
                path: 'courses',
                loadChildren: () =>
                    import('./features/modules/courses/courses.routes').then((r) => r.coursesRoutes),
            },
            {
                path: 'certificates',
                loadChildren: () =>
                    import('./features/modules/certificates/certificates.routes').then((r) => r.certificatesRoutes),
            },
            {
                path: 'financial',
                loadChildren: () =>
                    import('./features/modules/financial/financial.routes').then((r) => r.financialRoutes),
            },
            {
                path: 'forum',
                loadChildren: () =>
                    import('./features/modules/forum/forum.routes').then((r) => r.forumRoutes),
            },
            {
                path: 'support',
                loadChildren: () =>
                    import('./features/modules/support/support.routes').then((r) => r.supportRoutes),
            }
        ]
    }
];
