import { ApplicationConfig, provideZoneChangeDetection, LOCALE_ID } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router'; 
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';
import { authInterceptor } from './services/auth.interceptor'; // Importe o interceptor aqui

registerLocaleData(localePt);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()), 
    provideAnimationsAsync(),
    
    // HttpClient configurado para usar o interceptor de Token JWT
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    
    { provide: LOCALE_ID, useValue: 'pt-BR' }
  ]
};