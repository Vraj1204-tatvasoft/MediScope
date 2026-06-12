export interface PatientDashboardContainer {
    patientName: string;
    greeting: string;
    doctorsConnected: number;
    abnormalReadingCount: number;
    hasHealthAlert: boolean;
    latestVitals: LatestVitalMetricItem[];
    recentRecords: RecentSubmissionLogItem[];
    trendCharts: MetricTrendLineChartGroup[];
    myDoctors: ConnectedPhysicianItem[];
  }
  
  export interface LatestVitalMetricItem {
    metricType: string;
    displayName: string;
    displayValue: string;
    unit: string;
    status: 'Normal' | 'Warning' | 'Elevated' | 'Critical' | string;
    trendPercent: number;
    trendDirection: 'up' | 'down' | string;
    recordedAt: string;
    normalMin?: number;
    normalMax?: number;
  }
  
  export interface RecentSubmissionLogItem {
    submissionId: string;
    recordedAt: string;
    addedBy: string;
    recordedByRole: string;
    status: 'NORMAL' | 'ELEVATED' | 'CRITICAL' | string;
    metricValues: { [key: string]: string };
  }
  
  export interface MetricTrendLineChartGroup {
    metricType: string;
    displayName: string;
    unit: string;
    dataPoints: Array<{ dateLabel: string; value: number }>;
  }
  
  export interface ConnectedPhysicianItem {
    doctorId: string;
    fullName: string;
    specialization: string;
    isActive: boolean;
  }