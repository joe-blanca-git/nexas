import { inject, Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { StateUtil } from '../utils/UserState.util';
import { MenuService } from '../services/menu.service';
import { AuthService } from '../auth/auth.service';
import { ToastService } from '../services/toast.service';

const defaultPath = '/';

@Injectable({
  providedIn: 'root',
})
export class AuthGuardService {
  private stateUtil = inject(StateUtil);
  private menuService = inject(MenuService);
  private authService = inject(AuthService);

  constructor(
    private router: Router,
    private toastService: ToastService
  ) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    const isLoggedIn = this.authService.loggedIn;    
    const isAuthForm = ['login', 'recovery-password', 'update-password', 'register'].includes(route.routeConfig?.path || '');

    if (isLoggedIn && !this.authService.isTokenValid()) {      
      this.authService.logOut();
      this.router.navigate(['/auth/login']);
      return false;
    }

    if (isLoggedIn && isAuthForm) {
      this.router.navigate([defaultPath]);
      return false;
    }

    if (!isLoggedIn && !isAuthForm) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    return true;
  }
}