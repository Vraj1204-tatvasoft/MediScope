import { Component, OnInit, Input, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MetricDefinition } from '../../models/metric-definition.model';
import { MetricValueRecord, AddHealthMetricRequestDto } from '../../models/health-metric.model';
import { HealthMetricService } from '../../services/health-metric.service';
import { NotificationService } from '../../core/services/notification.service';
import { Router } from '@angular/router';
import { MatSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-add-health-data',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSpinner
  ],
  templateUrl: './add-health-data.component.html',
  styleUrls: ['./add-health-data.component.css']
})
export class AddHealthDataComponent implements OnInit {
  // ── EXTERNAL INPUT OVERRIDES FOR DOCTOR VIEW REUSE ────────────────
  @Input() explicitPatientId?: string;
  @Input() explicitPatientName?: string;
  
  // Optional event to trigger tab switching or list refreshes inside parent dashboards
  @Output() saveSuccess = new EventEmitter<void>();

  healthForm!: FormGroup;
  metricDefinitions = signal<MetricDefinition[]>([]);
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);
  maxDate = new Date();
  patientName = '';

  private notify = inject(NotificationService);

  constructor(
    private fb: FormBuilder,
    private metricService: HealthMetricService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.resolveTargetContext();
    this.initializeForm();
    this.loadMetrics();
  }

  private resolveTargetContext(): void {
    // If explicit inputs exist, we are operating inside a doctor's care plan workflow
    if (this.explicitPatientName) {
      this.patientName = this.explicitPatientName;
      return;
    }

    // Default Fallback: Retrieve profile attributes from active user context session storage
    const user = localStorage.getItem('ms_user');
    if (user) {
      const parsedUser = JSON.parse(user);
      this.patientName = parsedUser.fullName;
    }
  }

  private initializeForm(): void {
    this.healthForm = this.fb.group({
      recordedAt: [new Date(), Validators.required],
      notes: [''],
      metrics: this.fb.group({})
    });
  }

  private loadMetrics(): void {
    this.metricService.getMetricDefinitions().subscribe({
      next: (definitions) => {
        this.metricDefinitions.set(definitions);
        const metricsGroup = this.healthForm.get('metrics') as FormGroup;

        definitions.forEach(metric => {
          metricsGroup.addControl(
            metric.id.toString(),
            this.fb.control(null)
          );
        });

        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.notify.error('Failed to load metric configuration templates.');
      }
    });
  }

  clearForm(): void {
    this.healthForm.patchValue({
      recordedAt: new Date(),
      notes: ''
    });
    this.healthForm.get('metrics')?.reset();
  }

  saveHealthRecord(): void {
    if (this.healthForm.invalid) {
      return;
    }

    this.isSaving.set(true);
    const formValue = this.healthForm.value;
    const recordsPayload: MetricValueRecord[] = [];
    const metricsGroup = this.healthForm.get('metrics') as FormGroup;

    this.metricDefinitions().forEach(metric => {
      const control = metricsGroup.get(metric.id.toString());
      const rawValue = control ? control.value : null;

      const numericValue = rawValue !== null && rawValue !== undefined && rawValue !== ''
        ? Number(rawValue)
        : 0;

      if (numericValue > 0) {
        recordsPayload.push({
          metricDefinitionId: metric.id,
          metricType: metric.metricType,
          unit: metric.defaultUnit,
          value: numericValue
        });
      }
    });

    if (recordsPayload.length === 0) {
      this.notify.warn('Please fill in at least one metric value greater than 0 before saving.');
      this.isSaving.set(false);
      return;
    }

    let dateToSave = new Date(formValue.recordedAt || new Date());
    const userTimezoneOffset = dateToSave.getTimezoneOffset() * 60000;
    const localizedDate = new Date(dateToSave.getTime() - userTimezoneOffset);

    const payload: AddHealthMetricRequestDto = {
      recordedAt: localizedDate.toISOString(),
      notes: formValue.notes,
      metrics: recordsPayload,
      // If a doctor passes an ID, the API routes it correctly via route body payload tags
      ...(this.explicitPatientId && { patientId: this.explicitPatientId })
    };

    this.metricService.saveHealthRecord(payload, { showSuccess: false }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.clearForm();
        this.notify.success('Health record saved successfully.');
        
        // ── ROUTING DECISION MATRIX BASED ON ACCESSING ROLE ───────────────────
        if (this.explicitPatientId) {
          // If in Doctor Mode, notify the parent view instead of changing routes forcefully
          this.saveSuccess.emit();
        } else {
          // If in Patient Mode, route directly to personal self-logged charts
          this.router.navigate(['/patient/health-history']);
        }
      },
      error: (err) => {
        console.error(err);
        this.isSaving.set(false);
        if (!err?.error?.errors) {
          this.notify.error('Failed to record health metric entry.');
        }
      }
    });
  }
}