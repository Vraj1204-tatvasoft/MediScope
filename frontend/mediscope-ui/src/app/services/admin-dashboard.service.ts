import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service'; 
import { AdminDashboardContainer } from '../models/admin-dashboard.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AdminDashboardService {
  private baseHttp = inject(BaseHttpService);

  getAdminDashboard(): Observable<ApiResponse<AdminDashboardContainer>> {
    return this.baseHttp.get<AdminDashboardContainer>('dashboard/admin', {
      showError: true,
      showSuccess: false
    });
  }
}