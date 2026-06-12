import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';

import { DoctorPatientService } from '../../services/doctor-patient.service';
import { HealthHistoryService } from '../../services/health-history.service';
import { NotificationService } from '../../core/services/notification.service';
import { DoctorPatientResponseDto, RespondToRequestDto } from '../../models/doctor-patient,model';
import { HealthHistoryRow, HealthMetricSubmission } from '../../models/health-history.model';
import { SignalrService } from '../../services/signalr.service';

@Component({
  selector: 'app-pending-requests',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './pending-requests.component.html',
  styleUrls: ['./pending-requests.component.css']
})
export class PendingRequestsComponent implements OnInit, OnDestroy {
  private docPatientService = inject(DoctorPatientService);
  private historyService = inject(HealthHistoryService);
  private notify = inject(NotificationService);
  private signalrService = inject(SignalrService);

  // States
  requests = signal<DoctorPatientResponseDto[]>([]);
  isLoading = signal<boolean>(true);
  processingId = signal<string | null>(null);

  // Health records quick-look states
  expandedPatientId = signal<string | null>(null);
  isLoadingRecords = signal<boolean>(false);
  previewRecords = signal<HealthHistoryRow[]>([]);

  // Real-time Event Subscription Memory Leak Protection
  private signalrSubscription!: Subscription;

  // Count helper metric calculation
  pendingCount = computed(() => this.requests().length);

  ngOnInit(): void {
    this.loadPendingQueue();
    this.setupRealtimeListeners();
  }

  private setupRealtimeListeners(): void {
    this.signalrService.startConnection();

    this.signalrSubscription = this.signalrService.incomingRequest$.subscribe({
      next: (newRequest: DoctorPatientResponseDto) => {
        const alreadyExists = this.requests().some(r => r.doctorPatientId === newRequest.doctorPatientId);
        
        if (!alreadyExists) {
          this.requests.update(currentQueue => [newRequest, ...currentQueue]);
          this.notify.success(`New access link requested in real-time by ${newRequest.fullName}!`);
        }
      }
    });
  }

  loadPendingQueue(): void {
    this.isLoading.set(true);
    this.docPatientService.getPendingRequests().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.notify.error('Failed to load pending connection records pipeline.');
      }
    });
  }

  handleResponse(doctorPatientId: string, accept: boolean): void {
    this.processingId.set(doctorPatientId);
    const dto: RespondToRequestDto = { doctorPatientId, accept };

    this.docPatientService.respondToRequest(dto).subscribe({
      next: () => {
        this.processingId.set(null);
        this.notify.success(accept ? 'Patient request accepted successfully.' : 'Patient request declined successfully.');
        
        if (this.expandedPatientId() === doctorPatientId) {
          this.closeRecordsPreview();
        }
        
        this.loadPendingQueue(); 
      },
      error: () => {
        this.processingId.set(null);
      }
    });
  }

  toggleRecordsPreview(doctorPatientId: string, patientGuid: string): void {
    if (this.expandedPatientId() === doctorPatientId) {
      this.closeRecordsPreview();
      return;
    }

    this.expandedPatientId.set(doctorPatientId);
    this.isLoadingRecords.set(true);
    this.previewRecords.set([]);

    // Updated to call your backend with appropriate pagination parameters (page=1, size=5 for quick preview)
    this.historyService.getHistoryByPatientId(patientGuid, 1, 5).subscribe({
      next: (response) => {
        // Map the new backend model structure cleanly using the unified method
        this.previewRecords.set(this.mapSubmissionRows(response.items || []));
        this.isLoadingRecords.set(false);
      },
      error: () => {
        this.isLoadingRecords.set(false);
        this.notify.error('Failed to pre-fetch patient metrics background info.');
      }
    });
  }

  closeRecordsPreview(): void {
    this.expandedPatientId.set(null);
    this.previewRecords.set([]);
  }

  getInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : 'P';
  }

  // ─────────────────────────────────────
  // UNIFIED SUBMISSION MAPPING PARSER
  // ─────────────────────────────────────
  private mapSubmissionRows(submissions: HealthMetricSubmission[]): HealthHistoryRow[] {
    return submissions.map(submission => {
      const rowMetrics: HealthHistoryRow['metrics'] = {};
      const flaggedMetrics: string[] = [];

      let systolicVal: string | null = null;
      let diastolicVal: string | null = null;
      let bpIsAbnormal = false;

      submission.metrics.forEach(metric => {
        const typeKey = metric.metricType.toLowerCase().trim();
        const status = (metric.status || '').toUpperCase();

        const isAbnormal =
          status === 'HIGH' ||
          status === 'LOW' ||
          status === 'ELEVATED' ||
          status === 'CRITICAL';

        if (typeKey === 'systolic_blood_pressure') {
          systolicVal = Math.round(metric.value).toString();
          if (isAbnormal) bpIsAbnormal = true;
        } 
        else if (typeKey === 'diastolic_blood_pressure') {
          diastolicVal = Math.round(metric.value).toString();
          if (isAbnormal) bpIsAbnormal = true;
        } 
        else {
          let formattedValue = `${metric.value} ${metric.unit || ''}`;
          if (typeKey === 'heart_rate') formattedValue = `${Math.round(metric.value)}`;
          if (typeKey === 'sleep') formattedValue = `${metric.value} hrs`;

          rowMetrics[typeKey] = {
            displayValue: formattedValue,
            rawVal: metric.value
          };

          if (isAbnormal) flaggedMetrics.push(typeKey);
        }
      });

      // Synthetic BP configuration assembly
      if (systolicVal || diastolicVal) {
        rowMetrics['blood_pressure'] = {
          displayValue: `${systolicVal || '—'}/${diastolicVal || '—'}`,
          rawVal: Number(systolicVal || 0)
        };
        if (bpIsAbnormal) flaggedMetrics.push('blood_pressure');
      }

      return {
        submissionId: submission.submissionId,
        date: new Date(submission.recordedAt).toLocaleDateString('en-CA'),
        addedBy: submission.recordedByName || 'Patient',
        isDoctor: (submission.recordedByRole || '').toUpperCase() !== 'PATIENT',
        notes: submission.notes,
        status: this.normalizeStatus(submission.status),
        flaggedMetrics,
        isExpanded: false,
        metrics: rowMetrics
      };
    });
  }

  private normalizeStatus(status?: string): string {
    switch ((status || '').toUpperCase()) {
      case 'CRITICAL':
        return 'Critical';
      case 'ELEVATED':
      case 'HIGH':
      case 'LOW':
        return 'Elevated';
      default:
        return 'Normal';
    }
  }

  ngOnDestroy(): void {
    if (this.signalrSubscription) {
      this.signalrSubscription.unsubscribe();
    }
  }
}