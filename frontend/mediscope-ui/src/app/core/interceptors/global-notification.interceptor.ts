import { HttpInterceptorFn, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs/operators';
import { SHOW_SUCCESS, SHOW_ERROR } from '../tokens/http-context.tokens';
import { NotificationService } from '../services/notification.service';

export const globalNotificationInterceptor: HttpInterceptorFn = (req, next) => {
  const notify = inject(NotificationService);

  // Read configuration flags out of your custom BaseHttpService HttpContext structure
  const shouldShowSuccess = req.context.get(SHOW_SUCCESS) ?? false;
  const shouldShowError = req.context.get(SHOW_ERROR) ?? true; // Defaults to true to catch unexpected errors safely

  return next(req).pipe(
    tap({
        next: (event) => {
            if (event instanceof HttpResponse && shouldShowSuccess) {
              
              const responseBody = event.body as Record<string, any> | null;
              const successMessage = responseBody?.['message'] || responseBody?.['Message'] || 'Operation completed successfully.';
              
              notify.success(successMessage);
            }
          },
      error: (error: any) => {
        if (error instanceof HttpErrorResponse && shouldShowError) {
          let errorMessage = 'An unexpected server connection error occurred.';

          if (error.error) {
            // 1. Check if the backend returned your RFC standard validation object dictionary
            if (error.error.errors) {
              const validationMessages: string[] = [];
              Object.keys(error.error.errors).forEach((key) => {
                const messages = error.error.errors[key];
                if (Array.isArray(messages)) {
                  validationMessages.push(...messages);
                } else if (typeof messages === 'string') {
                  validationMessages.push(messages);
                }
              });
              errorMessage = validationMessages.join(' | ');
            } 
            // 2. Fallback to standard top-level string message fields
            else if (error.error.message) {
              errorMessage = error.error.message;
            } else if (error.error.title) {
              errorMessage = error.error.title;
            }
          }

          notify.error(errorMessage);
        }
      }
    })
  );
};