import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration } from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { AuthService } from './core/auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(),
    provideHttpClient(withFetch()),
    {
      // Sem isso, um F5 na página apaga o nome/roles do usuário da tela (StateUtil fica
      // vazio) mesmo com o cookie ainda válido, porque rehydrateUserState() só era chamado
      // no sucesso do login/registro, nunca no boot da aplicação.
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService) => () => authService.rehydrateUserState(),
      deps: [AuthService],
      multi: true
    }
  ]
};
