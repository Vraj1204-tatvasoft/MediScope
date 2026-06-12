export interface AdminDashboardContainer {
    stats: AdminSystemStats;
    readingSummary: AdminReadingSummary;
    platformGrowth: PlatformGrowthPoint[];
    readingSeverity: ReadingSeverityMap;
    alertsByMetric: AlertsByMetricItem[];
    doctorLoad: DoctorLoadItem[];
    recentActivity: RecentActivityRowItem[];
    unreadAlertCount: number;
  }
  
  export interface AdminSystemStats {
    totalPatients: number;
    totalDoctors: number;
    totalRecords: number;
    activeAlerts: number;
  }
  
  export interface AdminReadingSummary {
    normalCount: number;
    elevatedCount: number;
    criticalCount: number;
    total: number;
    normalPct: number;
    elevatedPct: number;
    criticalPct: number;
  }
  
  export interface PlatformGrowthPoint {
    monthLabel: string;
    patientCount: number;
    doctorCount: number;
  }
  
  export interface ReadingSeverityMap {
    normal: number;
    elevated: number;
    critical: number;
  }
  
  export interface AlertsByMetricItem {
    metricType: string;
    displayName: string;
    abnormalCount: number;
  }
  
  export interface DoctorLoadItem {
    doctorId: string;
    doctorName: string;
    fullName: string;
    specialization: string;
    activePatients: number;
  }
  
  export interface RecentActivityRowItem {
    submissionId: string;
    patientName: string;
    recordedAt: string;
    addedBy: 'Doctor' | 'Patient' | string;
    status: 'NORMAL' | 'ELEVATED' | 'CRITICAL' | string;
    metricValues: { [key: string]: string };
  }