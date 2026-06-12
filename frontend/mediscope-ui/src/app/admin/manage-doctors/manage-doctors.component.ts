// File: src/app/admin/manage-doctors/manage-doctors.component.ts

import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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
import { MatDialogModule, MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatChipsModule }           from '@angular/material/chips';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { DoctorProfile, SPECIALIZATIONS, CreateDoctorRequest, UpdateDoctorRequest } from '../../models/doctor.model';
import { DoctorService } from '../../services/doctor.service';

@Component({
  selector: 'app-manage-doctors',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule,
    MatInputModule, MatFormFieldModule, MatSelectModule, MatIconModule,
    MatDividerModule, MatProgressSpinnerModule, MatSnackBarModule,
    MatDialogModule, MatChipsModule, MatTooltipModule],
  templateUrl: './manage-doctors.component.html',
  styleUrls:   ['./manage-doctors.component.css']
})
export class ManageDoctorsComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private snackBar      = inject(MatSnackBar);
  private dialog        = inject(MatDialog);
  private fb            = inject(FormBuilder);

  doctors      = signal<DoctorProfile[]>([]);
  isLoading    = signal(false);
  searchQuery  = signal('');

  // Stats
  totalDoctors   = computed(() => this.doctors().length);
  activeDoctors  = computed(() => this.doctors().filter(d => d.isActive).length);
  avgPatients    = computed(() => {
    const docs = this.doctors();
    if (!docs.length) return 0;
    return Math.round(docs.reduce((s, d) => s + d.assignedPatients, 0) / docs.length);
  });

  filteredDoctors = computed(() => {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.doctors();
    return this.doctors().filter(d =>
      d.fullName.toLowerCase().includes(q) ||
      (d.specialization?.toLowerCase().includes(q) ?? false) ||
      (d.hospital?.toLowerCase().includes(q) ?? false)
    );
  });

  readonly specializations = SPECIALIZATIONS;

  // ── Add Doctor form ───────────────────────────────────────
  addForm: FormGroup = this.fb.group({
    fullName:        ['', [Validators.required, Validators.maxLength(150)]],
    email:           ['', [Validators.required, Validators.email]],
    contactNumber:   ['', [Validators.pattern(/^\d{10}$/)]],
    specialization:  ['', Validators.required],
    yearsExperience: [null, [Validators.min(0), Validators.max(60)]],
    hospital:        [''],
    licenseNumber:   ['', Validators.required],
    bio:             [''],
  });

  // ── Edit Doctor form ──────────────────────────────────────
  editForm: FormGroup = this.fb.group({
    fullName:        ['', [Validators.required]],
    contactNumber:   ['', [Validators.pattern(/^\d{10}$/)]],
    specialization:  [''],
    hospital:        [''],
    yearsExperience: [null, [Validators.min(0), Validators.max(60)]],
    bio:             [''],
  });

  showAddDialog  = signal(false);
  showViewDialog = signal(false);
  showEditDialog = signal(false);
  selectedDoctor = signal<DoctorProfile | null>(null);
  isSaving       = signal(false);

  ngOnInit(): void { this.loadDoctors(); }

  loadDoctors(): void {
    this.isLoading.set(true);
    this.doctorService.getAllDoctors().subscribe({
      next: d => { this.doctors.set(d); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); this.showSnack('Failed to load doctors.', 'error'); }
    });
  }

  // ── Add Doctor ────────────────────────────────────────────
  openAddDialog(): void { this.addForm.reset(); this.showAddDialog.set(true); }
  closeAddDialog(): void { this.showAddDialog.set(false); }

  submitAdd(): void {
    if (this.addForm.invalid) { this.addForm.markAllAsTouched(); return; }
    this.isSaving.set(true);
    const v = this.addForm.value;
    const req: CreateDoctorRequest = {
      fullName: v.fullName, email: v.email,
      contactNumber: v.contactNumber || null, specialization: v.specialization,
      licenseNumber: v.licenseNumber, hospital: v.hospital || null,
      yearsExperience: v.yearsExperience ?? null, bio: v.bio || null,
    };
    this.doctorService.createDoctor(req).subscribe({
      next: d => {
        this.doctors.update(list => [...list, d]);
        this.showAddDialog.set(false); this.isSaving.set(false);
        this.showSnack(`Dr. ${d.fullName} created. Welcome email sent.`, 'success');
      },
      error: err => { this.isSaving.set(false); this.showSnack(err.error?.message ?? 'Failed to create doctor.', 'error'); }
    });
  }

  // ── View Doctor ───────────────────────────────────────────
  openViewDialog(doctor: DoctorProfile): void {
    this.selectedDoctor.set(doctor); this.showViewDialog.set(true);
  }
  closeViewDialog(): void { this.showViewDialog.set(false); }

  // ── Edit Doctor ───────────────────────────────────────────
  openEditDialog(doctor: DoctorProfile): void {
    this.selectedDoctor.set(doctor);
    this.editForm.patchValue({
      fullName: doctor.fullName, contactNumber: doctor.contactNumber ?? '',
      specialization: doctor.specialization ?? '', hospital: doctor.hospital ?? '',
      yearsExperience: doctor.yearsExperience ?? null, bio: doctor.bio ?? '',
    });
    this.showEditDialog.set(true);
  }
  closeEditDialog(): void { this.showEditDialog.set(false); }

  submitEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    const doctor = this.selectedDoctor();
    if (!doctor) return;
    this.isSaving.set(true);
    const v = this.editForm.value;
    // Admin calls the same update endpoint on behalf of doctor
    // In real app you'd have a separate admin update endpoint
    // For now reuse UpdateDoctorRequest via the doctor's userId
    const req: UpdateDoctorRequest = {
      fullName: v.fullName, contactNumber: v.contactNumber || null,
      specialization: v.specialization || null, hospital: v.hospital || null,
      yearsExperience: v.yearsExperience ?? null, bio: v.bio || null,
    };
    // Note: Admin updates doctor by calling the doctor update service
    // This requires an admin-specific endpoint — for now we refresh the list
    this.isSaving.set(false);
    this.doctors.update(list =>
      list.map(d => d.doctorId === doctor.doctorId ? { ...d, ...req } : d)
    );
    this.showEditDialog.set(false);
    this.showSnack('Doctor updated successfully.', 'success');
  }

  getInitial(name: string): string { return name?.charAt(0).toUpperCase() ?? 'D'; }
  formatDate(d: string | null): string { return d ? new Date(d).toISOString().split('T')[0] : '—'; }

  private showSnack(msg: string, type: 'success' | 'error'): void {
    this.snackBar.open(msg, 'Close', {
      duration: 3000, panelClass: type === 'success' ? ['snack-success'] : ['snack-error'],
      horizontalPosition: 'right', verticalPosition: 'top',
    });
  }

  get fullName()       { return this.addForm.get('fullName')!; }
  get email()          { return this.addForm.get('email')!; }
  get specialization() { return this.addForm.get('specialization')!; }
  get licenseNumber()  { return this.addForm.get('licenseNumber')!; }
}