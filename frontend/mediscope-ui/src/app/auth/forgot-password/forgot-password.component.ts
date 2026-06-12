import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule }            from '@angular/material/card';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatInputModule }           from '@angular/material/input';
import { MatButtonModule }          from '@angular/material/button';
import { MatIconModule }            from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService }              from '../../core/services/auth.service';
 
@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls:   ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
 
  private authService = inject(AuthService);
  private router      = inject(Router);
  private fb          = inject(FormBuilder);
 
  isLoading   = signal(false);
  isSubmitted = signal(false);
  errorMsg    = signal('');
 
  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });
 
  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading.set(true);
    this.errorMsg.set('');
 
    this.authService.forgotPassword(this.form.value.email).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSubmitted.set(true);   // show success state
      },
      error: () => {
        this.isLoading.set(false);
        // Show generic message — never leak if email exists
        this.isSubmitted.set(true);
      }
    });
  }
 
  get email() { return this.form.get('email')!; }
}