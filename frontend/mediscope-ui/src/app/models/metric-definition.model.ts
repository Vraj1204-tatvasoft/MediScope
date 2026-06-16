export interface MetricDefinition {
    id: string;
    metricType: string;
    displayName: string;
    defaultUnit: string;
    normalMin?: number;
    normalMax?: number;
    description?: string;
    createdAt: string;
    updatedAt: string;
    isActive: boolean;
    normalRangeDisplay?: string;
  }
  
  export interface CreateMetricDefinitionRequest {
    metricType: string;
    displayName: string;
    defaultUnit: string;
    normalMin?: number;
    normalMax?: number;
    description?: string;
  }
  
  export interface UpdateMetricDefinitionRequest {
    displayName: string;
    defaultUnit: string;
    normalMin?: number;
    normalMax?: number;
    description?: string;
  }