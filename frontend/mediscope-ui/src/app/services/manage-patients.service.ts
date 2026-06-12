import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { AdminPatientOverviewContainer } from '../models/manage-patients.model';
import { BaseHttpService } from './base-http.service';

@Injectable({
  providedIn: 'root'
})
export class ManagePatientsService {
  private baseHttp = inject(BaseHttpService);

  getAdminPatients(
    page: number, 
    size: number, 
    search?: string, 
    gender?: string
  ): Observable<ApiResponse<AdminPatientOverviewContainer>> {
    let queryParams = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', size.toString());

    if (search?.trim()) {
      queryParams = queryParams.set('search', search.trim());
    }
    if (gender && gender !== 'ALL') {
      queryParams = queryParams.set('gender', gender);
    }

    return this.baseHttp.get<AdminPatientOverviewContainer>('patient/admin/all', {
      params: queryParams,
      showError: true,
      showSuccess: false
    });
  }
}