// File: src/app/patient/my-doctors/my-doctors.component.ts

import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subscription } from 'rxjs';
import { MatCardModule }            from '@angular/material/card';
import { MatButtonModule }          from '@angular/material/button';
import { MatIconModule }            from '@angular/material/icon';
import { MatInputModule }           from '@angular/material/input';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatDividerModule }         from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatChipsModule }           from '@angular/material/chips';
import { DoctorPatientService }     from '../../services/doctor-patient.service';
import { DoctorService }            from '../../services/doctor.service';
import { NotificationService }      from '../../core/services/notification.service';
import { SignalrService }           from '../../services/signalr.service';
import { DoctorProfile }            from '../../models/doctor.model';
import { UploadDocumentDialogComponent } from './upload-document-dialog/upload-document-dialog.component';
import { ViewDocumentsDialogComponent }  from './view-documents-dialog/view-documents-dialog.component';
import { PatientDoctorResponseDto, SendDoctorRequestDto } from '../../models/doctor-patient,model';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-my-doctors',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatDividerModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatDialogModule, MatChipsModule,
  ],
  templateUrl: './my-doctors.component.html',
  styleUrls:   ['./my-doctors.component.css']
})
export class MyDoctorsComponent implements OnInit, OnDestroy {

  private dpService      = inject(DoctorPatientService);
  private doctorService  = inject(DoctorService);
  private notify         = inject(NotificationService);
  private signalrService = inject(SignalrService);
  private dialog         = inject(MatDialog);
  private route          = inject(ActivatedRoute);
  // ── State ─────────────────────────────────────────────────
  myDoctors    = signal<PatientDoctorResponseDto[]>([]);
  allDoctors   = signal<DoctorProfile[]>([]);
  isLoading    = signal(true);
  searchQuery  = signal('');
  revoking     = signal<string | null>(null);
  requesting   = signal<string | null>(null);
  showConfirm  = signal<string | null>(null);

  // Send request without doctor — note dialog
  showRequestDialog = signal(false);
  requestNote       = signal('');

  private signalrSub!: Subscription;

  // ── Computed ──────────────────────────────────────────────
  connectedDoctors = computed(() =>
    this.myDoctors().filter(d => d.status === 'active')
  );

  // All non-terminal pending statuses shown in pending section
  pendingDoctors = computed(() =>
    this.myDoctors().filter(d =>
      d.status === 'pending_admin' || d.status === 'pending_doctor'
    )
  );

  availableDoctors = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();

    // Exclude doctors already in any active/pending state
    const linkedDoctorIds = new Set(
      this.myDoctors()
        .filter(d => d.status === 'active' || d.status === 'pending_admin' || d.status === 'pending_doctor')
        .filter(d => d.doctorId != null)
        .map(d => d.doctorId!)
    );

