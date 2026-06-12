import { Component, OnInit, ElementRef, ViewChild, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

// Material Layout Components
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// Chart.js Framework Elements
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

import { AdminDashboardService } from '../../services/admin-dashboard.service';
import { AdminDashboardContainer } from '../../models/admin-dashboard.model';

@Component({
  selector: 'app-admin-dashboard',
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
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private adminDashboardService = inject(AdminDashboardService);

  // Canvas DOM Anchor Bindings
  @ViewChild('growthChartCanvas') private growthChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('severityChartCanvas') private severityChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('alertsMetricChartCanvas') private alertsMetricChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('doctorLoadChartCanvas') private doctorLoadChartCanvas!: ElementRef<HTMLCanvasElement>;

  // System Core Presentation Signals
  isLoading = signal<boolean>(true);
  dashboardData = signal<AdminDashboardContainer | null>(null);

  private renderedCharts: Chart[] = [];

  // Computed Quick Lookups Mapping
  stats = computed(() => this.dashboardData()?.stats);
  summary = computed(() => this.dashboardData()?.readingSummary);
  recentActivities = computed(() => this.dashboardData()?.recentActivity || []);

  ngOnInit(): void {
    this.loadAdminMetricsPipeline();
  }

  private loadAdminMetricsPipeline(): void {
    this.isLoading.set(true);
    this.adminDashboardService.getAdminDashboard().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.dashboardData.set(res.data);
          // Allow macro task tracking loop to draw empty elements before executing canvas scripts
          setTimeout(() => this.initializeDashboardAnalyticsGraphs(), 50);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private initializeDashboardAnalyticsGraphs(): void {
    const data = this.dashboardData();
    if (!data) return;

    // Flush old allocations safely to prevent memory leak crashes
    this.renderedCharts.forEach(c => c.destroy());
    this.renderedCharts = [];

    // GRAPH 1: PLATFORM GROWTH TIMELINE (LINE CHART)
    if (this.growthChartCanvas) {
      this.renderedCharts.push(new Chart(this.growthChartCanvas.nativeElement, {
        type: 'line',
        data: {
          labels: data.platformGrowth.map(g => g.monthLabel),
          datasets: [{
            label: 'Patients',
            data: data.platformGrowth.map(g => g.patientCount),
            borderColor: '#2563eb',
            backgroundColor: 'rgba(37, 99, 235, 0.05)',
            tension: 0.3,
            fill: true
          }]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }
      }));
    }

    // GRAPH 2: READING SEVERITY SUMMARY (DOUGHNUT DONUT)
    if (this.severityChartCanvas) {
      this.renderedCharts.push(new Chart(this.severityChartCanvas.nativeElement, {
        type: 'doughnut',
        data: {
          labels: ['Normal', 'Elevated', 'Critical'],
          datasets: [{
            data: [data.readingSeverity.normal, data.readingSeverity.elevated, data.readingSeverity.critical],
            backgroundColor: ['#10b981', '#f59e0b', '#ef4444'],
            borderWidth: 2,
            hoverOffset: 4
          }]
        },
        options: { 
          responsive: true, 
          maintainAspectRatio: false, 
          plugins: { legend: { position: 'bottom', labels: { boxWidth: 12, font: { weight: 600 } } } },
          cutout: '70%'
        }
      }));
    }

    // GRAPH 3: ALERTS BY METRIC TYPE (HORIZONTAL BAR GRAPH)
    if (this.alertsMetricChartCanvas) {
      this.renderedCharts.push(new Chart(this.alertsMetricChartCanvas.nativeElement, {
        type: 'bar',
        data: {
          labels: data.alertsByMetric.map(a => a.displayName),
          datasets: [{
            label: 'Abnormal Readings',
            data: data.alertsByMetric.map(a => a.abnormalCount),
            backgroundColor: ['#3b82f6', '#f59e0b', '#06b6d4', '#ef4444', '#a855f7'],
            borderRadius: 6
          }]
        },
        options: { 
          indexAxis: 'y', //  FIXED: Moved safely inside the options node literal
          responsive: true, 
          maintainAspectRatio: false, 
          plugins: { 
            legend: { display: false } 
          },
          scales: {
            x: {
              ticks: {
                stepSize: 1 // Forces whole numbers on the bottom axis
              }
            }
          }
        }
      }));
    }

    // GRAPH 4: DOCTOR PATIENT LOAD INTENSITY (VERTICAL COLUMN GRAPH)
    if (this.doctorLoadChartCanvas) {
      this.renderedCharts.push(new Chart(this.doctorLoadChartCanvas.nativeElement, {
        type: 'bar',
        data: {
          labels: data.doctorLoad.map(d => d.fullName),
          datasets: [{
            label: 'Assigned Patients',
            data: data.doctorLoad.map(d => d.activePatients),
            backgroundColor: '#0ea5e9',
            borderRadius: 6,
            maxBarThickness: 26
          }]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } },
        scales: {
          y: {
            ticks: {
              stepSize: 1 // Forces whole numbers on the left axis
            }
          }
        } }
      }));
    }
  }
  // Inside admin-dashboard.component.ts class body
  readonly todayDateString = computed(() => {
    return new Date().toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  });
  // ── PRESENTATION FORMATTING CONVERSION HELPERS ─────────────────────────
  formatDateString(dateStr: string): string {
    if (!dateStr) return '—';
    return dateStr.split('T')[0];
  }

  getCleanCellValue(metrics: { [key: string]: string }, key: string): string {
    const rawValue = metrics[key];
    if (!rawValue) return '—';
    // Clean strings like "89.00 bpm" or "150.00 mg/dl" to read as integers "89" or "150"
    return rawValue.split('.')[0] + (key === 'sleep' ? ' hrs' : '');
  }

  getBPValueDisplay(metrics: { [key: string]: string }): string {
    if (metrics['bp']) return metrics['bp'].split('/')[0] + '/' + (metrics['dialostic_blood_pressure'] ? metrics['dialostic_blood_pressure'].split('.')[0] : '—');
    return '—';
  }

  formatStatusLabel(status: string): string {
    if (!status) return 'Unknown';
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  }

  ngOnDestroy(): void {
    this.renderedCharts.forEach(c => c.destroy());
  }
}