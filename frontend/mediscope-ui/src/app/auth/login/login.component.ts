import { Component, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ROLE_ROUTES } from '../../core/constants/api.constants';
import { UserRole } from '../../core/models/auth.model';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  private fb          = inject(FormBuilder);
  private authService = inject(AuthService);
  private router      = inject(Router);
  private notificationService = inject(NotificationService);
  // Signals
  isLoading      = signal(false);
  errorMessage   = signal('');
  showPassword   = signal(false);
  activeDemo     = signal<UserRole | null>(null);

  loginForm: FormGroup = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  // ── Toggle password visibility ───────────────────────────
  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  // ── Submit ───────────────────────────────────────────────
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        const role = response.data.user.role;
        const mustChangePassword = response.data.mustChangePassword;
        if (role === 'Doctor' && response.data.mustChangePassword) {
          this.notificationService.warn(
            'You must change your temporary password before continuing.'
          );
          this.router.navigate([
            ROLE_ROUTES['DoctorPasswordChange']
          ]);
          return;
        }
        this.router.navigate([ROLE_ROUTES[role]]);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.message ?? 'Invalid email or password. Please try again.'
        );
      }
    });
  }

  // ── Form helpers ─────────────────────────────────────────
  get email()    { return this.loginForm.get('email')!;    }
  get password() { return this.loginForm.get('password')!; }
}