    return this.allDoctors()
      .filter(d => !linkedDoctorIds.has(d.doctorId))
      .filter(d => !q ||
        d.fullName.toLowerCase().includes(q) ||
        (d.specialization?.toLowerCase().includes(q) ?? false) ||
        (d.hospital?.toLowerCase().includes(q) ?? false)
      );
  });

  ngOnInit(): void {
    this.loadAll();
    this.setupSignalR();
  }

  loadAll(): void {
    this.isLoading.set(true);

    this.dpService.getMyDoctors().subscribe({
      next: (d) => {
        this.myDoctors.set(d);
        const targetDoctorId = this.route.snapshot.queryParamMap.get('openDocsFor');
        
        if (targetDoctorId) {
          const targetDoctor = d.find(doc => doc.doctorId === targetDoctorId);
          
          if (targetDoctor && targetDoctor.doctorId && targetDoctor.fullName) {
            setTimeout(() => {
              this.openViewDocumentsDialog(targetDoctor.doctorId!, targetDoctor.fullName!);
            }, 100);
          }
        }
      },
      error: () => {}
    });

    this.doctorService.getAllDoctors().subscribe({
      next:  d  => { this.allDoctors.set(d); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  // ── Send request WITH doctor selected ─────────────────────
  sendRequest(doctorId: string): void {
    this.requesting.set(doctorId);
    const req: SendDoctorRequestDto = { doctorId };

    this.dpService.sendRequest(req).subscribe({
      next: link => {
        this.myDoctors.update(list => {
          const exists = list.some(d => d.doctorPatientId === link.doctorPatientId);
          return exists
            ? list.map(d => d.doctorPatientId === link.doctorPatientId ? link : d)
            : [...list, link];
        });
        this.requesting.set(null);
        this.notify.success('Request submitted. Awaiting admin review.');
      },
      error: () => this.requesting.set(null)
    });
  }

  // ── Send request WITHOUT selecting a doctor ───────────────
  openRequestWithoutDoctorDialog(): void {
    this.requestNote.set('');
    this.showRequestDialog.set(true);
  }

  closeRequestDialog(): void {
    this.showRequestDialog.set(false);
  }

  submitRequestWithoutDoctor(): void {
    this.showRequestDialog.set(false);
    this.requesting.set('no-doctor');

    const req: SendDoctorRequestDto = {
      doctorId:    null,
      patientNote: this.requestNote() || undefined,
    };

    this.dpService.sendRequest(req).subscribe({
      next: link => {
        this.myDoctors.update(list => [...list, link]);
        this.requesting.set(null);
        this.notify.success('Request submitted. Admin will assign a doctor for you.');
      },
      error: () => this.requesting.set(null)
    });
  }

  // ── Revoke ─────────────────────────────────────────────────
  confirmRevoke(doctorPatientId: string): void { this.showConfirm.set(doctorPatientId); }
  cancelRevoke(): void                         { this.showConfirm.set(null); }

  revokeAccess(doctorPatientId: string): void {
    this.revoking.set(doctorPatientId);
    this.showConfirm.set(null);

    this.dpService.revokeAccess(doctorPatientId).subscribe({
      next: () => {
        this.myDoctors.update(list =>
          list.map(d => d.doctorPatientId === doctorPatientId
            ? { ...d, status: 'revoked' as const }
            : d)
        );
        this.revoking.set(null);
      },
      error: () => this.revoking.set(null)
    });
  }

  // ── Documents ──────────────────────────────────────────────
  openUploadDialog(doctorId: string, doctorName: string): void {
    const ref = this.dialog.open(UploadDocumentDialogComponent, {
      width: '500px', disableClose: true,
      data: { doctorId, doctorName }
    });
    ref.afterClosed().subscribe(result => {
      if (result) this.notify.success('Document uploaded successfully.');
    });
  }

  openViewDocumentsDialog(doctorId: string, doctorName: string): void {
    this.dialog.open(ViewDocumentsDialogComponent, {
      width: '600px',
      data: { doctorId, doctorName }
    });
  }

  // ── Status helpers ─────────────────────────────────────────
  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      'pending_admin':   'Awaiting Admin Review',
      'pending_doctor':  'Awaiting Doctor Acceptance',
      'active':          'Connected',
      'declined_doctor': 'Declined by Doctor',
      'rejected_admin':  'Rejected',
      'revoked':         'Revoked',
    };
    return map[status] ?? status;
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'pending_admin':   'status-pending-admin',
      'pending_doctor':  'status-pending-doctor',
      'active':          'status-connected',
      'declined_doctor': 'status-declined',
      'rejected_admin':  'status-rejected',
    };
    return map[status] ?? '';
  }

  // ── SignalR ────────────────────────────────────────────────
  private setupSignalR(): void {

    this.signalrSub = this.signalrService.requestUpdate$.subscribe({
      next: (payload: any) => {
        if (!payload) return;

        const id     = payload.doctorPatientId || payload.DoctorPatientId;
        const status = payload.status          || payload.Status;
        const name   = payload.fullName        || payload.FullName;

        this.myDoctors.update(list =>
          list.map(d => d.doctorPatientId === id ? { ...d, status } : d)
        );

        if (status === 'active')
          this.notify.success(`Dr. ${name} accepted your request!`);
        else if (status === 'declined_doctor')
          this.notify.error(`Dr. ${name} declined your request.`);
        else if (status === 'pending_doctor')
          this.notify.success('Your request was approved by admin. Waiting for doctor.');
        else if (status === 'rejected_admin')
          this.notify.error('Your request was not approved by admin.');
      }
    });
  }

  getInitial(name: string | null): string {
    return name?.charAt(0).toUpperCase() ?? '?';
  }

  isRequesting(id: string): boolean { return this.requesting() === id; }
  isRevoking(id: string): boolean   { return this.revoking()   === id; }

  ngOnDestroy(): void {
    this.signalrSub?.unsubscribe();
  }
}