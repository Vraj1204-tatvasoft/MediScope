import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// Angular Material Components
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// Chart.js Core Registration
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

import { PatientDashboardService } from '../../services/patient-dashboard.service';
import { PatientDashboardContainer, LatestVitalMetricItem } from '../../models/patient-dashboard.model';

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './patient-dashboard.component.html',
  styleUrls: ['./patient-dashboard.component.css']
})
export class PatientDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private dashboardService = inject(PatientDashboardService);

  @ViewChild('bpChartCanvas') private bpChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('vitalsChartCanvas') private vitalsChartCanvas!: ElementRef<HTMLCanvasElement>;

  // Core Reactive States
  isLoading = signal<boolean>(true);
  dashboard = signal<PatientDashboardContainer | null>(null);

  // Active instances caching to prevent background memory leaks on tab swapping
  private activeCharts: Chart[] = [];

  // ── COMPUTED METRIC CARD OVERRIDES MAP ─────────────────────────────
  // Groups isolated systolic and diastolic database columns into a unified Blood Pressure visual card
  processedVitalsCards = computed(() => {
    const data = this.dashboard();
    if (!data) return [];

    const vitals = data.latestVitals;
    const cards: any[] = [];

    const systolic = vitals.find(v => v.metricType === 'systolic_blood_pressure');
    const diastolic = vitals.find(v => v.metricType === 'dialostic_blood_pressure');

    if (systolic || diastolic) {
      const sysVal = systolic ? Math.round(parseFloat(systolic.displayValue)) : '—';
      const diaVal = diastolic ? Math.round(parseFloat(diastolic.displayValue)) : '—';
      
      cards.push({
        displayName: 'BLOOD PRESSURE',
        displayValue: `${sysVal}/${diaVal}`,
        unit: 'mmHg',
        status: (systolic?.status === 'Critical' || diastolic?.status === 'Critical') ? 'Critical' : 
                (systolic?.status === 'Elevated' || diastolic?.status === 'Elevated') ? 'Elevated' : 'Normal',
        trendPercent: systolic?.trendPercent ?? 0,
        trendDirection: systolic?.trendDirection ?? 'up',
        icon: 'favorite',
        class: 'blood-pressure'
      });
    }

    // Map remaining health dimensions cleanly
    vitals.forEach(v => {
      if (v.metricType !== 'systolic_blood_pressure' && v.metricType !== 'dialostic_blood_pressure') {
        cards.push({
          displayName: v.displayName.toUpperCase(),
          displayValue: Math.round(parseFloat(v.displayValue)),
          unit: v.unit,
          status: v.status,
          trendPercent: v.trendPercent,
          trendDirection: v.trendDirection,
          icon: this.getMetricIcon(v.metricType),
          class: v.metricType
        });
      }
    });

    return cards;
  });

  ngOnInit(): void {
    this.fetchDashboardDataPipeline();
  }

  ngAfterViewInit(): void {
    // Pipeline initialization checks are executed inside the network response chain
  }

  private fetchDashboardDataPipeline(): void {
    this.isLoading.set(true);
    this.dashboardService.getDashboardData().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.dashboard.set(res.data);
          // Wait briefly for Angular execution signals to draw canvas elements onto the DOM layout
          setTimeout(() => this.renderTimelineTrendCharts(), 50);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private renderTimelineTrendCharts(): void {
    const data = this.dashboard();
    if (!data || !this.bpChartCanvas || !this.vitalsChartCanvas) return;

    // Destructure active chart objects safely to clear memory spaces completely
    this.activeCharts.forEach(c => c.destroy());
    this.activeCharts = [];

    // Extract independent array trends configuration vectors
    const charts = data.trendCharts;
    const sysTrend = charts.find(c => c.metricType === 'systolic_blood_pressure');
    const diaTrend = charts.find(c => c.metricType === 'dialostic_blood_pressure');
    const hrTrend = charts.find(c => c.metricType === 'heart_rate');
    const sugarTrend = charts.find(c => c.metricType === 'blood_sugar');

    // CHART 1: DUAL LINE BLOOD PRESSURE TRENDS GRAPH
    if (sysTrend || diaTrend) {
      const labels = (sysTrend?.dataPoints || diaTrend?.dataPoints || []).map(dp => dp.dateLabel);
      
      const bpChartInstance = new Chart(this.bpChartCanvas.nativeElement, {
        type: 'line',
        data: {
          labels: labels,
          datasets: [
            {
              label: 'Systolic',
              data: (sysTrend?.dataPoints || []).map(dp => dp.value),
              borderColor: '#3b82f6',
              backgroundColor: 'rgba(59, 130, 246, 0.05)',
              tension: 0.3,
              fill: true,
              spanGaps: true // ENSURES CONTINUOUS FILL LINES WITHOUT GAPS
            },
            {
              label: 'Diastolic',
              data: (diaTrend?.dataPoints || []).map(dp => dp.value),
              borderColor: '#0d9488',
              backgroundColor: 'transparent',
              tension: 0.3,
              spanGaps: true // ENSURES CONTINUOUS FILL LINES WITHOUT GAPS
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'bottom' } },
          scales: { y: { grid: { color: '#f1f5f9' } }, x: { grid: { display: false } } }
        }
      });
      this.activeCharts.push(bpChartInstance);
    }

    // CHART 2: HEART RATE AND GLUCOSE CROSS OVERVIEW GRAPH
    if (hrTrend || sugarTrend) {
      const labels = (hrTrend?.dataPoints || sugarTrend?.dataPoints || []).map(dp => dp.dateLabel);

      const vitalsChartInstance = new Chart(this.vitalsChartCanvas.nativeElement, {
        type: 'line',
        data: {
          labels: labels,
          datasets: [
            {
              label: 'Heart Rate (bpm)',
              data: (hrTrend?.dataPoints || []).map(dp => dp.value),
              borderColor: '#dc2626',
              backgroundColor: 'transparent',
              tension: 0.3,
              spanGaps: true //  CONTINUOUS
            },
            {
              label: 'Blood Sugar (mg/dl)',
              data: (sugarTrend?.dataPoints || []).map(dp => dp.value),
              borderColor: '#d97706',
              backgroundColor: 'transparent',
              tension: 0.3,
              spanGaps: true // CONTINUOUS
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'bottom' } },
          scales: { y: { grid: { color: '#f1f5f9' } }, x: { grid: { display: false } } }
        }
      });
      this.activeCharts.push(vitalsChartInstance);
    }
  }

  // UI HELPER UTILITIES
  formatStatusLabel(status: string): string {
    if (!status) return 'Normal';
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  }

  formatDateString(dateStr: string): string {
    if (!dateStr) return '—';
    return dateStr.split('T')[0];
  }

  getCombinedBPValue(metricValues: { [key: string]: string }): string {
    const sys = metricValues['systolic_blood_pressure'];
    const dia = metricValues['dialostic_blood_pressure'];
    if (!sys && !dia) return '—';
    
    const sysNum = sys ? sys.split('.')[0] : '—';
    const diaNum = dia ? dia.split('.')[0] : '—';
    return `${sysNum}/${diaNum}`;
  }

  getCleanMetricRowValue(metricValues: { [key: string]: string }, key: string): string {
    const rawVal = metricValues[key];
    if (!rawVal) return '—';
    return rawVal.split('.')[0] + (key === 'sleep' ? ' hrs' : '');
  }

  getInitial(name: string | undefined): string {
    return name ? name.charAt(0).toUpperCase() : 'D';
  }

  private getMetricIcon(type: string): string {
    switch (type.toLowerCase().trim()) {
      case 'heart_rate': return 'pulse';
      case 'blood_sugar': return 'opacity';
      case 'sleep': return 'bedtime';
      default: return 'analytics';
    }
  }

  ngOnDestroy(): void {
    this.activeCharts.forEach(c => c.destroy());
  }
}