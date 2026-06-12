export interface AdminPatientOverviewContainer {
    totalPatients: number;
    malePatients: number;
    femalePatients: number;
    criticalPatients: number;
    patients: AdminPatientPagedWrapper;
  }
  
  export interface AdminPatientPagedWrapper {
    items: AdminPatientRowItem[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
  }
  
  export interface AdminPatientRowItem {
    patientId: string;
    fullName: string;
    email: string;
    age: number;
    gender: string;
    bloodGroup: string;
    doctors: string[];
    totalRecords: number;
    latestStatus: 'Normal' | 'Warning' | 'Critical' | string;
  }