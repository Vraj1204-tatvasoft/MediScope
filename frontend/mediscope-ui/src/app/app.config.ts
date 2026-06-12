// FILE: src/app/app.config.ts

import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter }                                  from '@angular/router';
import { provideHttpClient, withInterceptors }            from '@angular/common/http';
import { MAT_DATE_LOCALE, provideNativeDateAdapter }     from '@angular/material/core';
import { MAT_SNACK_BAR_DEFAULT_OPTIONS }                 from '@angular/material/snack-bar';

import { routes }          from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideNativeDateAdapter(),
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    {
      provide: MAT_SNACK_BAR_DEFAULT_OPTIONS,
      useValue: { duration: 3000 }
    },
  ]
};