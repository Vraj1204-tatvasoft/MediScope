import { Component, Input, OnInit, effect, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions, ChartType, registerables, Chart } from 'chart.js';
import { HealthHistoryService } from '../../../../services/health-history.service';
import { MatIcon } from '@angular/material/icon';

// Register all chart elements natively
Chart.register(...registerables);

@Component({
  selector: 'app-trend-charts',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIcon, MatProgressSpinnerModule, BaseChartDirective],
  templateUrl: './trend-charts.component.html',
  styleUrls: ['./trend-charts.component.css']
})
export class TrendChartsComponent implements OnInit {
  @Input({ required: true }) patientId!: string;

  isLoading = signal<boolean>(true);
  hasData = signal<boolean>(false);

  // Chart Configuration Options Object Blocks
  chartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { grid: { display: false } },
      y: { ticks: { precision: 0 }, grace: '20%'}
    },
    plugins: {
      legend: { position: 'bottom', labels: { boxWidth: 12, usePointStyle: true } }
    }
  };

  // Dedicated Chart Data Buffers
  bpData!: ChartConfiguration['data'];
  HrData!: ChartConfiguration['data'];
  glucoseData!: ChartConfiguration['data'];
  sleepData!: ChartConfiguration['data'];

  private historyService = inject(HealthHistoryService);

  ngOnInit(): void {
    this.fetchTimelineTrendData();
  }

  fetchTimelineTrendData(): void {
    this.isLoading.set(true);
    
    const search = '';
    const status = 'ALL';
    const source = 'ALL';
    const sortBy = 'date';
    const sortDir = 'asc'; // Keep chronological order for trends

    //  DYNAMIC CHANNEL SWITCH
    const dataStream$ = this.patientId
      ? this.historyService.getHistoryByPatientId(this.patientId, 1, 100, search, status, source, sortBy, sortDir)
      : this.historyService.getMyMetrics(1, 100, search, status, source, sortBy, sortDir);

    dataStream$.subscribe({
      next: (response) => {
        const submissions = response.items || response;

        if (!submissions || submissions.length === 0) {
          this.hasData.set(false);
          this.isLoading.set(false);
          return;
        }

        this.hasData.set(true);
        this.renderChartsPipeline(submissions);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.hasData.set(false);
      }
    });
  }

  private renderChartsPipeline(submissions: any[]): void {
    // 1. Map labels array using truncated localized calendar keys
    const labels = submissions.map(s => s.recordedAt ? s.recordedAt.split('T')[0].substring(5) : '—');

    // 2. Initialize metric data extraction buffers
    const systolic: number[] = [];
    const diastolic: number[] = [];
    const hr: number[] = [];
    const glucose: number[] = [];
    const sleep: number[] = [];

    submissions.forEach(s => {
      const metrics = s.metrics || [];
      
      const getVal = (type: string) => metrics.find((m: any) => m.metricType.toLowerCase().trim() === type)?.value ?? null;

      // FIXED: Consolidated logic cleanly so values don't overwrite or skip assignments
      systolic.push(getVal('systolic_blood_pressure'));
      diastolic.push(getVal('dialostic_blood_pressure'));
      hr.push(getVal('heart_rate')); // 🛠️ FIX: Heart rate data is now extracted and pushed correctly
      glucose.push(getVal('blood_sugar'));
      sleep.push(getVal('sleep'));
    });

    // Chart 1: Blood Pressure Trend
    this.bpData = {
      labels,
      datasets: [
        { data: systolic, label: 'Systolic', borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,0.1)', fill: true, tension: 0.3, spanGaps: true },
        { data: diastolic, label: 'Diastolic', borderColor: '#06b6d4', backgroundColor: 'rgba(6,182,212,0.1)', fill: true, tension: 0.3, spanGaps: true }
      ]
    };

    // Chart 2: Heart Rate Trend
    this.HrData = {
      labels,
      datasets: [
        { data: hr, label: 'Heart Rate', borderColor: '#ef4444', backgroundColor: 'rgba(239,68,68,0.05)', fill: true, tension: 0.2, spanGaps: true }
      ]
    };

    // Chart 3: Glucose Trend
    this.glucoseData = {
      labels,
      datasets: [
        { data: glucose, label: 'Glucose', borderColor: '#f59e0b', backgroundColor: 'rgba(245,158,11,0.05)', fill: true, tension: 0.2, spanGaps: true },
      ]
    };

    // Chart 4: Sleep Duration
    this.sleepData = {
      labels,
      datasets: [
        { data: sleep, label: 'Sleep (hrs)', borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.08)', fill: true, tension: 0.3, spanGaps: true }
      ]
    };
  }
}