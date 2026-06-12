export interface DoctorDashboardContainer {
    doctorName: string;
    specialization: string;
    hospital: string;
    criticalPatientCount: number;
    criticalPatientNames: string[] | null;
    myPatients: number;
    activeAlerts: number;
    totalRecords: number;
    criticalPatients: number;
    patientStatusOverview: PatientStatusOverviewItem[];
    recentActivity: DoctorRecentActivityItem[];
  }
  
  export interface PatientStatusOverviewItem {
    patientId: string;
    fullName: string;
    totalRecords: number;
    totalAlerts: number;
    latestStatus: 'NORMAL' | 'ELEVATED' | 'CRITICAL' | string;
    latestRecordAt: string;
  }
  
  export interface DoctorRecentActivityItem {
    submissionId: string;
    patientId: string;
    patientName: string;
    recordedAt: string;
    addedBy: 'Doctor' | 'Patient' | string;
    status: 'NORMAL' | 'ELEVATED' | 'CRITICAL' | string;
    metricValues: { [key: string]: string };
  }

  export interface VitalTrendPoint {
    dateLabel: string;
    dateIso: string;
    value: number;
  }
  
  export interface VitalTrendResponse {
    datasetLabel: string;
    patientId: string;
    patientName: string;
    metricType: string;
    displayName: string;
    unit: string;
    color: string;
    points: VitalTrendPoint[];
  }

  export interface MetricDefinition {
    id: string;
    metricType: string;
    displayName: string;
  }