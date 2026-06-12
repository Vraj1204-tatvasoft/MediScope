import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BaseHttpService } from './base-http.service'; 
import { PatientDashboardContainer } from '../models/patient-dashboard.model';
import { ApiResponse } from '../models/api-response.model'; 

@Injectable({
  providedIn: 'root'
})
export class PatientDashboardService {
  private baseHttp = inject(BaseHttpService);

  getDashboardData(): Observable<ApiResponse<PatientDashboardContainer>> {
    return this.baseHttp.get<PatientDashboardContainer>('dashboard/patient', {
      showError: true,
      showSuccess: false
    });
  }
}