import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtil } from '../auth.util';

export const guestGuard: CanActivateFn = (route, state) => {
  const authUtil = inject(AuthUtil);
  const router = inject(Router);

  if (authUtil.getCookieAuth()) {
    // Se já tem token (logado), redireciona para a home
    return router.parseUrl('/');
  }

  // Se não tem token (visitante), permite acessar Login/Register
  return true;
};
