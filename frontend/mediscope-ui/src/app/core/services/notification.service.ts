import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private snackBar = inject(MatSnackBar);

  private readonly defaultConfig: MatSnackBarConfig = {
    duration: 4000,
    horizontalPosition: 'right',
    verticalPosition: 'top',
  };

  /**
   * Triggers a green success snackbar
   */
  success(message: string, action: string = 'Close'): void {
    this.snackBar.open(message, action, {
      ...this.defaultConfig,
      panelClass: ['snackbar-global', 'snackbar-success'],
    });
  }

  /**
   * Triggers a red error snackbar
   */
  error(message: string, action: string = 'OK'): void {
    this.snackBar.open(message, action, {
      ...this.defaultConfig,
      panelClass: ['snackbar-global', 'snackbar-error'],
      duration: 6000 
    });
  }

  /* Triggers a yellow warning snackbar
   */
  warn(message: string, action: string = 'Dismiss'): void {
    this.snackBar.open(message, action, {
      ...this.defaultConfig,
      panelClass: ['snackbar-global', 'snackbar-warning'],
    });
  }
}