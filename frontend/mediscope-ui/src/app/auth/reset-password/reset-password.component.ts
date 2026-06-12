import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';


// Custom validator to check if passwords match
export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('newPassword');
  const confirm = control.get('confirmPassword');
  return password && confirm && password.value === confirm.value ? null : { mismatch: true };
};

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatFormFieldModule, 
    MatInputModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  form: FormGroup;
  token: string | null = null;

  // UI State Signals
  isVerifyingToken = signal<boolean>(true);
  isTokenValid = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  isSubmitted = signal<boolean>(false);
  errorMsg = signal<string | null>(null);

  // Password Visibility Signals
  hidePassword = signal<boolean>(true);
  hideConfirm = signal<boolean>(true);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      newPassword:        ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$/)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordMatchValidator });
  }

  ngOnInit(): void {
    // 1. Grab token from URL: /reset-password?token=xyz...
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.isVerifyingToken.set(false);
      this.isTokenValid.set(false);
      return;
    }

    // 2. Validate token with backend
    this.authService.validateResetToken(this.token).subscribe({
      next: (res: any) => {
        // Handle both true and { success: true } depending on your BaseHttpService setup
        const isValid = typeof res === 'boolean' ? res : res?.success;
        this.isTokenValid.set(isValid);
        this.isVerifyingToken.set(false);
      },
      error: () => {
        this.isTokenValid.set(false);
        this.isVerifyingToken.set(false);
      }
    });
  }

  // Getters for cleaner HTML
  get newPassword() { return this.form.get('newPassword')!; }
  get confirmPassword() { return this.form.get('confirmPassword')!; }

  submit(): void {
    if (this.form.invalid || !this.token) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMsg.set(null);

    const { newPassword, confirmPassword } = this.form.value;

    this.authService.resetPassword(this.token, newPassword, confirmPassword).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        const isSuccess = typeof res === 'boolean' ? res : res?.success;
        
        if (isSuccess) {
          this.isSubmitted.set(true);
        } else {
          this.errorMsg.set(res?.message || 'Failed to reset password. Please try again.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMsg.set(err.message || 'An unexpected error occurred.');
      }
    });
  }
}