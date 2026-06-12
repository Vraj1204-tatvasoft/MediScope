
export interface PagedResponse<T> {
    items: T[];
  
    pageNumber: number;
  
    pageSize: number;
  
    totalCount: number;
  
    totalPages: number;
  
    hasPreviousPage: boolean;
  
    hasNextPage: boolean;
    summaryStats?: {
      totalRecords: number;
      normal: number;
      elevated: number;
      critical: number;
    };
  }

 