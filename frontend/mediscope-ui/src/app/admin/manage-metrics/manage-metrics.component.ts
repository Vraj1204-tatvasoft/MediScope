import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MetricDefinition, CreateMetricDefinitionRequest, UpdateMetricDefinitionRequest } from '../../models/metric-definition.model';
import { MetricDefinitionService } from '../../services/metric-definition.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-manage-metrics',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './manage-metrics.component.html',
  styleUrl: './manage-metrics.component.css'
})
export class ManageMetricsComponent implements OnInit {
  metrics = signal<MetricDefinition[]>([]);
  loading = signal(false);
  search = signal('');

  showAddModal = signal(false);
  showEditModal = signal(false);
  showToggleModal = signal(false);
  selectedMetric = signal<MetricDefinition | null>(null);

  // Injecting Notification Service
  private notify = inject(NotificationService);

  filteredMetrics = computed(() => {
    const search = this.search().toLowerCase();
    return this.metrics().filter(metric =>
      metric.displayName.toLowerCase().includes(search) ||
      metric.metricType.toLowerCase().includes(search)
    );
  });

  metricsWithRange = computed(() =>
    this.metrics().filter(m => m.normalMin != null).length
  );

  metricForm!: ReturnType<FormBuilder['group']>;

  constructor(
    private fb: FormBuilder,
    private metricService: MetricDefinitionService
  ) {
    this.metricForm = this.fb.group({
      metricType: ['', Validators.required],
      displayName: ['', Validators.required],
      defaultUnit: ['', Validators.required],
      normalMin: [null as number | null],
      normalMax: [null as number | null],
      description: ['']
    });
  }

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.loading.set(true);
    this.metricService.getAll().subscribe({
      next: res => {
        this.metrics.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load metric definitions list.');
      }
    });
  }

  openAddModal(): void {
    this.metricForm.reset();
    this.showAddModal.set(true);
  }

  openEditModal(metric: MetricDefinition): void {
    this.selectedMetric.set(metric);
    this.metricForm.patchValue({
      metricType: metric.metricType,
      displayName: metric.displayName,
      defaultUnit: metric.defaultUnit,
      normalMin: metric.normalMin,
      normalMax: metric.normalMax,
      description: metric.description
    });
    this.showEditModal.set(true);
  }

  openToggleModal(metric: MetricDefinition): void {
    this.selectedMetric.set(metric);
    this.showToggleModal.set(true); 
  }

  closeModals(): void {
    this.showAddModal.set(false);
    this.showEditModal.set(false);
    this.showToggleModal.set(false);
  }

  createMetric(): void {
    if (this.metricForm.invalid) return;

    const request = this.metricForm.getRawValue() as CreateMetricDefinitionRequest;
    this.metricService.create(request).subscribe({
      next: () => {
        this.closeModals();
        this.loadMetrics();
        this.notify.success('New metric definition created successfully.');
      },
      error: () => {
        this.notify.error('Failed to create metric definition.');
      }
    });
  }

  updateMetric(): void {
    if (this.metricForm.invalid || !this.selectedMetric()) return;

    const request = this.metricForm.getRawValue() as UpdateMetricDefinitionRequest;
    this.metricService.update(this.selectedMetric()!.id, request).subscribe({
      next: () => {
        this.closeModals();
        this.loadMetrics();
        this.notify.success('Metric definition updated successfully.');
      },
      error: () => {
        this.notify.error('Failed to update metric definition changes.');
      }
    });
  }

  toggleMetricStatus(): void {
    const metric = this.selectedMetric();
    if (!metric) return;

    const actionText = metric.isActive ? 'deactivated' : 'activated';

    this.metricService.toggleStatus(metric.id).subscribe({
      next: () => {
        this.closeModals();
        this.loadMetrics();
        this.notify.success(`Metric "${metric.displayName}" was ${actionText} successfully.`);
      },
      error: () => {
        this.notify.error('Failed to change metric status.');
      }
    });
  }
}