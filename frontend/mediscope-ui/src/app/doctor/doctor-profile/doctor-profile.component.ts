// ════════════════════════════════════════════════════════════════════════════
// FILE: src/app/doctor/profile/doctor-profile.component.ts
// ════════════════════════════════════════════════════════════════════════════
import { Component, OnInit, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule }            from '@angular/material/card';
import { MatButtonModule }          from '@angular/material/button';
import { MatInputModule }           from '@angular/material/input';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatSelectModule }          from '@angular/material/select';
import { MatIconModule }            from '@angular/material/icon';
import { MatDividerModule }         from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule }           from '@angular/material/chips';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { AuthService }    from '../../core/services/auth.service';
import { DoctorProfile, SPECIALIZATIONS, UpdateDoctorRequest } from '../../models/doctor.model';
import { DoctorService } from '../../services/doctor.service';

function passwordMatchValidator(c: AbstractControl): ValidationErrors | null {
  const np = c.get('newPassword'); const cnp = c.get('confirmNewPassword');
  if (np && cnp && np.value !== cnp.value) { cnp.setErrors({ mismatch: true }); return { mismatch: true }; }
  return null;
}

@Component({
  selector: 'app-doctor-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule,
    MatInputModule, MatFormFieldModule, MatSelectModule, MatIconModule,
    MatDividerModule, MatProgressSpinnerModule, MatSnackBarModule,
    MatChipsModule, MatTooltipModule],
  templateUrl: './doctor-profile.component.html',
  styleUrls:   ['./doctor-profile.component.css']
})
export class DoctorProfileComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private authService   = inject(AuthService);
  private fb            = inject(FormBuilder);
  private snackBar      = inject(MatSnackBar);

  profile        = signal<DoctorProfile | null>(null);
  isEditMode     = signal(false);
  isLoading      = signal(false);
  isSaving       = signal(false);
  showPwSection  = signal(false);
  showCurrentPw  = signal(false);
  showNewPw      = signal(false);
  showConfirmPw  = signal(false);

  readonly specializations = SPECIALIZATIONS;

  editForm: FormGroup = this.fb.group({
    fullName:        ['', [Validators.required, Validators.maxLength(150)]],
    contactNumber:   ['', [Validators.pattern(/^\d{10}$/)]],
    specialization:  [''],
    hospital:        [''],
    yearsExperience: [null, [Validators.min(0), Validators.max(60)]],
    bio:             [''],
  });

  passwordForm: FormGroup = this.fb.group({
    currentPassword:    ['', Validators.required],
    newPassword:        ['', [Validators.required, Validators.minLength(8)]],
    confirmNewPassword: ['', Validators.required],
  }, { validators: passwordMatchValidator });

  ngOnInit(): void { this.loadProfile(); }

  loadProfile(): void {
    this.isLoading.set(true);
    this.doctorService.getMyProfile().subscribe({
      next: d => { this.profile.set(d); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); this.showSnack('Failed to load profile.', 'error'); }
    });
  }

  enterEditMode(): void {
    const p = this.profile(); if (!p) return;
    this.editForm.patchValue({
      fullName: p.fullName, contactNumber: p.contactNumber ?? '',
      specialization: p.specialization ?? '', hospital: p.hospital ?? '',
      yearsExperience: p.yearsExperience ?? null, bio: p.bio ?? '',
    });
    this.isEditMode.set(true);
  }

  cancelEdit(): void { this.isEditMode.set(false); this.editForm.reset(); }

  saveProfile(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    this.isSaving.set(true);
    const v = this.editForm.value;
    const req: UpdateDoctorRequest = {
      fullName: v.fullName, contactNumber: v.contactNumber || null,
      specialization: v.specialization || null, hospital: v.hospital || null,
      yearsExperience: v.yearsExperience ?? null, bio: v.bio || null,
    };
    this.doctorService.updateMyProfile(req).subscribe({
      next: updated => {
        this.profile.set(updated); this.isEditMode.set(false);
        this.isSaving.set(false); this.showSnack('Profile updated.', 'success');
      },
      error: err => { this.isSaving.set(false); this.showSnack(err.error?.message ?? 'Update failed.', 'error'); }
    });
  }
  changePassword(): void {

    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
  
    this.isSaving.set(true);
  
    const request = {
      currentPassword:
        this.passwordForm.value.currentPassword,
  
      newPassword:
        this.passwordForm.value.newPassword,
  
      confirmNewPassword:
        this.passwordForm.value.confirmNewPassword
    };
  
    this.doctorService.changePassword(request)
      .subscribe({
  
        next: () => {
  
          this.isSaving.set(false);
          localStorage.setItem(
            'mustChangePassword',
            'false'
          );
          this.showSnack(
            'Password changed successfully. Please login again.',
            'success'
          );
  
          this.passwordForm.reset();
  
          this.showPwSection.set(false);
  
          setTimeout(() => {
            this.authService.logout();
          }, 1500);
        },
  
        error: (err : any) => {
  
          this.isSaving.set(false);
  
          this.showSnack(
            err.error?.message ?? 'Failed to change password.',
            'error'
          );
        }
      });
  }
  toggleCurrentPw(): void { this.showCurrentPw.set(!this.showCurrentPw()); }
  toggleNewPw(): void     { this.showNewPw.set(!this.showNewPw()); }
  toggleConfirmPw(): void { this.showConfirmPw.set(!this.showConfirmPw()); }
  togglePwSection(): void { this.showPwSection.set(!this.showPwSection()); this.passwordForm.reset(); }

  getUserInitial(): string { return this.profile()?.fullName?.charAt(0).toUpperCase() ?? 'D'; }
  formatDate(d: string | null): string { return d ? new Date(d).toISOString().split('T')[0] : '—'; }

  private showSnack(msg: string, type: 'success' | 'error'): void {
    this.snackBar.open(msg, 'Close', {
      duration: 3000, panelClass: type === 'success' ? ['snack-success'] : ['snack-error'],
      horizontalPosition: 'right', verticalPosition: 'top',
    });
  }

  get fullName()        { return this.editForm.get('fullName')!; }
  get contactNumber()   { return this.editForm.get('contactNumber')!; }
  get yearsExperience() { return this.editForm.get('yearsExperience')!; }
  get currentPassword() { return this.passwordForm.get('currentPassword')!; }
  get newPassword()     { return this.passwordForm.get('newPassword')!; }
  get confirmNewPassword() { return this.passwordForm.get('confirmNewPassword')!; }
}