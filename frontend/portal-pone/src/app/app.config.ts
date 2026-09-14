import { APP_INITIALIZER, ApplicationConfig, LOCALE_ID, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation, withRouterConfig } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration } from '@angular/platform-browser';
import { provideStore } from '@ngrx/store';
import { userProfileReducer } from './core/utils/UserState.util';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './core/interceptors/error-interceptor';
import { AuthService } from './core/auth/auth.service';
import { AuthGuardService } from './core/guards/auth.guard';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

registerLocaleData(localePt, 'pt-BR');

export const appConfig: ApplicationConfig = {
  providers: [
    // Deixe apenas este provideStore com o seu reducer:
    provideStore({ userProfile: userProfileReducer }), 
    
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(
      routes,
      withEnabledBlockingInitialNavigation(),
      withRouterConfig({ onSameUrlNavigation: 'reload' })
    ),
    provideClientHydration(),
    provideHttpClient(withInterceptors([errorInterceptor]), withFetch()),    
    AuthService,
    AuthGuardService,
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService) => () => authService.rehydrateUserState(),
      deps: [AuthService],
      multi: true,
    },
    { provide: LOCALE_ID, useValue: 'pt-BR' },
  ],
};