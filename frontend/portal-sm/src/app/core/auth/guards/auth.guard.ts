import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const authService = inject(AuthService);

  if (!isPlatformBrowser(platformId)) {
    return true; // Ignora o guard no servidor (SSR/Prerendering)
  }

  // Verifica se o token existe e se não está expirado usando a lógica do sistema (AuthService)
  const isValid = authService.isTokenValid();

  if (!isValid) {
    authService.logOut();
    return false;
  }

  return true;
};
