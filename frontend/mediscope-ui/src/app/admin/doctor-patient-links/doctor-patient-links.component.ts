// File: src/app/admin/doctor-patient-links/doctor-patient-links.component.ts

import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { MatCardModule }            from '@angular/material/card';
import { MatButtonModule }          from '@angular/material/button';
import { MatIconModule }            from '@angular/material/icon';
import { MatInputModule }           from '@angular/material/input';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatSelectModule }          from '@angular/material/select';
import { MatDividerModule }         from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { MatChipsModule }           from '@angular/material/chips';
import { MatTabsModule }            from '@angular/material/tabs';

import { DoctorPatientService }       from '../../services/doctor-patient.service';
import { DoctorService }              from '../../services/doctor.service';
import { NotificationService }        from '../../core/services/notification.service';
import { DoctorProfile } from '../../models/doctor.model';
import { AdminConnectionRequestDto, AdminApproveRequestDto, AdminRejectRequestDto } from '../../models/doctor-patient,model';

@Component({
  selector: 'app-doctor-patient-links',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSelectModule,
    MatDividerModule, MatProgressSpinnerModule,
    MatTooltipModule, MatChipsModule, MatTabsModule,
  ],
  templateUrl: './doctor-patient-links.component.html',
  styleUrls:   ['./doctor-patient-links.component.css']
})
export class DoctorPatientLinksComponent implements OnInit {

  private dpService     = inject(DoctorPatientService);
  private doctorService = inject(DoctorService);
  private notify        = inject(NotificationService);

  // ── State ─────────────────────────────────────────────────
  allRequests  = signal<AdminConnectionRequestDto[]>([]);
  allDoctors   = signal<DoctorProfile[]>([]);
  isLoading    = signal(true);
  searchQuery  = signal('');
  filterDoctor = signal('ALL');
  filterStatus = signal('ALL');

  // Approve dialog
  showApproveDialog   = signal(false);
  selectedRequest     = signal<AdminConnectionRequestDto | null>(null);
  selectedDoctorId    = signal('');
  approveNote         = signal('');
  isSubmitting        = signal(false);

  // Reject dialog
  showRejectDialog = signal(false);
  rejectNote       = signal('');
  rejectingId      = signal<string | null>(null);

  // ── Computed stats ─────────────────────────────────────────
  pendingAdminCount  = computed(() =>
    this.allRequests().filter(r => r.status === 'pending_admin').length);
  pendingDoctorCount = computed(() =>
    this.allRequests().filter(r => r.status === 'pending_doctor').length);
  activeCount        = computed(() =>
    this.allRequests().filter(r => r.status === 'active').length);
  totalCount         = computed(() => this.allRequests().length);

  // ── Filtered list ──────────────────────────────────────────
  filteredRequests = computed(() => {
    const q   = this.searchQuery().toLowerCase().trim();
    const doc = this.filterDoctor();
    const st  = this.filterStatus();

    return this.allRequests().filter(r => {
      const matchSearch = !q ||
        r.patientName.toLowerCase().includes(q) ||
        (r.doctorName?.toLowerCase().includes(q) ?? false);

      const matchDoctor = doc === 'ALL' || r.doctorId === doc;
      const matchStatus = st  === 'ALL' || r.status   === st;

      return matchSearch && matchDoctor && matchStatus;
    });
  });

  // Pending tab — admin review needed
  pendingRequests = computed(() =>
    this.filteredRequests().filter(r => r.status === 'pending_admin')
  );

  // All tab
  allFilteredRequests = computed(() => this.filteredRequests());

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.dpService.getAllRequestsForAdmin().subscribe({
      next:  r  => { this.allRequests.set(r); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });

    this.doctorService.getAllDoctors().subscribe({
      next: d => this.allDoctors.set(d)
    });
  }

  // ── Approve dialog ─────────────────────────────────────────
  openApproveDialog(request: AdminConnectionRequestDto): void {

    // Doctor already selected by patient
    if (request.doctorId) {
      const payload: AdminApproveRequestDto = {
        doctorPatientId: request.doctorPatientId,
        doctorId: request.doctorId,
        adminNote: undefined
      };
      this.dpService.approveRequest(payload).subscribe({
        next: () => {
          this.allRequests.update(list =>
            list.map(r =>
              r.doctorPatientId === request.doctorPatientId ? { ...r, status: 'pending_doctor' as const } : r
            )
          );
          this.notify.success('Request forwarded to selected doctor.');
        }
      });
      return;
    }
    // No doctor selected → open assignment dialog
    this.selectedRequest.set(request);
    this.selectedDoctorId.set('');
    this.approveNote.set('');
    this.showApproveDialog.set(true);
  }

