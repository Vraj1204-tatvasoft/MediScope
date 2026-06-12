// src/app/auth/register/register.component.ts
import { Component, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

function futureDateValidator(control: AbstractControl): ValidationErrors | null {

  if (!control.value) {
    return null;
  }

  const selectedDate = new Date(control.value);
  const today = new Date();

  // Remove time portion
  today.setHours(0, 0, 0, 0);

  if (selectedDate > today) {
    return { futureDate: true };
  }

  return null;
}
function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password        = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ mismatch: true });
    return { mismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  private fb          = inject(FormBuilder);
  private authService = inject(AuthService);
  private router      = inject(Router);

  // Signals
  currentStep    = signal<1 | 2>(1);
  isLoading      = signal(false);
  errorMessage   = signal('');
  showPassword   = signal(false);
  showConfirmPw  = signal(false);
  maxDate = new Date().toISOString().split('T')[0];
  readonly BLOOD_GROUPS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];
  readonly GENDERS      = ['Male', 'Female', 'Other'];

  // Step 1 — Personal Information
  step1Form: FormGroup = this.fb.group({
    firstName:     ['', [Validators.required, Validators.maxLength(75)]],
    lastName:      ['', [Validators.required, Validators.maxLength(75)]],
    email:         ['', [Validators.required, Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)]],
    contactNumber: ['', [Validators.pattern(/^\d{10}$/)]],
    dateOfBirth: [
      '',
      [
        Validators.required,
        futureDateValidator
      ]
    ],
    gender:        ['', Validators.required],
    address:       [''],
  });

  // Step 2 — Health & Security
  step2Form: FormGroup = this.fb.group({
    bloodGroup:      [''],
    password:        ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$/)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordMatchValidator });

  // ── Step navigation ──────────────────────────────────────
  goToStep2(): void {
    if (this.step1Form.invalid) {
      this.step1Form.markAllAsTouched();
      return;
    }
    this.currentStep.set(2);
    this.errorMessage.set('');
  }

  goToStep1(): void {
    this.currentStep.set(1);
    this.errorMessage.set('');
  }

  // ── Submit ───────────────────────────────────────────────
  onSubmit(): void {
    if (this.step2Form.invalid) {
      this.step2Form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const s1 = this.step1Form.value;
    const s2 = this.step2Form.value;

    const payload = {
      fullName:      `${s1.firstName} ${s1.lastName}`.trim(),
      email:         s1.email,
      contactNumber: s1.contactNumber || null,
      dateOfBirth:   s1.dateOfBirth   || null,
      gender:        s1.gender        || null,
      address:       s1.address       || null,
      bloodGroup:    s2.bloodGroup    || null,
      password:      s2.password,
      confirmPassword: s2.confirmPassword,
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/patient/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.message ?? 'Registration failed. Please try again.'
        );
      }
    });
  }

  // ── Toggles ──────────────────────────────────────────────
  togglePassword():   void { this.showPassword.update(v => !v);  }
  toggleConfirmPw():  void { this.showConfirmPw.update(v => !v); }

  // ── Getters ──────────────────────────────────────────────
  get firstName()       { return this.step1Form.get('firstName')!;     }
  get lastName()        { return this.step1Form.get('lastName')!;      }
  get email()           { return this.step1Form.get('email')!;         }
  get contactNumber()   { return this.step1Form.get('contactNumber')!; }
  get dateOfBirth()     { return this.step1Form.get('dateOfBirth')!;   }
  get gender()          { return this.step1Form.get('gender')!;        }
  get password()        { return this.step2Form.get('password')!;      }
  get confirmPassword() { return this.step2Form.get('confirmPassword')!;}
}