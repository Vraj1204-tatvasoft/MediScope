export interface MetricDefinition {
    id: string;
    metricType: string;      // e.g., 'BLOOD_PRESSURE', 'HEART_RATE', 'CHOLESTEROL'
    displayName: string;     // e.g., 'Blood Pressure', 'Heart Rate'
    defaultUnit: string;     // e.g., 'mmHg', 'bpm'
    normalMin?: number;
    normalMax?: number;
    normalRangeDisplay?: string; // e.g., "90–120 / 60–80 mmHg" or "60–100 bpm"
    description?: string;
  }
  
  export interface MetricValueRecord {
    metricDefinitionId: string;
    metricType: string;      
    unit: string;            
    value: number;
    valueSecondary?: number;
  }
  
  export interface AddHealthMetricRequestDto {
    submissionId?: string;
    recordedAt: string;      // ISO string date format or YYYY-MM-DD
    notes?: string;
    patientId?: string;      
    appointmentId?: string;
    metrics: MetricValueRecord[];
  }