import { Component, OnInit, ElementRef, ViewChild, signal, computed, inject, OnDestroy, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// Chart.js Registration
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

import { DoctorDashboardService } from '../../services/doctor-dashboard.service';
import { DoctorDashboardContainer } from '../../models/doctor-dashboard.model';

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './doctor-dashboard.component.html',
  styleUrls: ['./doctor-dashboard.component.css']
})
export class DoctorDashboardComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DoctorDashboardService);

  @ViewChild('bpCompareCanvas') private trendsCanvas!: ElementRef<HTMLCanvasElement>;
  
  isLoading = signal<boolean>(true);
  isChartLoading  = signal<boolean>(false);
  dashboardData = signal<DoctorDashboardContainer | null>(null);
  availableMetrics = signal<{value: string, label: string}[]>([]);
  private trendChartInstance: Chart | null = null;
  recentActivities = computed(() => this.dashboardData()?.recentActivity || []);
  patientOverview = computed(() => this.dashboardData()?.patientStatusOverview || []);
  filters = {
    metric: 'blood_pressure',
    patient: 'all',
    duration: 'last_month',
    customStart: '',
    customEnd: ''
  };
  todayDateString: string = new Date().toISOString().split('T')[0];
  ngOnInit(): void {
    this.loadMetricDropdown();
    this.loadDoctorMetricsChannel();
  }

  private loadDoctorMetricsChannel(): void {
    this.isLoading.set(true);
    this.dashboardService.getDoctorDashboard().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.dashboardData.set(res.data);
          this.fetchAndRenderTrends();
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onFilterChange(): void {
    if (this.filters.duration === 'custom') {
      // Wait until both dates are selected
      if (!this.filters.customStart || !this.filters.customEnd) {
        return; 
      }
  
      // From Date cannot be after To Date
      if (this.filters.customStart > this.filters.customEnd) {
        console.warn("Invalid range: Start date must be before End date.");
        return; 
      }
  
      // Neither date can be in the future
      if (this.filters.customStart > this.todayDateString || this.filters.customEnd > this.todayDateString) {
        console.warn("Invalid range: Future dates are not permitted.");
        return;
      }
    }
  
    // If all validations pass, fetch the data
    this.fetchAndRenderTrends();
  }

  private fetchAndRenderTrends(): void {
    this.isChartLoading.set(true);
    
    this.dashboardService.getVitalTrends(
      this.filters.metric, 
      this.filters.patient, 
      this.filters.duration, 
      this.filters.customStart, 
      this.filters.customEnd
    ).subscribe({
      next: (res:any) => {
        if (res.success && res.data) {
          this.renderLineChart(res.data);
        }
        this.isChartLoading.set(false);
      },
      error: () => this.isChartLoading.set(false)
    });
  }
  private loadMetricDropdown(): void {
    this.dashboardService.getMetricDefinitions().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const dropdownOptions: {value: string, label: string}[] = [];
          let addedBloodPressure = false;
  
          res.data.forEach(metric => {
            // If it is a blood pressure metric, group it into a single dropdown item
            if (metric.metricType.includes('blood_pressure')) {
              if (!addedBloodPressure) {
                dropdownOptions.push({ value: 'blood_pressure', label: 'Blood Pressure' });
                addedBloodPressure = true;
              }
            } else {
              // Otherwise, add it exactly as it came from the API
              dropdownOptions.push({ value: metric.metricType, label: metric.displayName });
            }
          });
  
          this.availableMetrics.set(dropdownOptions);
        }
      }
    });
  }
  private renderLineChart(trendsData: any[]): void {
    if (!this.trendsCanvas) return;
    if (this.trendChartInstance) this.trendChartInstance.destroy();

    // 1. Extract a unified X-Axis timeline across all datasets
    
    const allDates = new Set<string>();
    trendsData.forEach(dataset => {
      dataset.points.forEach((p: any) => {
        // Split '2026-06-10T14:30:00Z' and only keep the '2026-06-10' part
        const dateOnly = p.dateIso.split('T')[0];
        allDates.add(dateOnly);
      });
    });
    
    // Sort dates chronologically
    const sortedDates = Array.from(allDates).sort();
    
    // Format to labels (e.g. "MMM dd")
    const chartLabels = sortedDates.map(iso => {
      const d = new Date(iso);
      return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });

    // 2. Map datasets to the unified timeline
    const datasets = trendsData.map(dataset => {
      // Create an array matching the exact length of chartLabels
      const dataPoints = sortedDates.map(dateOnly => {
        const match = dataset.points.find((p: any) => p.dateIso.startsWith(dateOnly));
        return match ? match.value : null; // null makes the line break gracefully instead of dropping to 0
      });

      return {
        label: dataset.datasetLabel,
        data: dataPoints,
        borderColor: dataset.color,
        backgroundColor: dataset.color + '33', // 20% opacity
        borderWidth: 2,
        tension: 0.3, // smooth curves
        pointRadius: 4,
        pointHoverRadius: 6,
        spanGaps: true // Connects the line even if a day is missing
      };
    });

    // 3. Render the Line Chart
    this.trendChartInstance = new Chart(this.trendsCanvas.nativeElement, {
      type: 'line',
      data: {
        labels: chartLabels,
        datasets: datasets
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12, font: { weight: 600 } } },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          y: { grid: { color: '#f1f5f9' }, title: { display: true, text: trendsData[0]?.unit || '' } },
          x: { grid: { display: false } }
        }
      }
    });
  }

  // ── DYNAMIC PARSING HELPER METHODS ─────────────────────────────────
  
  formatDateString(dateStr: string): string {
    if (!dateStr) return '—';
    return dateStr.split('T')[0];
  }

  getBPDisplayValue(metrics: { [key: string]: string }): string {
    let systolic = '—';
    let diastolic = '—';

    if (metrics['bp']) {
      systolic = metrics['bp'].split('/')[0].trim();
    } else if (metrics['systolic_blood_pressure']) {
      systolic = metrics['systolic_blood_pressure'].split('.')[0].trim();
    }

    if (metrics['dialostic_blood_pressure']) {
      diastolic = metrics['dialostic_blood_pressure'].split('.')[0].trim();
    }

    return `${systolic}/${diastolic}`;
  }

  getCleanMetricValue(metrics: { [key: string]: string }, key: string): string {
    const rawVal = metrics[key];
    if (!rawVal) return '—';
    return rawVal.split('.')[0].trim(); // Removes decimals (e.g. "85.00 mg/dl" -> "85")
  }

  getInitial(name: string | undefined): string {
    return name ? name.charAt(0).toUpperCase() : 'P';
  }

  getAvatarColor(name: string | undefined): string {
    const initial = this.getInitial(name);
    if (['A', 'E', 'I', 'O', 'U'].includes(initial)) return 'blue-bg';
    if (['R', 'S', 'T', 'L', 'N'].includes(initial)) return 'purple-bg';
    return 'default-bg';
  }

  formatStatusLabel(status: string): string {
    if (!status) return 'Unknown';
    const s = status.toLowerCase();
    return s === 'critical' ? 'Critical' : s === 'elevated' ? 'Warning' : 'Normal';
  }

  ngOnDestroy(): void {
    if (this.trendChartInstance) this.trendChartInstance.destroy();
  }
}