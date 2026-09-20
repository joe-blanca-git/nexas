import { Routes } from '@angular/router';
import { guestGuard } from './core/auth/guards/guest.guard';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./feature/dashboard/pages/home/home.component').then((c) => c.HomeComponent)
  },
  {
    path: 'organizations',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./feature/dashboard/pages/organizations/organizations.component').then(
        (c) => c.OrganizationsComponent
      )
  },
  {
    path: 'applications',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./feature/dashboard/pages/applications/applications.component').then(
        (c) => c.ApplicationsComponent
      )
  },
  {
    path: 'applications/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./feature/dashboard/pages/application-detail/application-detail.component').then(
        (c) => c.ApplicationDetailComponent
      )
  },
  {
    path: 'account',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./feature/dashboard/pages/account/account.component').then(
        (c) => c.AccountComponent
      )
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./feature/auth/pages/login/login.component').then((c) => c.LoginComponent)
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./feature/auth/pages/forgot-password/forgot-password.component').then(
        (c) => c.ForgotPasswordComponent
      )
  },
  {
    path: 'reset-password',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./feature/auth/pages/reset-password/reset-password.component').then(
        (c) => c.ResetPasswordComponent
      )
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./feature/auth/pages/register/register.component').then(
        (c) => c.RegisterComponent
      )
  }
];
