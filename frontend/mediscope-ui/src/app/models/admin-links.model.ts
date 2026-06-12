export interface AdminOverviewResponseDto {
    totalConnections: number;
    activeLinks: number;
    pendingLinks: number;
    revokedLinks: number;
    doctors: AdminDoctorOverviewItem[];
    requests: AdminConnectionRequestItem[];
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }
  
  export interface AdminDoctorOverviewItem {
    doctorId: string;
    doctorName: string;
    specialization: string;
    patientCount: number;
    patients: string[];
  }
  
  export interface AdminConnectionRequestItem {
    doctorPatientId: string;
    patientName: string;
    doctorName: string;
    specialization: string;
    status: 'active' | 'pending' | 'revoked' | string;
    requestedAt: string;
  }
  
  export interface AdminLinkFilters {
    searchQuery: string;
    doctorId: string;
    status: string;
  }