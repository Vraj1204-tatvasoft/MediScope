// File: src/app/patient/profile/patient-profile.component.ts

import { Component, OnInit, signal, inject } from '@angular/core';
import {
  FormBuilder, FormGroup, Validators,
  ReactiveFormsModule, AbstractControl, ValidationErrors
} from '@angular/forms';
import { CommonModule } from '@angular/common';

// Angular Material
import { MatCardModule }           from '@angular/material/card';
import { MatButtonModule }          from '@angular/material/button';
import { MatInputModule }           from '@angular/material/input';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatSelectModule }          from '@angular/material/select';
import { MatIconModule }            from '@angular/material/icon';
import { MatChipsModule }           from '@angular/material/chips';
import { MatDividerModule }         from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDatepickerModule }      from '@angular/material/datepicker';
import { MatNativeDateModule }      from '@angular/material/core';
import { MatTooltipModule }         from '@angular/material/tooltip';

import { AuthService }       from '../../core/services/auth.service';
import {
  PatientProfile, UpdateProfileRequest,
  BLOOD_GROUPS, GENDERS
} from '../../models/patient.model';
import { PatientService } from '../../services/patient.service';
import { NotificationService } from '../../core/services/notification.service';

// Password match validator
function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const np  = control.get('newPassword');
  const cnp = control.get('confirmNewPassword');
  if (np && cnp && np.value !== cnp.value) {
    cnp.setErrors({ mismatch: true });
    return { mismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-patient-profile',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatInputModule,
    MatFormFieldModule, MatSelectModule, MatIconModule,
    MatChipsModule, MatDividerModule, MatProgressSpinnerModule,
    MatDialogModule, MatDatepickerModule, MatNativeDateModule, 
    MatTooltipModule,
  ],
  templateUrl: './patient-profile.component.html',
  styleUrls:   ['./patient-profile.component.css']
})
export class PatientProfileComponent implements OnInit {

  private patientService = inject(PatientService);
  private authService    = inject(AuthService);
  private fb             = inject(FormBuilder);
  private dialog         = inject(MatDialog);
  
  // Injecting Global Notification Service
  private notify         = inject(NotificationService);

  // Signals
  profile       = signal<PatientProfile | null>(null);
  isEditMode    = signal(false);
  isLoading     = signal(false);
  isSaving      = signal(false);
  showPwSection = signal(false);
  showCurrentPw = signal(false);
  showNewPw     = signal(false);
  showConfirmPw = signal(false);
  maxDate = new Date().toISOString().split('T')[0];
  readonly bloodGroups = BLOOD_GROUPS;
  readonly genders     = GENDERS;

  // Edit form
  editForm: FormGroup = this.fb.group({
    fullName:      ['', [Validators.required, Validators.maxLength(150)]],
    email:         ['', [Validators.required, Validators.email]],
    contactNumber: ['', [Validators.pattern(/^\d{10}$/)]],
    bloodGroup:    [''],
    gender:        [''],
    dateOfBirth:   [null],
    address:       [''],
  });

  // Change password form
  passwordForm: FormGroup = this.fb.group({
    currentPassword:    ['', Validators.required],
    newPassword:        ['', [Validators.required, Validators.minLength(8)]],
    confirmNewPassword: ['', Validators.required],
  }, { validators: passwordMatchValidator });

  ngOnInit(): void {
    this.loadProfile();
  }

  // ── Load ─────────────────────────────────────────────────
  loadProfile(): void {
    this.isLoading.set(true);
    this.patientService.getMyProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.notify.error('Failed to load profile details.');
      }
    });
  }

  // ── Edit Mode ─────────────────────────────────────────────
  enterEditMode(): void {
    const p = this.profile();
    if (!p) return;

    this.editForm.patchValue({
      fullName:      p.fullName,
      email:         p.email,
      contactNumber: p.contactNumber ?? '',
      bloodGroup:    p.bloodGroup    ?? '',
      gender:        p.gender        ?? '',
      dateOfBirth:   p.dateOfBirth   ? new Date(p.dateOfBirth) : null,
      address:       p.address       ?? '',
    });
    this.editForm.get('email')?.disable();
    this.isEditMode.set(true);
  }

  cancelEdit(): void {
    this.isEditMode.set(false);
    this.editForm.reset();
  }

  // ── Save Profile ──────────────────────────────────────────
  saveProfile(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.notify.warn('Please complete all mandatory field profiles correctly.');
      return;
    }

    this.isSaving.set(true);
    const v = this.editForm.getRawValue(); // Fallback reading handles disabled field value mapping

    const request: UpdateProfileRequest = {
      fullName:      v.fullName,
      email:         v.email,
      contactNumber: v.contactNumber || null,
      bloodGroup:    v.bloodGroup    || null,
      gender:        v.gender        || null,
      dateOfBirth:   v.dateOfBirth
        ? (v.dateOfBirth as Date).toISOString().split('T')[0]
        : null,
      address:       v.address || null,
    };

    this.patientService.updateProfile(request).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.isEditMode.set(false);
        this.isSaving.set(false);
        this.notify.success('Profile updated successfully.');
      },
      error: (err) => {
        this.isSaving.set(false);
        if (!err?.error?.errors) {
          this.notify.error(err.error?.message ?? 'Profile modification update failed.');
        }
      }
    });
  }

  // ── Change Password ───────────────────────────────────────
  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);

    this.patientService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showPwSection.set(false);
        this.passwordForm.reset();
        this.notify.success('Password changed successfully. Please log in again.');
        setTimeout(() => this.authService.logout(), 2000);
      },
      error: (err) => {
        this.isSaving.set(false);
        if (!err?.error?.errors) {
          this.notify.error(err.error?.message ?? 'Password modification change failed.');
        }
      }
    });
  }

  // ── Helpers ───────────────────────────────────────────────
  getUserInitial(): string {
    return this.profile()?.fullName?.charAt(0).toUpperCase() ?? 'U';
  }

  formatDate(date: string | null): string {
    if (!date) return '—';
    const d = new Date(date);
    return d.toISOString().split('T')[0];
  }

  toggleSection(section: 'password'): void {
    this.showPwSection.update(v => !v);
    this.passwordForm.reset();
  }

  toggleCurrentPassword(): void {
    this.showCurrentPw.update(v => !v);
  }
  
  toggleNewPassword(): void {
    this.showNewPw.update(v => !v);
  }
  
  toggleConfirmPassword(): void {
    this.showConfirmPw.update(v => !v);
  }

  // Form getters
  get fullName()           { return this.editForm.get('fullName')!;           }
  get email()              { return this.editForm.get('email')!;              }
  get contactNumber()      { return this.editForm.get('contactNumber')!;      }
  get currentPassword()    { return this.passwordForm.get('currentPassword')!; }
  get newPassword()        { return this.passwordForm.get('newPassword')!;    }
  get confirmNewPassword() { return this.passwordForm.get('confirmNewPassword')!; }
}