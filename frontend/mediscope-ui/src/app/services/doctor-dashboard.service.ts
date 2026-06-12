import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service'; // Adjust path reference cleanly
import { DoctorDashboardContainer, MetricDefinition, VitalTrendResponse } from '../models/doctor-dashboard.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class DoctorDashboardService {
  private baseHttp = inject(BaseHttpService);

  getDoctorDashboard(): Observable<ApiResponse<DoctorDashboardContainer>> {
    return this.baseHttp.get<DoctorDashboardContainer>('dashboard/doctor', {
      showError: true,
      showSuccess: false
    });
  }

  getVitalTrends(
    metricType: string,
    patientId: string,
    duration: string,
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResponse<VitalTrendResponse[]>> {
    const queryParams: any = {
      metricType,
      patientId,
      duration
    };

    // Only append custom dates if they exist to prevent sending "undefined" or empty strings to the API
    if (fromDate) queryParams.fromDate = fromDate;
    if (toDate) queryParams.toDate = toDate;

    return this.baseHttp.get<VitalTrendResponse[]>('dashboard/doctor/vital-trends', {
      params: queryParams,
      showError: true,
      showSuccess: false
    });
  }

  getMetricDefinitions(): Observable<ApiResponse<MetricDefinition[]>> {
    return this.baseHttp.get<MetricDefinition[]>('metric-definitions', {
      showError: true,
      showSuccess: false
    });
  }
}