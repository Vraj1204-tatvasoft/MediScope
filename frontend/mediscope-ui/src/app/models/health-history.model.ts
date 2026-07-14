export interface HealthMetricItem {
  id: string;
  metricType: string;
  displayName: string;
  value: number;
  unit: string;
  normalMin?: number;
  normalMax?: number;
  status: string;
}

export interface HealthMetricSubmission {
  submissionId: string;
  patientId: string;
  recordedByUserId: string;
  recordedByRole: string;
  recordedByName: string;
  recordedAt: string;
  notes?: string;
  status: string;
  createdAt: string;
  metrics: HealthMetricItem[];
}

export interface HealthHistoryRow {
  submissionId: string;
  date: string;
  addedBy: string;
  isDoctor: boolean;
  notes?: string;
  status: string;
  flaggedMetrics: string[];
  isExpanded?: boolean;
  metrics: {
    [metricType: string]: {
      displayValue: string;
      rawVal: number;
    };
  };
  canEdit?: boolean;
}

export interface HistorySummaryStats {
  totalRecords: number;
  normal: number;
  elevated: number;
  critical: number;
}