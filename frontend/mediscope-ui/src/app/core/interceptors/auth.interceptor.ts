// FILE: src/app/core/interceptors/auth.interceptor.ts

import {
  HttpInterceptorFn, HttpRequest,
  HttpHandlerFn, HttpErrorResponse, HttpResponse
} from '@angular/common/http';
import { inject }    from '@angular/core';
import { catchError, switchMap, tap, throwError } from 'rxjs';
import { TokenService }         from '../services/token.service';
import { AuthService }          from '../services/auth.service';
import { NotificationService }  from '../services/notification.service';
import { SHOW_SUCCESS, SHOW_ERROR } from '../tokens/http-context.tokens';
 
export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const tokenService = inject(TokenService);
  const authService  = inject(AuthService);
  const notify       = inject(NotificationService);
 
  // Attach access token
  const token   = tokenService.getAccessToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;
 
  return next(authReq).pipe(
 
    tap(event => {
      // Show success snackbar if context token set
      if (event instanceof HttpResponse) {
        const showSuccess = req.context.get(SHOW_SUCCESS);
        if (showSuccess && (event.body as any)?.success) {
          notify.success((event.body as any)?.message ?? 'Operation successful.');
        }
      }
    }),
 
    catchError((error: HttpErrorResponse) => {
 
      // Handle session invalidation (new login from another device)
      // Backend returns sessionExpired: true when SessionId mismatch detected
      if (error.status === 401) {
        const isSessionExpired = error.error?.sessionExpired === true;
 
        if (isSessionExpired) {
          // Don't try to refresh — session is intentionally invalidated
          notify.error(
            error.error?.message ??
            'You have been signed in from another device. Please log in again.'
          );
          setTimeout(() => authService.logout(), 1500);
          return throwError(() => error);
        }
 
        // Regular 401 on non-auth endpoints → try refresh
        if (!req.url.includes('/auth/')) {
          return authService.refreshToken().pipe(
            switchMap(response => {
              const newToken  = tokenService.getAccessToken();
              const retryReq  = req.clone({
                setHeaders: { Authorization: `Bearer ${newToken}` }
              });
              return next(retryReq);
            }),
            catchError(refreshError => {
              // Refresh failed → logout
              authService.logout();
              return throwError(() => refreshError);
            })
          );
        }
      }
 
      // Show error snackbar if context token allows
      const showError = req.context.get(SHOW_ERROR);
      if (showError && error.status !== 401) {
        const message = error.error?.message ?? 'An unexpected error occurred.';
        notify.error(message);
      }
 
      return throwError(() => error);
    })
  );
};