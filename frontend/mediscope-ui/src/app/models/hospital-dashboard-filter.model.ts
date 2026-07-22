export interface HospitalDashboardFilter {
    search?: string;
    wardId?: string;
    roomTypeId?: string;
    floor?: number;
    occupancyStatus?: number;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDir: 'asc' | 'desc';
  }