  closeApproveDialog(): void {
    this.showApproveDialog.set(false);
    this.selectedRequest.set(null);
    this.selectedDoctorId.set('');
  }

  submitApprove(): void {
    if (!this.selectedDoctorId()) {
      this.notify.error('Please select a doctor to assign.');
      return;
    }

    const req = this.selectedRequest();
    if (!req) return;

    this.isSubmitting.set(true);

    const payload: AdminApproveRequestDto = {
      doctorPatientId: req.doctorPatientId,
      doctorId:        this.selectedDoctorId(),
      adminNote:       this.approveNote() || undefined,
    };

    this.dpService.approveRequest(payload).subscribe({
      next: () => {
        // Update status in local list
        this.allRequests.update(list =>
          list.map(r => r.doctorPatientId === req.doctorPatientId
            ? { ...r, status: 'pending_doctor' as const,
                doctorId:   this.selectedDoctorId(),
                doctorName: this.allDoctors()
                  .find(d => d.doctorId === this.selectedDoctorId())?.fullName ?? null }
            : r)
        );
        this.isSubmitting.set(false);
        this.closeApproveDialog();
        this.notify.success('Request approved. Doctor has been notified.');
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  // ── Reject dialog ──────────────────────────────────────────
  openRejectDialog(request: AdminConnectionRequestDto): void {
    this.selectedRequest.set(request);
    this.rejectNote.set('');
    this.showRejectDialog.set(true);
  }

  closeRejectDialog(): void {
    this.showRejectDialog.set(false);
    this.selectedRequest.set(null);
  }

  submitReject(): void {
    const req = this.selectedRequest();
    if (!req) return;

    this.isSubmitting.set(true);
    this.rejectingId.set(req.doctorPatientId);

    const payload: AdminRejectRequestDto = {
      doctorPatientId: req.doctorPatientId,
      adminNote:       this.rejectNote() || undefined,
    };

    this.dpService.rejectRequest(payload).subscribe({
      next: () => {
        this.allRequests.update(list =>
          list.map(r => r.doctorPatientId === req.doctorPatientId
            ? { ...r, status: 'rejected_admin' as const }
            : r)
        );
        this.isSubmitting.set(false);
        this.rejectingId.set(null);
        this.closeRejectDialog();
        this.notify.success('Request rejected. Patient has been notified.');
      },
      error: () => { this.isSubmitting.set(false); this.rejectingId.set(null); }
    });
  }

  // ── Helpers ───────────────────────────────────────────────
  getInitial(name: string | null): string {
    return name?.charAt(0).toUpperCase() ?? '?';
  }

  formatDate(d: string | null): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-CA');
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'pending_admin':   'chip-pending-admin',
      'pending_doctor':  'chip-pending-doctor',
      'active':          'chip-active',
      'declined_doctor': 'chip-declined',
      'rejected_admin':  'chip-rejected',
      'revoked':         'chip-revoked',
    };
    return map[status] ?? 'chip-default';
  }

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      'pending_admin':   'Pending Admin',
      'pending_doctor':  'Pending Doctor',
      'active':          'Connected',
      'declined_doctor': 'Declined',
      'rejected_admin':  'Rejected',
      'revoked':         'Revoked',
    };
    return map[status] ?? status;
  }

  // Doctors available to assign — active only
  get assignableDoctors(): DoctorProfile[] {
    return this.allDoctors().filter(d => d.isActive);
  }

  // Unique doctors from requests for the filter dropdown
  get doctorFilterOptions(): { id: string; name: string }[] {
    const seen = new Map<string, string>();
    this.allRequests().forEach(r => {
      if (r.doctorId && r.doctorName)
        seen.set(r.doctorId, r.doctorName);
    });
    return Array.from(seen.entries()).map(([id, name]) => ({ id, name }));
  }
}